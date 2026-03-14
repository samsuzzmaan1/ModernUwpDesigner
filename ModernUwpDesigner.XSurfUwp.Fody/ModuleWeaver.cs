using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ModernUwpDesigner.XSurfUwp.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            FixDesignInstanceTypeSetter();
        }

        private const string XamlTypeInfoProvider = "XSurfUwp.ModernUwpDesigner_XSurfUwp_XamlTypeInfo.XamlTypeInfoProvider";
        private const string DesignInstance = "XSurfUwp.DesignInstance";
        private const string SetTypeValue = nameof(SetTypeValue);
        private const string set_Type = nameof(set_Type);

        private void FixDesignInstanceTypeSetter()
        {
            /*
            
            Original:

            .method private instance void set_4_DesignInstance_Type(object 'instance', object Value) cil managed 
            {
                .maxstack 2
                .locals init (
                    [0] class XSurfUwp.DesignInstance that
                )

                // {
                nop
                // DesignInstance designInstance = (DesignInstance)instance;
                ldarg.1
                castclass XSurfUwp.DesignInstance
                stloc.0
                // designInstance.Type = (Type)Value;
                ldloc.0
                ldarg.2
                castclass [System.Runtime]System.Type
                callvirt instance void XSurfUwp.DesignInstance::set_Type(class [System.Runtime]System.Type)
                // }
                nop
                ret
            }

            Patched:

            .method private instance void set_4_DesignInstance_Type(object 'instance', object Value) cil managed 
            {
                .maxstack 2

                // {
                nop
                // ((DesignInstance)instance).SetTypeValue(Value);
                ldarg.1
                castclass XSurfUwp.DesignInstance
                ldarg.2
                call void XSurfUwp.DesignInstance::SetTypeValue(object)
                // }
                nop
                ret
            }
            */

            var infoProviderType = ModuleDefinition.GetType(XamlTypeInfoProvider);
            if (infoProviderType is null)
            {
                WriteError($"[FixDesignInstanceTypeSetter] Could not find {XamlTypeInfoProvider} in the module.");
                return;
            }

            var typeSetterMethod = infoProviderType.Methods.FirstOrDefault(m =>
                m.ReturnType.MetadataType is MetadataType.Void &&
                m.Name.EndsWith("_DesignInstance_Type", StringComparison.Ordinal) &&
                m.Parameters.Count is 2);

            if (typeSetterMethod is null)
            {
                WriteError($"[FixDesignInstanceTypeSetter] Could not find the {DesignInstance}.Type setter method in {XamlTypeInfoProvider}.");
                return;
            }

            var diType = ModuleDefinition.GetType(DesignInstance);
            if (diType is null)
            {
                WriteError($"[FixDesignInstanceTypeSetter] Could not find {DesignInstance} in the module.");
                return;
            }

            var setTypeValue = diType.Methods.FirstOrDefault(m => m.Name == SetTypeValue);
            if (setTypeValue is null)
            {
                WriteError($"[FixDesignInstanceTypeSetter] Could not find {SetTypeValue} method in {DesignInstance}.");
                return;
            }

            bool patched = false;
            var instructions = typeSetterMethod.Body.Instructions;
            for (int i = 1; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                if (instruction.OpCode != OpCodes.Castclass ||
                    instruction.Operand is not TypeReference typeRef ||
                    !typeRef.FullName.Equals("System.Type", StringComparison.Ordinal))
                    continue;

                int setterIdx = i + 1;
                if (setterIdx >= instructions.Count)
                {
                    WriteError($"[FixDesignInstanceTypeSetter] Unexpected end of instructions after castclass in {typeSetterMethod.FullName}.");
                    return;
                }

                var setterInstruction = instructions[setterIdx];
                if (setterInstruction.OpCode != OpCodes.Callvirt || setterInstruction.Operand is not MethodReference methodRef || methodRef.Name != set_Type)
                {
                    WriteError($"[FixDesignInstanceTypeSetter] Expected a callvirt to {set_Type} after castclass in {typeSetterMethod.FullName}, but found {setterInstruction}.");
                    return;
                }

                instructions[setterIdx] = Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(setTypeValue));
                instructions.RemoveAt(i);

                if (instructions[i - 2].OpCode == OpCodes.Ldloc_0 &&
                    instructions[i - 3].OpCode == OpCodes.Stloc_0)
                {
                    instructions.RemoveAt(i - 2);
                    instructions.RemoveAt(i - 3);
                    typeSetterMethod.Body.Variables.RemoveAt(0);
                }
                else
                {
                    WriteWarning("[FixDesignInstanceTypeSetter] Unexpected instruction pattern around the castclass, Verify the changes under ILSpy.");
                }

                patched = true;
                break;
            }

            if (!patched)
            {
                WriteError($"[FixDesignInstanceTypeSetter] Could not find the expected instruction pattern in {typeSetterMethod.FullName} to patch.");
            }
            else
            {
                WriteDebug($"[FixDesignInstanceTypeSetter] Successfully patched {typeSetterMethod.FullName} to call {SetTypeValue} instead of setting the Type property directly.");
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning() => [];
    }
}

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Graphics.Effects;
using WinRT;
using WinRT.Interop;

namespace XSurfUwp.Composition
{
    [WindowsRuntimeType]
    [WindowsRuntimeHelperType(typeof(ABI.XSurfUwp.Composition.IGraphicsEffectD2D1Interop))]
    [Guid("2FC57384-A068-44D7-A331-30982FCF7177")]
    public partial interface IGraphicsEffectD2D1Interop
    {
        Guid GetEffectId();

        GRAPHICS_EFFECT_PROPERTY_MAPPING GetNamedPropertyMapping([MarshalAs(UnmanagedType.LPWStr)] string name, out uint index);

        uint GetPropertyCount();

        object GetProperty(uint index);

        IGraphicsEffectSource GetSource(uint index);

        uint GetSourceCount();
    }

    public enum GRAPHICS_EFFECT_PROPERTY_MAPPING
    {
        UNKNOWN = 0,
        DIRECT = 1,
        VECTORX = 2,
        VECTORY = 3,
        VECTORZ = 4,
        VECTORW = 5,
        RECT_TO_VECTOR4 = 6,
        RADIANS_TO_DEGREES = 7,
        COLORMATRIX_ALPHA_MODE = 8,
        COLOR_TO_VECTOR3 = 9,
        COLOR_TO_VECTOR4 = 10
    }
}

namespace ABI.XSurfUwp.Composition
{
    [StructLayout(LayoutKind.Sequential)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Guid("2FC57384-A068-44D7-A331-30982FCF7177")]
    internal unsafe struct IGraphicsEffectD2D1Interop
    {
        private Vftbl* lpVtbl;

        [Guid("2FC57384-A068-44D7-A331-30982FCF7177")]
        public struct Vftbl
        {
            internal IUnknownVftbl IUnknownVftbl;

            public unsafe delegate* unmanaged[Stdcall]<IntPtr, Guid*, int> GetEffectId;
            public unsafe delegate* unmanaged[Stdcall]<IntPtr, char*, uint*, global::XSurfUwp.Composition.GRAPHICS_EFFECT_PROPERTY_MAPPING*, int> GetNamedPropertyMapping;
            public unsafe delegate* unmanaged[Stdcall]<IntPtr, uint*, int> GetPropertyCount;
            public unsafe delegate* unmanaged[Stdcall]<IntPtr, uint, nint*, int> GetProperty;
            public unsafe delegate* unmanaged[Stdcall]<IntPtr, uint, nint*, int> GetSource;
            public unsafe delegate* unmanaged[Stdcall]<IntPtr, uint*, int> GetSourceCount;

            public static readonly IntPtr AbiToProjectionVftablePtr;

            unsafe static Vftbl()
            {
                AbiToProjectionVftablePtr = ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), sizeof(IUnknownVftbl) + (sizeof(void*) * 6));
                *(Vftbl*)AbiToProjectionVftablePtr = new Vftbl
                {
                    IUnknownVftbl = IUnknownVftbl.AbiToProjectionVftbl,
                    GetEffectId = (delegate* unmanaged[Stdcall]<IntPtr, Guid*, int>)&GetEffectIdFromAbi,
                    GetNamedPropertyMapping = (delegate* unmanaged[Stdcall]<IntPtr, char*, uint*, global::XSurfUwp.Composition.GRAPHICS_EFFECT_PROPERTY_MAPPING*, int>)&GetNamedPropertyMappingFromAbi,
                    GetPropertyCount = (delegate* unmanaged[Stdcall]<IntPtr, uint*, int>)&GetPropertyCountFromAbi,
                    GetProperty = (delegate* unmanaged[Stdcall]<IntPtr, uint, nint*, int>)&GetPropertyFromAbi,
                    GetSource = (delegate* unmanaged[Stdcall]<IntPtr, uint, nint*, int>)&GetSourceFromAbi,
                    GetSourceCount = (delegate* unmanaged[Stdcall]<IntPtr, uint*, int>)&GetSourceCountFromAbi
                };
            }

            [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvStdcall) })]
            private unsafe static int GetEffectIdFromAbi(IntPtr thisPtr, Guid* guid)
            {
                *guid = ComWrappersSupport.FindObject<global::XSurfUwp.Composition.IGraphicsEffectD2D1Interop>(thisPtr).GetEffectId();
                return 0;
            }

            [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvStdcall) })]
            private unsafe static int GetNamedPropertyMappingFromAbi(IntPtr thisPtr, char* name, uint* index, global::XSurfUwp.Composition.GRAPHICS_EFFECT_PROPERTY_MAPPING* mapping)
            {
                try
                {
                    var result = ComWrappersSupport.FindObject<global::XSurfUwp.Composition.IGraphicsEffectD2D1Interop>(thisPtr).GetNamedPropertyMapping(new(name), out uint indexValue);
                    *index = indexValue;
                    *mapping = result;
                    return 0;
                }
                catch (Exception ex)
                {
                    return ex.HResult;
                }
            }

            [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvStdcall) })]
            private unsafe static int GetPropertyCountFromAbi(IntPtr thisPtr, uint* count)
            {
                try
                {
                    *count = ComWrappersSupport.FindObject<global::XSurfUwp.Composition.IGraphicsEffectD2D1Interop>(thisPtr).GetPropertyCount();
                    return 0;
                }
                catch (Exception ex)
                {
                    return ex.HResult;
                }
            }

            [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvStdcall) })]
            private unsafe static int GetPropertyFromAbi(IntPtr thisPtr, uint index, nint* value)
            {
                try
                {
                    *value = ((IWinRTObject)ComWrappersSupport.FindObject<global::XSurfUwp.Composition.IGraphicsEffectD2D1Interop>(thisPtr).GetProperty(index)).NativeObject.GetRef();
                    return 0;
                }
                catch (Exception ex)
                {
                    return ex.HResult;
                }
            }

            [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvStdcall) })]
            private unsafe static int GetSourceFromAbi(IntPtr thisPtr, uint index, nint* source)
            {
                try
                {
                    *source = ((IWinRTObject)ComWrappersSupport.FindObject<global::XSurfUwp.Composition.IGraphicsEffectD2D1Interop>(thisPtr).GetSource(index)).NativeObject.GetRef();
                    return 0;
                }
                catch (Exception ex)
                {
                    return ex.HResult;
                }
            }

            [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvStdcall) })]
            private unsafe static int GetSourceCountFromAbi(IntPtr thisPtr, uint* count)
            {
                try
                {
                    *count = ComWrappersSupport.FindObject<global::XSurfUwp.Composition.IGraphicsEffectD2D1Interop>(thisPtr).GetSourceCount();
                    return 0;
                }
                catch (Exception ex)
                {
                    return ex.HResult;
                }
            }
        }

        internal Guid GetEffectId()
        {
            int hr;
            Unsafe.SkipInit(out Guid result);
            if ((hr = lpVtbl->GetEffectId((nint)Unsafe.AsPointer(ref this), &result)) >= 0)
            {
                return result;
            }

            throw Marshal.GetExceptionForHR(hr);
        }

        internal global::XSurfUwp.Composition.GRAPHICS_EFFECT_PROPERTY_MAPPING GetNamedPropertyMapping(string name, out uint index)
        {
            int hr;
            global::XSurfUwp.Composition.GRAPHICS_EFFECT_PROPERTY_MAPPING mapping;
            fixed (char* namePtr = name)
            {
                uint outIndex;
                if ((hr = lpVtbl->GetNamedPropertyMapping((nint)Unsafe.AsPointer(ref this), namePtr, &outIndex, &mapping)) >= 0)
                {
                    index = outIndex;
                    return mapping;
                }
            }
    
            throw Marshal.GetExceptionForHR(hr);
        }

        internal uint GetPropertyCount()
        {
            int hr;
            uint count;
            if ((hr = lpVtbl->GetPropertyCount((nint)Unsafe.AsPointer(ref this), &count)) >= 0)
            {
                return count;
            }

            throw Marshal.GetExceptionForHR(hr);
        }

        internal object GetProperty(uint index)
        {
            int hr;
            nint value;
            if ((hr = lpVtbl->GetProperty((nint)Unsafe.AsPointer(ref this), index, &value)) >= 0)
            {
                var obj = MarshalInspectable<object>.FromAbi(value);
                Marshal.Release(value);
                return obj;
            }

            throw Marshal.GetExceptionForHR(hr);
        }

        internal IGraphicsEffectSource GetSource(uint index)
        {
            int hr;
            nint source;
            if ((hr = lpVtbl->GetSource((nint)Unsafe.AsPointer(ref this), index, &source)) >= 0)
            {
                var obj = MarshalInterface<IGraphicsEffectSource>.FromAbi(source);
                Marshal.Release(source);
                return obj;
            }

            throw Marshal.GetExceptionForHR(hr);
        }

        internal uint GetSourceCount()
        {
            int hr;
            uint count;
            if ((hr = lpVtbl->GetSourceCount((nint)Unsafe.AsPointer(ref this), &count)) >= 0)
            {
                return count;
            }

            throw Marshal.GetExceptionForHR(hr);
        }

        internal static ObjectReference<IUnknownVftbl> FromAbi(IntPtr thisPtr)
        {
            return ObjectReference<IUnknownVftbl>.FromAbi(thisPtr, typeof(IGraphicsEffectD2D1Interop).GUID);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static unsafe class IGraphicsEffectD2D1InteropMethods
    {
        public static Guid IID => typeof(IGraphicsEffectD2D1Interop).GUID;

        public static IntPtr AbiToProjectionVftablePtr => IGraphicsEffectD2D1Interop.Vftbl.AbiToProjectionVftablePtr;
    }
}

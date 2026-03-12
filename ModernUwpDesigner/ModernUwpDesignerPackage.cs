using Microsoft.VisualStudio;
using Microsoft.VisualStudio.DesignTools.DesignerContract;
using Microsoft.VisualStudio.DesignTools.DesignerHost;
using Microsoft.VisualStudio.DesignTools.DesignerHost.HostServices;
using Microsoft.VisualStudio.DesignTools.DesignerHost.Platform;
using Microsoft.VisualStudio.DesignTools.SurfaceDesigner.Documents.Project;
using Microsoft.VisualStudio.DesignTools.SurfaceDesigner.Views;
using Microsoft.VisualStudio.DesignTools.Utility;
using Microsoft.VisualStudio.DesignTools.Utility.Extensions;
using Microsoft.VisualStudio.DesignTools.UwpDesignerHost;
using Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner.Documents;
using Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner.Views;
using Microsoft.VisualStudio.DesignTools.XamlDesignerHost.DesignSurface;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace ModernUwpDesigner
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = false, AllowsBackgroundLoading = true)]
    [Guid(ModernUwpDesignerPackage.PackageGuidString)]
    public sealed class ModernUwpDesignerPackage : AsyncPackage
    {
        /// <summary>
        /// ModernUwpDesignerPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "c87560ed-02e7-4c00-8dc6-b1ac79534ce4";

        private const int MinimumSupportedRuntimeVersion = 10;
        private const int MinimumSupportedSdkBuild = 26100;

        private static Hook _updateRuntimeArchitectureHook;
        private static Hook _incompatibleDesignerRuntimeArchitectureHook;
        private static Hook _getTargetPlatformFromProjectStorageHook;
        //private static Hook _isValidToolboxItemSourceHook;
        //private static Hook _createToolboxItemDataHook;
        //private static Hook _provideToolboxItemDiscoveryAttributeCctorHook;
        //private static Hook _uwpToolboxMultiTargetingProviderCctorHook;

        //private static unsafe delegate*<object, IDictionary<string, string>> GetResolutionMap;
        //private static unsafe delegate*<object, bool, string, string, IEnumerable<PlatformName>, object> ToolboxItemDataCctor;

        private static readonly PlatformSpecification ModernUwpSpecificationUap = new(PlatformNames.UAP, "10.0-..", ["Managed", "Native"], FrameworkNames.NetCoreApp, "10.0-..", null, XamlRuntimeNames.UAP, null);
        private static readonly PlatformSpecification ModernUwpSpecificationWindows = new(PlatformNames.Windows, "10.0-..", ["Managed", "Native"], FrameworkNames.NetCoreApp, "10.0-..", null, XamlRuntimeNames.UAP, null);

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            unsafe
            {
                var platformProps = new Dictionary<string, string>
                {
                    { "PlatformCreatorAssembly", "Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner" },
                    { "PlatformCreatorType", "Microsoft.VisualStudio.DesignTools.UwpSurfaceDesigner.UwpPlatformCreator" },
                    { "HostPlatformAssembly", typeof(UwpHostPlatform).Assembly.Location },
                    { "HostPlatformType", "Microsoft.VisualStudio.DesignTools.UwpDesignerHost.UwpHostPlatform" },
                    { "IsolationUnification", "true" },
                    { "ReferenceAssemblyMode", "None" },
                    { "DefaultTargetFramework", $"{FrameworkNames.NetCoreApp}, Version={MinimumSupportedRuntimeVersion}.0" },
                    { "UserControlTemplateName", "MyUserControl.xaml" },
                    { "PlatformSurfaceIsolatedGuid", "{D617FC9B-7AE9-4219-B022-359A3D13B875}" },
                    { "SupportsToolboxAutoPopulation", "true" },
                    { "SupportsExtensionSdks", "true" },
                    { "LegacyExtensionSdkPlatformsAndRequiredVCLibs", "Windows, 10.0;Microsoft.VCLibs.120, Version=14.0" },
                    { "ToolboxPage", "{8A63BDE2-AEB9-4AF9-A00D-DBC9BD7D509C}" },
                    { "DesignerTechnology", "Microsoft:Windows.UI.Xaml" },
                    { "ClipboardFormat", "CF_WINDOWSUIXAML_TOOL" },
                    { "AppPackageType", "WindowsXaml" }
                };

                var RegisterPlatformConfiguration = (delegate*<PlatformSpecification, IDictionary<string, string>, void>)typeof(PlatformConfigurationService).GetMethod("RegisterPlatformConfiguration", BindingFlags.NonPublic | BindingFlags.Static).MethodHandle.GetFunctionPointer();
                RegisterPlatformConfiguration(ModernUwpSpecificationUap, platformProps);
                RegisterPlatformConfiguration(ModernUwpSpecificationWindows, platformProps);
            }

            if (RuntimeInformation.ProcessArchitecture is Architecture.Arm64 &&
                _updateRuntimeArchitectureHook is null &&
                _incompatibleDesignerRuntimeArchitectureHook is null)
            {
                var property = typeof(UwpSceneView).GetProperty("IncompatibleDesignerRuntimeArchitecture", BindingFlags.NonPublic | BindingFlags.Instance);
                if (property is not null)
                {
                    _incompatibleDesignerRuntimeArchitectureHook = new Hook(property.GetMethod, IncompatibleDesignerRuntimeArchitectureHook);
                }

                var method = typeof(HostPlatformBase).GetMethod("UpdateRuntimeArchitecture", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method is not null)
                {
                    _updateRuntimeArchitectureHook = new Hook(method, UpdateRuntimeArchitectureHook);
                }
            }

            if (_getTargetPlatformFromProjectStorageHook is null)
            {
                var method = typeof(VSUtilities).GetMethod("GetTargetPlatformFromProjectStorage", BindingFlags.NonPublic | BindingFlags.Static);
                if (method is not null)
                {
                    _getTargetPlatformFromProjectStorageHook = new Hook(method, GetTargetPlatformFromProjectStorageHook);
                }
            }

            //var assembly = typeof(VSDocOutlineProvider).Assembly;

            /*if (_isValidToolboxItemSourceHook is null &&
                assembly.GetType("MS.Internal.Package.Toolbox.ToolboxItemSourceBase", false, false)
                is { } sourceBaseType)
            {
                var isValidToolboxItemSource = sourceBaseType.GetMethod("IsValidToolboxItemSource", BindingFlags.NonPublic | BindingFlags.Instance);
                if (isValidToolboxItemSource is not null)
                {
                    _isValidToolboxItemSourceHook = new Hook(isValidToolboxItemSource, IsValidToolboxItemSourceHook);
                }
            }*/

            /*if (_createToolboxItemDataHook is null &&
                assembly.GetType("MS.Internal.Package.Toolbox.ProvideStaticUwpToolboxItemsAttribute", false, false)
                is { } attrType)
            {
                var createToolboxItemData = attrType.GetMethod("CreateToolboxItemData", BindingFlags.NonPublic | BindingFlags.Static, null, [typeof(bool), typeof(string), typeof(IEnumerable<PlatformName>)], null);
                _createToolboxItemDataHook = new Hook(createToolboxItemData, CreateToolboxItemDataHook);
            }*/

            /*if (_provideToolboxItemDiscoveryAttributeCctorHook is null)
            {
                var constructor = typeof(ProvideToolboxItemDiscoveryAttribute).GetConstructor([typeof(string), typeof(string), typeof(Type), typeof(Type), typeof(string[])]);
                _provideToolboxItemDiscoveryAttributeCctorHook = new Hook(constructor, ProvideToolboxItemDiscoveryAttributeCctorHook);
            }*/

            /*if (_uwpToolboxMultiTargetingProviderCctorHook is null &&
                assembly.GetType("MS.Internal.Package.Toolbox.UwpToolboxMultiTargetingProvider", false, false)
                is { } providerType)
            {
                var IMarshaledServiceProviderType = assembly.GetType("MS.Internal.Package.Toolbox.IMarshaledServiceProvider", false, false);
                var constructor = providerType.GetConstructor([typeof(FrameworkName[]), IMarshaledServiceProviderType]);
                _uwpToolboxMultiTargetingProviderCctorHook = new Hook(constructor, UwpToolboxMultiTargetingProviderCctorHook);
            }*/
        }

        protected override void Dispose(bool disposing)
        {
            _incompatibleDesignerRuntimeArchitectureHook?.Dispose();
            _incompatibleDesignerRuntimeArchitectureHook = null;

            _updateRuntimeArchitectureHook?.Dispose();
            _updateRuntimeArchitectureHook = null;

            _getTargetPlatformFromProjectStorageHook?.Dispose();
            _getTargetPlatformFromProjectStorageHook = null;

            //_isValidToolboxItemSourceHook?.Dispose();
            //_isValidToolboxItemSourceHook = null;

            //_createToolboxItemDataHook?.Dispose();
            //_createToolboxItemDataHook = null;

            //_provideToolboxItemDiscoveryAttributeCctorHook?.Dispose();
            //_provideToolboxItemDiscoveryAttributeCctorHook = null;

            //_uwpToolboxMultiTargetingProviderCctorHook?.Dispose();
            //_uwpToolboxMultiTargetingProviderCctorHook = null;

            base.Dispose(disposing);
        }

        private delegate bool IncompatibleDesignerRuntimeArchitecture(UwpSceneView instance);

        private bool IncompatibleDesignerRuntimeArchitectureHook(IncompatibleDesignerRuntimeArchitecture original, UwpSceneView instance)
        {
            if (((SceneView)(object)instance).ProjectContext is ProjectContextBase context)
            {
                var hostProject = context.HostProject;
                var platformIdentifier = hostProject.PlatformIdentifier;

                if (hostProject.BuildPlatform.Equals("ARM64", StringComparison.OrdinalIgnoreCase) &&
                    platformIdentifier.TargetFrameworkIdentifier.Equals(FrameworkNames.NetCoreApp, StringComparison.Ordinal) &&
                    platformIdentifier.TargetFrameworkVersion.Major >= MinimumSupportedRuntimeVersion)
                {
                    return false;
                }
            }

            return original(instance);
        }

        private delegate void UpdateRuntimeArchitecture(HostPlatformBase instance, SurfaceProcessInfo surfaceProcessInfo, IHostProject hostProject);

        private void UpdateRuntimeArchitectureHook(UpdateRuntimeArchitecture original, HostPlatformBase instance, SurfaceProcessInfo surfaceProcessInfo, IHostProject hostProject)
        {
            if (string.Equals(surfaceProcessInfo.Architecture, "ARM64", StringComparison.OrdinalIgnoreCase))
            {
                string architecture = (surfaceProcessInfo.RuntimeArchitecture = "ARM64");
                surfaceProcessInfo.Architecture = architecture;
            }
            else
            {
                original(instance, surfaceProcessInfo, hostProject);
            }
        }

        private delegate PlatformName GetTargetPlatformFromProjectStorage(IVsBuildPropertyStorage projectStorage);

        private PlatformName GetTargetPlatformFromProjectStorageHook(GetTargetPlatformFromProjectStorage original, IVsBuildPropertyStorage projectStorage)
        {
            var og = original(projectStorage);
            if (og is not null &&
                og.Version.Build >= MinimumSupportedSdkBuild &&
                og.Identifier.Equals(PlatformNames.Windows, StringComparison.Ordinal) &&
                VSUtilities.GetProjectFilePropertyValue((IVsHierarchy)projectStorage, "DefaultXamlRuntime", _PersistStorageType.PST_PROJECT_FILE)
                .Equals(XamlRuntimeNames.UAP, StringComparison.Ordinal) &&
                GetTargetFramework((IVsHierarchy)projectStorage) is { } framework &&
                framework.Identifier.Equals(FrameworkNames.NetCoreApp, StringComparison.Ordinal) &&
                framework.Version.Major >= MinimumSupportedRuntimeVersion)
            {
                og = new(PlatformNames.UAP, og.Version, og.MinVersion);
            }

            return og;
        }

        private static FrameworkName GetTargetFramework(IVsHierarchy hierarchy)
        {
            hierarchy = hierarchy.GetEffectiveHierarchy();

            if (hierarchy.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID4.VSHPROPID_TargetFrameworkMoniker, out object obj)
                != 0)
            {
                return null;
            }

            string text = obj as string;
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            return new(text);
        }

        /*private delegate bool IsValidToolboxItemSource(object instance, object resolver);

        private unsafe bool IsValidToolboxItemSourceHook(IsValidToolboxItemSource original, object instance, object resolver)
        {
            if (GetResolutionMap is null)
            {
                GetResolutionMap = (delegate*<object, IDictionary<string, string>>)resolver.GetType().GetProperty("ResolutionMap", BindingFlags.Public | BindingFlags.Instance).GetMethod.MethodHandle.GetFunctionPointer();
            }

            if (GetResolutionMap is not null)
            {
                var resourceMap = GetResolutionMap(resolver);
                foreach (var key in resourceMap.Keys)
                {
                    switch (key[0])
                    {
                        case 'M':
                        case 'm':
                            if (key.StartsWith("Microsoft.Windows.UI.Xaml,", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }

                            break;
                    }
                }
            }

            return original(instance, resolver);
        }*/

        /*private delegate object CreateToolboxItemData(bool isCommon, string typeName, IEnumerable<PlatformName> targetPlatforms);

        private unsafe object CreateToolboxItemDataHook(CreateToolboxItemData original, bool isCommon, string typeName, IEnumerable<PlatformName> targetPlatforms)
        {
            var assembly = typeof(VSDocOutlineProvider).Assembly;
            var type = assembly.GetType("MS.Internal.Package.Toolbox.ToolboxItemData", false, false);

            if (ToolboxItemDataCctor is null && type is not null)
            {
                var method = type.GetConstructor([typeof(bool), typeof(string), typeof(string), typeof(IEnumerable<PlatformName>)]);
                if (method is not null)
                {
                    ToolboxItemDataCctor = (delegate*<object, bool, string, string, IEnumerable<PlatformName>, object>)method.MethodHandle.GetFunctionPointer();
                }
            }

            if (ToolboxItemDataCctor is not null && type is not null)
            {
                string text = Assembly.CreateQualifiedName("Microsoft.Windows.UI.Xaml, Version=10.0.26100.52, Culture=neutral, PublicKeyToken=31bf3856ad364e35", typeName);

                var instance = FormatterServices.GetUninitializedObject(type);
                ToolboxItemDataCctor(instance, isCommon, text, null, targetPlatforms);
                return instance;
            }

            return original(isCommon, typeName, targetPlatforms);
        }*/

        /*private delegate void ProvideToolboxItemDiscoveryAttributeCctor(ProvideToolboxItemDiscoveryAttribute instance, string name, string helpKeyword, Type discoveryType, Type itemCreatorType, string[] frameworksToEnumerate);

        private void ProvideToolboxItemDiscoveryAttributeCctorHook(ProvideToolboxItemDiscoveryAttributeCctor original, ProvideToolboxItemDiscoveryAttribute instance, string name, string helpKeyword, Type discoveryType, Type itemCreatorType, string[] frameworksToEnumerate)
        {
            if (helpKeyword == "UniversalWindowsComponents")
            {
                original(instance, name, helpKeyword, discoveryType, itemCreatorType, [".NETCoreApp", .. frameworksToEnumerate]);
                return;
            }

            original(instance, name, helpKeyword, discoveryType, itemCreatorType, frameworksToEnumerate);
        }*/

        /*private delegate void UwpToolboxMultiTargetingProviderCctor(object instance, FrameworkName[] currentTargetFrameworks, object serviceProvider);

        private void UwpToolboxMultiTargetingProviderCctorHook(UwpToolboxMultiTargetingProviderCctor original, object instance, FrameworkName[] currentTargetFrameworks, object serviceProvider)
        {
            original(instance, [new(".NETCoreApp", new(10, 0)), ..currentTargetFrameworks], serviceProvider);
        }*/
    }
}

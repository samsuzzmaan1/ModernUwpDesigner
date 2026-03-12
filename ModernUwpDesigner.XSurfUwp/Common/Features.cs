using Windows.Foundation.Metadata;

namespace XSurfUwp.Common
{
    internal static class Features
    {
        public static readonly bool IsOnWindows11OrHigher = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 14);
    }
}

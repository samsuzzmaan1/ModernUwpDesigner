using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace XSurfUwp.Composition
{
    [GeneratedComInterface]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("7cc8cb07-5a0d-46bb-8c54-9beafe63b476")]
    internal unsafe partial interface IUIElementStaticsPrivate
    {
        // IInspectable methods
        void GetIids();
        void GetRuntimeClassName();
        void GetTrustLevel();

        // IUIElementStaticsPrivate methods
        void add_PopupOpening();
        void remove_PopupOpening();
        void add_PopupPlacement();
        void remove_PopupPlacement();
        void InternalGetIsEnabled();
        void InternalPutIsEnabled();
        void GetRasterizationScale();
        void PutRasterizationScale();
        void PutPopupRootLightDismissBounds();
        void EnablePopupZIndexSorting();
        nint GetElementLayerVisual(nint pElement);
    }
}

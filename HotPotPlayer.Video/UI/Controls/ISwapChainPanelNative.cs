using DirectN;
using System;
using System.Runtime.InteropServices;

namespace HotPotPlayer.Video.UI.Controls
{

    [ComImport, Guid("63aad0b8-7c24-40ff-85a8-640d944cc325"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface ISwapChainPanelNative
    {
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        HRESULT SetSwapChain(IDXGISwapChain1 swapChain);
    }
}

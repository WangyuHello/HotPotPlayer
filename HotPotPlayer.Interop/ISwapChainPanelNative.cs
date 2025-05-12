using System.Runtime.InteropServices;

namespace HotPotPlayer.Interop
{

    [ComImport, Guid("63aad0b8-7c24-40ff-85a8-640d944cc325"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface ISwapChainPanelNative
    {
        [PreserveSig]
        int SetSwapChain(IDXGISwapChain1 swapChain);
    }
}

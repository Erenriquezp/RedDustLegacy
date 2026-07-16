#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.Runtime.InteropServices;
#endif
using UnityEngine;

/// <summary>
/// Suprime los hotkeys de accesibilidad de Windows (Sticky Keys, Filter Keys,
/// Toggle Keys) mientras el juego esta en foco. Los restaura exactamente al
/// valor previo al salir o perder el foco.
///
/// Problema raiz: presionar Shift repetidamente dispara el dialogo de
/// "Teclas especiales" de Windows (5 x Shift = StickyKeys). Esto interrumpe
/// partidas donde Shift es la tecla de Dash.
///
/// Solucion: en Awake, leer los flags actuales via SystemParametersInfo,
/// borrar el bit SKF_HOTKEYACTIVE (0x4) y escribir de vuelta. Al cerrar o
/// perder foco se restauran los flags originales.
///
/// No requiere privilegios de administrador. Solo afecta a la sesion del
/// usuario que ejecuta el juego.
/// </summary>
public class WindowsInputGuard : MonoBehaviour
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    // ── Win32 API ─────────────────────────────────────────────────────────
    private const uint SPI_GETSTICKYKEYS  = 0x003A;
    private const uint SPI_SETSTICKYKEYS  = 0x003B;
    private const uint SPI_GETFILTERKEYS  = 0x0032;
    private const uint SPI_SETFILTERKEYS  = 0x0033;
    private const uint SPI_GETTOGGLEKEYS  = 0x0034;
    private const uint SPI_SETTOGGLEKEYS  = 0x0035;
    private const uint SPIF_SENDCHANGE    = 0x0002;

    // Bit que habilita el hotkey de cada feature (5 x Shift / 8 s Shift / 5 s NumLock)
    private const uint SKF_HOTKEYACTIVE  = 0x00000004;  // StickyKeys
    private const uint FKF_HOTKEYACTIVE  = 0x00000004;  // FilterKeys
    private const uint TKF_HOTKEYACTIVE  = 0x00000004;  // ToggleKeys

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct STICKYKEYS { public uint cbSize; public uint dwFlags; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct FILTERKEYS
    {
        public uint cbSize; public uint dwFlags;
        public uint iWaitMSec; public uint iDelayMSec;
        public uint iRepeatMSec; public uint iBounceMSec;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct TOGGLEKEYS { public uint cbSize; public uint dwFlags; }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(
        uint uiAction, uint uiParam, ref STICKYKEYS pvParam, uint fWinIni);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(
        uint uiAction, uint uiParam, ref FILTERKEYS pvParam, uint fWinIni);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(
        uint uiAction, uint uiParam, ref TOGGLEKEYS pvParam, uint fWinIni);

    // ── Estado guardado ───────────────────────────────────────────────────
    private STICKYKEYS _savedSticky;
    private FILTERKEYS _savedFilter;
    private TOGGLEKEYS _savedToggle;
    private bool _suppressed;

#endif

    // ── Bootstrap ─────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject("[WindowsInputGuard]");
        go.AddComponent<WindowsInputGuard>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        SuppressHotkeys();
    }

    // Cuando la ventana pierde foco (Alt+Tab, etc.) restauramos para que
    // el usuario pueda usar Sticky Keys normalmente fuera del juego.
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) SuppressHotkeys();
        else          RestoreHotkeys();
    }

    private void OnApplicationQuit()
    {
        RestoreHotkeys();
    }

    // ── Logica de supresion ───────────────────────────────────────────────
    private void SuppressHotkeys()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (_suppressed) return;

        // -- StickyKeys --
        _savedSticky = new STICKYKEYS { cbSize = (uint)Marshal.SizeOf<STICKYKEYS>() };
        SystemParametersInfo(SPI_GETSTICKYKEYS, _savedSticky.cbSize,
                             ref _savedSticky, 0);

        if ((_savedSticky.dwFlags & SKF_HOTKEYACTIVE) != 0)
        {
            var patch = _savedSticky;
            patch.dwFlags &= ~SKF_HOTKEYACTIVE;
            SystemParametersInfo(SPI_SETSTICKYKEYS, patch.cbSize,
                                 ref patch, SPIF_SENDCHANGE);
        }

        // -- FilterKeys (8 segundos de Shift) --
        _savedFilter = new FILTERKEYS { cbSize = (uint)Marshal.SizeOf<FILTERKEYS>() };
        SystemParametersInfo(SPI_GETFILTERKEYS, _savedFilter.cbSize,
                             ref _savedFilter, 0);

        if ((_savedFilter.dwFlags & FKF_HOTKEYACTIVE) != 0)
        {
            var patch = _savedFilter;
            patch.dwFlags &= ~FKF_HOTKEYACTIVE;
            SystemParametersInfo(SPI_SETFILTERKEYS, patch.cbSize,
                                 ref patch, SPIF_SENDCHANGE);
        }

        // -- ToggleKeys (5 segundos de NumLock) --
        _savedToggle = new TOGGLEKEYS { cbSize = (uint)Marshal.SizeOf<TOGGLEKEYS>() };
        SystemParametersInfo(SPI_GETTOGGLEKEYS, _savedToggle.cbSize,
                             ref _savedToggle, 0);

        if ((_savedToggle.dwFlags & TKF_HOTKEYACTIVE) != 0)
        {
            var patch = _savedToggle;
            patch.dwFlags &= ~TKF_HOTKEYACTIVE;
            SystemParametersInfo(SPI_SETTOGGLEKEYS, patch.cbSize,
                                 ref patch, SPIF_SENDCHANGE);
        }

        _suppressed = true;
        Debug.Log("[WindowsInputGuard] Hotkeys de accesibilidad suprimidos.");
#endif
    }

    private void RestoreHotkeys()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (!_suppressed) return;

        SystemParametersInfo(SPI_SETSTICKYKEYS, _savedSticky.cbSize,
                             ref _savedSticky, SPIF_SENDCHANGE);
        SystemParametersInfo(SPI_SETFILTERKEYS, _savedFilter.cbSize,
                             ref _savedFilter, SPIF_SENDCHANGE);
        SystemParametersInfo(SPI_SETTOGGLEKEYS, _savedToggle.cbSize,
                             ref _savedToggle, SPIF_SENDCHANGE);

        _suppressed = false;
        Debug.Log("[WindowsInputGuard] Hotkeys de accesibilidad restaurados.");
#endif
    }
}

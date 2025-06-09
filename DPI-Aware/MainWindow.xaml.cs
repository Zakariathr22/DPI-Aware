using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace DPI_Aware;

/// <summary>
/// Represents the main window of the application.
/// Handles DPI-aware sizing and enforces min/max dimensions on DPI change.
/// </summary>
public sealed partial class MainWindow : Window
{
    private IntPtr _hWnd;
    private nint _oldWndProc = IntPtr.Zero;
    private WndProcDelegate? _newWndProc;

    // Base dimensions in device-independent pixels (DIPs)
    private const int baseMinWidthDip = 500;
    private const int baseMinHeightDip = 500;
    private const int baseMaxWidthDip = 600;
    private const int baseMaxHeightDip = 600;

    /// <summary>
    /// Initializes the main window and applies DPI-aware window sizing and constraints.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

        _hWnd = WindowNative.GetWindowHandle(this);

        SetupWindow();

        SizeChanged += (_, _) => UpdateSize(AppWindow.Size);

        HookWndProc();
    }

    /// <summary>
    /// Computes window min/max sizes based on current DPI and applies them.
    /// </summary>
    private void SetupWindow()
    {
        uint dpi = GetDpiForWindow(_hWnd);
        double scaleFactor = dpi / 96.0;

        int minWidthPx = (int)Math.Round(baseMinWidthDip * scaleFactor);
        int minHeightPx = (int)Math.Round(baseMinHeightDip * scaleFactor);
        int maxWidthPx = (int)Math.Round(baseMaxWidthDip * scaleFactor);
        int maxHeightPx = (int)Math.Round(baseMaxHeightDip * scaleFactor);

        var presenter = OverlappedPresenter.Create();
        presenter.PreferredMinimumWidth = minWidthPx;
        presenter.PreferredMinimumHeight = minHeightPx;
        presenter.PreferredMaximumWidth = maxWidthPx;
        presenter.PreferredMaximumHeight = maxHeightPx;
        presenter.IsMaximizable = false;

        AppWindow.SetPresenter(presenter);

        EnforceWindowBounds(minWidthPx, minHeightPx, maxWidthPx, maxHeightPx);
        UpdateSize(AppWindow.Size);
    }

    /// <summary>
    /// Ensures the current window size does not violate the given min/max constraints.
    /// </summary>
    /// <param name="minWidth">Minimum allowed width in physical pixels.</param>
    /// <param name="minHeight">Minimum allowed height in physical pixels.</param>
    /// <param name="maxWidth">Maximum allowed width in physical pixels.</param>
    /// <param name="maxHeight">Maximum allowed height in physical pixels.</param>
    private void EnforceWindowBounds(int minWidth, int minHeight, int maxWidth, int maxHeight)
    {
        var size = AppWindow.Size;

        int newWidth = size.Width;
        int newHeight = size.Height;

        if (newWidth < minWidth)
            newWidth = minWidth;
        else if (newWidth > maxWidth)
            newWidth = maxWidth;

        if (newHeight < minHeight)
            newHeight = minHeight;
        else if (newHeight > maxHeight)
            newHeight = maxHeight;

        if (newWidth != size.Width || newHeight != size.Height)
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(newWidth, newHeight));
        }
    }

    /// <summary>
    /// Hooks into the window procedure to monitor DPI change events.
    /// </summary>
    private void HookWndProc()
    {
        _newWndProc = new WndProcDelegate(CustomWndProc);
        if (IntPtr.Size == 8) // Check if the system is 64-bit
        {
            _oldWndProc = SetWindowLongPtr(_hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
        }
        else
        {
            _oldWndProc = SetWindowLong(_hWnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
        }
    }

    /// <summary>
    /// Custom window procedure to intercept DPI change messages.
    /// </summary>
    private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        const int WM_DPICHANGED = 0x02E0;

        if (msg == WM_DPICHANGED)
        {
            SetupWindow(); // Re-apply size limits for new DPI
        }

        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    /// <summary>
    /// Delegate matching the native WNDPROC signature.
    /// </summary>
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    // Constants
    private const int GWLP_WNDPROC = -4;
    private const int GWL_WNDPROC = -4;

    // PInvoke declarations

    /// <summary>
    /// Gets the DPI for a specified window handle.
    /// </summary>
    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    /// <summary>
    /// Subclasses a window by setting a new window procedure.
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowLong(nint hWnd, int nIndex, nint dwNewLong);

    /// <summary>
    /// Calls the original window procedure.
    /// </summary>
    [DllImport("user32.dll")]
    private static extern nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint Msg, nint wParam, nint lParam);
}

/// <summary>
/// Represents the main window of the application, providing functionality to manage its dimensions and notify changes
/// to bound properties.
/// </summary>
public partial class MainWindow : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _windowWidth;

    /// <summary>
    /// Gets or sets the current width of the window in physical pixels.
    /// </summary>
    public int WindowWidth
    {
        get => _windowWidth;
        set
        {
            if (_windowWidth != value)
            {
                _windowWidth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowWidth)));
            }
        }
    }

    private int _windowHeight;

    /// <summary>
    /// Gets or sets the current height of the window in physical pixels.
    /// </summary>
    public int WindowHeight
    {
        get => _windowHeight;
        set
        {
            if (_windowHeight != value)
            {
                _windowHeight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowHeight)));
            }
        }
    }

    /// <summary>
    /// Updates the exposed WindowWidth and WindowHeight properties.
    /// </summary>
    /// <param name="size">New size of the window.</param>
    private void UpdateSize(Windows.Graphics.SizeInt32 size)
    {
        WindowWidth = size.Width;
        WindowHeight = size.Height;
    }
}

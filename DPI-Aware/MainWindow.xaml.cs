using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace DPI_Aware;

public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _windowWidth;
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

    public MainWindow()
    {
        InitializeComponent();

        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        uint dpi = GetDpiForWindow(hWnd);
        double scaleFactor = dpi / 96.0;

        int minWidthPx = (int)Math.Round(500 * scaleFactor);
        int minHeightPx = (int)Math.Round(500 * scaleFactor);
        int maxWidthPx = (int)Math.Round(1000 * scaleFactor);
        int maxHeightPx = (int)Math.Round(1000 * scaleFactor);

        AppWindow.SetIcon("Assets/Tiles/GalleryIcon.ico");
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

        OverlappedPresenter presenter = OverlappedPresenter.Create();
        presenter.PreferredMinimumWidth = minWidthPx;
        presenter.PreferredMinimumHeight = minHeightPx;
        presenter.PreferredMaximumWidth = maxWidthPx;
        presenter.PreferredMaximumHeight = maxHeightPx;
        presenter.IsMaximizable = false;

        AppWindow.SetPresenter(presenter);

        UpdateSize(AppWindow.Size);

        SizeChanged += (s, e) =>
        {
            UpdateSize(AppWindow.Size);
        };
    }

    private void UpdateSize(Windows.Graphics.SizeInt32 size)
    {
        WindowWidth = size.Width;
        WindowHeight = size.Height;
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);
}

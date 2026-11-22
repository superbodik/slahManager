using System.ComponentModel;
using System.Windows;
using SlahManager.Localization;
using SlahManager.Services.Settings;

namespace SlahManager.Views.Tray;

public sealed class TrayController : IDisposable
{
    private readonly Window _window;
    private readonly SettingsService _settings;
    private readonly TrayIcon _icon;
    private bool _exiting;

    public TrayController(Window window, SettingsService settings)
    {
        _window = window;
        _settings = settings;

        _icon = new TrayIcon(
            "slahManager",
            () => Localizer.Instance.Get("tray.open"),
            () => Localizer.Instance.Get("tray.exit"));

        _icon.OpenRequested += (_, _) => Restore();
        _icon.ExitRequested += (_, _) => Exit();

        _window.Closing += OnClosing;
        Application.Current.SessionEnding += (_, _) => _exiting = true;
    }

    public void ShowHidden()
    {
        _window.ShowInTaskbar = false;
        _window.ShowActivated = false;
        _window.WindowState = WindowState.Minimized;
        _window.Show();
        _window.Hide();
        _window.ShowActivated = true;
    }

    public void Restore()
    {
        _window.ShowInTaskbar = true;
        _window.Show();
        _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    public void Dispose() => _icon.Dispose();

    private void Exit()
    {
        _exiting = true;
        Application.Current.Shutdown();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_exiting || !_settings.RunInTray || !_settings.GameBoostEnabled)
        {
            return;
        }

        e.Cancel = true;
        _window.Hide();
    }
}

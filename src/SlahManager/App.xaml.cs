using System.Windows;
using System.Windows.Threading;
using SlahManager.Infrastructure;
using SlahManager.Services;
using SlahManager.Services.Platform;
using SlahManager.ViewModels;
using SlahManager.Views;
using SlahManager.Views.Tray;

namespace SlahManager;

public partial class App : Application
{
    private SingleInstanceGuard? _instance;
    private AppServices? _services;
    private TrayController? _tray;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnUnhandledException;

        _instance = SingleInstanceGuard.Acquire(AppPaths.InstanceKey);
        if (!_instance.IsFirst)
        {
            _instance.SignalExisting();
            Shutdown();
            return;
        }

        _services = AppServices.Create();
        _services.Settings.ApplyLanguage();

        var window = new MainWindow { DataContext = new MainViewModel(_services) };
        _tray = new TrayController(window, _services.Settings);
        _instance.ListenForSignal(() => Dispatcher.BeginInvoke(() => _tray.Restore()));
        _services.Boost.Start();

        if (e.Args.Contains(StartupService.TrayArgument, StringComparer.OrdinalIgnoreCase))
        {
            _tray.ShowHidden();
        }
        else
        {
            window.Show();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Boost.Dispose();
        _tray?.Dispose();
        _instance?.Dispose();
        base.OnExit(e);
    }

    private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.Error("Unhandled UI exception", e.Exception);
        MessageBox.Show(e.Exception.Message, "slahManager", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}

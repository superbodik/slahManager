using System.Drawing;
using System.Windows.Interop;
using SlahManager.Infrastructure;
using SlahManager.Infrastructure.Native;

namespace SlahManager.Views.Tray;

public sealed class TrayIcon : IDisposable
{
    public const string HostWindowName = "slahManager.TrayHost";
    public const uint CallbackMessage = 0x8001;

    private const uint IconId = 1;
    private const uint LeftButtonUp = 0x0202;
    private const uint RightButtonUp = 0x0205;
    private const uint OpenCommand = 1;
    private const uint ExitCommand = 2;
    private const int MessageOnlyParent = -3;

    private readonly HwndSource _source;
    private readonly Icon _icon;
    private readonly string _tooltip;
    private readonly Func<string> _openText;
    private readonly Func<string> _exitText;
    private readonly uint _taskbarCreated;

    public TrayIcon(string tooltip, Func<string> openText, Func<string> exitText)
    {
        _tooltip = tooltip;
        _openText = openText;
        _exitText = exitText;
        _icon = LoadIcon();
        _taskbarCreated = User32.RegisterWindowMessageW("TaskbarCreated");

        _source = new HwndSource(new HwndSourceParameters(HostWindowName) { ParentWindow = new IntPtr(MessageOnlyParent) });
        _source.AddHook(OnMessage);

        Add();
    }

    public event EventHandler? OpenRequested;

    public event EventHandler? ExitRequested;

    public void Dispose()
    {
        var data = CreateData();
        Shell32.Shell_NotifyIconW(Shell32.NotifyDelete, ref data);
        _source.Dispose();
        _icon.Dispose();
    }

    private static Icon LoadIcon()
    {
        var path = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(path) && Icon.ExtractAssociatedIcon(path) is { } icon)
        {
            return icon;
        }

        return (Icon)SystemIcons.Application.Clone();
    }

    private NotifyIconData CreateData() => new()
    {
        Size = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NotifyIconData>(),
        WindowHandle = _source.Handle,
        Id = IconId,
        Flags = Shell32.FlagMessage | Shell32.FlagIcon | Shell32.FlagTip,
        CallbackMessage = CallbackMessage,
        Icon = _icon.Handle,
        Tip = _tooltip,
        Info = string.Empty,
        InfoTitle = string.Empty
    };

    private void Add()
    {
        var data = CreateData();
        if (!Shell32.Shell_NotifyIconW(Shell32.NotifyAdd, ref data))
        {
            Logger.Error("Tray icon could not be added");
        }
    }

    private IntPtr OnMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == CallbackMessage)
        {
            var mouseMessage = (uint)(lParam.ToInt64() & 0xFFFF);

            if (mouseMessage == LeftButtonUp)
            {
                OpenRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (mouseMessage == RightButtonUp)
            {
                ShowMenu();
            }

            handled = true;
        }
        else if (_taskbarCreated != 0 && (uint)message == _taskbarCreated)
        {
            Add();
        }

        return IntPtr.Zero;
    }

    private void ShowMenu()
    {
        var menu = User32.CreatePopupMenu();
        User32.AppendMenuW(menu, User32.MenuString, new UIntPtr(OpenCommand), _openText());
        User32.AppendMenuW(menu, User32.MenuString, new UIntPtr(ExitCommand), _exitText());

        User32.GetCursorPos(out var point);
        User32.SetForegroundWindow(_source.Handle);

        var command = User32.TrackPopupMenuEx(
            menu,
            User32.TrackReturnCommand | User32.TrackRightButton,
            point.X,
            point.Y,
            _source.Handle,
            IntPtr.Zero);

        User32.PostMessageW(_source.Handle, 0, IntPtr.Zero, IntPtr.Zero);
        User32.DestroyMenu(menu);

        if (command == OpenCommand)
        {
            OpenRequested?.Invoke(this, EventArgs.Empty);
        }
        else if (command == ExitCommand)
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

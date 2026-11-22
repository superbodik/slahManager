using System.Runtime.InteropServices;
using System.Text;

namespace SlahManager.Infrastructure.Native;

public sealed class ProcessHandle : IDisposable
{
    private const int MaxPath = 1024;

    private readonly IntPtr _handle;

    private ProcessHandle(IntPtr handle)
    {
        _handle = handle;
    }

    public static ProcessHandle? OpenForQuery(int processId) =>
        Open(processId, Kernel32.ProcessQueryLimitedInformation);

    public static ProcessHandle? OpenForModify(int processId) =>
        Open(processId, Kernel32.ProcessSetInformation | Kernel32.ProcessQueryLimitedInformation);

    public string? ImagePath()
    {
        var builder = new StringBuilder(MaxPath);
        var size = (uint)builder.Capacity;
        return Kernel32.QueryFullProcessImageNameW(_handle, 0, builder, ref size) ? builder.ToString() : null;
    }

    public bool DisablePowerThrottling()
    {
        var full = ProcessPowerThrottlingState.ExecutionSpeed | ProcessPowerThrottlingState.IgnoreTimerResolution;
        return TrySetThrottling(full) || TrySetThrottling(ProcessPowerThrottlingState.ExecutionSpeed);
    }

    public void Dispose() => Kernel32.CloseHandle(_handle);

    private static ProcessHandle? Open(int processId, uint access)
    {
        var handle = Kernel32.OpenProcess(access, false, processId);
        return handle == IntPtr.Zero ? null : new ProcessHandle(handle);
    }

    private bool TrySetThrottling(uint controlMask)
    {
        var state = new ProcessPowerThrottlingState
        {
            Version = ProcessPowerThrottlingState.CurrentVersion,
            ControlMask = controlMask,
            StateMask = 0
        };

        return Kernel32.SetProcessInformation(
            _handle,
            Kernel32.ProcessPowerThrottlingClass,
            ref state,
            (uint)Marshal.SizeOf<ProcessPowerThrottlingState>());
    }
}

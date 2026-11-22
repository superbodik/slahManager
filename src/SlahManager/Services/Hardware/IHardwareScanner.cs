using SlahManager.Models.Hardware;

namespace SlahManager.Services.Hardware;

public interface IHardwareScanner
{
    HardwareProfile Scan();
}

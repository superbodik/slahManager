using SlahManager.Services.Settings;

namespace SlahManager.Services.Platform;

public sealed class RestorePointGuard
{
    private readonly RestorePointService _service;
    private readonly SettingsService _settings;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _attempted;

    public RestorePointGuard(RestorePointService service, SettingsService settings)
    {
        _service = service;
        _settings = settings;
    }

    public bool IsPending => !_attempted && _settings.CreateRestorePoint && ElevationService.IsElevated;

    public async Task EnsureAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (!IsPending)
            {
                return;
            }

            _attempted = true;
            await _service.CreateAsync();
        }
        finally
        {
            _gate.Release();
        }
    }
}

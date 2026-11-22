using SlahManager.Infrastructure;
using SlahManager.Services.Hardware;

namespace SlahManager.Services.Platform;

public sealed class ActivationService
{
    private const string WindowsApplicationId = "55c92734-d682-4d71-983e-d6ec3f16059f";
    private const int LicensedStatus = 1;

    public Task<ActivationState> GetStateAsync() => Task.Run(Query);

    private static ActivationState Query()
    {
        try
        {
            var products = WmiClient.Query(
                "SELECT LicenseStatus FROM SoftwareLicensingProduct " +
                $"WHERE PartialProductKey IS NOT NULL AND ApplicationId='{WindowsApplicationId}'");

            if (products.Count == 0)
            {
                return ActivationState.NotActivated;
            }

            return products.Any(product => WmiClient.Number(product, "LicenseStatus") == LicensedStatus)
                ? ActivationState.Activated
                : ActivationState.NotActivated;
        }
        catch (Exception exception)
        {
            Logger.Error("Activation status check failed", exception);
            return ActivationState.Unknown;
        }
    }
}

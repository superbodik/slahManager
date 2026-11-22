namespace SlahManager.Models.Tweaks.Operations;

public sealed class ServiceOperation : OperationDefinition
{
    public string Name { get; init; } = string.Empty;

    public ServiceStartup Startup { get; init; } = ServiceStartup.Disabled;

    public ServiceStartup? DefaultStartup { get; init; }
}

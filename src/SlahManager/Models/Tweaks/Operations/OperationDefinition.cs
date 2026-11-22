using System.Text.Json.Serialization;

namespace SlahManager.Models.Tweaks.Operations;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(RegistryOperation), "registry")]
[JsonDerivedType(typeof(RegistryFlagOperation), "registryFlag")]
[JsonDerivedType(typeof(ServiceOperation), "service")]
[JsonDerivedType(typeof(ScheduledTaskOperation), "scheduledTask")]
[JsonDerivedType(typeof(PowerPlanOperation), "powerPlan")]
[JsonDerivedType(typeof(PowerSettingOperation), "powerSetting")]
[JsonDerivedType(typeof(AppxOperation), "appx")]
[JsonDerivedType(typeof(ScriptOperation), "script")]
public abstract class OperationDefinition;

# slahManager project map

```
slahManager/
├── SlahManager.slnx
├── PROJECT_RULES.md
├── PROJECT_MAP.md
├── README.md                  public description and download instructions
├── .gitignore
├── dist/                      published single file executable (generated)
├── release/                   build attached to GitHub releases (generated)
└── src/SlahManager/
    ├── SlahManager.csproj
    ├── App.xaml / App.xaml.cs
    ├── Assets/
    ├── Properties/
    ├── Data/
    ├── Infrastructure/
    ├── Localization/
    ├── Models/
    ├── Services/
    ├── ViewModels/
    ├── Views/
    └── Themes/
```

## Entry point

| File | Purpose |
| --- | --- |
| `App.xaml` | Merges theme dictionaries and maps each page view model to its view. |
| `App.xaml.cs` | Single instance check, builds services, applies the saved language, starts Game Boost, creates the tray, supports the `--tray` start mode, stops everything on exit. |
| `SlahManager.csproj` | Project definition. Embeds `Data/**/*.json`. Picks the manifest by configuration. |

## Assets

| File | Purpose |
| --- | --- |
| `app.ico` | Application icon, seven sizes. Used by the executable and the tray. |

## Properties

| File | Purpose |
| --- | --- |
| `admin.manifest` | Release builds. Requires administrator rights and enables per monitor DPI. |
| `standard.manifest` | Debug builds. Runs as the invoking user. |
| `PublishProfiles/SingleExe.pubxml` | Self contained, compressed, single file win-x64 publish into `dist/`. |

## Data

Embedded in the assembly. Edit the files and rebuild.

| Path | Purpose |
| --- | --- |
| `Tweaks/gaming.json` | Game Mode, game capture, multimedia scheduler, responsiveness, network throttling, pointer, foreground boost, GPU scheduling, power plan. |
| `Tweaks/input.json` | Global timer requests, accessibility popups, windowed game optimizations, USB suspend, PCIe link state, core parking, CPU full speed. |
| `Tweaks/network.json` | Wi-Fi power saving, adapter sleep, low latency Ethernet, Nagle, update sharing. |
| `Tweaks/performance.json` | Startup delay, menu delay, SysMain, background apps, Fast Startup, transparency, window animations. |
| `Tweaks/privacy.json` | Diagnostic data, telemetry services and tasks, advertising ID, suggestions, web search, activity history, Recall, widgets. |
| `Tweaks/debloat.json` | Removal of preinstalled apps. All entries are permanent. |
| `Strings/en.json` | English interface text. Fallback language. |
| `Strings/ru.json` | Russian interface text and tweak translations. |

## Infrastructure

Cross cutting helpers with no business logic.

| File | Purpose |
| --- | --- |
| `AppPaths.cs` | Data folder and file locations. The `SLAHMANAGER_DATA` variable moves everything to another folder. Derives the instance key. |
| `EmbeddedData.cs`, `EmbeddedFile.cs` | Reads the embedded JSON files. |
| `JsonSettings.cs` | Shared JSON options: camelCase, string enums, out of order polymorphic metadata. |
| `Logger.cs` | Append only file log under the data folder. |
| `SingleInstanceGuard.cs` | Named mutex and show signal so a second launch reopens the first window. |
| `Processes/` | `ProcessRunner`, `ProcessResult`, `PowerShellRunner`, `PowerShellText`: external programs and generated scripts. |
| `Native/` | Raw Windows API imports: `Kernel32`, `Ntdll`, `PowrProf`, `Shell32`, `User32`, their structures, `ProcessHandle` for process queries and power throttling, `NvApi` (NVIDIA digital vibrance) and `AdlApi` (AMD saturation), both loaded at run time only when the driver is present. |

## Localization

| File | Purpose |
| --- | --- |
| `AppLanguage.cs`, `AppLanguageExtensions.cs` | Supported languages and their file codes. |
| `StringTable.cs` | Loads a string table from the embedded data. |
| `Localizer.cs` | Current language, key lookup with English fallback, change notifications. |
| `LocExtension.cs` | `{loc:Loc key}` markup extension that updates live when the language changes. |

## Models

Data only.

| Folder | Contents |
| --- | --- |
| `Hardware/` | `HardwareProfile` and its parts: `CpuInfo`, `GpuInfo`, `MemoryInfo`, `StorageInfo`, `OsInfo`, plus `DeviceKind`, `GpuVendor`, `StorageKind`. |
| `Profiles/` | `SystemProfile`: detected usage profile with localization keys. |
| `Settings/` | `AppSettings`: language, restore point, tray, Game Boost options and custom games. |
| `Backup/` | `TweakBackup` and `OperationSnapshot`: original state saved before a change. |
| `Boost/` | `BoostOptions`, `BoostState`, `GameProcess`, `PowerSettingTarget`, `BoostJournal`, `BoostJournalEntry`, `VibranceJournalEntry`. |
| `Display/` | `DisplayVendor`, `VibranceTarget` (raw driver value and percent mapping). |
| `Network/` | `ConnectionKind`, `PingStats`, `PingReport`. |
| `Tweaks/` | `TweakDefinition`, `TweakRequirements`, `TweakCategory`, `RiskLevel`, `Availability`, `TweakResult`, `CategoryDescriptor(s)`. |
| `Tweaks/Operations/` | Polymorphic operations: `RegistryOperation`, `RegistryFlagOperation`, `ServiceOperation`, `ScheduledTaskOperation`, `PowerPlanOperation`, `PowerSettingOperation`, `AppxOperation`, `ScriptOperation` (PowerShell check, capture, apply and restore with saved state), with their enums. |

## Services

Logic with no UI dependencies.

| Folder | Contents |
| --- | --- |
| `AppServices.cs` | Composition root that wires every service. |
| `Hardware/` | `HardwareScanner` composes `CpuDetector`, `GpuDetector`, `MemoryDetector`, `StorageDetector`, `DeviceKindDetector`, `OsDetector`. `GpuClassifier` decides vendor and discrete or integrated. `WmiClient` wraps WMI queries. |
| `Tweaks/` | `TweakCatalog` loads definitions. `TweakEngine` applies, checks and reverts with snapshots and rollback. `BackupStore` persists snapshots. `RequirementEvaluator` decides availability for the detected hardware. |
| `Tweaks/Executors/` | One executor per operation kind plus `OperationExecutorRegistry`. Registry helpers: `RegistryAccess`, `RegistryValueConverter`, `RegistrySnapshotCodec`, `RegistryFlagString`. |
| `Boost/` | `GameBoostService` ticks every two seconds. `IGameDetector` and `GameDetector` find games, `GameLocations` holds library paths and name rules. `TimerBooster` holds the 0.5 ms timer, `ProcessBooster` raises priority and disables power throttling, `PowerBooster` applies power settings with a journal in `BoostJournalStore`. `BoostPowerSettings` lists the targets. `TimerPolicy` checks the system wide timer requirement. `WifiScanBooster` pauses Wi-Fi background scanning with `WifiJournalStore`. `VibranceBooster` raises digital vibrance while CS2 runs with `VibranceJournalStore`. |
| `Display/` | `VibranceController` reads and writes vibrance on every NVIDIA and AMD screen. |
| `Network/` | `PingTester` measures router and internet latency, jitter and loss on the interface that actually carries traffic. |
| `Platform/` | `ElevationService`, `RestorePointService`, `RestorePointGuard`, `ActivationService` (status only), `ProfileDetector`, `PowerProfile` (power API wrapper), `StartupService` (logon task), `ShellService`, `DialogService`. |
| `Settings/` | `SettingsStore` reads and writes `settings.json`. `SettingsService` exposes settings, cleans the custom game list and applies the language. |

## ViewModels

| Path | Purpose |
| --- | --- |
| `Items/ChoiceOption.cs` | One option of a segmented choice, used for vibrance levels. |
| `Base/` | `ObservableObject`, `LocalizedObject` (refreshes on language change), `RelayCommand`, `AsyncRelayCommand`, `ParameterizedCommand`. |
| `MainViewModel.cs` | Shell: scans hardware, builds pages, owns navigation and loading state. |
| `Pages/PageViewModel.cs` | Base for navigation pages with localized title and badge. |
| `Pages/DashboardViewModel.cs` | Overview: detected profile, hardware and Windows details, apply recommended, restore point. |
| `Pages/GameBoostViewModel.cs` | Game Boost: live status, options, CS2 vibrance, ping test, custom games, one click input lag and ping fix. |
| `Pages/TweakPageViewModel.cs` | One tweak category with an applied counter. |
| `Pages/SettingsViewModel.cs` | Language, restore point, tray, start with Windows, revert everything this app changed, open logs. |
| `Items/TweakItemViewModel.cs` | One tweak row: state, busy and error handling, toggle command. |
| `Items/SpecRowViewModel.cs`, `SpecGroupViewModel.cs`, `SpecTone.cs` | Rows and groups of the overview details table. |
| `Builders/SpecGroupBuilder.cs` | Builds the overview details from the hardware profile. |

## Views

| Path | Purpose |
| --- | --- |
| `MainWindow.xaml(.cs)` | Custom title bar, sidebar navigation, administrator notice, page host. |
| `Pages/DashboardPage.xaml` | Overview page. |
| `Pages/GameBoostPage.xaml` | Game Boost page. |
| `Pages/TweakPage.xaml` | Generic tweak list page used by every category. |
| `Pages/SettingsPage.xaml` | Settings page. |
| `Tray/TrayIcon.cs` | Notification area icon on plain Win32: message window, click handling, native menu. |
| `Tray/TrayController.cs` | Hides the window to the tray on close, restores it, supports silent start. |

## Themes

| File | Purpose |
| --- | --- |
| `Colors.xaml` | Every color token. The only place colors are defined. |
| `Typography.xaml` | Font families and text styles. |
| `Buttons.xaml` | Primary and ghost buttons. |
| `Switch.xaml` | Animated on off switch. |
| `Segmented.xaml` | Segmented choice control used for language. |
| `TextBox.xaml` | Flat text box. |
| `Navigation.xaml` | Sidebar list and items. |
| `ScrollBars.xaml` | Thin dark scroll bar. |
| `WindowControls.xaml` | Minimize, maximize and close buttons. |

## Runtime data

Stored per user under `%LOCALAPPDATA%\slahManager`, or under the folder named by `SLAHMANAGER_DATA`.

| Path | Purpose |
| --- | --- |
| `settings.json` | Saved language and options. |
| `backups/<tweak id>.json` | Original state captured before a tweak was applied. Removed after a revert. |
| `boost-journal.json` | Original power values while a Game Boost session is active. Removed when it ends. |
| `wifi-journal.json` | Wi-Fi interfaces whose scanning is paused. Removed when the game closes. |
| `vibrance-journal.json` | Original screen vibrance while CS2 runs. Removed when the game closes. |
| `logs/app.log` | Record of changes and errors. |

## Data flow

1. `App` takes the instance lock, builds `AppServices`, applies the saved language and starts `GameBoostService`.
2. `MainViewModel` scans hardware, evaluates each tweak's requirements and builds one item per tweak.
3. Each item asks `TweakEngine` whether it is applied by reading the real system state.
4. Toggling an item runs `TweakEngine`: capture snapshots, save backup, apply operations, roll back on failure.
5. `GameBoostService` scans for games. When one appears it holds the timer, raises priority and applies the power
   profile, and when it closes it releases everything and deletes the journal.

# slahManager project rules

slahManager is a Windows desktop utility that detects the hardware of the PC it runs on and applies reversible
optimizations for gaming, input latency, performance and privacy, removes preinstalled bloat, and runs a live Game
Boost while a game is open. These rules apply to every change made in this repository.

## Stack

- C# on .NET 10, WPF, MVVM. Target framework `net10.0-windows`.
- No third party UI libraries. The only package reference is `System.Management` (WMI).
- Native Windows calls go through small wrappers in `Infrastructure/Native`. No WinForms.
- Debug builds run as the invoking user. Release builds request administrator rights through `Properties/admin.manifest`.
- Tweak definitions and string tables are embedded in the assembly, so the published program is one file.

## Build and publish

- Check build: `dotnet build SlahManager.slnx`. It must produce zero errors and zero warnings.
- Single file executable: `dotnet publish src/SlahManager/SlahManager.csproj -p:PublishProfile=SingleExe`.
  The result is `dist/SlahManager.exe`: self contained, compressed, no runtime install needed.
- A running Debug copy locks `bin/Debug`. Verify with a Release build instead of stopping the user's program.

## Code rules

- No comments anywhere: C#, XAML, JSON, project files, scripts. Names must make the code self explanatory.
- One type per file. File name equals the type name.
- Everything is split into folders by responsibility. Namespaces mirror the folder path. File scoped namespaces.
- Nullable reference types are enabled. Do not suppress warnings.
- Models hold data only. Services hold logic and never touch UI types. View models never touch controls.
  Views contain no logic beyond window chrome behavior.
- A new kind of system change is added as three parts: an operation model in `Models/Tweaks/Operations`, an executor
  in `Services/Tweaks/Executors`, and its registration in `Services/AppServices.cs`.
- A new tweak is data only: an entry in a JSON file under `Data/Tweaks`. No code change should be needed.
- Anything that needs a test double sits behind an interface, for example `IGameDetector` and `IHardwareScanner`.

## Safety rules

- Every tweak is reversible unless it declares `"reversible": false`, and the UI marks those as permanent.
- Before a tweak is applied, the engine captures the original state of every operation and stores it in the backup
  store. If any operation fails, everything already applied is rolled back.
- A system restore point is created once per session before the first change, unless disabled in settings.
- Reverting uses the stored backup. If no backup exists, the operation's default value is used, otherwise the value is removed.
- Game Boost changes only what it can undo. Power settings, paused Wi-Fi scanning and screen vibrance are journaled to disk before they are written and restored
  when the game closes, when Game Boost is switched off, when the program exits, and at the next start after a crash.
- Only one instance of the program runs per data folder. A second launch shows the first window and exits.
- The app reads activation status only. It does not contain, download or run any activation bypass.
- State is always derived from the real system, never from a cached flag.
- The tray start mode must not take focus from a running game.

## Testing rules

- The user runs the program on the same machine, sometimes during a game. Never stop `SlahManager` processes that
  were not started by the test itself, never touch the user's `settings.json`, backups or boost journal.
- Point tests at a throwaway data folder with the `SLAHMANAGER_DATA` environment variable.
- Test game detection and boosting with a stub detector and a stand in process, never against real games.
- Do not write system power settings, network adapter settings or screen colors, and do not open visible windows, while a game is running.
- Drive the interface with UI Automation patterns and capture only the program's own window. Do not simulate mouse
  clicks on the desktop.

## Localization rules

- Two languages are supported: English and Russian. English is the fallback.
- All user visible text lives in `Data/Strings/en.json` and `Data/Strings/ru.json`. No hardcoded user facing strings in C# or XAML.
- XAML uses `{loc:Loc some.key}`. Code uses `Localizer.Instance`. View models that expose text derive from `LocalizedObject`
  so the interface updates immediately when the language changes.
- Tweak text is translated with the keys `tweak.<id>.name` and `tweak.<id>.description`. The English text stays in the tweak JSON.
- Messages that mix text and numbers use format keys with positional arguments, for example `"Applied {0} of {1}"`.
- Every key added to `en.json` must also be added to `ru.json`.

## Design rules

- Dark, minimal, near black. The palette is defined once in `Themes/Colors.xaml`. Do not hardcode colors elsewhere.
- One near white accent. Muted semantic colors only for status and risk (positive, warning, danger).
- No gradients, shadows, glow, glass effects, emoji or decorative icons. Separation is done with one pixel hairlines.
- Text only navigation. Each tweak category shows an applied counter.
- Typography: Segoe UI Variable for interface text, Cascadia Mono for labels, tags and technical values.
  Section labels are small uppercase mono. Page titles use the light display weight.
- Text is rendered in grayscale anti aliasing to avoid colored fringes on black.
- Controls are flat with a four pixel corner radius. Hover and press states change tone only.
- Layout must hold for both languages. Russian text is longer, so no fixed widths on text containers.
- The application icon is a near black rounded square with a light lowercase s, stored in `Assets/app.ico`.

## Repository rules

- Never mention the AI assistant or its tooling anywhere in the repository: code, docs, commit messages, metadata,
  file names.
- Keep `PROJECT_MAP.md` in sync whenever files or folders are added, moved or removed.

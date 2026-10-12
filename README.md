# CMS2018 Mods

Standalone Unity Mod Manager mods for **Car Mechanic Simulator 2018**.

| Mod | Current source version | Purpose |
| --- | --- | --- |
| [Workshop Companion Autosave](mods/CMS2018Autosave/README.md) | 0.1.0 | Configurable autosave using the game's normal save routine. No companion app required. |
| [Workshop Companion Live Bridge](mods/CMS2018Bridge/README.md) | 0.1.1 | Read-only local exports for a separate Workshop Companion app. Does not save or change gameplay. |

## Requirements and installation

Developed against CMS2018 **1.7.0**, Unity **2017.4.24**, and Unity Mod Manager **0.33.2** on Windows. Other combinations have not been verified.

Download Unity Mod Manager from [Nexus Mods](https://www.nexusmods.com/site/mods/21). Follow each mod's linked README for installation and settings. Do not install source-code ZIPs as mods: a UMM package must contain the built mod DLL, Info.json and README inside the matching mod folder.

Autosave begins in test mode. Back up the entire selected save profile outside the live save directory and test a separate profile before enabling real saves. This version does not create rolling backups.

## Build and tests

Use the .NET 8 SDK. Mod plugins target the game's legacy .NET 3.5 runtime; tests target .NET 8 and use mod-owned game API stand-ins. Reference assemblies and test packages are restored from NuGet. Building a plugin requires your own installed game and UMM: game/Unity/UMM DLLs are referenced locally, never distributed here.

```powershell
dotnet test mods/CMS2018Bridge/Tests/CMS2018Bridge.Tests.csproj
dotnet test mods/CMS2018Autosave/Tests/CMS2018Autosave.Tests.csproj --filter "FullyQualifiedName!~PackageTests"

dotnet build mods/CMS2018Bridge/Plugin/CMS2018Bridge.csproj -c Release "-p:GameDirectory=C:/Program Files (x86)/Steam/steamapps/common/Car Mechanic Simulator 2018"
dotnet build mods/CMS2018Autosave/Plugin/CMS2018Autosave.csproj -c Release "-p:GameDirectory=C:/Program Files (x86)/Steam/steamapps/common/Car Mechanic Simulator 2018"
```

For the two Autosave archive tests, set `CMS2018_AUTOSAVE_ZIP` to an actual standalone Autosave v0.1.0 ZIP and rerun its tests without the filter. A source checkout alone does not include that binary archive.

The optional Bridge `Tests/check_runtime_types.py` uses `dnfile` for read-only metadata inspection against the installed game framework. This catches missing runtime types that compiling against reference assemblies alone may miss. It does not establish gameplay correctness.

## Status and next release

Bridge **0.2.0 is planned, not released**. Intended work: actionable contract-validation reasons, stale connection expiry, contract-note continuity in the companion, and navigation/source-label corrections. Current 0.1.1 can connect while reporting contracts unavailable; real objective extraction still needs runtime investigation. Do not treat it as a hand-in guarantee.

Companion UI fixes belong to the separate [CMS2018 Companion project](https://github.com/VillarrealFabianAl/cms2018-companion), not this mods-only repository. Both mods are independent of QoLTweaks and neither redistributes or patches its code.

## Repository contents and permissions

Only mod-owned source, tests and documentation are included. No game assets, original game DLLs, loader DLLs, saves, companion preferences, credentials or private development logs are included. No open-source license has been selected; public visibility does not itself grant reuse or redistribution permission. Contact the repository owner before reuse.

Thanks to Red Dot Games, newman55 and the Unity Mod Manager contributors, and testers providing feedback. Developed with assistance from OpenAI Codex.

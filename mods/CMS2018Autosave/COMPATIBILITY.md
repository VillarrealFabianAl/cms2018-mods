# Compatibility evidence

Read-only inspection of the installed game and loader, 2026-10-10:

- CMS 2018 1.7.0; Unity player 2017.4.24. Manager log reports UnityModManager 0.33.2.0 and CLR 2.0.50727.1433.
- Game and manager reference mscorlib 2.0/System.Core 3.5. Plugin now targets net35, as approved; Core also targets netstandard2.0 for modern tests. Settings use System.Xml, not unavailable System.Xml.Linq runtime dependencies.
- Manager configuration starts after MainMenuManager.Awake. Installed metadata exposes ModEntry.OnToggle/OnUpdate/OnGUI/OnSaveGUI, ModSettings.GetPath, UI.Instance.Opened. DLL compiles directly against these installed signatures.
- GameLoader.Get/Save, isReady, carLoader/carLifter; GameManager.IsGameReady/BenchmarkActive; GameScript.CurrentSceneType/CanExit/CanOpenPieMenu; SceneType.Garage and gameMode.Garage are verified.
- ScreenFader.IsRunning reads fadeComplete; SceneLoader.BlockProgress reads blockProgress. GameScript.isExitingToGarage is cached private-field reflection; missing state fails closed.
- DifficultyManager.CanSaveData reads the selected difficulty's canSaveData. ProfileManager.RecoverBackupIsRuning is an additional blocker.
- ProfileManager.GetSelectedProfile returns the selected profile index; changing its identity cancels pending work.
- CarLoader.IsCarLoaded reads done. CarLifter.isMoving and connected oil/welder/detailing/crane flags block transient operations.
- Machine getters read pending group/item fields. We conservatively block whenever a machine retains parts, avoiding dependence on incomplete animation flags.
- GameLoader.Save and ParkingManager.Save were inspected as synchronous methods; saving and callbacks run on the same Unity thread. Our SaveCoordinator provides re-entry protection. SaveComplete begins false and is NOT treated as an initial busy flag or proof of disk persistence.
- GameLoader.Save dispatches normal car/order/inventory/warehouse serialization and its normal popup. No asynchronous method is introduced by this plugin.

Packaging embeds our Core sources into one plugin DLL to avoid legacy assembly-resolution dependencies. It includes no Unity, manager, framework or proprietary game DLLs. All local reference files have Copy Local disabled.

Guard behavior is conservative and still needs real-game validation. Build success verifies signatures, not the lifecycle of every activity or success of physical disk writes. Runtime findings are recorded separately in TEST-RECORD.md.

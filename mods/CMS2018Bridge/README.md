# Workshop Companion Live Bridge 0.1.1

0.1.1 fixes legacy exception compatibility and repeated physical-part objectives across game groups. Matching repeated objectives export once; conflicting state makes that car's requirements unavailable rather than breaking the whole connection. The companion executable does not need replacing for this update.

Requires CMS2018 1.7.0 and installed Unity Mod Manager 0.33.2 or newer compatible loader. Install this ZIP through UMM or put its CMS2018CompanionBridge folder in the game's Mods folder while the game is closed. Enable Workshop Companion Live Bridge. It is independent of Workshop Companion Autosave; leave your autosave settings as they are.

Use the updated companion from this release, select the same profile as the game, and leave automatic checks on. The bridge writes a local snapshot every second in `%LOCALAPPDATA%/CMS2018WorkshopCompanion/bridge/snapshot.json`. It does not save the game, buy parts, change cars, modify QoL settings or read external process memory. It exports loaded garage cars and discovered contract requirements, not inventories or prices in this first version.

Live / auto follows the focused car after two consecutive updates. Pin car keeps a specific car selected even if it changes garage slots. Personal car pins last only while that game object exists in the current session. Pins clear on profile/session changes. Click Saved data for the original save-file view, including unloaded parked cars and restoration conditions.

Shopping and inventory remain saved-data based and are labelled separately. The live view never adds requirements to shopping automatically. Hidden objectives stay unidentified; uncertain/mismatched requirement arrays are not guessed. Live conditions, catalog part links and hand-in guarantees are not provided.

During load, pause, recovery, fade or unstable placement the bridge publishes waiting status. The companion freezes the last live view rather than mixing it with saved data. After five seconds without a fresh update it shows stale; Saved data remains available. Disabling automatic checks stops automatic reads, labels the view Cached game data immediately and disables pin selection. Use Check now or re-enable automatic checks to reconnect.

QoLTweaks 2.1.0 compatibility is based on inspecting its actual arrays and swap coroutine, not on a supplied companion API. This bridge does not patch or redistribute QoLTweaks. Test live objectives and car swaps on a backed-up separate profile before relying on them. Automated/browser tests use synthetic game boundaries; check the included source TEST-RECORD for actual runtime status.

If updates fail, inspect bridge status in UMM, confirm matching profile and restart the game if you installed while it was open. Temporary file locks are retried on the next one-second tick. Keep only one bridge writer/game instance per Windows account; one shared snapshot represents one running game.

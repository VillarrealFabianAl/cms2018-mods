# Workshop Companion Autosave 0.1.0

An autosave-only mod for Car Mechanic Simulator 2018 1.7.0 and Unity Mod Manager 0.33.2. It uses the game's normal save command. No cheats, purchases, game DLL replacement or companion write access.

## Install

Close CMS 2018. In the existing Unity Mod Manager installer, select CMS 2018, open Mods and install the supplied ZIP. Alternatively, extract its CMS2018CompanionAutosave folder into the game's Mods directory. Do not nest that folder twice. Restart the game. Open the manager's mod menu and expand Workshop Companion Autosave's options.

## Start in test mode

Test mode is ON for a new installation. It checks when saving would be allowed, without calling the game's save function or writing game progress. Close the mod menu and enter the garage; after the interval and two seconds of observed safe garage time, the latest attempt will say “Test: would save (no progress written).” The companion will not refresh from a simulated save.

Preset intervals: 30 seconds, 1, 2, 5, 10, 15 and 30 minutes. Custom intervals accept whole numbers in seconds or minutes, from 30 seconds through 120 minutes. Apply saves the choice immediately. The defaults are one minute and test mode enabled. The manager's Save button also saves our settings.

Countdown runs while the garage is loaded and gameplay is active/focused. It freezes while paused or unfocused. Leaving the garage, switching profiles, changing interval/test mode, and disabling the timer reset it. An expired timer keeps one pending request; it never catches up with multiple saves.

Known initial-version timing limitations: the countdown can begin while a garage is still initializing (saving remains blocked), and a new blocker appearing during the final safety recheck restarts the countdown rather than preserving the pending attempt. These do not bypass the safety checks.

## Real autosaves

Before turning test mode OFF: close the game, make a full verified copy of the selected profile outside the live save folder, and test on a separate profile. Restart, check the backup confirmation in the mod options, then uncheck test mode. Turning it off restarts the interval; it does not save immediately.

Real-save mode is remembered. Keep test mode ON until you have completed the backup and test-profile checks. The confirmation is your acknowledgement, not automatic backup creation or verification. This initial version does not create rolling backups.

Saves wait until normal garage free-roam. Shops, menus, part work, inspecting, loading, car-lifter movement and connected service tools defer saves. Collect parts from the wheel balancer, tire changer, spring clamp, battery charger and repair table first; this version waits even if those machines have finished working. Unknown game state blocks saving.

The original game save notification remains. A save can cause a brief pause. “Game save routine returned” means the normal save call returned, not that a reload or backup was verified. Your companion quietly checks the resulting save files through its existing polling.

## Problems and removal

Errors stop further autosave attempts. Read the short message in the options; Retry resets the timer. If settings cannot be read, test mode is restored. Existing game/manual saves still behave normally.

Disable the mod in the manager to stop it immediately. To uninstall, close the game and remove only Mods/CMS2018CompanionAutosave, or uninstall that mod through the manager. Keep your profile backups and existing Unity Mod Manager. Do not remove or replace other game files.

Built/tested against the locally installed legacy .NET 3.5 game APIs. Unit/build success does not establish in-game save safety. Refer to the separate test record for which runtime checks have actually been completed. No support claim for other game versions or combinations of mods.

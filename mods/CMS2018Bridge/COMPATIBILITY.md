# Compatibility boundaries

- Compiles against installed CMS2018 1.7.0 / Unity 2017.4.24 / UMM 0.33.2 APIs with net35; no game or loader DLLs shipped.
- Read-only private-field reflection: GameScript.isExitingToGarage and OrderGenerator.selectedJobs. Missing fields fail closed.
- QoL rewrites task arrays; bridge copies and validates these arrays together and matches exact part paths. Unknown/duplicate paths invalidate the car's contract rather than guessing.
- QoL car swaps are coroutine-based; bridge observes fades, readiness, unique placement and a two-snapshot identity/placement stabilization gate. The exact runtime overlap must still be exercised in game.
- Contract car identity binds order ID, model and config. Personal identity is session/object based, not portable save identity.
- Wire completion integer: -1 unknown, 0 unfinished, 1 complete. Explicit serializer preserves hidden null fields without depending on Unity JSON nullable support.
- Atomic Windows replacement can fail briefly while a reader holds the file; writer preserves old data and retries next tick. No partial-overwrite fallback.
- First release intentionally omits live inventory, prices, part-ID inference, original-condition predictions, shopping synchronization and game-control actions.

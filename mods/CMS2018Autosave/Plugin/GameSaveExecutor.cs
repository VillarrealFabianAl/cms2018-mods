using System;
using CMS2018CompanionAutosave.Core;
namespace CMS2018CompanionAutosave
{
    public sealed class GameSaveExecutor : ISaveExecutor
    {
        readonly Func<GarageSnapshot> read;
        public GameSaveExecutor(Func<GarageSnapshot> readState) { read = readState; }
        public bool Execute()
        {
            if (!SafetyGate.Evaluate(read()).Allowed) throw new InvalidOperationException("Garage state changed before saving; autosave stopped.");
            var loader = GameLoader.Get();
            if (loader == null) throw new InvalidOperationException("Garage is unavailable.");
            return loader.Save();
        }
    }
}

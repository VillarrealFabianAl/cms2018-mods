namespace CMS2018CompanionAutosave.Core
{
    public static class SafetyGate
    {
        public static GateResult Evaluate(GarageSnapshot s)
        {
            if (s == null || !s.StateKnown) return Block("Game activity could not be verified");
            if (!s.InGarage || s.ProfileId == null || s.ProfileId.Trim().Length == 0) return Block("Waiting for a loaded garage profile");
            if (!s.Focused || s.Paused || s.ModMenuOpen) return Block("Waiting for active gameplay");
            if (!s.LoaderReady || !s.GameReady || s.Transitioning) return Block("Waiting for garage loading to finish");
            if (!s.SaveAllowed || s.BenchmarkActive) return Block("Saving is unavailable in this mode");
            if (!s.NormalGarageMode || s.TransactionBusy) return Block("Waiting for garage work to finish");
            if (s.SaveBusy) return Block("Waiting for the current save");
            return new GateResult(true, "Garage is ready");
        }
        static GateResult Block(string reason) { return new GateResult(false, reason); }
    }
}

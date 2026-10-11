namespace CMS2018CompanionAutosave.Core
{
    public class GarageSnapshot
    {
        public string ProfileId;
        public bool InGarage, Paused, Focused, StateKnown, LoaderReady, GameReady, SaveAllowed;
        public bool Transitioning, ModMenuOpen, NormalGarageMode, TransactionBusy, SaveBusy, BenchmarkActive;
    }
    public class GateResult
    {
        public bool Allowed { get; private set; }
        public string Reason { get; private set; }
        public GateResult(bool allowed, string reason) { Allowed = allowed; Reason = reason; }
    }
    public enum ScheduleDecision { None, Attempt }
}

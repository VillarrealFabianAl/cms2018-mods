using System;
namespace CMS2018CompanionAutosave.Core
{
    public interface ISaveExecutor { bool Execute(); }
    public enum AttemptResult { Simulated, GameReturned, Failed, AlreadyRunning }
    public sealed class SaveCoordinator
    {
        public bool Running { get; private set; }
        public string Error { get; private set; }
        public AttemptResult TryAttempt(bool testMode, ISaveExecutor executor)
        {
            if (Running) return AttemptResult.AlreadyRunning;
            Error = null;
            if (testMode) return AttemptResult.Simulated;
            Running = true;
            try
            {
                if (executor == null || !executor.Execute()) { Error = "The game did not report a completed save request."; return AttemptResult.Failed; }
                return AttemptResult.GameReturned;
            }
            catch (Exception ex) { Error = ex.Message; return AttemptResult.Failed; }
            finally { Running = false; }
        }
    }
}

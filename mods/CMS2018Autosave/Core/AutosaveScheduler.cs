using System;

namespace CMS2018CompanionAutosave.Core
{
    public class AutosaveScheduler
    {
        int interval = 60;
        string profile;
        bool garage, initialized, enabled, dry, dispatched, failed;
        public double RemainingSeconds { get; private set; } = 60;
        public bool Pending { get; private set; }
        public double SafeSeconds { get; private set; }
        public bool Failed { get { return failed; } }
        public void Reset(int intervalSeconds)
        {
            if (intervalSeconds < 30 || intervalSeconds > 7200) throw new ArgumentOutOfRangeException("intervalSeconds");
            interval = intervalSeconds; RemainingSeconds = interval; Pending = false; SafeSeconds = 0; dispatched = false;
        }
        public void Reenable() { failed = false; Reset(interval); }
        public void Complete(bool succeeded) { if (succeeded) Reset(interval); else { failed = true; Pending = false; dispatched = false; SafeSeconds = 0; } }
        public ScheduleDecision Tick(double deltaSeconds, GarageSnapshot state, SettingsModel settings, GateResult gate)
        {
            if (state == null || settings == null || gate == null) throw new ArgumentNullException();
            bool identityChanged = initialized && (state.ProfileId != profile || state.InGarage != garage || settings.Enabled != enabled || settings.TestMode != dry);
            bool intervalChanged = settings.IntervalSeconds != interval;
            if (identityChanged || intervalChanged || !settings.Enabled || !state.InGarage || string.IsNullOrEmpty(state.ProfileId)) Reset(settings.IntervalSeconds);
            profile = state.ProfileId; garage = state.InGarage; enabled = settings.Enabled; dry = settings.TestMode; initialized = true;
            if (identityChanged) return ScheduleDecision.None;
            if (!settings.Enabled || !state.InGarage || string.IsNullOrEmpty(state.ProfileId) || failed) return ScheduleDecision.None;
            if (state.Paused || !state.Focused || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds) || deltaSeconds < 0)
            { SafeSeconds = 0; return ScheduleDecision.None; }
            SafeSeconds = gate.Allowed ? SafeSeconds + deltaSeconds : 0;
            if (dispatched) return ScheduleDecision.None;
            RemainingSeconds = Math.Max(0, RemainingSeconds - deltaSeconds);
            if (RemainingSeconds == 0) Pending = true;
            if (Pending && gate.Allowed && SafeSeconds >= 2) { dispatched = true; return ScheduleDecision.Attempt; }
            return ScheduleDecision.None;
        }
    }
}

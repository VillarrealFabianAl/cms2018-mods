using System;
using System.Diagnostics;
using UnityModManagerNet;
using CMS2018CompanionAutosave.Core;
namespace CMS2018CompanionAutosave
{
    public static class Entry
    {
        internal static ModSettings Settings;
        internal static readonly AutosaveScheduler Timer = new AutosaveScheduler();
        internal static readonly SaveCoordinator Saves = new SaveCoordinator();
        internal static readonly GameStateAdapter Adapter = new GameStateAdapter();
        static readonly Stopwatch Clock = new Stopwatch();
        static double last;
        static bool active;
        internal static string Status = "Waiting for a loaded garage", LastAttempt = "None", Message;
        public static bool SaveRunning { get { return Saves.Running; } }
        public static bool Load(UnityModManager.ModEntry entry)
        {
            try
            {
                string warning; Settings = ModSettings.Read(entry, out warning); Message = warning;
                Timer.Reset(Settings.Model.IntervalSeconds);
                entry.OnToggle = Toggle; entry.OnUpdate = Update; entry.OnGUI = SettingsMenu.Draw; entry.OnSaveGUI = SaveSettings;
                Clock.Start(); last = Clock.Elapsed.TotalSeconds;
                entry.Logger.Log("Companion Autosave 0.1.0 loaded; test mode " + Settings.Model.TestMode + ", interval " + Settings.Model.IntervalSeconds + "s.");
                return true;
            }
            catch (Exception ex) { entry.Logger.Error("Autosave could not start: " + ex); return false; }
        }
        static bool Toggle(UnityModManager.ModEntry entry, bool value)
        {
            active = value; Timer.Reenable(); last = Clock.Elapsed.TotalSeconds;
            Status = value ? "Waiting for a loaded garage" : "Disabled"; return true;
        }
        internal static void Changed(UnityModManager.ModEntry entry)
        {
            Timer.Reset(Settings.Model.IntervalSeconds); last = Clock.Elapsed.TotalSeconds;
            SaveSettings(entry);
        }
        internal static void Reenable() { Timer.Reenable(); Status = "Waiting for a safe garage"; }
        static void SaveSettings(UnityModManager.ModEntry entry)
        {
            try { Settings.Save(entry); }
            catch (Exception ex) { Message = "Settings could not be saved: " + ex.Message; entry.Logger.Error(Message); }
        }
        static void Update(UnityModManager.ModEntry entry, float unusedScaledDelta)
        {
            double now = Clock.Elapsed.TotalSeconds; double delta = now - last; last = now;
            if (!active || !entry.Active || Settings == null) return;
            try
            {
                var state = Adapter.Read(); var gate = SafetyGate.Evaluate(state);
                // A long suspended frame cannot count as two seconds of observed safety.
                if (delta > 1) { Timer.Tick(0, state, Settings.Model, new GateResult(false, "Resuming gameplay")); delta = 0; }
                var decision = Timer.Tick(delta, state, Settings.Model, gate);
                if (!Settings.Model.Enabled) Status = "Disabled";
                else if (Timer.Failed) Status = "Stopped after an error — use Retry";
                else if (!gate.Allowed) Status = gate.Reason + (Adapter.Error == null ? "" : ": " + Adapter.Error);
                else if (Timer.Pending) Status = "Waiting for two seconds of safe garage";
                else Status = "Next " + (Settings.Model.TestMode ? "test" : "auto") + " save in " + Math.Ceiling(Timer.RemainingSeconds) + " seconds";
                if (decision != ScheduleDecision.Attempt) return;
                var fresh = Adapter.Read();
                if (!SafetyGate.Evaluate(fresh).Allowed || fresh.ProfileId != state.ProfileId)
                { Timer.Reset(Settings.Model.IntervalSeconds); Status = "Game state changed; countdown restarted"; return; }
                bool testMode = Settings.Model.TestMode;
                Status = testMode ? "Testing save timing" : "Saving";
                var result = Saves.TryAttempt(testMode, new GameSaveExecutor(Adapter.Read));
                bool success = result == AttemptResult.Simulated || result == AttemptResult.GameReturned;
                Timer.Complete(success);
                LastAttempt = DateTime.Now.ToString("HH:mm:ss") + (result == AttemptResult.Simulated ? " — Test: would save (no progress written)" : result == AttemptResult.GameReturned ? " — Game save routine returned" : " — Save failed");
                if (!success) Message = Saves.Error ?? "Save request could not run";
                entry.Logger.Log(LastAttempt);
            }
            catch (Exception ex) { Timer.Complete(false); Message = ex.GetType().Name + ": " + ex.Message; Status = "Stopped after an error — use Retry"; entry.Logger.Error(Message); }
        }
    }
}

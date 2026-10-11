using System;
using System.Globalization;
using UnityEngine;
using UnityModManagerNet;
namespace CMS2018CompanionAutosave
{
    internal static class SettingsMenu
    {
        static readonly int[] Intervals = { 30,60,120,300,600,900,1800 };
        static readonly string[] Labels = { "30 seconds", "1 minute", "2 minutes", "5 minutes", "10 minutes", "15 minutes", "30 minutes" };
        static string custom = "1", validation;
        static bool minutes = true, backupConfirmed;
        public static void Draw(UnityModManager.ModEntry entry)
        {
            var model = Entry.Settings.Model;
            GUILayout.Label("WORKSHOP COMPANION AUTOSAVE", UnityModManager.UI.bold);
            GUILayout.Label(model.TestMode ? "TEST MODE — no saves written" : "AUTOSAVE ACTIVE — the game will save your current profile", UnityModManager.UI.bold);
            bool enabled = GUILayout.Toggle(model.Enabled,"Enable autosave timer");
            if (enabled != model.Enabled) { model.Enabled=enabled; if(enabled) Entry.Reenable(); Entry.Changed(entry); }
            bool dry = GUILayout.Toggle(model.TestMode,"Test mode (check timing without saving progress)");
            if (dry && !model.TestMode) { model.TestMode=true; Entry.Changed(entry); }
            if (!dry && model.TestMode)
            {
                validation = "Confirm your backup below before enabling real saves.";
                if (backupConfirmed) { model.TestMode=false; validation=null; Entry.Changed(entry); }
            }
            if (model.TestMode) backupConfirmed = GUILayout.Toggle(backupConfirmed,"I have backed up this profile and want to test real saves");
            GUILayout.Space(8);
            GUILayout.Label("Save interval: " + Format(model.IntervalSeconds));
            int selected = Array.IndexOf(Intervals,model.IntervalSeconds);
            int next = GUILayout.SelectionGrid(selected,Labels,4);
            if(next>=0 && next!=selected) { model.IntervalSeconds=Intervals[next]; custom=minutes?(model.IntervalSeconds/60.0).ToString(CultureInfo.InvariantCulture):model.IntervalSeconds.ToString(CultureInfo.InvariantCulture); validation=null; Entry.Changed(entry); }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Custom",GUILayout.Width(60)); custom=GUILayout.TextField(custom,10,GUILayout.Width(100));
            minutes=GUILayout.SelectionGrid(minutes?1:0,new[]{"Seconds","Minutes"},2,GUILayout.Width(170))==1;
            if(GUILayout.Button("Apply",GUILayout.Width(80)))
            {
                int seconds;
                if(Core.SettingsModel.TryInterval(custom,minutes,out seconds)) { model.IntervalSeconds=seconds; validation=null; Entry.Changed(entry); }
                else validation="Use a whole number between 30 seconds and 120 minutes.";
            }
            GUILayout.EndHorizontal();
            if(validation!=null) GUILayout.Label(validation);
            GUILayout.Space(8); GUILayout.Label("Status: " + Entry.Status); GUILayout.Label("Latest attempt: " + Entry.LastAttempt);
            if(Entry.Message!=null) GUILayout.Label(Entry.Message);
            if(Entry.Timer.Failed && GUILayout.Button("Retry autosave timer")) { Entry.Reenable(); Entry.Message=null; }
            GUILayout.Space(8);
            GUILayout.Label("Saves wait until you return to an idle garage. Collect parts from workshop machines and disconnect tools first.");
            GUILayout.Label("Close the mod menu for a timing test. Your companion updates after real saves, not during test mode.");
            GUILayout.Label("The game's normal save notification is retained. A save routine return is not a verified backup.");
        }
        static string Format(int seconds) { return seconds % 60 == 0 ? (seconds/60)+" minutes" : seconds+" seconds"; }
    }
}

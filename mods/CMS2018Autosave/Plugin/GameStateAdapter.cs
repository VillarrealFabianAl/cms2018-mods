using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using CMS2018CompanionAutosave.Core;

namespace CMS2018CompanionAutosave
{
    public sealed class GameStateAdapter
    {
        readonly FieldInfo exiting = typeof(GameScript).GetField("isExitingToGarage", BindingFlags.Instance | BindingFlags.NonPublic);
        public string Error { get; private set; }
        public GarageSnapshot Read()
        {
            Error = null;
            var state = new GarageSnapshot { Focused = Application.isFocused, Paused = Time.timeScale <= 0, ModMenuOpen = UnityModManager.UI.Instance != null && UnityModManager.UI.Instance.Opened };
            try
            {
                var script = GameScript.Get();
                if (script == null || script.CurrentSceneType != SceneType.Garage) { state.StateKnown = true; return state; }
                state.InGarage = true;
                var loader = GameLoader.Get(); var mode = GameMode.Get(); var profiles = ProfileManager.Get();
                var difficulty = UnityEngine.Object.FindObjectOfType<DifficultyManager>();
                if (loader == null || mode == null || profiles == null || difficulty == null || exiting == null)
                { Error = "Required game state is unavailable"; return state; }
                int profile = profiles.GetSelectedProfile();
                if (profile < 0) { Error = "No selected profile"; return state; }
                state.ProfileId = profile.ToString(System.Globalization.CultureInfo.InvariantCulture);
                state.LoaderReady = loader.isReady;
                state.GameReady = GameManager.IsGameReady;
                state.SaveAllowed = difficulty.CanSaveData() && !profiles.RecoverBackupIsRuning;
                state.Transitioning = SceneLoader.BlockProgress || (bool)exiting.GetValue(script);
                var fader = ScreenFader.Get();
                if (fader == null) { Error = "Screen transition state is unavailable"; return state; }
                state.Transitioning |= fader.IsRunning();
                state.BenchmarkActive = GameManager.BenchmarkActive;
                state.NormalGarageMode = mode.GetCurrentMode() == gameMode.Garage;
                state.TransactionBusy = !script.CanExit || !script.CanOpenPieMenu;
                // Conservatively wait until machine parts have been collected, not just until animations stop.
                state.TransactionBusy |= script.GetGroupOnWheelBalancer() != null || script.GetGroupOnTireChanger() != null || script.GetGroupOnSpringClamp() != null || script.GetItemOnBatteryCharger() != null || script.GetItemOnRepairTable() != null;
                if (loader.carLifter == null || loader.carLoader == null) { Error = "Garage machines have not initialized"; return state; }
                foreach (var lifter in loader.carLifter) if (lifter != null && lifter.isMoving) state.TransactionBusy = true;
                foreach (var car in loader.carLoader)
                {
                    if (car == null) continue;
                    if (!string.IsNullOrEmpty(car.carToLoad) && !car.IsCarLoaded()) state.TransactionBusy = true;
                    state.TransactionBusy |= car.WelderIsConnected || car.InteriorDetailingToolkitIsConnected || car.OilbinIsConnected || car.EngineCraneIsConnected;
                }
                // Built-in save routines are synchronous on this thread: callbacks cannot interleave them.
                // SaveComplete starts false before the first manual save, so it is NOT used as a busy flag.
                // Our re-entry guard belongs to SaveCoordinator. Including its Running flag here
                // would make the executor reject its own final safety check.
                state.SaveBusy = false;
                state.StateKnown = true;
                return state;
            }
            catch (Exception ex) { Error = ex.GetType().Name + ": " + ex.Message; state.StateKnown = false; return state; }
        }
    }
}

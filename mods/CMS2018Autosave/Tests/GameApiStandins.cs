using CMS2018CompanionAutosave.Core;
namespace CMS2018CompanionAutosave { public static class Entry { public static SaveCoordinator Coordinator=new SaveCoordinator(); public static bool SaveRunning { get { return Coordinator.Running; } } } }
namespace UnityEngine
{
    public class Object { public static T FindObjectOfType<T>() where T:new() { return new T(); } }
    public static class Application { public static bool isFocused=true; }
    public static class Time { public static float timeScale=1; }
}
namespace UnityModManagerNet { public static class UnityModManager { public class UI { public static UI Instance=new UI(); public bool Opened; } } }
public enum SceneType { Garage, Menu }
public enum gameMode { Garage, PartMount }
public class GameScript
{
    public static GameScript Instance=new GameScript(); public static GameScript Get() { return Instance; }
    public SceneType CurrentSceneType=SceneType.Garage;
    public bool CanExit=true, CanOpenPieMenu=true;
    private bool isExitingToGarage=false;
    public bool Exiting { get { return isExitingToGarage; } set { isExitingToGarage=value; } }
    public object MachineGroup;
    public object GetGroupOnWheelBalancer() { return MachineGroup; }
    public object GetGroupOnTireChanger() { return null; }
    public object GetGroupOnSpringClamp() { return null; }
    public object GetItemOnBatteryCharger() { return null; }
    public object GetItemOnRepairTable() { return null; }
}
public class GameMode { public static GameMode Get() { return new GameMode(); } public gameMode GetCurrentMode() { return gameMode.Garage; } }
public class ProfileManager { public bool RecoverBackupIsRuning; public static ProfileManager Get() { return new ProfileManager(); } public int GetSelectedProfile() { return 0; } }
public class DifficultyManager { public bool CanSaveData() { return true; } }
public static class SceneLoader { public static bool BlockProgress; }
public static class GameManager { public static bool IsGameReady=true, BenchmarkActive; }
public class ScreenFader { public static ScreenFader Get() { return new ScreenFader(); } public bool IsRunning() { return false; } }
public class CarLifter { public bool isMoving; }
public class CarLoader { public string carToLoad; public bool WelderIsConnected,InteriorDetailingToolkitIsConnected,OilbinIsConnected,EngineCraneIsConnected; public bool IsCarLoaded() { return true; } }

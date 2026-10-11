using System.Collections.Generic;
namespace UnityEngine { public static class Time { public static float timeScale=1; } }
public enum SceneType { Garage, Menu }
public static class SceneLoader { public static bool BlockProgress; }
public static class GameManager { public static bool IsGameReady=true; }
public class ScreenFader { public static ScreenFader Instance=new ScreenFader();public static ScreenFader Get(){return Instance;}public bool Running;public bool IsRunning(){return Running;} }
public class ProfileManager { public static ProfileManager Instance=new ProfileManager();public static ProfileManager Get(){return Instance;}public bool RecoverBackupIsRuning;public int Profile;public int GetSelectedProfile(){return Profile;} }
public class GameScript { public static GameScript Instance=new GameScript();public static GameScript Get(){return Instance;}public SceneType CurrentSceneType=SceneType.Garage;private bool isExitingToGarage=false;public CarLoader Focus;public CarLoader GetIOMouseOverCarLoader2(){return Focus;} }
public class GameLoader { public static GameLoader Instance=new GameLoader();public static GameLoader Get(){return Instance;}public bool isReady=true;public CarLoader[] carLoader=new CarLoader[0]; }
public class CarLoader { public string carToLoad="example";public int ConfigVersion;public bool Customer,Loaded=true;public int Order=1,Place=1;public List<carPart> Parts=new List<carPart>();public bool GetCustomerCar(){return Customer;}public bool IsCarLoaded(){return Loaded;}public int GetOrderConnection(){return Order;}public int GetPlaceNo(){return Place;}public string GetName(){return "Example";}public List<carPart> GetCarParts(){return Parts;} }
public class carPart {public string name;public object handle;public bool unmounted;}
public class CarLoaderPlaces {public static CarLoaderPlaces Get(){return new CarLoaderPlaces();}public int GetCarLoaderId(CarLoader c){return c.Place;} }
public class Job {public int id=1,configVersion,carLoaderID=1;public string carFile="example";public float globalCondition=.75f;public JobPart[] jobParts=new JobPart[0];}
public class JobPart {public string type="Mechanical",subtype="General";public int partsCount;public List<string> partList=new List<string>();public bool[] _partdone=new bool[0],_partfound=new bool[0];public string[] _name=new string[0];}
public class OrderGenerator {public static OrderGenerator Instance=new OrderGenerator();public static OrderGenerator Get(){return Instance;}private List<Job> selectedJobs=new List<Job>();public List<Job> Jobs {get{return selectedJobs;}}public Job GetJobForCarLoader(int id){return selectedJobs.Find(j=>j.carLoaderID==id);} }

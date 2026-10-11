using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace CMS2018CompanionBridge
{
    public sealed class GameSnapshotAdapter
    {
        readonly Dictionary<CarLoader,string> personal=new Dictionary<CarLoader,string>();
        readonly FieldInfo selectedJobs=typeof(OrderGenerator).GetField("selectedJobs",BindingFlags.Instance|BindingFlags.NonPublic);
        readonly FieldInfo exiting=typeof(GameScript).GetField("isExitingToGarage",BindingFlags.Instance|BindingFlags.NonPublic);
        int lastProfile=-1;string lastSession;
        public bool Transitioning {get;private set;}
        public string Error {get;private set;}
        public BridgeSnapshot Read(string sessionId,long sequence,DateTime utcNow)
        {
            var s=new BridgeSnapshot{session_id=sessionId,sequence=sequence,captured_at_utc=utcNow.ToUniversalTime().ToString("o",System.Globalization.CultureInfo.InvariantCulture)};
            Error=null;Transitioning=false;
            try
            {
                var profiles=ProfileManager.Get();if(profiles==null)return s;
                s.profile=profiles.GetSelectedProfile();if(s.profile<0)return s;
                if(lastProfile!=s.profile || lastSession!=sessionId){personal.Clear();lastProfile=s.profile;lastSession=sessionId;}
                var script=GameScript.Get();var loader=GameLoader.Get();var orders=OrderGenerator.Get();var places=CarLoaderPlaces.Get();
                if(script==null || script.CurrentSceneType!=SceneType.Garage)return s;
                if(loader==null || !loader.isReady || !GameManager.IsGameReady || loader.carLoader==null || places==null || orders==null || selectedJobs==null || exiting==null)return s;
                var fader=ScreenFader.Get();if(fader==null)return s;
                Transitioning=SceneLoader.BlockProgress || (bool)exiting.GetValue(script) || fader.IsRunning() || profiles.RecoverBackupIsRuning || Time.timeScale<=0;
                if(Transitioning){s.status="suspended";return s;}
                var jobs=selectedJobs.GetValue(orders) as List<Job>;if(jobs==null)return s;
                var cars=new List<BridgeCar>();var occupied=new HashSet<int>();var active=new HashSet<CarLoader>();
                var focused=script.GetIOMouseOverCarLoader2();
                foreach(var c in loader.carLoader)
                {
                    if(c==null || string.IsNullOrEmpty(c.carToLoad))continue;
                    if(!c.IsCarLoaded()){Transitioning=true;s.status="suspended";return s;}
                    active.Add(c);int place=c.GetPlaceNo();if(!occupied.Add(place)){Transitioning=true;s.status="suspended";return s;}
                    var car=new BridgeCar{model_id=c.carToLoad,config_version=c.ConfigVersion,customer_car=c.GetCustomerCar(),name=c.GetName(),location=place.ToString(System.Globalization.CultureInfo.InvariantCulture),valid=true};
                    Job job=null;
                    if(car.customer_car)
                    {
                        int slot=places.GetCarLoaderId(c);var matches=jobs.FindAll(j=>j!=null && j.carLoaderID==slot);
                        if(matches.Count==1 && matches[0].id==c.GetOrderConnection() && matches[0].carFile==car.model_id && matches[0].configVersion==car.config_version)job=matches[0];
                        else car.valid=false;
                        car.contract_id=job==null ? -1 : job.id;
                        car.car_id="job:"+c.GetOrderConnection()+":"+car.model_id+":"+car.config_version;car.identity_strength="contract";
                    }
                    else {string id;if(!personal.TryGetValue(c,out id)){id="personal:"+Guid.NewGuid().ToString("N");personal.Add(c,id);}car.car_id=id;car.identity_strength="session-object";}
                    if(job!=null){car.minimum_condition_pct=job.globalCondition>=0 && job.globalCondition<=1 ? job.globalCondition*100d : -1;ReadRequirements(c,job,car);}
                    cars.Add(car);if(c==focused)s.focused_car_id=car.car_id;
                }
                var expired=new List<CarLoader>();foreach(var c in personal.Keys)if(!active.Contains(c))expired.Add(c);foreach(var c in expired)personal.Remove(c);
                s.cars=cars.ToArray();s.status="ready";return s;
            }
            catch(Exception ex){Error=ex.GetType().Name+": "+ex.Message;s.status="error";s.cars=new BridgeCar[0];s.focused_car_id=null;return s;}
        }
        static void ReadRequirements(CarLoader loader,Job job,BridgeCar car)
        {
            if(job.jobParts==null){car.valid=false;return;}
            var parts=loader.GetCarParts();if(parts==null){car.valid=false;return;}
            var tasks=new List<BridgeRequirement>();var occurrences=new Dictionary<string,int>();var paths=new Dictionary<string,BridgeRequirement>();
            foreach(var group in job.jobParts)
            {
                if(group==null || group.partList==null || group._partfound==null || group._partdone==null || group._name==null){car.valid=false;continue;}
                var names=(string[])group._name.Clone();var found=(bool[])group._partfound.Clone();var done=(bool[])group._partdone.Clone();var list=group.partList.ToArray();
                // Oil service may have a positively classified trailing task with no instance path.
                bool oilExtra=group.subtype=="Oil" && names.Length==list.Length+1 && names[names.Length-1]=="Change oil (drain old and refill with new)";
                if(found.Length!=names.Length || done.Length!=names.Length || (list.Length!=names.Length && !oilExtra)){car.valid=false;continue;}
                for(int i=0;i<names.Length;i++)
                {
                    string path=i<list.Length ? list[i] : null;
                    BridgeRequirement previous;
                    if(!string.IsNullOrEmpty(path) && paths.TryGetValue(path,out previous))
                    {
                        // A physical part can be listed in General and a specific objective group.
                        // Group labels are not part identity; disagreement in state is unsafe.
                        if(previous.found!=found[i] || (found[i] && (previous.done!=done[i] || previous.name!=names[i])))car.valid=false;
                        continue;
                    }
                    string groupId=(group.type??"")+"/"+(group.subtype??"");string identity=groupId+"/"+(names[i]??"");int count;occurrences.TryGetValue(identity,out count);occurrences[identity]=count+1;
                    var task=new BridgeRequirement{found=found[i],category=group.type??"Unknown",subtype=group.subtype??"Unknown",kind="unknown",key="hidden:"+tasks.Count};
                    if(task.found)
                    {
                        task.done=done[i];task.name=names[i];
                        if(!string.IsNullOrEmpty(path))
                        {
                            task.key=TaskKeys.ForPart(car.car_id,job.id,path);task.instance_path=path;task.kind="part";
                            var matches=parts.FindAll(p=>p!=null && p.name==path);
                            if(matches.Count!=1)car.valid=false;
                            // Names and exact instance paths are exported; no inferred catalog IDs.
                        }
                        else {task.key=TaskKeys.ForService(car.car_id,job.id,groupId,names[i],count);task.kind=oilExtra && i==names.Length-1 ? "service" : "unknown";}
                    }
                    // Hidden paths are used only for internal deduplication, never exported.
                    if(!string.IsNullOrEmpty(path))paths.Add(path,task);
                    tasks.Add(task);
                }
            }
            car.requirements=car.valid ? tasks.ToArray() : new BridgeRequirement[0];
        }
    }
}

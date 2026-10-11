using System;
using System.Collections.Generic;
namespace CMS2018CompanionBridge
{
    public sealed class CapturePolicy
    {
        double last=double.NegativeInfinity;
        string stable;
        int samples;
        public BridgeSnapshot Observe(double elapsedSeconds,BridgeSnapshot candidate,bool transitioning)
        {
            if(elapsedSeconds-last<1) return null;
            last=elapsedSeconds;
            if(candidate==null) return null;
            if(candidate.status!="ready" || transitioning) { stable=null;samples=0;if(transitioning)candidate.status="suspended";candidate.cars=new BridgeCar[0];candidate.focused_car_id=null;return candidate; }
            if(candidate.cars==null || candidate.cars.Length>64) { candidate.status="error";candidate.cars=new BridgeCar[0];stable=null;samples=0;return candidate; }
            var identities=new HashSet<string>();var placements=new List<string>();
            foreach(var car in candidate.cars)
            {
                if(car==null || string.IsNullOrEmpty(car.car_id) || !identities.Add(car.car_id)) { candidate.status="error";candidate.cars=new BridgeCar[0];stable=null;samples=0;return candidate; }
                placements.Add(car.car_id+":"+car.contract_id+":"+car.location);
                var paths=new HashSet<string>();var keys=new HashSet<string>();
                if(car.requirements==null || car.requirements.Length>2000) { car.valid=false;car.requirements=new BridgeRequirement[0]; }
                foreach(var task in car.requirements)
                {
                    if(task==null || string.IsNullOrEmpty(task.key) || !keys.Add(task.key)) {car.valid=false;continue;}
                    if(!task.found) {task.name=null;task.part_id=null;task.instance_path=null;task.done=null;task.kind="unknown";}
                    if(task.completion < -1 || task.completion>1)car.valid=false;
                    if(!string.IsNullOrEmpty(task.instance_path) && !paths.Add(task.instance_path))car.valid=false;
                }
                if(!car.valid)car.requirements=new BridgeRequirement[0];
            }
            placements.Sort(StringComparer.Ordinal);
            string signature=candidate.session_id+":"+candidate.profile+":"+string.Join(";",placements.ToArray());
            samples=signature==stable ? samples+1 : 1;stable=signature;
            if(samples<2)candidate.status="suspended";
            return candidate;
        }
    }
}

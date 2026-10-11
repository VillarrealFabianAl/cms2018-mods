using System;
namespace CMS2018CompanionBridge
{
    [Serializable] public sealed class BridgeSnapshot
    {
        public int schema_version=1;
        public string bridge_version="0.1.1",game_version="1.7.0",session_id,captured_at_utc,status="unavailable",focused_car_id;
        public long sequence;
        public int profile=-1;
        public BridgeCar[] cars=new BridgeCar[0];
    }
    [Serializable] public sealed class BridgeCar
    {
        public string car_id,identity_strength,model_id,name,location;
        public int config_version;
        public bool customer_car,valid;
        public int contract_id=-1;
        public double minimum_condition_pct=-1;
        public BridgeRequirement[] requirements=new BridgeRequirement[0];
    }
    // Unity JsonUtility cannot serialize nullable booleans. Completion uses -1=unknown, 0=pending, 1=done on wire.
    [Serializable] public sealed class BridgeRequirement
    {
        public string key,category,subtype,kind,instance_path,part_id,name;
        public bool found;
        public int completion=-1;
        public bool? done { get { return completion<0 ? (bool?)null : completion==1; } set { completion=value.HasValue ? (value.Value ? 1 : 0) : -1; } }
    }
    public static class TaskKeys
    {
        static string Encode(string value) { return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value??"")); }
        public static string ForPart(string car,int job,string path) { return Encode(car)+"|"+job+"|p|"+Encode(path); }
        public static string ForService(string car,int job,string group,string name,int occurrence) { return Encode(car)+"|"+job+"|s|"+Encode(group)+"|"+Encode(name)+"|"+occurrence; }
    }
}

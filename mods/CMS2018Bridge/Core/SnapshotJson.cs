using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
namespace CMS2018CompanionBridge
{
    // Small explicit protocol serializer, no reflection or runtime JSON dependency.
    public static class SnapshotJson
    {
        static Dictionary<string,object> Map(params object[] pairs){var d=new Dictionary<string,object>();for(int i=0;i<pairs.Length;i+=2)d.Add((string)pairs[i],pairs[i+1]);return d;}
        public static string Serialize(BridgeSnapshot s)
        {
            var cars=new List<object>();
            foreach(var c in s.cars)
            {
                var tasks=new List<object>();foreach(var t in c.requirements)if(t!=null)tasks.Add(Map("key",t.key,"category",t.category,"subtype",t.subtype,"kind",t.kind,"found",t.found,"completion",t.completion,"instance_path",t.instance_path,"part_id",t.part_id,"name",t.name));
                cars.Add(Map("car_id",c.car_id,"identity_strength",c.identity_strength,"model_id",c.model_id,"name",c.name,"location",c.location,"config_version",c.config_version,"customer_car",c.customer_car,"valid",c.valid,"contract_id",c.contract_id,"minimum_condition_pct",c.minimum_condition_pct,"requirements",tasks));
            }
            var b=new StringBuilder();Write(b,Map("schema_version",s.schema_version,"bridge_version",s.bridge_version,"game_version",s.game_version,"session_id",s.session_id,"sequence",s.sequence,"captured_at_utc",s.captured_at_utc,"profile",s.profile,"status",s.status,"focused_car_id",s.focused_car_id,"cars",cars));return b.ToString();
        }
        static void Quote(StringBuilder b,string value){b.Append('"');foreach(char ch in value){switch(ch){case '"':b.Append("\\\"");break;case '\\':b.Append("\\\\");break;case '\n':b.Append("\\n");break;case '\r':b.Append("\\r");break;case '\t':b.Append("\\t");break;default:if(ch<32)b.Append("\\u"+((int)ch).ToString("x4"));else b.Append(ch);break;}}b.Append('"');}
        static void Write(StringBuilder b,object value)
        {
            if(value==null){b.Append("null");return;}if(value is string){Quote(b,(string)value);return;}if(value is bool){b.Append((bool)value?"true":"false");return;}
            var map=value as Dictionary<string,object>;if(map!=null){b.Append('{');bool first=true;foreach(var p in map){if(!first)b.Append(',');first=false;Quote(b,p.Key);b.Append(':');Write(b,p.Value);}b.Append('}');return;}
            var list=value as IEnumerable;if(list!=null){b.Append('[');bool first=true;foreach(var item in list){if(!first)b.Append(',');first=false;Write(b,item);}b.Append(']');return;}
            if(value is double && (double.IsNaN((double)value)||double.IsInfinity((double)value)))throw new ArgumentException("Nonfinite protocol number");
            b.Append(Convert.ToString(value,CultureInfo.InvariantCulture));
        }
    }
}

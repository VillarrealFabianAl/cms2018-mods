using System;
using System.Diagnostics;
using System.IO;
using UnityModManagerNet;
namespace CMS2018CompanionBridge
{
    public static class Entry
    {
        internal static readonly string Folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CMS2018WorkshopCompanion/bridge");
        static readonly string Session=Guid.NewGuid().ToString("N");
        static readonly Stopwatch Clock=new Stopwatch();
        static readonly CapturePolicy Policy=new CapturePolicy();
        static readonly GameSnapshotAdapter Adapter=new GameSnapshotAdapter();
        static readonly SnapshotWriter Writer=new SnapshotWriter(Folder);
        static double last=-1,lastError=-60;static long sequence;static bool enabled;static int profile=-1;
        internal static string Status="Waiting for the game",Error;
        public static bool Load(UnityModManager.ModEntry entry){entry.OnToggle=Toggle;entry.OnUpdate=Update;entry.OnGUI=BridgeMenu.Draw;Clock.Start();entry.Logger.Log("Read-only live bridge loaded. Output: "+Folder);return true;}
        static bool Toggle(UnityModManager.ModEntry entry,bool value){enabled=value;if(!value)PublishUnavailable(entry);return true;}
        static void PublishUnavailable(UnityModManager.ModEntry entry){try{Writer.Write(new BridgeSnapshot{session_id=Session,sequence=++sequence,profile=profile,status="unavailable",captured_at_utc=DateTime.UtcNow.ToString("o")});Status="Disabled";}catch(Exception ex){Report(entry,ex);}}
        static void Update(UnityModManager.ModEntry entry,float unused)
        {
            double now=Clock.Elapsed.TotalSeconds;if(!enabled || !entry.Active || now-last<1)return;last=now;
            try{var candidate=Adapter.Read(Session,++sequence,DateTime.UtcNow);profile=candidate.profile;var snapshot=Policy.Observe(now,candidate,Adapter.Transitioning);if(snapshot==null)return;Writer.Write(snapshot);Status=snapshot.status+" · "+snapshot.cars.Length+" garage cars";Error=Adapter.Error;}
            catch(Exception ex){Report(entry,ex);}
        }
        static void Report(UnityModManager.ModEntry entry,Exception ex){Error=ex.GetType().Name+": "+ex.Message;Status="Bridge update failed";double now=Clock.Elapsed.TotalSeconds;if(now-lastError>=30){lastError=now;entry.Logger.Error(Error);}}
    }
}

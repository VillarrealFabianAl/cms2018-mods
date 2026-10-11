using System;
using System.IO;
using System.Text;
namespace CMS2018CompanionBridge
{
    public sealed class SnapshotWriter
    {
        readonly string folder;
        public SnapshotWriter(string destination){folder=Path.GetFullPath(destination);}
        public void Write(BridgeSnapshot snapshot)
        {
            byte[] bytes=new UTF8Encoding(false,true).GetBytes(SnapshotJson.Serialize(snapshot));
            if(bytes.Length>2097152)throw new IOException("Bridge snapshot exceeds 2 MiB");
            Directory.CreateDirectory(folder);string target=Path.Combine(folder,"snapshot.json"),temp=Path.Combine(folder,"snapshot-"+Guid.NewGuid().ToString("N")+".tmp");
            try {File.WriteAllBytes(temp,bytes);if(File.Exists(target))File.Replace(temp,target,null);else File.Move(temp,target);}
            finally {if(File.Exists(temp))File.Delete(temp);}
        }
    }
}

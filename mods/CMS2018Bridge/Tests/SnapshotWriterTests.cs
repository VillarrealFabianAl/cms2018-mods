using System;
using System.IO;
using System.Text.Json;
using Xunit;
using CMS2018CompanionBridge;
public class SnapshotWriterTests:IDisposable
{
    readonly string folder=Path.Combine(Path.GetTempPath(),"bridge-tests-"+Guid.NewGuid());
    public void Dispose(){if(Directory.Exists(folder))Directory.Delete(folder,true);}
    [Fact]public void Utf8AndAtomicReplacement(){var w=new SnapshotWriter(folder);var s=CapturePolicyTests.Sample();s.cars[0].name="é\"\nCar";w.Write(s);s.sequence=2;w.Write(s);using(var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,"snapshot.json")))){Assert.Equal(2,doc.RootElement.GetProperty("sequence").GetInt64());Assert.Equal("é\"\nCar",doc.RootElement.GetProperty("cars")[0].GetProperty("name").GetString());Assert.Equal(0,doc.RootElement.GetProperty("cars")[0].GetProperty("requirements")[0].GetProperty("completion").GetInt32());}Assert.Single(Directory.GetFiles(folder));}
    [Fact]public void FailedPublicationRetainsPrevious(){var w=new SnapshotWriter(folder);var s=CapturePolicyTests.Sample();w.Write(s);var previous=File.ReadAllBytes(Path.Combine(folder,"snapshot.json"));s.cars[0].name=new string('x',2100000);Assert.Throws<IOException>(()=>w.Write(s));Assert.Equal(previous,File.ReadAllBytes(Path.Combine(folder,"snapshot.json")));Assert.Single(Directory.GetFiles(folder));}
    [Fact]public void NullIdentifiersSerializeAsNull(){var s=CapturePolicyTests.Sample();s.cars[0].requirements[0].part_id=null;using(var d=JsonDocument.Parse(SnapshotJson.Serialize(s)))Assert.Equal(JsonValueKind.Null,d.RootElement.GetProperty("cars")[0].GetProperty("requirements")[0].GetProperty("part_id").ValueKind);}
    [Fact]public void LockedDestinationLeavesPreviousAndNextTickCanRecover(){if(!OperatingSystem.IsWindows())return;var w=new SnapshotWriter(folder);var s=CapturePolicyTests.Sample();w.Write(s);s.sequence=2;using(var held=new FileStream(Path.Combine(folder,"snapshot.json"),FileMode.Open,FileAccess.Read,FileShare.Read)){Assert.ThrowsAny<IOException>(()=>w.Write(s));Assert.Single(Directory.GetFiles(folder));}w.Write(s);using(var d=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,"snapshot.json"))))Assert.Equal(2,d.RootElement.GetProperty("sequence").GetInt64());}
}

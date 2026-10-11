using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using Xunit;
public class PackageTests
{
    static string Archive() { string path=Environment.GetEnvironmentVariable("CMS2018_AUTOSAVE_ZIP"); if(!string.IsNullOrEmpty(path)) return path; var dir=new DirectoryInfo(AppContext.BaseDirectory); while(dir!=null && !File.Exists(Path.Combine(dir.FullName,"mods/CMS2018Autosave/Info.json"))) dir=dir.Parent; Assert.NotNull(dir); return Path.Combine(dir.FullName,"../../outputs/CMS2018-Autosave/CMS2018CompanionAutosave-v0.1.0.zip"); }
    [Fact] public void PackageContainsOnlyOurFiles()
    {
        using(var zip=ZipFile.OpenRead(Archive()))
        {
            Assert.Equal(new[]{"CMS2018CompanionAutosave/CMS2018CompanionAutosave.dll","CMS2018CompanionAutosave/Info.json","CMS2018CompanionAutosave/README.md"},zip.Entries.Select(e=>e.FullName).OrderBy(x=>x).ToArray());
            Assert.All(zip.Entries,e=>Assert.True(e.Length>0));
        }
    }
    [Fact] public void MetadataMatchesEntry()
    {
        using(var zip=ZipFile.OpenRead(Archive())) using(var reader=new StreamReader(zip.GetEntry("CMS2018CompanionAutosave/Info.json").Open())) using(var doc=JsonDocument.Parse(reader.ReadToEnd()))
        {
            var r=doc.RootElement; Assert.Equal("CMS2018CompanionAutosave",r.GetProperty("Id").GetString()); Assert.Equal("CMS2018CompanionAutosave.Entry.Load",r.GetProperty("EntryMethod").GetString()); Assert.Equal("CMS2018CompanionAutosave.dll",r.GetProperty("AssemblyName").GetString()); Assert.Equal("0.1.0",r.GetProperty("Version").GetString());
        }
    }
}

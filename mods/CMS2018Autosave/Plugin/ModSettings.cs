using System;
using System.IO;
using System.Xml;
using UnityModManagerNet;
using CMS2018CompanionAutosave.Core;
namespace CMS2018CompanionAutosave
{
    // Uses the manager's settings path and OnSaveGUI lifecycle; our parser fails closed.
    public sealed class ModSettings : UnityModManager.ModSettings
    {
        public SettingsModel Model = new SettingsModel();
        public static ModSettings Read(UnityModManager.ModEntry entry, out string warning)
        {
            var result = new ModSettings(); warning = null;
            try
            {
                string path = result.GetPath(entry);
                if (File.Exists(path)) result.Model = SettingsModel.FromXml(File.ReadAllText(path), out warning);
            }
            catch (Exception ex) { warning = "Settings could not be loaded; test mode restored. " + ex.Message; }
            return result;
        }
        public override void Save(UnityModManager.ModEntry entry)
        {
            // Write only our mod settings, never game profile files.
            File.WriteAllText(GetPath(entry), Model.ToXml());
        }
    }
}

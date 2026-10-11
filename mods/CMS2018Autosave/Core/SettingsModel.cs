using System;
using System.Globalization;
using System.Xml;
using System.IO;

namespace CMS2018CompanionAutosave.Core
{
    public class SettingsModel
    {
        public bool Enabled = true;
        public bool TestMode = true;
        public int IntervalSeconds = 60;
        public static bool TryInterval(string text, bool minutes, out int seconds)
        {
            seconds = 0;
            int value;
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) return false;
            long total = minutes ? (long)value * 60 : value;
            if (total < 30 || total > 7200) return false;
            seconds = (int)total;
            return true;
        }
        public string ToXml()
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("SettingsModel"); doc.AppendChild(root);
            foreach (var item in new[] { new[] { "Enabled", Enabled ? "true" : "false" }, new[] { "TestMode", TestMode ? "true" : "false" }, new[] { "IntervalSeconds", IntervalSeconds.ToString(CultureInfo.InvariantCulture) } })
            { var element = doc.CreateElement(item[0]); element.InnerText = item[1]; root.AppendChild(element); }
            return doc.OuterXml;
        }
        public static SettingsModel FromXml(string xml, out string warning)
        {
            warning = null;
            try
            {
                var options = new XmlReaderSettings { XmlResolver = null };
#if NET35
                options.ProhibitDtd = true;
#else
                options.DtdProcessing = DtdProcessing.Prohibit;
#endif
                var doc = new XmlDocument { XmlResolver = null };
                using (var reader = XmlReader.Create(new StringReader(xml), options)) doc.Load(reader);
                var root = doc.DocumentElement;
                if (root == null || root.Name != "SettingsModel" || root.NamespaceURI.Length != 0 || root.Attributes.Count != 0) throw new FormatException();
                string[] names = { "Enabled", "TestMode", "IntervalSeconds" };
                bool[] seen = new bool[3];
                foreach (XmlNode child in root.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Comment || child.NodeType == XmlNodeType.Whitespace) continue;
                    var field = child as XmlElement;
                    if (field == null || field.NamespaceURI.Length != 0 || field.Attributes.Count != 0) throw new FormatException();
                    int index = Array.IndexOf(names, field.Name);
                    if (index < 0 || seen[index]) throw new FormatException();
                    seen[index] = true;
                    foreach (XmlNode value in field.ChildNodes)
                        if (value.NodeType != XmlNodeType.Text && value.NodeType != XmlNodeType.Whitespace) throw new FormatException();
                }
                if (!seen[0] || !seen[1] || !seen[2]) throw new FormatException();
                bool enabled, dry;
                int interval;
                if (root == null || root.Name != "SettingsModel" || root["Enabled"] == null || root["TestMode"] == null || root["IntervalSeconds"] == null ||
                    !bool.TryParse(root["Enabled"].InnerText, out enabled) || !bool.TryParse(root["TestMode"].InnerText, out dry) ||
                    !TryInterval(root["IntervalSeconds"].InnerText, false, out interval)) throw new FormatException();
                return new SettingsModel { Enabled = enabled, TestMode = dry, IntervalSeconds = interval };
            }
            catch (Exception ex) when (ex is System.Xml.XmlException || ex is FormatException || ex is ArgumentException)
            {
                warning = "Settings were invalid; test mode and the 1-minute interval were restored.";
                return new SettingsModel();
            }
        }
    }
}

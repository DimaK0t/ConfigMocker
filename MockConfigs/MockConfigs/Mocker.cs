using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Configuration;
using System.Web.Configuration;
using System.Xml.Linq;

namespace MockConfigs
{
    public class Mocker
    {
        public void MockConfigs()
        {
            MockConnectionStrings();
            MockAppSettings();
        }

        public void MockConnectionStrings()
        {
            var path = GetPathToAlternativeConfigs("connectionStrings");

            if (!File.Exists(path))
            {
                return;
            }

            var localConfig = XDocument.Load(path);
            var newConnStrings = localConfig.Descendants("connectionStrings").Elements().ToList();
            var connString = WebConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings cs in connString)
            {
                var fi = typeof (ConfigurationElement).GetField("_bReadOnly",BindingFlags.Instance | BindingFlags.NonPublic);
                fi.SetValue(cs, false);
                var element = newConnStrings.FirstOrDefault(x => x.Attribute("name").Value.ToString() == cs.Name);
                if (element != null)
                {
                    cs.ConnectionString = element.Attribute("connectionString").Value;
                }
            }
        }

        public void MockAppSettings()
        {
            var path = GetPathToAlternativeConfigs("appSettings");

            if (!File.Exists(path))
            {
                return;
            }

            var alternativeConfig = XDocument.Load(path);
            var newSettings = alternativeConfig.Descendants("appSettings").Elements().ToList();
            NameValueCollection appSettings = WebConfigurationManager.AppSettings;

            for (var i = 0; i < appSettings.Count; i++)
            {
                var key = appSettings.GetKey(i);

                var element = newSettings.FirstOrDefault(x => x.Attribute("key").Value.ToString() == key);
                if (element != null)
                {
                    appSettings[key] = element.Attribute("value").Value;
                }
            }

            ConfigurationManager.RefreshSection("appSettings");
        }

        private string GetPathToAlternativeConfigs(string sectionName)
        {
            var section = (ConfigurationSection) WebConfigurationManager.OpenWebConfiguration(null).GetSection(sectionName);
            var source = section.SectionInformation.ConfigSource;

            if (string.IsNullOrEmpty(source))
            {
                source = "Web.config";
            }

            return HostingEnvironment.MapPath("~/" + Path.ChangeExtension(source, Environment.MachineName + ".config"));
        }
    }
}

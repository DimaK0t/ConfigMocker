using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Linq;

namespace ConfigMocker
{
    public class ConfigMocker : IConfigMocker
    {
        public void Mock()
        {
            MockConnectionStrings();
            MockAppSettings();
        }

        public void MockConnectionStrings()
        {
            var alternativeConfig = GetAlternativeConfigFor("connectionStrings");

            if (alternativeConfig == null)
            {
                return;
            }

            var newConnStrings = alternativeConfig.Descendants("connectionStrings").Elements("add").ToList();
            var connString = WebConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings cs in connString)
            {
                var readOnly = typeof (ConfigurationElement).GetField("_bReadOnly",BindingFlags.Instance | BindingFlags.NonPublic);
                readOnly.SetValue(cs, false);
                var csReplacement = newConnStrings.FirstOrDefault(x => x.Attribute("name").Value == cs.Name);
                if (csReplacement != null)
                {
                    cs.ConnectionString = csReplacement.Attribute("connectionString").Value;
                }
            }
        }

        public void MockAppSettings()
        {
            var alternativeConfig = GetAlternativeConfigFor("appSettings");

            if (alternativeConfig == null)
            {
                return;
            }

            var newSettings = alternativeConfig.Descendants("appSettings").Elements("add").ToList();
            var appSettings = WebConfigurationManager.AppSettings;

            for (var i = 0; i < appSettings.Count; i++)
            {
                var key = appSettings.GetKey(i);

                var settingReplacement = newSettings.FirstOrDefault(x => x.Attribute("key").Value == key);
                if (settingReplacement != null)
                {
                    appSettings[key] = settingReplacement.Attribute("value").Value;
                }
            }

            ConfigurationManager.RefreshSection("appSettings");
        }

        private XDocument GetAlternativeConfigFor(string sectionName)
        {
            var path = GetPathToAlternativeConfigs("appSettings");

            return File.Exists(path) ? XDocument.Load(path) : null;
        }

        private string GetPathToAlternativeConfigs(string sectionName)
        {
            var section = WebConfigurationManager.OpenWebConfiguration(null).GetSection(sectionName);
            var source = section.SectionInformation.ConfigSource;

            if (string.IsNullOrEmpty(source))
            {
                source = "Web.config";
            }

            return HostingEnvironment.MapPath("~/" + Path.ChangeExtension(source, Environment.MachineName + ".config"));
        }
    }
}

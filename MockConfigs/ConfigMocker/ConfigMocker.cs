using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Linq;

namespace ConfigMocker
{
    public class ConfigMocker : IConfigMocker
    {
        private Configuration _config;

        private void LoadConfig()
        {
            if (HttpRuntime.AppDomainAppId != null) // web app
            {
                _config = WebConfigurationManager.OpenWebConfiguration("~");
            }
            else
            {
                _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
        }

        public void Mock()
        {
            MockConnectionStrings();
            MockAppSettings();
        }

        public void MockConnectionStrings()
        {
            if (_config == null)
            {
                LoadConfig();
            }

            var alternativeConfig = GetAlternativeConfigFor("connectionStrings");

            if (alternativeConfig == null)
            {
                return;
            }

            var newConnStrings = alternativeConfig.Descendants("connectionStrings").Elements("add").ToList();
            var connString = ConfigurationManager.ConnectionStrings;

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
            if (_config == null)
            {
                LoadConfig();
            }

            var alternativeConfig = GetAlternativeConfigFor("appSettings");

            if (alternativeConfig == null)
            {
                return;
            }

            var newSettings = alternativeConfig.Descendants("appSettings").Elements("add").ToList();
            var appSettings = ConfigurationManager.AppSettings;

            for (var i = 0; i < appSettings.Count; i++)
            {
                var key = appSettings.GetKey(i);

                var settingReplacement = newSettings.FirstOrDefault(x => x.Attribute("key").Value == key);
                if (settingReplacement != null)
                {
                    appSettings[key] = settingReplacement.Attribute("value").Value;
                }
            }
        }

        private XDocument GetAlternativeConfigFor(string sectionName)
        {
            var path = GetPathToAlternativeConfig(sectionName);

            if (!File.Exists(path))
            {
                return null;
            }

            return XDocument.Load(path);
        }

        private string GetPathToAlternativeConfig(string sectionName)
        {
            var section = _config.GetSection(sectionName);
            var configSource = section.SectionInformation.ConfigSource;

            if (string.IsNullOrEmpty(configSource))
            {
                configSource = Path.GetFileName(_config.FilePath);
            }

            var alternativeConfigPath = Path.ChangeExtension(configSource, Environment.MachineName + ".config");
            return HostingEnvironment.MapPath("~/" + alternativeConfigPath);
        }
    }
}

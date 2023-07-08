using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings
{
    public class ConfigSettings
    {
        public static Connectionstrings ConnectionString => ConfigurationSettingsHelper.GetConfigurationSectionObject<Connectionstrings>("ConnectionStrings");
        public static WebConfigAttributes WebConfigAttributes => ConfigurationSettingsHelper.GetConfigurationSectionObject<WebConfigAttributes>("WebConfigAttributes");
        public static CustomEmail CustomEmail => ConfigurationSettingsHelper.GetConfigurationSectionObject<CustomEmail>("CustomEmail");
        
    }
}

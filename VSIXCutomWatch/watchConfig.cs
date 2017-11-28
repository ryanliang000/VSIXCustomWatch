using System.Configuration;
using System.Linq;

namespace VSIXCutomWatch
{
    class WatchConfig
    {
        public const string c_dll = "CallbackDll";
        public const string c_func = "CallbackFunc";

        public static void GetAppConfig(out string dllName, out string funcName)
        {
            dllName = "";
            funcName = "";
            string file = System.Reflection.Assembly.GetExecutingAssembly().Location;
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            ConfigurationManager.RefreshSection("appSettings");
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (key == c_dll)
                {
                    dllName = config.AppSettings.Settings[key].Value.ToString();
                }
                else if (key == c_func)
                {
                    funcName = config.AppSettings.Settings[key].Value.ToString();
                }
            }
        }

        public static void SetAppConfig(string dllName, string funcName)
        {
            string file = System.Reflection.Assembly.GetExecutingAssembly().Location;
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            ConfigurationManager.RefreshSection("appSettings");
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (key == c_dll)
                {
                    config.AppSettings.Settings[key].Value = dllName;
                }
                else if (key == c_func)
                {
                    config.AppSettings.Settings[key].Value = funcName;
                }
            }
            config.Save(ConfigurationSaveMode.Full, true);
        }

        public static string AppConfigFile()
        {
            string file = System.Reflection.Assembly.GetExecutingAssembly().Location;
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            return config.FilePath;
        }

        public static void InitAppConfig()
        {
            string file = System.Reflection.Assembly.GetExecutingAssembly().Location;
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            if (config.AppSettings.Settings.AllKeys.Count() == 0)
            {
                config.AppSettings.Settings.Add(c_dll,   "");
                config.AppSettings.Settings.Add(c_func,  "");
                config.Save(ConfigurationSaveMode.Full, true);
            }
        }
    }
};

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nini.Config;

namespace Kernel.BFHAdmin.Client.GUI
{
    public static class Config
    {
        internal static IConfigSource AppConfigRoot;
        internal static Nini.Config.IConfig AppSettings;
        internal static IConfigSource ExtraConfigRoot;
        internal static IConfigSource PrivateExtraConfigRoot;
        internal static Nini.Config.IConfig ExtraConfig;
        public static string RconServerAddress;
        public static int RconServerPort;
        public static string RconServerPassword;
        private static string ExtraConfigFile = "Config.xml";
        private static string PrivateExtraConfigFile = "Config.Private.xml";

        static Config()
        {
            AppConfigRoot = new Nini.Config.DotNetConfigSource(DotNetConfigSource.GetFullConfigPath());
            AppSettings = AppConfigRoot.Configs["appSettings"];

            bool hasExtraConfig = false;
            IConfigSource extraConfigRoot = null;
            if (File.Exists(ExtraConfigFile))
            {
                hasExtraConfig = true;
                ExtraConfigRoot = new Nini.Config.XmlConfigSource(ExtraConfigFile);
                extraConfigRoot = ExtraConfigRoot;
            }
            if (File.Exists(PrivateExtraConfigFile))
            {
                hasExtraConfig = true;
                PrivateExtraConfigRoot = new Nini.Config.XmlConfigSource(PrivateExtraConfigFile);
                if (extraConfigRoot != null)
                    extraConfigRoot.Merge(PrivateExtraConfigRoot);
                else
                    extraConfigRoot = PrivateExtraConfigRoot;
            }

            if (hasExtraConfig)
            {
                ExtraConfig = extraConfigRoot.Configs["Settings"];
                RconServerAddress = ExtraConfig.GetString("RconServerAddress");
                RconServerPort = ExtraConfig.GetInt("RconServerPort");
                RconServerPassword = ExtraConfig.GetString("RconServerPassword");
            }
            else
            {
                throw new ApplicationException("Can't find config file: " + ExtraConfigFile);
            }

        }


    }
}
    ;
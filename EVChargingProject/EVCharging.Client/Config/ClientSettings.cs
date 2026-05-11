using System;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace EVCharging.Client.Config
{
    public class ClientSettings
    {
        public string VehicleRoot { get; private set; }
        public string ClientLogFile { get; private set; }
        public int DelayBetweenRowsMs { get; private set; }
        public int SimulateBreakAfterRows { get; private set; }

        private ClientSettings()
        {
        }

        public static ClientSettings Load()
        {
            return new ClientSettings
            {
                VehicleRoot = ReadString("VehicleRoot", "Vehicles"),
                ClientLogFile = ReadString("ClientLogFile", "client.log"),
                DelayBetweenRowsMs = ReadInt("DelayBetweenRowsMs", 0),
                SimulateBreakAfterRows = ReadInt("SimulateBreakAfterRows", 5)
            };
        }

        public string FindVehicleRoot()
        {
            if (Path.IsPathRooted(VehicleRoot) && Directory.Exists(VehicleRoot))
            {
                return VehicleRoot;
            }

            string current = AppDomain.CurrentDomain.BaseDirectory;
            for (int i = 0; i < 10 && !string.IsNullOrEmpty(current); i++)
            {
                string candidate = Path.Combine(current, VehicleRoot);
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                DirectoryInfo parent = Directory.GetParent(current.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                current = parent == null ? null : parent.FullName;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, VehicleRoot);
        }

        public string GetAbsoluteLogPath()
        {
            if (Path.IsPathRooted(ClientLogFile))
            {
                return ClientLogFile;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ClientLogFile);
        }

        private static string ReadString(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static int ReadInt(string key, int defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            int parsed;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }

            return defaultValue;
        }
    }
}

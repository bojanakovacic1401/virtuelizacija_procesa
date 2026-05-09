using System;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace EVCharging.Server.Config
{
    public class ServerSettings
    {
        public string DataRoot { get; private set; }
        public double VoltageSpikeThreshold { get; private set; }
        public double CurrentSpikeThreshold { get; private set; }
        public double PowerFactorMinimum { get; private set; }

        private ServerSettings()
        {
        }

        public static ServerSettings Load()
        {
            return new ServerSettings
            {
                DataRoot = ReadString("DataRoot", "Data"),
                VoltageSpikeThreshold = ReadDouble("VoltageSpikeThreshold", 10.0),
                CurrentSpikeThreshold = ReadDouble("CurrentSpikeThreshold", 5.0),
                PowerFactorMinimum = ReadDouble("PowerFactorMinimum", 0.90)
            };
        }

        public string GetAbsoluteDataRoot()
        {
            if (Path.IsPathRooted(DataRoot))
            {
                return DataRoot;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DataRoot);
        }

        private static string ReadString(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static double ReadDouble(string key, double defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            double parsed;
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }

            return defaultValue;
        }
    }
}

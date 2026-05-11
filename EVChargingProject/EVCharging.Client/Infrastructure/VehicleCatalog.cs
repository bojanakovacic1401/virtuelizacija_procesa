using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EVCharging.Client.Infrastructure
{
    public class VehicleCatalog
    {
        private readonly List<string> _folders;
        private readonly Dictionary<int, string> _choiceByNumber;

        public VehicleCatalog(string vehicleRoot)
        {
            VehicleRoot = vehicleRoot;
            _folders = new List<string>();
            _choiceByNumber = new Dictionary<int, string>();
            Load();
        }

        public string VehicleRoot { get; private set; }
        public IList<string> Folders { get { return _folders.AsReadOnly(); } }

        public string GetByNumber(int number)
        {
            string folder;
            if (_choiceByNumber.TryGetValue(number, out folder))
            {
                return folder;
            }

            return null;
        }

        public bool ContainsChargingProfile(string folder)
        {
            return File.Exists(Path.Combine(folder, "Charging_Profile.csv"));
        }

        private void Load()
        {
            if (!Directory.Exists(VehicleRoot))
            {
                return;
            }

            string[] folders = Directory.GetDirectories(VehicleRoot)
                .Where(ContainsChargingProfile)
                .OrderBy(Path.GetFileName)
                .ToArray();

            for (int i = 0; i < folders.Length; i++)
            {
                _folders.Add(folders[i]);
                _choiceByNumber.Add(i + 1, folders[i]);
            }
        }
    }
}

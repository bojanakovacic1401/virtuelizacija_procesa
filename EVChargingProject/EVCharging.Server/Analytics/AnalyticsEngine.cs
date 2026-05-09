using System;
using System.Collections.Generic;
using EVCharging.Server.Config;
using EVCharging.Server.Events;
using EVCharging.Shared.Models;

namespace EVCharging.Server.Analytics
{
    public class AnalyticsEngine
    {
        private readonly ServerSettings _settings;
        private readonly Dictionary<string, ChargingSample> _previousByVehicle;

        public AnalyticsEngine(ServerSettings settings)
        {
            _settings = settings;
            _previousByVehicle = new Dictionary<string, ChargingSample>(StringComparer.OrdinalIgnoreCase);
        }

        public List<WarningEventArgs> Analyze(ChargingSample current)
        {
            var warnings = new List<WarningEventArgs>();

            ChargingSample previous;
            if (_previousByVehicle.TryGetValue(current.VehicleId, out previous))
            {
                double deltaV = current.VoltageRmsAvg - previous.VoltageRmsAvg;
                if (Math.Abs(deltaV) > _settings.VoltageSpikeThreshold)
                {
                    warnings.Add(new WarningEventArgs(
                        "VoltageSpike",
                        current.VehicleId,
                        current.RowIndex,
                        previous.VoltageRmsAvg,
                        current.VoltageRmsAvg,
                        _settings.VoltageSpikeThreshold,
                        string.Format("Promena napona ΔV={0:0.###} V prelazi prag.", deltaV)));
                }

                double deltaI = current.CurrentRmsAvg - previous.CurrentRmsAvg;
                if (Math.Abs(deltaI) > _settings.CurrentSpikeThreshold)
                {
                    warnings.Add(new WarningEventArgs(
                        "CurrentSpike",
                        current.VehicleId,
                        current.RowIndex,
                        previous.CurrentRmsAvg,
                        current.CurrentRmsAvg,
                        _settings.CurrentSpikeThreshold,
                        string.Format("Promena struje ΔI={0:0.###} A prelazi prag.", deltaI)));
                }
            }

            double powerFactor = current.RealPowerAvg / current.ApparentPowerAvg;
            if (powerFactor < _settings.PowerFactorMinimum)
            {
                warnings.Add(new WarningEventArgs(
                    "PowerFactorWarning",
                    current.VehicleId,
                    current.RowIndex,
                    _settings.PowerFactorMinimum,
                    powerFactor,
                    _settings.PowerFactorMinimum,
                    string.Format("Faktor snage PF={0:0.####} je ispod praga.", powerFactor)));
            }

            _previousByVehicle[current.VehicleId] = current;
            return warnings;
        }
    }
}

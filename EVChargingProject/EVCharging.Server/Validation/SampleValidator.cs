using System;
using System.Collections.Generic;
using EVCharging.Shared.Models;

namespace EVCharging.Server.Validation
{
    public class SampleValidator
    {
        public List<string> Validate(ChargingSample sample)
        {
            var errors = new List<string>();

            if (sample == null)
            {
                errors.Add("Sample je null.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(sample.VehicleId))
            {
                errors.Add("VehicleId je obavezan.");
            }

            if (sample.RowIndex <= 0)
            {
                errors.Add("RowIndex mora biti veći od nule.");
            }

            if (sample.Timestamp == DateTime.MinValue)
            {
                errors.Add("Timestamp nije validan.");
            }

            ValidateFinite(errors, "VoltageRmsMin", sample.VoltageRmsMin);
            ValidateFinite(errors, "VoltageRmsAvg", sample.VoltageRmsAvg);
            ValidateFinite(errors, "VoltageRmsMax", sample.VoltageRmsMax);
            ValidateFinite(errors, "CurrentRmsMin", sample.CurrentRmsMin);
            ValidateFinite(errors, "CurrentRmsAvg", sample.CurrentRmsAvg);
            ValidateFinite(errors, "CurrentRmsMax", sample.CurrentRmsMax);
            ValidateFinite(errors, "RealPowerMin", sample.RealPowerMin);
            ValidateFinite(errors, "RealPowerAvg", sample.RealPowerAvg);
            ValidateFinite(errors, "RealPowerMax", sample.RealPowerMax);
            ValidateFinite(errors, "ReactivePowerMin", sample.ReactivePowerMin);
            ValidateFinite(errors, "ReactivePowerAvg", sample.ReactivePowerAvg);
            ValidateFinite(errors, "ReactivePowerMax", sample.ReactivePowerMax);
            ValidateFinite(errors, "ApparentPowerMin", sample.ApparentPowerMin);
            ValidateFinite(errors, "ApparentPowerAvg", sample.ApparentPowerAvg);
            ValidateFinite(errors, "ApparentPowerMax", sample.ApparentPowerMax);
            ValidateFinite(errors, "FrequencyMin", sample.FrequencyMin);
            ValidateFinite(errors, "FrequencyAvg", sample.FrequencyAvg);
            ValidateFinite(errors, "FrequencyMax", sample.FrequencyMax);

            if (sample.VoltageRmsMin <= 0 || sample.VoltageRmsAvg <= 0 || sample.VoltageRmsMax <= 0)
            {
                errors.Add("Napon mora biti veći od nule.");
            }

            if (sample.FrequencyMin <= 0 || sample.FrequencyAvg <= 0 || sample.FrequencyMax <= 0)
            {
                errors.Add("Frekvencija mora biti veća od nule.");
            }

            if (sample.ApparentPowerAvg <= 0)
            {
                errors.Add("ApparentPowerAvg mora biti veći od nule zbog računanja faktora snage.");
            }

            ValidateRange(errors, "Voltage RMS", sample.VoltageRmsMin, sample.VoltageRmsAvg, sample.VoltageRmsMax);
            ValidateRange(errors, "Current RMS", sample.CurrentRmsMin, sample.CurrentRmsAvg, sample.CurrentRmsMax);
            ValidateRange(errors, "Real Power", sample.RealPowerMin, sample.RealPowerAvg, sample.RealPowerMax);
            ValidateRange(errors, "Reactive Power", sample.ReactivePowerMin, sample.ReactivePowerAvg, sample.ReactivePowerMax);
            ValidateRange(errors, "Apparent Power", sample.ApparentPowerMin, sample.ApparentPowerAvg, sample.ApparentPowerMax);
            ValidateRange(errors, "Frequency", sample.FrequencyMin, sample.FrequencyAvg, sample.FrequencyMax);

            return errors;
        }

        private static void ValidateFinite(List<string> errors, string name, double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                errors.Add(name + " nije konačna numerička vrednost.");
            }
        }

        private static void ValidateRange(List<string> errors, string name, double min, double avg, double max)
        {
            if (min > avg || avg > max)
            {
                errors.Add(name + " mora imati redosled Min <= Avg <= Max.");
            }
        }
    }
}

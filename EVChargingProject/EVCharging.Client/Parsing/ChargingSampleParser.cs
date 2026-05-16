using System;
using System.Collections.Generic;
using System.Globalization;
using EVCharging.Shared.Models;

namespace EVCharging.Client.Parsing
{
    public class ChargingSampleParser
    {
        private readonly Dictionary<string, int> _indexByHeader;

        public ChargingSampleParser(string headerLine)
        {
            _indexByHeader = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            string[] headers = CsvLineSplitter.Split(headerLine);

            for (int i = 0; i < headers.Length; i++)
            {
                string normalized = Normalize(headers[i]);
                if (!_indexByHeader.ContainsKey(normalized))
                {
                    _indexByHeader.Add(normalized, i);
                }
            }
        }

        public bool TryParse(string line, int rowIndex, string vehicleId, out ChargingSample sample, out string error)
        {
            sample = null;
            error = null;

            string[] values = CsvLineSplitter.Split(line);

            try
            {
                sample = new ChargingSample
                {
                    Timestamp = ParseDate(values, "Date Time", "Timestamp"),

                    VoltageRmsMin = ParseDouble(values, "Voltage RMS Min (V)", "VoltageRmsMin"),
                    VoltageRmsAvg = ParseDouble(values, "Voltage RMS Avg (V)", "VoltageRmsAvg"),
                    VoltageRmsMax = ParseDouble(values, "Voltage RMS Max (V)", "VoltageRmsMax"),

                    CurrentRmsMin = ParseDouble(values, "Current RMS Min (A)", "CurrentRmsMin"),
                    CurrentRmsAvg = ParseDouble(values, "Current RMS Avg (A)", "CurrentRmsAvg"),
                    CurrentRmsMax = ParseDouble(values, "Current RMS Max (A)", "CurrentRmsMax"),

                    RealPowerMin = ParseDouble(values, "Real Power Min (kW)", "RealPowerMin"),
                    RealPowerAvg = ParseDouble(values, "Real Power Avg (kW)", "RealPowerAvg"),
                    RealPowerMax = ParseDouble(values, "Real Power Max (kW)", "RealPowerMax"),

                    ReactivePowerMin = ParseDouble(values, "Reactive Power Min (kVAR)", "ReactivePowerMin"),
                    ReactivePowerAvg = ParseDouble(values, "Reactive Power Avg (kVAR)", "ReactivePowerAvg"),
                    ReactivePowerMax = ParseDouble(values, "Reactive Power Max (kVAR)", "ReactivePowerMax"),

                    ApparentPowerMin = ParseDouble(values, "Apparent Power Min (kVA)", "ApparentPowerMin"),
                    ApparentPowerAvg = ParseDouble(values, "Apparent Power Avg (kVA)", "ApparentPowerAvg"),
                    ApparentPowerMax = ParseDouble(values, "Apparent Power Max (kVA)", "ApparentPowerMax"),

                    FrequencyMin = ParseDouble(values, "Frequency Min (Hz)", "FrequencyMin"),
                    FrequencyAvg = ParseDouble(values, "Frequency Avg (Hz)", "FrequencyAvg"),
                    FrequencyMax = ParseDouble(values, "Frequency Max (Hz)", "FrequencyMax"),

                    RowIndex = rowIndex,
                    VehicleId = vehicleId
                };

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private DateTime ParseDate(string[] values, params string[] headers)
        {
            string raw = GetValue(values, headers);
            DateTime parsed;

            string[] formats =
            {
                "yyyy/MM/dd HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss",
                "M/d/yyyy H:mm",
                "M/d/yyyy H:mm:ss",
                "MM/dd/yyyy HH:mm:ss"
            };

            if (DateTime.TryParseExact(raw, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            throw new FormatException("Nevalidan timestamp: " + raw);
        }

        private double ParseDouble(string[] values, params string[] headers)
        {
            string raw = GetValue(values, headers);
            double parsed;

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }

            throw new FormatException("Nevalidna numerička vrednost za kolonu '" + headers[0] + "': " + raw);
        }

        private string GetValue(string[] values, params string[] headers)
        {
            foreach (string header in headers)
            {
                int index;
                if (_indexByHeader.TryGetValue(Normalize(header), out index))
                {
                    if (index < 0 || index >= values.Length)
                    {
                        throw new InvalidOperationException("CSV red nema vrednost za kolonu: " + header);
                    }

                    return values[index].Trim();
                }
            }

            throw new InvalidOperationException("CSV ne sadrži obaveznu kolonu: " + headers[0]);
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().Trim('"').ToLowerInvariant();
        }
    }
}

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace EVCharging.Shared.Models
{
    [DataContract(Namespace = "urn:evcharging")]
    public class ChargingSample
    {
        [DataMember(Order = 1)] public DateTime Timestamp { get; set; }

        [DataMember(Order = 2)] public double VoltageRmsMin { get; set; }
        [DataMember(Order = 3)] public double VoltageRmsAvg { get; set; }
        [DataMember(Order = 4)] public double VoltageRmsMax { get; set; }

        [DataMember(Order = 5)] public double CurrentRmsMin { get; set; }
        [DataMember(Order = 6)] public double CurrentRmsAvg { get; set; }
        [DataMember(Order = 7)] public double CurrentRmsMax { get; set; }

        [DataMember(Order = 8)] public double RealPowerMin { get; set; }
        [DataMember(Order = 9)] public double RealPowerAvg { get; set; }
        [DataMember(Order = 10)] public double RealPowerMax { get; set; }

        [DataMember(Order = 11)] public double ReactivePowerMin { get; set; }
        [DataMember(Order = 12)] public double ReactivePowerAvg { get; set; }
        [DataMember(Order = 13)] public double ReactivePowerMax { get; set; }

        [DataMember(Order = 14)] public double ApparentPowerMin { get; set; }
        [DataMember(Order = 15)] public double ApparentPowerAvg { get; set; }
        [DataMember(Order = 16)] public double ApparentPowerMax { get; set; }

        [DataMember(Order = 17)] public double FrequencyMin { get; set; }
        [DataMember(Order = 18)] public double FrequencyAvg { get; set; }
        [DataMember(Order = 19)] public double FrequencyMax { get; set; }

        [DataMember(Order = 20)] public int RowIndex { get; set; }
        [DataMember(Order = 21)] public string VehicleId { get; set; }

        public string ToCsvLine()
        {
            return string.Join(",", new string[]
            {
                Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                Format(VoltageRmsMin), Format(VoltageRmsAvg), Format(VoltageRmsMax),
                Format(CurrentRmsMin), Format(CurrentRmsAvg), Format(CurrentRmsMax),
                Format(RealPowerMin), Format(RealPowerAvg), Format(RealPowerMax),
                Format(ReactivePowerMin), Format(ReactivePowerAvg), Format(ReactivePowerMax),
                Format(ApparentPowerMin), Format(ApparentPowerAvg), Format(ApparentPowerMax),
                Format(FrequencyMin), Format(FrequencyAvg), Format(FrequencyMax),
                RowIndex.ToString(CultureInfo.InvariantCulture),
                Escape(VehicleId)
            });
        }

        public static string CsvHeader()
        {
            return "Timestamp,VoltageRmsMin,VoltageRmsAvg,VoltageRmsMax,CurrentRmsMin,CurrentRmsAvg,CurrentRmsMax,RealPowerMin,RealPowerAvg,RealPowerMax,ReactivePowerMin,ReactivePowerAvg,ReactivePowerMax,ApparentPowerMin,ApparentPowerAvg,ApparentPowerMax,FrequencyMin,FrequencyAvg,FrequencyMax,RowIndex,VehicleId";
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "#{0} {1:yyyy-MM-dd HH:mm:ss} {2} Uavg={3:0.###}V Iavg={4:0.###}A Pavg={5:0.###}kW",
                RowIndex, Timestamp, VehicleId, VoltageRmsAvg, CurrentRmsAvg, RealPowerAvg);
        }

        private static string Format(double value)
        {
            return value.ToString("0.############", CultureInfo.InvariantCulture);
        }

        private static string Escape(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}
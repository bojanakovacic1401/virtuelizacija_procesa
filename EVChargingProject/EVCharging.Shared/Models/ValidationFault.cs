using System.Runtime.Serialization;

namespace EVCharging.Shared.Models
{
    [DataContract(Namespace = "urn:evcharging")]
    public class ValidationFault
    {
        public ValidationFault()
        {
        }

        public ValidationFault(string vehicleId, int rowIndex, string reason)
        {
            VehicleId = vehicleId;
            RowIndex = rowIndex;
            Reason = reason;
        }

        [DataMember(Order = 1)] public string VehicleId { get; set; }
        [DataMember(Order = 2)] public int RowIndex { get; set; }
        [DataMember(Order = 3)] public string Reason { get; set; }

        public override string ToString()
        {
            return string.Format("VehicleId={0}, RowIndex={1}, Reason={2}", VehicleId, RowIndex, Reason);
        }
    }
}
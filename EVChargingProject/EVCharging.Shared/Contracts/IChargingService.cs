using System.ServiceModel;
using EVCharging.Shared.Models;

namespace EVCharging.Shared.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Allowed, Namespace = "urn:evcharging")]
    public interface IChargingService
    {
        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        void StartSession(string vehicleId);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        void PushSample(ChargingSample sample);

        [OperationContract]
        void EndSession();
    }
}
using System;

namespace EVCharging.Server.Events
{
    public class TransferEventArgs : EventArgs
    {
        public TransferEventArgs(string vehicleId, string status, string sessionPath)
        {
            VehicleId = vehicleId;
            Status = status;
            SessionPath = sessionPath;
            Time = DateTime.Now;
        }

        public string VehicleId { get; private set; }
        public string Status { get; private set; }
        public string SessionPath { get; private set; }
        public DateTime Time { get; private set; }
    }

    public class SampleEventArgs : EventArgs
    {
        public SampleEventArgs(string vehicleId, int rowIndex)
        {
            VehicleId = vehicleId;
            RowIndex = rowIndex;
            Time = DateTime.Now;
        }

        public string VehicleId { get; private set; }
        public int RowIndex { get; private set; }
        public DateTime Time { get; private set; }
    }

    public class WarningEventArgs : EventArgs
    {
        public WarningEventArgs(string warningType, string vehicleId, int rowIndex, double previousValue, double currentValue, double threshold, string message)
        {
            WarningType = warningType;
            VehicleId = vehicleId;
            RowIndex = rowIndex;
            PreviousValue = previousValue;
            CurrentValue = currentValue;
            Threshold = threshold;
            Message = message;
            Time = DateTime.Now;
        }

        public string WarningType { get; private set; }
        public string VehicleId { get; private set; }
        public int RowIndex { get; private set; }
        public double PreviousValue { get; private set; }
        public double CurrentValue { get; private set; }
        public double Threshold { get; private set; }
        public string Message { get; private set; }
        public DateTime Time { get; private set; }
    }

    public static class ChargingEvents
    {
        public delegate void TransferStartedHandler(object sender, TransferEventArgs args);
        public delegate void SampleReceivedHandler(object sender, SampleEventArgs args);
        public delegate void TransferCompletedHandler(object sender, TransferEventArgs args);
        public delegate void WarningRaisedHandler(object sender, WarningEventArgs args);

        public static event TransferStartedHandler OnTransferStarted;
        public static event SampleReceivedHandler OnSampleReceived;
        public static event TransferCompletedHandler OnTransferCompleted;
        public static event WarningRaisedHandler OnWarningRaised;

        public static void RaiseTransferStarted(object sender, TransferEventArgs args)
        {
            if (OnTransferStarted != null)
            {
                OnTransferStarted(sender, args);
            }
        }

        public static void RaiseSampleReceived(object sender, SampleEventArgs args)
        {
            if (OnSampleReceived != null)
            {
                OnSampleReceived(sender, args);
            }
        }

        public static void RaiseTransferCompleted(object sender, TransferEventArgs args)
        {
            if (OnTransferCompleted != null)
            {
                OnTransferCompleted(sender, args);
            }
        }

        public static void RaiseWarning(object sender, WarningEventArgs args)
        {
            if (OnWarningRaised != null)
            {
                OnWarningRaised(sender, args);
            }
        }
    }
}

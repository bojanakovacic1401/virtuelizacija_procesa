using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using EVCharging.Server.Analytics;
using EVCharging.Server.Config;
using EVCharging.Server.Events;
using EVCharging.Server.Infrastructure;
using EVCharging.Server.Validation;
using EVCharging.Shared.Contracts;
using EVCharging.Shared.Models;

namespace EVCharging.Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ChargingService : IChargingService, IDisposable
    {
        private readonly ServerSettings _settings;
        private readonly SampleValidator _validator;
        private readonly AnalyticsEngine _analytics;
        private FileSessionWriter _writer;
        private string _vehicleId;
        private bool _sessionStarted;
        private bool _disposed;

        public ChargingService()
        {
            _settings = ServerSettings.Load();
            _validator = new SampleValidator();
            _analytics = new AnalyticsEngine(_settings);
        }

        ~ChargingService()
        {
            Dispose(false);
        }

        public void StartSession(string vehicleId)
        {
            if (string.IsNullOrWhiteSpace(vehicleId))
            {
                throw CreateFault(vehicleId, 0, "VehicleId ne sme biti prazan.");
            }

            if (_sessionStarted)
            {
                EndSession();
            }

            _vehicleId = vehicleId.Trim();
            string safeVehicleId = PathSanitizer.ToSafeFolderName(_vehicleId);
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string sessionDirectory = Path.Combine(_settings.GetAbsoluteDataRoot(), safeVehicleId, date);

            _writer = new FileSessionWriter(sessionDirectory);
            _sessionStarted = true;

            ChargingEvents.RaiseTransferStarted(this, new TransferEventArgs(_vehicleId, "prenos u toku", sessionDirectory));
        }

        public void PushSample(ChargingSample sample)
        {
            EnsureSession();

            if (sample != null)
            {
                sample.VehicleId = _vehicleId;
            }

            List<string> errors = _validator.Validate(sample);
            if (errors.Count > 0)
            {
                string reason = string.Join(" | ", errors.ToArray());
                int rowIndex = sample == null ? 0 : sample.RowIndex;
                _writer.WriteReject(_vehicleId, rowIndex, reason);
                throw CreateFault(_vehicleId, rowIndex, reason);
            }

            _writer.WriteSample(sample);
            ChargingEvents.RaiseSampleReceived(this, new SampleEventArgs(_vehicleId, sample.RowIndex));

            List<WarningEventArgs> warnings = _analytics.Analyze(sample);
            foreach (WarningEventArgs warning in warnings)
            {
                _writer.WriteWarning(warning);
                ChargingEvents.RaiseWarning(this, warning);
            }
        }

        public void EndSession()
        {
            if (!_sessionStarted)
            {
                return;
            }

            string path = _writer == null ? string.Empty : _writer.SessionDirectory;
            ChargingEvents.RaiseTransferCompleted(this, new TransferEventArgs(_vehicleId, "prenos završen", path));

            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }

            _sessionStarted = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing && _writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }

            _disposed = true;
        }

        private void EnsureSession()
        {
            if (!_sessionStarted || _writer == null)
            {
                throw CreateFault(_vehicleId, 0, "Sesija nije započeta. Prvo pozvati StartSession.");
            }
        }

        private static FaultException<ValidationFault> CreateFault(string vehicleId, int rowIndex, string reason)
        {
            var fault = new ValidationFault(vehicleId, rowIndex, reason);
            return new FaultException<ValidationFault>(fault, new FaultReason(reason));
        }
    }
}

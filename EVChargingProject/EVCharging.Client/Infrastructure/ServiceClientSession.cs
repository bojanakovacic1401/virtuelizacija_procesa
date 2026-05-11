using System;
using System.ServiceModel;
using EVCharging.Shared.Contracts;
using EVCharging.Shared.Models;

namespace EVCharging.Client.Infrastructure
{
    public class ServiceClientSession : IDisposable
    {
        private readonly ChannelFactory<IChargingService> _factory;
        private readonly IChargingService _proxy;
        private bool _disposed;

        public ServiceClientSession()
        {
            _factory = new ChannelFactory<IChargingService>("ChargingServiceEndpoint");
            _proxy = _factory.CreateChannel();
        }

        ~ServiceClientSession()
        {
            Dispose(false);
        }

        public void StartSession(string vehicleId)
        {
            ThrowIfDisposed();
            _proxy.StartSession(vehicleId);
        }

        public void PushSample(ChargingSample sample)
        {
            ThrowIfDisposed();
            _proxy.PushSample(sample);
        }

        public void EndSession()
        {
            ThrowIfDisposed();
            _proxy.EndSession();
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

            if (disposing)
            {
                CloseProxy();
                CloseFactory();
            }

            _disposed = true;
        }

        private void CloseProxy()
        {
            IClientChannel channel = _proxy as IClientChannel;
            if (channel == null)
            {
                return;
            }

            try
            {
                if (channel.State == CommunicationState.Faulted)
                {
                    channel.Abort();
                }
                else
                {
                    channel.Close();
                }
            }
            catch
            {
                channel.Abort();
            }
        }

        private void CloseFactory()
        {
            try
            {
                if (_factory.State == CommunicationState.Faulted)
                {
                    _factory.Abort();
                }
                else
                {
                    _factory.Close();
                }
            }
            catch
            {
                _factory.Abort();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ServiceClientSession");
            }
        }
    }
}

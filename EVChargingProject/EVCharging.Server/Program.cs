using System;
using System.ServiceModel;
using EVCharging.Server.Config;
using EVCharging.Server.Events;
using EVCharging.Server.Infrastructure;
using EVCharging.Server.Services;

namespace EVCharging.Server
{
    internal static class Program
    {
        private static void Main()
        {
            Console.Title = "EV Charging WCF Server";
            ServerSettings settings = ServerSettings.Load();

            SubscribeToEvents();

            using (var watcher = new ServerFileWatcher(settings.GetAbsoluteDataRoot()))
            using (ServiceHost host = new ServiceHost(typeof(ChargingService)))
            {
                try
                {
                    host.Open();

                    Console.WriteLine("==============================================");
                    Console.WriteLine("EV Charging WCF Server je pokrenut.");
                    Console.WriteLine("Endpoint: net.tcp://localhost:9001/ChargingService");
                    Console.WriteLine("Data folder: " + settings.GetAbsoluteDataRoot());
                    Console.WriteLine("Status: čeka klijenta...");
                    Console.WriteLine("Pritisni ENTER za gašenje servera.");
                    Console.WriteLine("==============================================");
                    Console.ReadLine();

                    host.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška pri pokretanju servera:");
                    Console.WriteLine(ex.Message);

                    if (host.State == CommunicationState.Faulted)
                    {
                        host.Abort();
                    }

                    Console.WriteLine("Pritisni ENTER za izlaz.");
                    Console.ReadLine();
                }
            }
        }

        private static void SubscribeToEvents()
        {
            ChargingEvents.OnTransferStarted += delegate (object sender, TransferEventArgs e)
            {
                Console.WriteLine("[START] {0} | {1} | {2}", e.VehicleId, e.Status, e.SessionPath);
            };

            ChargingEvents.OnSampleReceived += delegate (object sender, SampleEventArgs e)
            {
                if (e.RowIndex == 1 || e.RowIndex % 25 == 0)
                {
                    Console.WriteLine("[SAMPLE] {0} | red {1}", e.VehicleId, e.RowIndex);
                }
            };

            ChargingEvents.OnTransferCompleted += delegate (object sender, TransferEventArgs e)
            {
                Console.WriteLine("[END] {0} | {1} | {2}", e.VehicleId, e.Status, e.SessionPath);
            };

            ChargingEvents.OnWarningRaised += (sender, e) =>
            {
                Console.WriteLine("[WARNING] {0} | {1} | red {2} | {3}", e.WarningType, e.VehicleId, e.RowIndex, e.Message);
            };
        }
    }
}

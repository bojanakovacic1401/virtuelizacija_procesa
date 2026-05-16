using System;
using System.IO;
using System.ServiceModel;
using System.Threading;
using EVCharging.Client.Config;
using EVCharging.Client.Infrastructure;
using EVCharging.Client.Logging;
using EVCharging.Client.Parsing;
using EVCharging.Shared.Models;

namespace EVCharging.Client
{
    internal static class Program
    {
        private static void Main()
        {
            Console.Title = "EV Charging WCF Client";

            ClientSettings settings = ClientSettings.Load();
            string vehicleRoot = settings.FindVehicleRoot();

            using (var logger = new ClientLogger(settings.GetAbsoluteLogPath()))
            {
                try
                {
                    var catalog = new VehicleCatalog(vehicleRoot);

                    if (!Directory.Exists(vehicleRoot))
                    {
                        Console.WriteLine("Folder sa vozilima nije pronađen: " + vehicleRoot);
                        logger.Error("Folder sa vozilima nije pronađen: " + vehicleRoot);
                        WaitForExit();
                        return;
                    }

                    if (catalog.Folders.Count == 0)
                    {
                        Console.WriteLine("Nijedan Charging_Profile.csv nije pronađen u: " + vehicleRoot);
                        logger.Error("Nijedan Charging_Profile.csv nije pronađen u: " + vehicleRoot);
                        WaitForExit();
                        return;
                    }

                    PrintMenu(catalog, vehicleRoot);
                    string input = Console.ReadLine();

                    if (string.Equals(input, "A", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (string folder in catalog.Folders)
                        {
                            TransferVehicle(folder, settings, logger, false);
                        }
                    }
                    else if (string.Equals(input, "S", StringComparison.OrdinalIgnoreCase))
                    {
                        string folder = catalog.Folders[0];
                        Console.WriteLine("Simulacija prekida koristi prvo vozilo: " + Path.GetFileName(folder));
                        TransferVehicle(folder, settings, logger, true);
                    }
                    else
                    {
                        int selected;
                        if (!int.TryParse(input, out selected))
                        {
                            Console.WriteLine("Nevalidan unos.");
                            WaitForExit();
                            return;
                        }

                        string selectedFolder = catalog.GetByNumber(selected);
                        if (selectedFolder == null)
                        {
                            Console.WriteLine("Nevalidan izbor.");
                            WaitForExit();
                            return;
                        }

                        TransferVehicle(selectedFolder, settings, logger, false);
                    }

                    Console.WriteLine("Gotovo. Detalji su u client.log, a server čuva Data/<VehicleId>/<YYYY-MM-DD>/ fajlove.");
                }
                catch (EndpointNotFoundException ex)
                {
                    Console.WriteLine("Server nije dostupan. Prvo pokreni EVCharging.Server.");
                    Console.WriteLine(ex.Message);
                    logger.Error("Server nije dostupan: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška u klijentu:");
                    Console.WriteLine(ex.Message);
                    logger.Error("Greška u klijentu: " + ex);
                }
            }

            WaitForExit();
        }

        private static void PrintMenu(VehicleCatalog catalog, string vehicleRoot)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("EV Charging WCF Client");
            Console.WriteLine("Zvanični dataset folder: " + vehicleRoot);
            Console.WriteLine("Pronađeno vozila: " + catalog.Folders.Count);
            Console.WriteLine("Izaberi vozilo za slanje:");
            Console.WriteLine("==============================================");

            for (int i = 0; i < catalog.Folders.Count; i++)
            {
                Console.WriteLine("{0}. {1}", i + 1, Path.GetFileName(catalog.Folders[i]));
            }

            Console.WriteLine("A. Pošalji sva vozila sekvencijalno");
            Console.WriteLine("S. Simuliraj prekid prenosa radi Dispose testa");
            Console.Write("Unos: ");
        }

        private static void TransferVehicle(string vehicleFolder, ClientSettings settings, ClientLogger logger, bool simulateBreak)
        {
            string vehicleId = Path.GetFileName(vehicleFolder);
            string profilePath = Path.Combine(vehicleFolder, "Charging_Profile.csv");

            Console.WriteLine();
            Console.WriteLine("---- Vozilo: {0} ----", vehicleId);
            Console.WriteLine("Fajl: " + profilePath);
            logger.Info("Počinje slanje vozila: " + vehicleId);

            int sent = 0;
            int localRejected = 0;
            int serverRejected = 0;
            bool endedNormally = false;

            using (var client = new ServiceClientSession())
            {
                try
                {
                    client.StartSession(vehicleId);
                    Console.WriteLine("StartSession poslat. Prenos u toku...");

                    using (var reader = new StreamReader(profilePath))
                    {
                        string header = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(header))
                        {
                            logger.Error("CSV nema header: " + profilePath);
                            return;
                        }

                        var parser = new ChargingSampleParser(header);
                        string line;
                        int rowIndex = 0;

                        while ((line = reader.ReadLine()) != null)
                        {
                            rowIndex++;

                            using (var memory = CsvMemoryBuffer.CreateReadableBuffer(line))
                            {
                                string bufferedLine = CsvMemoryBuffer.ReadFromBuffer(memory);

                                ChargingSample sample;
                                string parseError;
                                if (!parser.TryParse(bufferedLine, rowIndex, vehicleId, out sample, out parseError))
                                {
                                    localRejected++;
                                    logger.Warn(string.Format("Lokalno odbijen red {0}, vozilo {1}: {2}", rowIndex, vehicleId, parseError));
                                    continue;
                                }

                                try
                                {
                                    client.PushSample(sample);
                                    sent++;

                                    if (sent % 25 == 0)
                                    {
                                        Console.WriteLine("Poslato redova: " + sent);
                                    }
                                }
                                catch (FaultException ex)
                                {
                                    Console.WriteLine("[ODBIJEN RED] " + sample.RowIndex + " | " + ex.Message);
                                    
                                }
                                catch (CommunicationException ex)
                                {
                                    Console.WriteLine("[GRESKA KOMUNIKACIJE] Red " + sample.RowIndex + " | " + ex.Message);
                                    throw;
                                }
                                catch (TimeoutException ex)
                                {
                                    Console.WriteLine("[TIMEOUT] Red " + sample.RowIndex + " | " + ex.Message);
                                    throw;
                                }
                            }

                            if (simulateBreak && rowIndex >= settings.SimulateBreakAfterRows)
                            {
                                throw new IOException("Simulacija prekida prenosa posle reda " + rowIndex);
                            }

                            if (settings.DelayBetweenRowsMs > 0)
                            {
                                Thread.Sleep(settings.DelayBetweenRowsMs);
                            }
                        }
                    }

                    client.EndSession();
                    endedNormally = true;
                    Console.WriteLine("Prenos završen. Poslato={0}, lokalno odbijeno={1}, server odbio={2}",
                        sent, localRejected, serverRejected);
                    logger.Info(string.Format("Završen prenos vozila {0}. Poslato={1}, lokalno odbijeno={2}, server odbio={3}",
                        vehicleId, sent, localRejected, serverRejected));
                }
                catch (Exception ex)
                {
                    logger.Error("Prekid prenosa za vozilo " + vehicleId + ". Resursi se zatvaraju kroz Dispose. Razlog: " + ex.Message);
                    Console.WriteLine("Prenos je prekinut: " + ex.Message);

                    if (!simulateBreak)
                    {
                        throw;
                    }
                }
                finally
                {
                    if (!endedNormally)
                    {
                        Console.WriteLine("Dispose test: izlazak iz using blokova zatvara StreamReader, MemoryStream, WCF kanal i logger.");
                    }
                }
            }
        }

        private static void WaitForExit()
        {
            Console.WriteLine("Pritisni ENTER za izlaz.");
            Console.ReadLine();
        }
    }
}

# EVChargingProject - Punjač za električna vozila

Kompletan školski projekat za temu **„Razmena i analiza podataka punjača za električna vozila korišćenjem WCF servisa i fajl sistema“**.

Projekat je urađen kao Visual Studio solution za **.NET Framework 4.7.2** i ostaje u domenu tehnika iz priloženih vežbi:

- WCF servis, `ServiceContract`, `OperationContract`, `FaultContract`
- `DataContract` modeli
- kolekcije (`List`, `Dictionary`)
- `IDisposable` i `Dispose(bool)` pattern
- `StreamReader`, `StreamWriter`, `FileStream`, `MemoryStream`
- rad sa fajlovima i direktorijumima
- delegati i događaji
- anonimne metode pri pretplati na događaje
- `FileSystemWatcher` za praćenje promene fajlova
- konfiguracija kroz `App.config`

## Struktura solution-a

```text
EVChargingProject.sln
├── EVCharging.Shared   - WCF ugovori i DataContract klase
├── EVCharging.Server   - WCF server, fajl sistem, validacija, događaji, analitika
└── EVCharging.Client   - klijent koji čita zvanični dataset i šalje redove serveru
```

## Zvanični dataset

U projekat su ubačeni originalni fajlovi iz Harvard Dataverse dataset-a `EV-CPW Dataset`:

```text
EVCharging.Client/Vehicles/<VehicleId>/Charging_Profile.csv
EVCharging.Client/Vehicles/<VehicleId>/Waveforms/*.csv
```

Klijent koristi `Charging_Profile.csv`, jer to eksplicitno traži projektni zadatak. `Waveforms` fajlovi su takođe ostavljeni u projektu kao deo originalnog dataset-a.

## Pokretanje

1. Otvoriti `EVChargingProject.sln` u Visual Studio-u.
2. Build Solution.
3. Pokrenuti prvo `EVCharging.Server`.
4. Zatim pokrenuti `EVCharging.Client`.
5. U klijentu izabrati vozilo unosom rednog broja ili `A` za slanje svih vozila.

Server čuva podatke u:

```text
EVCharging.Server/bin/Debug/Data/<VehicleId>/<YYYY-MM-DD>/session.csv
EVCharging.Server/bin/Debug/Data/<VehicleId>/<YYYY-MM-DD>/rejects.csv
EVCharging.Server/bin/Debug/Data/<VehicleId>/<YYYY-MM-DD>/warnings.csv
```

## Važne konfiguracije

Server konfiguracija je u `EVCharging.Server/App.config`:

- `DataRoot`
- `VoltageSpikeThreshold`
- `CurrentSpikeThreshold`
- `PowerFactorMinimum`
- `netTcpBinding` sa `transferMode="Streamed"`
- povećan `maxReceivedMessageSize`

Klijent konfiguracija je u `EVCharging.Client/App.config`:

- `VehicleRoot`
- `ClientLogFile`
- `DelayBetweenRowsMs`
- endpoint ka serveru

## Napomena za odbranu

Ovaj projekat koristi sekvencijalno slanje redova: jedan CSV red = jedan `PushSample` poziv. Time se demonstrira streaming logika iz zadatka, dok se `MemoryStream` koristi za privremeno skladištenje reda pre parsiranja i slanja.

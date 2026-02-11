# Brainboxes.IO

Official .NET library for monitoring and controlling [Brainboxes](https://www.brainboxes.com) Ethernet-attached data acquisition and control devices.

[![NuGet](https://img.shields.io/nuget/v/Brainboxes.IO.svg)](https://www.nuget.org/packages/Brainboxes.IO)

## Quick Start

Install the [NuGet package](https://www.nuget.org/packages/Brainboxes.IO):

```
dotnet add package Brainboxes.IO
```

Control digital outputs:

```csharp
using Brainboxes.IO;

using (var device = EDDevice.Create("192.168.0.100"))
{
    // Set digital output 0 high
    device.Outputs[0].Value = 1;

    // Read all digital inputs
    foreach (var input in device.Inputs)
    {
        Console.WriteLine($"Input {input.IONumber}: {input.Value}");
    }
}
```

Monitor input changes with events:

```csharp
using (var device = EDDevice.Create("192.168.0.100"))
{
    device.IOLineStateChanged += (sender, e) =>
    {
        Console.WriteLine($"Line {e.Line} changed to {e.State}");
    };

    Console.ReadKey(); // Keep running to receive events
}
```

Send receive data over ethernet connected serial ports:

```csharp
// Create a device for a known model
ESDevice es = new ES246("192.168.0.100");

// Connect opens all serial ports
es.Connect();

// Send data through port 0
es.Ports[0].Send("Hello");

// Receive data (blocks until terminator received or timeout)
string response = es.Ports[0].Receive();

// Disconnect all ports
es.Disconnect();
```

## Supported Devices

### Ethernet Remote IO (ED series)

[Digital and analog I/O modules](https://www.brainboxes.com/products/remote-io) for process control and automation.

#### Digital IO Devices

| Class | Product | Digital Inputs | Digital Outputs | Description |
|-------|---------|---------------|----------------|-------------|
| `ED588` | ED-588 | 8 | 8 | 8 DI + 8 DO + Serial Gateway |
| `ED516` | ED-516 | 16 | 0 | 16 Digital Inputs + Serial Gateway |
| `ED527` | ED-527 | 0 | 16 | 16 Digital Outputs + Serial Gateway |
| `ED538` | ED-538 | 8 | 4 (relay) | 8 DI + 4 Relays + Serial Gateway |
| `ED004` | ED-004 | 4 | 4 | 4 DIO + RS-232 |
| `ED204` | ED-204 | 4 | 4 | 4 DIO + RS-232 |
| `ED008` | ED-008 | 8 | 8 | 8 Digital IO Ports |
| `ED038` | ED-038 | 3 | 3 (relay) | 3 Relays + 3 Digital Inputs |

#### Analog/Temp IO Devices

| Class | Product | Analog Inputs | Analog Outputs | Sensor Type | Description |
|-------|---------|--------------|----------------|-------------|-------------|
| `ED549` | ED-549 | 8 | 0 | Voltage/Current/Temp | 8 Analog Inputs + Serial Gateway |
| `ED560` | ED-560 | 0 | 4 | Voltage/Current | 4 Analog Outputs + Serial Gateway |
| `ED582` | ED-582 | 4 | 0 | RTD (PT100/PT1000) | 4 RTD Temperature Inputs + Serial Gateway |
| `ED593` | ED-593 | 8 | 0 | Thermocouple | 8 Thermocouple Inputs + Serial Gateway |

### Edge Controller

| Class | Product | Digital IO | Serial Ports | Description |
|-------|---------|---------------|----------------|-------------|
| `BB400` | BB-400 | 8 | 1 |Industrial Raspberry Pi Edge Controller |

### Ethernet to Serial (ES series)

Brainboxes full range of [Ethernet to Serial Devices](https://www.brainboxes.com/products/ethernet-to-serial). Network-enable serial devices over Ethernet.

#### Light Industrial RS-232 Devices

| Class | Product | Ports | Description |
|-------|---------|-------|-------------|
| `ES246` | ES-246 | 1 | 1-port RS-232 |
| `ES257` | ES-257 | 2 | 2-port RS-232 |
| `ES357` | ES-357 | 2 | Enhanced 1-port RS-232 + 1-port RS422/485 |
| `ES446` | ES-446 | 1 | PoE 1-port RS-232 |
| `ES457` | ES-457 | 2 | PoE 2-port RS-232 |
| `ES701` | ES-701 | 4 | 4-port RS-232 |
| `ES279` | ES-279 | 8 | 8-port RS-232 |

#### Light Industrial RS-422/485 Devices

| Class | Product | Ports | Description |
|-------|---------|-------|-------------|
| `ES313` | ES-313 | 1 | 1-port RS-422/485 |
| `ES320` | ES-320 | 2 | 2-port RS-422/485 |
| `ES413` | ES-413 | 1 | PoE 1-port RS-422/485 |
| `ES420` | ES-420 | 2 | PoE 2-port RS-422/485 |
| `ES346` | ES-346 | 4 | 4-port RS-422/485 |
| `ES842` | ES-842 | 8 | 8-port RS-422/485 |

#### Industrial RS-232/422/485 Devices

| Class | Product | Ports | Description |
|-------|---------|-------|-------------|
| `ES511` | ES-511 | 1 | Industrial 1-port RS-232/422/485 |
| `ES522` | ES-522 | 2 | Industrial 2-port RS-232/422/485 |
| `ES551` | ES-551 | 1 | Industrial Isolated 1-port RS-422/485 |
| `ES571` | ES-571 | 1 | Industrial 1-port RS-422/485 + Ethernet Switch|


### Industrial IoT

[Brainboxes Edge Controllers](https://www.brainboxes.com/products/industrial-edge-controller)

| Device | Description |
|--------|-------------|
| BB-400 | Industrial Raspberry Pi with Brainboxes I/O |

## Features

- **Two protocols**: ASCII (DCON) on port 9500 and Modbus TCP on port 502
- **Digital I/O**: Read inputs, control outputs, count transitions
- **Analog I/O**: Read analog inputs, set analog outputs, configurable data formats
- **Event-driven**: Subscribe to input state changes and device status events
- **Serial communication**: Send and receive data through Ethernet-to-serial converters
- **Thread-safe**: Built-in locking for concurrent access
- **Connection monitoring**: Automatic detection of connection loss and device availability

## Target Frameworks

| Framework | Platform |
|-----------|----------|
| .NET 10 | Windows, Linux, macOS |
| .NET Standard 2.1 | Any compatible runtime |
| .NET Standard 2.0 | Any compatible runtime |
| .NET Framework 4.8 | Windows |

## Documentation

- [API Reference](https://docs.brainboxes.com/api/dotnet/) - Full API documentation
- [NuGet Package](https://www.nuget.org/packages/Brainboxes.IO) - Package download and version history
- [Brainboxes Remote IO](https://www.brainboxes.com/remote-io) - Product information
- [Samples](Samples/) - Example projects in C# and VB.NET

## Building from Source

```bash
# Build
dotnet build Brainboxes.IO.sln

# Build release
dotnet build Brainboxes.IO.sln -c Release

# Run unit tests
dotnet test --filter "TestCategory=Unit"

# Run all tests (requires Brainboxes hardware)
dotnet test Brainboxes.IO.Tests/Brainboxes.IO.Tests.csproj
```

## Project Structure

```
Brainboxes.IO/          # Main library
Brainboxes.IO.Tests/    # Unit and integration tests
Samples/
  C-Sharp/              # C# example projects
  VB/                   # VB.NET example projects
docs/                   # DocFX documentation source
```

## License

This project is released under the [Unlicense](UNLICENSE) (public domain).

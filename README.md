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
    device.Connect();

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
    device.Connect();

    device.IOLineStateChanged += (sender, e) =>
    {
        Console.WriteLine($"Line {e.Line} changed to {e.State}");
    };

    Console.ReadKey(); // Keep running to receive events
}
```

## Supported Devices

### Ethernet Remote IO (ED series)

Digital and analog I/O modules for process control and automation.

| Device | Description |
|--------|-------------|
| ED-204 | 4 digital outputs |
| ED-516 | 16 digital inputs |
| ED-527 | 16 digital I/O |
| ED-549 | Mixed digital + analog I/O |
| ED-560 | Analog I/O with Modbus |
| ED-582 | Thermocouple input |
| ED-588 | 8 relay outputs |

### Ethernet to Serial (ES series)

Network-enable serial devices over Ethernet.

| Device | Description |
|--------|-------------|
| ES-246 | 1-port RS-232 |
| ES-257 | 1-port RS-232/422/485 |
| ES-446 | 4-port RS-232 |
| ES-457 | 4-port RS-232/422/485 |

### Industrial IoT

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

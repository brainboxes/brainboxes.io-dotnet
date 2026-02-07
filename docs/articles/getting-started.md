# Getting Started

## Prerequisites

- .NET 10, .NET Standard 2.0/2.1, or .NET Framework 4.8
- A Brainboxes Ethernet-attached device on your network (ED-series, ES-series, or BB-400)
- The device's IP address (use [Brainboxes Boost.IO Manager](https://www.brainboxes.com/faq/items/brainboxes-boost-io-manager) to discover devices on your network)

## Installation

Install the NuGet package:

```bash
dotnet add package Brainboxes.IO
```

Or via the Package Manager Console:

```powershell
Install-Package Brainboxes.IO
```

## Quick Start — Remote IO (ED-series)

The most common use case is reading inputs and controlling outputs on an ED-series device:

```csharp
using Brainboxes.IO;

// EDDevice.Create auto-detects the device type from its IP address
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    // Read a digital input (0 = LOW, 1 = HIGH)
    int inputState = ed.Inputs[0].Value;
    Console.WriteLine($"Input 0: {inputState}");

    // Set a digital output (0 = OPEN, 1 = CLOSED)
    ed.Outputs[0].Value = 1;

    // Toggle an output
    ed.Outputs[0].Toggle();
}
// Connection is automatically closed when the using block exits
```

The `using` statement ensures the connection is properly closed even if an exception occurs.

## Quick Start — Ethernet to Serial (ES-series)

For ES-series devices, you send and receive serial data through network-accessible ports:

```csharp
using Brainboxes.IO;

ESDevice es = new ES246("192.168.0.100");
try
{
    es.Connect();

    // Send data through serial port 0
    es.Ports[0].Send("Hello device");

    // Receive a response (blocks until terminating character received or timeout)
    string response = es.Ports[0].Receive();
    Console.WriteLine($"Received: {response}");
}
finally
{
    es.Disconnect();
}
```

## Connection Types

### TCP Connection (default)

All Brainboxes Ethernet devices use TCP connections. The port number determines the protocol:

```csharp
// ASCII protocol on port 9500 (default — human-readable commands)
var asciiConnection = new TCPConnection("192.168.0.100");

// Modbus TCP protocol on port 502 (binary protocol)
var modbusConnection = new TCPConnection("192.168.0.100", TCPConnection.DEFAULT_MODBUSTCP_PORT);

// Custom timeout settings
var connection = new TCPConnection(
    ip: "192.168.0.100",
    port: 9500,
    timeout: 5000,           // read/write timeout (ms)
    connectionTimeout: 30000  // socket connect timeout (ms)
);
```

### Manual Connection (advanced)

Instead of using the `EDDevice.Create` factory method, you can construct devices manually:

```csharp
var connection = new TCPConnection("192.168.0.100", 9500, timeout: 5000);
var device = new ED588(connection);
device.Connect();
// ... use device ...
device.Disconnect();
```

## Monitoring for Changes

ED-series devices support event-driven monitoring of IO line state changes:

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    // Monitor a specific input for rising edge (LOW → HIGH)
    ed.Inputs[0].IOLineRisingEdge += (line, device, changeType) =>
    {
        Console.WriteLine($"Input {line.IONumber} went HIGH at {DateTime.Now}");
    };

    // Monitor all IO lines for any change
    ed.IOLineChanged += (line, device, changeType) =>
    {
        Console.WriteLine($"Line {line} changed: {changeType}");
    };

    Console.WriteLine("Monitoring... press any key to stop.");
    Console.ReadKey();
}
```

## Error Handling

```csharp
try
{
    using (EDDevice ed = EDDevice.Create("192.168.0.100"))
    {
        ed.Outputs[0].Value = 1;
    }
}
catch (System.Net.Sockets.SocketException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Device did not respond: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Invalid operation: {ex.Message}");
}
```

## Next Steps

- [Working with Digital IO](digital-io.md) — inputs, outputs, events, latches, and counters
- [Working with Analog IO](analog-io.md) — analog inputs/outputs, data formats, and temperature
- [Ethernet to Serial Guide](ethernet-to-serial.md) — ES devices and custom serial protocols
- [Protocols Guide](protocols.md) — ASCII vs Modbus TCP protocol details
- [Architecture Overview](architecture.md) — library design, threading, and class hierarchy
- [Device Reference](device-reference.md) — complete list of supported devices with IO counts

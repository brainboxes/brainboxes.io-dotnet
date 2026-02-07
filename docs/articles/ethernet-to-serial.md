# Ethernet to Serial Guide

This guide covers using ES-series devices to communicate with serial devices over Ethernet using the Brainboxes.IO library.

## Overview

ES-series devices convert Ethernet TCP/IP connections to physical RS-232 or RS-422/485 serial ports. This allows you to communicate with serial devices (scales, barcode readers, instruments, PLCs) over your existing Ethernet network without running serial cables.

Each physical serial port on the ES device is accessible via a separate TCP connection (ports 9001, 9002, etc.).

## Basic Usage

```csharp
using Brainboxes.IO;

// Create a device for a known model
ESDevice es = new ES246("192.168.0.100");

// Connect opens all serial ports
es.Connect();

// Send data through port 0
es.Ports[0].Send("Hello");

// Receive data (blocks until terminator received or timeout)
try
{
    string response = es.Ports[0].Receive();
    Console.WriteLine($"Received: {response}");
}
catch (TimeoutException)
{
    Console.WriteLine("No response within timeout");
}

// Disconnect all ports
es.Disconnect();
```

## Auto-Detection

If you don't know the device model, use the factory method:

```csharp
ESDevice es = ESDevice.Create("192.168.0.100");
Console.WriteLine($"Found: {es}"); // e.g. "ES246 : 1 Port RS232"
es.Connect();
```

The `Create` method reads the device's XML configuration page to determine the correct subclass.

## Working with Multiple Ports

Multi-port devices (e.g. ES-257 with 2 ports) expose each port independently:

```csharp
ESDevice es = new ES257("192.168.0.100");
es.Connect();

// Each port can communicate with a different serial device
es.Ports[0].Label = "Weighing scale";
es.Ports[1].Label = "Barcode reader";

es.Ports[0].Send("READ_WEIGHT");
es.Ports[1].Send("SCAN");

string weight = es.Ports[0].Receive();
string barcode = es.Ports[1].Receive();
```

You can also connect to individual ports rather than all at once:

```csharp
// Connect only port 0
es.Ports[0].Connect();
```

## Configuring the Serial Protocol

The `DefaultSerialProtocol` controls how data is framed and encoded. You can customise it per-port:

### Terminating Characters

```csharp
// Single character terminator (default: \r)
es.Ports[0].Protocol = new DefaultSerialProtocol()
{
    TerminatingCharacters = "\r",
};

// Multi-character terminator
es.Ports[0].Protocol = new DefaultSerialProtocol()
{
    TerminatingCharacters = "\r\n",
};
```

### Character Encoding

```csharp
// ASCII encoding (default)
es.Ports[0].Protocol = new DefaultSerialProtocol()
{
    Encoding = System.Text.Encoding.ASCII,
};

// Unicode for international character support
es.Ports[0].Protocol = new DefaultSerialProtocol()
{
    Encoding = System.Text.Encoding.Unicode,
};

// UTF-8
es.Ports[0].Protocol = new DefaultSerialProtocol()
{
    Encoding = System.Text.Encoding.UTF8,
};
```

## Custom Serial Protocols

For complex serial devices with non-standard framing, implement the `ISerialProtocol` interface:

```csharp
public class MyDeviceProtocol : ISerialProtocol
{
    public void Send(Stream stream, string data)
    {
        // Custom encoding: prefix with STX, suffix with ETX
        byte[] payload = Encoding.ASCII.GetBytes("\x02" + data + "\x03");
        stream.Write(payload, 0, payload.Length);
    }

    public string Receive(Stream stream)
    {
        // Custom decoding: read until ETX (0x03)
        var buffer = new List<byte>();
        int b;
        while ((b = stream.ReadByte()) != 0x03 && b != -1)
        {
            if (b != 0x02) // skip STX
                buffer.Add((byte)b);
        }
        return Encoding.ASCII.GetString(buffer.ToArray());
    }

    public object Clone()
    {
        return new MyDeviceProtocol();
    }
}

// Usage
es.Ports[0].Protocol = new MyDeviceProtocol();
```

## Applying a Protocol to All Ports

Setting the protocol on the `ESDevice` applies a clone to each port:

```csharp
// All ports use the same protocol settings
es.Protocol = new DefaultSerialProtocol()
{
    TerminatingCharacters = "\r\n",
    Encoding = System.Text.Encoding.UTF8,
};
```

Each port gets its own `Clone()` of the protocol, so changing one port's protocol later does not affect the others.

## Connection Monitoring

Monitor port connection status:

```csharp
es.DeviceStatusChangedEvent += (device, property, newValue) =>
{
    Console.WriteLine($"Device {property}: {newValue}");
};
```

## Supported ES Devices

| Device | Ports | Serial Type | Description |
|--------|-------|------------|-------------|
| ES-246 | 1 | RS-232 | 1-port Ethernet to Serial |
| ES-257 | 2 | RS-232 | 2-port Ethernet to Serial |
| ES-313 | 1 | RS-422/485 | Industrial 1-port |
| ES-320 | 2 | RS-422/485 | Industrial 2-port |
| ES-346 | 1 | RS-232 | Enhanced 1-port |
| ES-357 | 2 | RS-232 | Enhanced 2-port |
| ES-413 | 1 | RS-422/485 | PoE 1-port |
| ES-420 | 2 | RS-422/485 | PoE 2-port |
| ES-446 | 1 | RS-232 | PoE 1-port |
| ES-457 | 2 | RS-232 | PoE 2-port |
| ES-511 | 1 | RS-232 | Compact 1-port |
| ES-522 | 2 | RS-232 | Compact 2-port |
| ES-551 | 1 | RS-422/485 | Compact industrial 1-port |
| ES-571 | 2 | RS-422/485 | Compact industrial 2-port |
| ES-701 | 1 | RS-232 | Wireless 1-port |
| ES-279 | 2 | RS-232 | Dual isolated |
| ES-842 | 4 | RS-232 | 4-port |

## Tips

- **Timeouts**: The default receive timeout is 2000 ms. Increase it for slow serial devices using `TCPConnection.Timeout`.
- **Threading**: `Receive()` is a blocking call. For applications with a UI or multiple ports, receive on a background thread.
- **Labels**: Use `port.Label` to identify ports during debugging — the label appears in `ToString()` output.
- **ES vs ED**: ES devices provide raw serial ports — they do **not** have `Inputs`/`Outputs` collections. For remote IO, use an ED-series device.

# Brainboxes.IO Library

Main library for communicating with Brainboxes devices.

## Structure

```
Connection/     # TCP and serial connection classes
Device/         # Device implementations (ED, ES, BB-400)
Protocol/       # ASCII and Modbus protocol implementations
Properties/     # Assembly info
```

## Key Classes

- `EDDevice` - Ethernet Remote IO base class
- `ESDevice` - Ethernet to Serial base class
- `BB400` - Industrial Raspberry Pi
- `TCPConnection` - TCP/IP connection handler
- `SerialConnection` - Serial/COM port connection handler
- `ASCIIProtocol` / `ModbusProtocol` - Protocol implementations
- `IConnection` - Connection interface (implemented by TCP and Serial)
- `IIOProtocol` - Protocol interface for IO operations

## Build

```bash
dotnet build Brainboxes.IO.csproj
```

## NuGet Package

Package ID: `Brainboxes.IO`
License: Unlicense (SPDX identifier in csproj)

## Architecture

### Connection Layer
- `IConnection` defines `Connect()`, `Disconnect()`, `Send()`, `Receive()`
- `TCPConnection` uses raw TCP sockets
- `SerialConnection` uses `System.IO.Ports.SerialPort`
- Connection factory: `Connection.Create(ipOrComPort)`

### Protocol Layer
- `ASCIIProtocol` - Human-readable command format (e.g., `@01`, `#0100FF`)
- `ModbusProtocol` - Standard Modbus TCP protocol
- Protocols handle command formatting and response parsing

### Device Layer
- `EDDevice` combines connection + protocol for Remote IO devices
- Device-specific subclasses (ED527, ED588, etc.) in `Device/` folder
- `ESDevice` for Ethernet-to-Serial converters

## Important Conventions

- Always use `CultureInfo.InvariantCulture` for numeric formatting (ASCII protocol uses `.` decimal separator)
- Wrap device usage in `using` statements for proper cleanup
- Set `Connection.Timeout` before operations that may block

## Notes

- .NET Framework 4.8 build requires Windows and PFX signing key
- `System.IO.Ports` package used for non-net48 targets

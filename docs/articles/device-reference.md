# Device Reference

Complete reference of all Brainboxes devices supported by the Brainboxes.IO library.

## ED-Series (Ethernet Remote IO)

ED-series devices provide digital and/or analog IO lines accessible over Ethernet. All ED devices connect via `TCPConnection` and support both ASCII (port 9500) and Modbus TCP (port 502) protocols.

### Digital IO Devices

| Class | Product | Digital Inputs | Digital Outputs | Description |
|-------|---------|---------------|----------------|-------------|
| `ED588` | [ED-588](http://www.brainboxes.com/product/ed-588) | 8 | 8 | 8 DI + 8 DO + Serial Gateway |
| `ED516` | [ED-516](http://www.brainboxes.com/product/ed-516) | 16 | 0 | 16 Digital Inputs + Serial Gateway |
| `ED527` | [ED-527](http://www.brainboxes.com/product/ed-527) | 0 | 16 | 16 Digital Outputs + Serial Gateway |
| `ED538` | [ED-538](http://www.brainboxes.com/product/ed-538) | 8 | 4 (relay) | 8 DI + 4 Relays + Serial Gateway |
| `ED004` | [ED-004](http://www.brainboxes.com/product/ed-004) | 4 | 4 | 4 DIO + RS-232 |
| `ED204` | [ED-204](http://www.brainboxes.com/product/ed-204) | 4 | 4 | 4 DIO + RS-232 |
| `ED008` | [ED-008](http://www.brainboxes.com/product/ed-008) | 8 | 8 | 8 Digital IO Ports |
| `ED038` | [ED-038](http://www.brainboxes.com/product/ed-038) | 3 | 3 (relay) | 3 Relays + 3 Digital Inputs |

### Analog IO Devices

| Class | Product | Analog Inputs | Analog Outputs | Sensor Type | Description |
|-------|---------|--------------|----------------|-------------|-------------|
| `ED549` | ED-549 | 8 | 0 | Voltage/Current/Temp | 8 Analog Inputs + Serial Gateway |
| `ED560` | ED-560 | 0 | 4 | Voltage/Current | 4 Analog Outputs + Serial Gateway |
| `ED582` | ED-582 | 4 | 0 | RTD (PT100/PT1000) | 4 RTD Temperature Inputs + Serial Gateway |
| `ED593` | ED-593 | 8 | 0 | Thermocouple | 8 Thermocouple Inputs + Serial Gateway |

### Edge Controller

| Class | Product | Digital Inputs | Digital Outputs | Description |
|-------|---------|---------------|----------------|-------------|
| `BB400` | [BB-400](http://www.brainboxes.com/product/bb-400) | 8 | 8 | Industrial Raspberry Pi Edge Controller |

The BB-400 has the same IO as the ED-588 but also runs Linux, allowing code to execute locally on the device.

## ES-Series (Ethernet to Serial)

ES-series devices convert Ethernet TCP/IP connections to physical serial ports. Each port connects via its own TCP connection (ports 9001, 9002, etc.).

### RS-232 Devices

| Class | Product | Ports | Description |
|-------|---------|-------|-------------|
| `ES246` | ES-246 | 1 | 1-port RS-232 |
| `ES257` | ES-257 | 2 | 2-port RS-232 |
| `ES346` | ES-346 | 1 | Enhanced 1-port RS-232 |
| `ES357` | ES-357 | 2 | Enhanced 2-port RS-232 |
| `ES446` | ES-446 | 1 | PoE 1-port RS-232 |
| `ES457` | ES-457 | 2 | PoE 2-port RS-232 |
| `ES511` | ES-511 | 1 | Compact 1-port RS-232 |
| `ES522` | ES-522 | 2 | Compact 2-port RS-232 |
| `ES701` | ES-701 | 1 | Wireless 1-port RS-232 |
| `ES279` | ES-279 | 2 | Dual isolated RS-232 |
| `ES842` | ES-842 | 4 | 4-port RS-232 |

### RS-422/485 Devices

| Class | Product | Ports | Description |
|-------|---------|-------|-------------|
| `ES313` | ES-313 | 1 | Industrial 1-port RS-422/485 |
| `ES320` | ES-320 | 2 | Industrial 2-port RS-422/485 |
| `ES413` | ES-413 | 1 | PoE 1-port RS-422/485 |
| `ES420` | ES-420 | 2 | PoE 2-port RS-422/485 |
| `ES551` | ES-551 | 1 | Compact 1-port RS-422/485 |
| `ES571` | ES-571 | 2 | Compact 2-port RS-422/485 |

## Common Patterns

### Factory Method (recommended)

The factory method auto-detects the device type by connecting and reading the device's XML configuration:

```csharp
// ED-series: returns the correct subclass (ED588, ED516, etc.)
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    Console.WriteLine($"Found: {ed.GetType().Name}");
}

// ES-series: returns the correct subclass (ES246, ES257, etc.)
ESDevice es = ESDevice.Create("192.168.0.100");
```

### Direct Construction

When you know the device type, construct it directly to avoid the auto-detection network call:

```csharp
// ED-series with auto-created connection
var connection = new TCPConnection("192.168.0.100");
var ed = new ED588(connection);
ed.Connect();

// ES-series (constructor creates connections for all ports)
var es = new ES246("192.168.0.100");
es.Connect();
```

### Modbus TCP

To use Modbus TCP instead of ASCII, connect on port 502:

```csharp
var connection = new TCPConnection("192.168.0.100", TCPConnection.DEFAULT_MODBUSTCP_PORT);
var ed = new ED588(connection, new ModbusTCPProtocol());
ed.Connect();
```

## Default TCP Ports

| Port | Purpose |
|------|---------|
| 9500 | ASCII protocol (ED-series default) |
| 502 | Modbus TCP protocol |
| 9001 | ES-series serial port 1 |
| 9002 | ES-series serial port 2 |
| 9003+ | ES-series serial ports 3+ |
| 80 | Device web configuration page (used by factory methods) |

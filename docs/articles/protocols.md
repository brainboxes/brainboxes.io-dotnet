# Protocols Guide

Brainboxes devices communicate using one of two protocols: **ASCII** (human-readable text) or **Modbus TCP** (industrial binary standard). The protocol is determined by the TCP port number.

## Protocol Comparison

| Feature | ASCII Protocol | Modbus TCP Protocol |
|---------|---------------|-------------------|
| TCP Port | 9500 | 502 |
| Format | Human-readable text | Binary |
| Terminator | Carriage return (`\r`) | MBAP header (7 bytes) |
| Response prefix | `>` (success), `?` (error) | Function code in header |
| Factory reset | Supported | Not supported |
| Device restart | Supported | Not supported |
| Configuration | Full read/write | Read only |
| Interoperability | Brainboxes proprietary | Industry standard |

## ASCII Protocol

The default protocol. Commands are plain-text strings terminated by carriage return. The protocol handles encoding and decoding automatically.

### How it Works

1. Your code calls `ed.Inputs[0].Value`
2. The `ASCIIProtocol` encodes this as `@01\r` (read all digital inputs for device address 01)
3. The device responds with `>0F\r` (a hex bitmask of input states)
4. The protocol decodes the response and updates all input line values

### Command Format

```
@{address}{command}{data}\r
```

- **Address**: Two hex digits (default `01`)
- **Command**: Operation code
- **Data**: Optional parameters

### Response Format

```
>{data}\r     (success)
?{address}\r  (error)
```

### Using ASCII Protocol Directly

In most cases the `EDDevice` handles protocol calls for you. For advanced use, you can send raw commands:

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    // Send a raw ASCII command and read the response
    string response = ed.SendCommand("@01");
    Console.WriteLine($"Response: {response}");
}
```

### Protocol Configuration

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    ASCIIProtocol protocol = (ASCIIProtocol)ed.Protocol;

    // Read device configuration
    string config = protocol.GetDeviceConfiguration(ed.Connection.Stream);

    // Set analog data format
    protocol.AnalogFormat = AnalogDataFormat.Engineering;

    // Set temperature unit
    protocol.TempUnit = TemperatureUnit.Celsius;
}
```

## Modbus TCP Protocol

An industry-standard binary protocol supported by many SCADA and PLC systems. Use this when integrating Brainboxes devices into existing Modbus infrastructure.

### How it Works

1. Commands are binary frames with a 7-byte MBAP (Modbus Application Protocol) header
2. The header contains: transaction ID, protocol ID, length, and unit ID
3. Standard Modbus function codes are used (read coils, write registers, etc.)
4. Multi-byte values use big-endian byte order

### Connecting via Modbus

```csharp
using Brainboxes.IO;

var connection = new TCPConnection("192.168.0.100", TCPConnection.DEFAULT_MODBUSTCP_PORT);
var device = new ED588(connection);
device.Connect();

// IO operations work the same way
int input = device.Inputs[0].Value;
device.Outputs[0].Value = 1;

device.Disconnect();
```

### Limitations

Some operations are not available over Modbus TCP:

- `FactoryReset()` — throws `NotImplementedException`
- `Restart()` — throws `NotImplementedException`
- `GetDeviceConfiguration()` — throws `NotImplementedException`
- `SetDeviceConfiguration()` — throws `NotImplementedException`
- `SendCommand()` (raw ASCII) — not applicable

Use the ASCII protocol for these operations.

## Choosing a Protocol

**Use ASCII (port 9500) when:**
- You are building a dedicated Brainboxes application
- You need device configuration (factory reset, restart)
- You want human-readable debugging (commands/responses are plain text)
- You are prototyping or learning the API

**Use Modbus TCP (port 502) when:**
- You are integrating into an existing Modbus SCADA/PLC system
- You need interoperability with non-Brainboxes Modbus devices
- Your infrastructure already speaks Modbus

## Serial Protocols (ES-series)

ES-series Ethernet-to-Serial devices use a different protocol model. Each serial port has an `ISerialProtocol` that controls how data is encoded and framed for the connected serial device.

The `DefaultSerialProtocol` provides:

- Configurable character encoding (default: ASCII)
- Configurable terminating characters (default: `\r`)
- Automatic terminator appending on send and stripping on receive

```csharp
ESDevice es = new ES246("192.168.0.100");
es.Connect();

// Customise the serial protocol
es.Ports[0].Protocol = new DefaultSerialProtocol()
{
    TerminatingCharacters = "\r\n",
    Encoding = System.Text.Encoding.UTF8,
};

es.Ports[0].Send("Hello");
string response = es.Ports[0].Receive();
```

For more complex serial devices, implement the `ISerialProtocol` interface to create a custom protocol class.

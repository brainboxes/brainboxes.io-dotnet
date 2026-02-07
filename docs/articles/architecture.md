# Architecture Overview

The Brainboxes.IO library is organised into three layers: **Connection**, **Protocol**, and **Device**. Each layer has a clear responsibility, and the layers compose together to provide the public API.

## Layer Diagram

```
┌─────────────────────────────────────────────────────┐
│                    Device Layer                      │
│  EDDevice (Remote IO)     ESDevice (Serial)          │
│  Inputs[] / Outputs[]     Ports[] / BBSerialPort     │
│  IOLine / Events          Send / Receive             │
├─────────────────────────────────────────────────────┤
│                   Protocol Layer                     │
│  ASCIIProtocol            ModbusTCPProtocol          │
│  (port 9500, text)        (port 502, binary)         │
│  DefaultSerialProtocol    (ES-series encoding)       │
├─────────────────────────────────────────────────────┤
│                  Connection Layer                     │
│  TCPConnection            SerialConnection            │
│  (Ethernet)               (COM port)                 │
│  IsConnected / IsAvailable / Stream                  │
├─────────────────────────────────────────────────────┤
│                   Stream Layer                        │
│  Socket → NetworkStream → BBNetworkStream → BBStream │
│  (thread-safe read/write locks)                      │
└─────────────────────────────────────────────────────┘
```

## Connection Layer

`IConnection` defines the contract: `Connect()`, `Disconnect()`, `IsConnected`, `IsAvailable`, and a `Stream` property for raw I/O.

Two implementations:

| Class | Transport | Typical Use |
|-------|-----------|------------|
| `TCPConnection` | Ethernet TCP/IP | All ED and ES devices over the network |
| `SerialConnection` | COM port | Direct USB/serial connection via Boost.IO virtual COM port |

### Connection State Polling

The `Connection` base class uses a `System.Threading.Timer` to periodically refresh `IsConnected` and `IsAvailable` values. These are cached to avoid excessive network probes:

- **IsConnected** — checked via a zero-byte `Socket.Send()` (non-blocking). Cache timeout is 0 ms by default (very fast check).
- **IsAvailable** — checked via ICMP ping. Cache timeout defaults to the connection timeout (typically 2000 ms).

The polling timer uses **one-shot mode** (period = `Timeout.Infinite`) and re-arms itself after each callback completes. This prevents timer overlap if a callback takes longer than expected.

### Factory Method

`Connection.Create(ip, port, timeout)` auto-detects the transport type. IP addresses starting with `COM` create a `SerialConnection`; all others create a `TCPConnection`.

## Protocol Layer

Protocols encode commands into bytes and decode responses. The protocol is determined by the TCP port:

| Protocol | TCP Port | Format | Use |
|----------|----------|--------|-----|
| `ASCIIProtocol` | 9500 | Human-readable text, CR-terminated | Default for ED-series |
| `ModbusTCPProtocol` | 502 | Binary with 7-byte MBAP header | Standard industrial protocol |
| `DefaultSerialProtocol` | N/A | Configurable encoding and terminators | ES-series serial ports |

### ASCII Protocol

Commands are plain-text strings like `@01` (read all inputs) or `#011F` (set outputs). Responses start with `>` (success) or `?` (error) and are CR-terminated.

### Modbus TCP Protocol

Binary frames with a 7-byte MBAP (Modbus Application Protocol) header. Supports standard function codes for reading/writing coils and registers. Some advanced operations (e.g. `FactoryReset`, `Restart`) are not available over Modbus.

### Protocol Interface Hierarchy

```
IProtocol
├── IIOProtocol        (ED-series: GetAllDigitalLineStates, SetDigitalOutputLineState, etc.)
└── ISerialProtocol    (ES-series: Send, Receive, encoding, terminators)
```

## Device Layer

### EDDevice (Remote IO)

`EDDevice` is the primary class for ED-series Ethernet Remote IO devices. It wraps a connection and protocol, and exposes IO lines through indexed collections:

- `ed.Inputs[n]` — digital/analog input lines (read-only)
- `ed.Outputs[n]` — digital/analog output lines (read/write)
- `ed.IOLines[n]` — all lines by logical number

Each `IOLine` has a `Value` property (digital: 0/1, analog: `AValue` as double). Values are cached for `IOLineCacheTimeout` milliseconds (default 10 ms). Reading any line refreshes all lines of the same type in one network call.

**Factory method**: `EDDevice.Create(ip)` connects to the device, reads its XML configuration page, and returns the correct subclass (e.g. `ED588`, `ED516`, `ED549`).

### ESDevice (Ethernet to Serial)

`ESDevice` represents an Ethernet-to-Serial converter. Each physical serial port is a `BBSerialPort` in the `Ports` collection. Each port has its own `TCPConnection` (port 9001, 9002, etc.) and `ISerialProtocol`.

### Event System

ED-series devices use a polling timer (separate from the connection timer) to detect IO line changes:

1. Timer fires → reads all line states → compares with cached values
2. If a change is detected, fires events: `IOLineChanged`, `IOLineRisingEdge`, `IOLineFallingEdge`
3. For high-frequency changes within a single polling interval, the latch status is checked to detect `Latched` state

Events can be subscribed at the device level (`ed.IOLineChanged`) or per-line (`ed.Inputs[0].IOLineRisingEdge`).

### Device Status Events

Both ED and ES devices fire `DeviceStatusChangedEvent` when `IsConnected` or `IsAvailable` changes. This enables auto-reconnect patterns:

```csharp
ed.DeviceStatusChangedEvent += (device, property, newValue) =>
{
    if (!device.IsConnected && device.IsAvailable)
    {
        device.Connect(); // auto-reconnect
    }
};
```

## Stream Hierarchy

The library wraps .NET streams with thread-safe wrappers:

```
Socket
 └── NetworkStream
      └── BBNetworkStream (sets ownsSocket = false for clean disconnect)
           └── BBStream (adds streamReadLock / streamWriteLock)
```

`BBStream` ensures that concurrent reads and writes are serialised, which is critical since the connection polling timer and user code may access the stream simultaneously.

## Threading Model

| Component | Timer | Purpose |
|-----------|-------|---------|
| Connection polling | `_pollingTimer` | Refresh `IsConnected` / `IsAvailable` cache |
| IO line polling | `_ioLineCacheTimer` | Detect IO state changes and fire events |

Both timers use one-shot mode to prevent overlap. All IO operations acquire locks on the `BBStream` read/write lock objects to ensure thread safety.

## Class Hierarchy

```
Device<TConnection, TProtocol>
├── EDDevice : Device<IConnection, IIOProtocol>
│   ├── BB400
│   ├── ED588, ED538, ED516, ED527
│   ├── ED549, ED560, ED582, ED593
│   ├── ED004, ED204, ED008, ED038
│   └── (generic EDDevice via factory)
└── ESDevice : Device<IConnection, ISerialProtocol>
    ├── ES246, ES257, ES320, ES357
    ├── ES511, ES522, ES551, ES571
    ├── ES446, ES457, ES413, ES420
    └── ES279, ES346, ES701, ES842
```

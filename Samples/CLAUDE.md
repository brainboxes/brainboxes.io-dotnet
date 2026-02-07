# Brainboxes.IO Samples

Example projects demonstrating how to use the Brainboxes.IO library.

## Structure

```
C-Sharp/
    Ethernet-remote-io/     # ED device examples
    Ethernet-to-serial/     # ES device examples
    General/                # General device examples
    ...                     # Standalone samples
VB/                         # VB.NET examples
```

## Sample Categories

### Ethernet Remote IO (ED devices)
- `SimpleControlOutputs` - Basic digital output control
- `SimpleMonitorInputs` - Basic digital input monitoring
- `InputCounter` - Count input state changes
- `DemoKit` - Interactive demo applications (WinForms)
- `DemoKit-Analog-Core` - Analog I/O demo (.NET Core)
### Ethernet to Serial (ES devices)
- `ConsoleApplication` - Basic serial communication
- `NetCore-Ethernet-Serial-Console-App` - .NET Core serial example

### Integration Examples
- `Remote-IO-to-Database` - Database logging
- `ListenForInputChange` - Event-driven input monitoring

### VB.NET
- `Ethernet-to-serial.ConsoleApplication` - VB.NET serial example

## Running Samples

1. Update device IP in the sample's config or source code
2. Build and run:

```bash
cd Samples/C-Sharp/Ethernet-remote-io/Brainboxes.IO.Samples.CSharp.ED.SimpleControlOutputs
dotnet run
```

## Common Patterns

### Basic Device Usage
```csharp
using (var ed = EDDevice.Create("YOUR_DEVICE_IP"))
{
    ed.Connect();
    ed.Protocol.SetDigitalOutputLineState(0, 1); // Set output 0 high
    var inputs = ed.Protocol.GetAllDigitalInputLineStates();
    ed.Disconnect();
}
```

### Event-Driven Monitoring
```csharp
ed.IOLineStateChanged += (sender, e) => {
    Console.WriteLine($"Line {e.Line} changed to {e.State}");
};
```

## Notes

- Samples reference Brainboxes.IO via project reference
- Some samples target .NET Framework (Windows only)
- Newer samples target .NET Core/.NET 5+ for cross-platform

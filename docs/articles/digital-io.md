# Working with Digital IO

This guide covers reading digital inputs, controlling digital outputs, monitoring for changes with events, and using latches and counters on ED-series devices.

## Reading Digital Inputs

Digital inputs read the state of external signals: **HIGH** (1) or **LOW** (0).

```csharp
using Brainboxes.IO;

using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    // Read a single input
    int state = ed.Inputs[0].Value;
    Console.WriteLine($"Input 0 is {(state == 1 ? "HIGH" : "LOW")}");

    // Read all inputs
    for (int i = 0; i < ed.Inputs.Count; i++)
    {
        Console.WriteLine($"Input {i}: {ed.Inputs[i].Value}");
    }
}
```

### Value Caching

IO values are cached for `IOLineCacheTimeout` milliseconds (default 10 ms). Reading any single input refreshes **all** digital inputs in one network call. This means iterating through all inputs is efficient — only one command is sent to the device.

You can adjust the cache timeout:

```csharp
// Faster polling (more network traffic)
ed.IOLineCacheTimeout = 1;

// Slower polling (less network traffic)
ed.IOLineCacheTimeout = 100;
```

## Controlling Digital Outputs

Digital outputs control relays, LEDs, motors, or other actuators: **CLOSED** (1) or **OPEN** (0).

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    // Set an output HIGH (closed)
    ed.Outputs[0].Value = 1;

    // Set an output LOW (open)
    ed.Outputs[0].Value = 0;

    // Toggle an output (returns the new value)
    int newState = ed.Outputs[0].Toggle();
    Console.WriteLine($"Output 0 is now {newState}");
}
```

### Write Suppression

If you set an output to its current cached value within the cache window, no command is sent to the device. This makes it safe to write values in a loop without generating unnecessary network traffic.

### Setting All Outputs at Once

You can set all outputs simultaneously using a raw command:

```csharp
// Set all outputs using a bitmask via SendCommand
ed.SendCommand("@01001F"); // device-specific ASCII command
```

## Event-Driven Monitoring

Instead of polling in a loop, you can subscribe to events that fire when IO lines change state. Events are driven by the device's internal polling timer.

### Per-Line Events

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    // Rising edge: input went from LOW (0) to HIGH (1)
    ed.Inputs[0].IOLineRisingEdge += (line, device, changeType) =>
    {
        Console.WriteLine($"Input {line.IONumber} went HIGH");
    };

    // Falling edge: input went from HIGH (1) to LOW (0)
    ed.Inputs[0].IOLineFallingEdge += (line, device, changeType) =>
    {
        Console.WriteLine($"Input {line.IONumber} went LOW");
    };

    // Any change (rising, falling, or latched)
    ed.Inputs[0].IOLineChanged += (line, device, changeType) =>
    {
        Console.WriteLine($"Input {line.IONumber}: {changeType}");
    };

    Console.WriteLine("Monitoring input 0... press any key to stop.");
    Console.ReadKey();
}
```

### Device-Level Events

To monitor all IO lines at once, subscribe at the device level:

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    ed.IOLineChanged += (line, device, changeType) =>
    {
        Console.WriteLine($"{line} changed: {changeType}");
    };

    ed.IOLineRisingEdge += (line, device, changeType) =>
    {
        Console.WriteLine($"{line} rising edge");
    };

    Console.ReadKey();
}
```

### Change Types

The `IOChangeTypes` enum describes what happened:

| Value | Meaning |
|-------|---------|
| `NoChange` | No state change detected |
| `RisingEdge` | LOW → HIGH (input) or OPEN → CLOSED (output) |
| `FallingEdge` | HIGH → LOW (input) or CLOSED → OPEN (output) |
| `Latched` | Both rising and falling edges occurred within one polling interval |
| `Undefined` | Initial state (device just connected, no previous value) |

The `Latched` state catches high-frequency changes that happen faster than the polling interval. For example, if an input goes HIGH then LOW between two polls, neither a simple rising nor falling edge would be detected, but the latch registers capture both transitions.

## Latch Status

Latch registers on the device record whether a transition has occurred since the last latch read. This is useful for detecting pulses that are shorter than the polling interval.

```csharp
// Check if input 0 has gone HIGH since last check
bool wentHigh = ed.Inputs[0].HighLatchedStatus;

// Check if input 0 has gone LOW since last check
bool wentLow = ed.Inputs[0].LowLatchedStatus;
```

Latch values are also cached with the same `IOLineCacheTimeout` as regular values.

## Counters

Digital input lines have hardware counters that track the number of transitions:

```csharp
// Read the current count
int count = ed.Inputs[0].Count;
Console.WriteLine($"Input 0 has transitioned {count} times");

// Reset the counter
ed.Inputs[0].ClearCount();
```

### Counter Events

You can subscribe to count change events:

```csharp
ed.Inputs[0].IOLineCount += (line, device, changeType) =>
{
    Console.WriteLine($"Input {line.IONumber} count changed: {line.Count}");
};
```

## Device Status Monitoring

Monitor the device connection and availability:

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    ed.DeviceStatusChangedEvent += (device, property, newValue) =>
    {
        Console.WriteLine($"{property} changed to {newValue}");

        if (!device.IsConnected && device.IsAvailable)
        {
            Console.WriteLine("Reconnecting...");
            device.Connect();
        }
    };

    Console.ReadKey();
}
```

## Labels

You can assign labels to IO lines for easier debugging:

```csharp
ed.Inputs[0].Label = "Door sensor";
ed.Outputs[0].Label = "Alarm LED";

// Labels appear in ToString() and Describe()
Console.WriteLine(ed.Inputs[0]);           // "Door sensor DIn00 (line 00)"
Console.WriteLine(ed.Inputs[0].Describe()); // Full diagnostic output
```

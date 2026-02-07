# Working with Analog IO

This guide covers analog inputs and outputs on ED-series devices that support analog IO, including data formats, temperature measurement, and delta/target events.

## Analog Devices

Not all ED-series devices have analog IO. The following devices support analog lines:

| Device | Analog Lines | Type |
|--------|-------------|------|
| ED-549 | 8 analog inputs | Voltage, current, or temperature |
| ED-560 | 4 analog outputs | Voltage or current |
| ED-582 | 4 RTD inputs | Temperature (PT100/PT1000) |
| ED-593 | 8 thermocouple inputs | Temperature |

## Reading Analog Inputs

Use the `AValue` property to read analog values:

```csharp
using Brainboxes.IO;

using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    // Read a single analog input
    double value = ed.Inputs[0].AValue;
    Console.WriteLine($"Analog input 0: {value}");

    // Read all analog inputs
    for (int i = 0; i < ed.Inputs.Count; i++)
    {
        Console.WriteLine($"Analog input {i}: {ed.Inputs[i].AValue}");
    }
}
```

Like digital values, analog values are cached for `IOLineCacheTimeout` milliseconds (default 10 ms) and reading any line refreshes all analog lines in one network call.

## Setting Analog Outputs

On the ED-560 (4 analog outputs):

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    // Set analog output 0 to 5.0 (units depend on AnalogDataFormat)
    ed.Outputs[0].AValue = 5.0;
}
```

## Data Formats

The `AnalogDataFormat` enum controls how analog values are interpreted:

| Format | Description |
|--------|------------|
| `Engineering` | Real-world units (volts, milliamps, degrees) |
| `PercentFSR` | Percentage of full-scale range (0.0 – 100.0) |
| `TwosComplement` | Raw ADC value in two's complement |
| `Hexadecimal` | Raw ADC value in hexadecimal |

Set the data format through the protocol:

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    ASCIIProtocol protocol = (ASCIIProtocol)ed.Protocol;

    // Read in engineering units (default)
    protocol.AnalogFormat = AnalogDataFormat.Engineering;
    double voltage = ed.Inputs[0].AValue; // e.g. 3.245 (volts)

    // Read as percentage of full-scale range
    protocol.AnalogFormat = AnalogDataFormat.PercentFSR;
    double percent = ed.Inputs[0].AValue; // e.g. 32.45 (%)
}
```

## Temperature Measurement

The ED-582 (RTD) and ED-593 (thermocouple) devices measure temperature. The `TemperatureUnit` enum controls the unit:

| Unit | Description |
|------|------------|
| `Celsius` | Degrees Celsius |
| `Fahrenheit` | Degrees Fahrenheit |

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    ASCIIProtocol protocol = (ASCIIProtocol)ed.Protocol;

    // Read temperature in Celsius
    protocol.TempUnit = TemperatureUnit.Celsius;
    double tempC = ed.Inputs[0].AValue;
    Console.WriteLine($"Temperature: {tempC} °C");

    // Read temperature in Fahrenheit
    protocol.TempUnit = TemperatureUnit.Fahrenheit;
    double tempF = ed.Inputs[0].AValue;
    Console.WriteLine($"Temperature: {tempF} °F");
}
```

## Analog Events

Analog lines support three types of events for change notification. Unlike digital events, analog events require explicit subscription with threshold parameters.

### Delta Events

Fire when the value changes by more than a specified amount:

```csharp
using (EDDevice ed = EDDevice.Create("192.168.0.100"))
{
    AIOLineChangedEventHandler handler = (line, device, changeType, previousValue, currentValue) =>
    {
        Console.WriteLine($"Analog {line.IONumber} changed from {previousValue} to {currentValue}");
    };

    // Notify when value changes by more than 0.5
    ed.Inputs[0].SubscribeToDeltaEvent(ref handler, delta: 0.5);

    Console.ReadKey();

    ed.Inputs[0].UnsubscribeToDeltaEvent(ref handler);
}
```

### Target Events

Fire when the value crosses a specified threshold:

```csharp
AIOLineChangedEventHandler handler = (line, device, changeType, previousValue, currentValue) =>
{
    if (changeType == AIOChangeTypes.Above)
        Console.WriteLine($"Value went above target: {currentValue}");
    else if (changeType == AIOChangeTypes.Below)
        Console.WriteLine($"Value went below target: {currentValue}");
};

// Notify when value crosses 3.3V
ed.Inputs[0].SubscribeToTargetEvent(ref handler, target: 3.3);
```

### Target Range Events

Fire when the value enters or exits a range around a target:

```csharp
AIOLineChangedEventHandler handler = (line, device, changeType, previousValue, currentValue) =>
{
    if (changeType == AIOChangeTypes.Enter)
        Console.WriteLine($"Value entered range: {currentValue}");
    else if (changeType == AIOChangeTypes.Exit)
        Console.WriteLine($"Value left range: {currentValue}");
};

// Notify when value enters/exits the range 3.3 ± 0.2
ed.Inputs[0].SubscribeToTargetRangeEvent(ref handler, target: 3.3, delta: 0.2);
```

### AIOChangeTypes

| Value | Event Type | Meaning |
|-------|-----------|---------|
| `Delta` | Delta | Value changed by more than the delta threshold |
| `Above` | Target | Value crossed above the target |
| `Below` | Target | Value crossed below the target |
| `Enter` | Target Range | Value entered the target ± delta range |
| `Exit` | Target Range | Value left the target ± delta range |

## Counter Modes

For devices with digital inputs alongside analog, you can configure how counters operate:

| Mode | Description |
|------|------------|
| `CounterMode16Bits` | 16-bit counter (0 – 65,535) |
| `CounterMode32Bits` | 32-bit counter (0 – 4,294,967,295) |

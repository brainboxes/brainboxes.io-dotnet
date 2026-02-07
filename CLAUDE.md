# Brainboxes.IO .NET API

Official .NET API for communication with Brainboxes Ethernet-attached data acquisition and control devices.

## Project Structure

```
Brainboxes.IO/          # Main library (NuGet package)
Brainboxes.IO.Tests/    # Unit and integration tests
Samples/                # Example projects (C# and VB.NET)
docs/                   # DocFX documentation
```

## Build Commands

```bash
# Build the solution
dotnet build Brainboxes.IO.sln

# Build release
dotnet build Brainboxes.IO.sln -c Release

# Run tests (requires Brainboxes hardware or tests will skip)
dotnet test Brainboxes.IO.Tests/Brainboxes.IO.Tests.csproj

# Run tests excluding long-running ones
dotnet test --filter "TestCategory!=LongRunning"

# Generate documentation
cd docs && dotnet docfx docfx.json
```

## Target Frameworks

- .NET 10
- .NET Standard 2.0 / 2.1
- .NET Framework 4.8 (Windows only, requires PFX signing)

## Supported Devices

- **BB-400**: Industrial Raspberry Pi
- **ED series**: Ethernet Remote IO (ED-588, ED-516, ED-527, ED-549, ED-560, ED-582)
- **ES series**: Ethernet to Serial (ES-246, ES-257)

## Key Namespaces

- `Brainboxes.IO` - Core device classes and interfaces
- `Brainboxes.IO.Connection` - TCP and serial connection handling
- `Brainboxes.IO.Protocol` - ASCII and Modbus protocols
- `Brainboxes.IO.Device` - Device-specific implementations

## Architecture Notes

- Devices communicate via `IConnection` (TCP or Serial)
- Protocols (`ASCIIProtocol`, `ModbusProtocol`) define command/response formats
- `EDDevice` wraps connection + protocol for Ethernet Remote IO
- `ESDevice` wraps connection + protocol for Ethernet to Serial
- Connection pooling in tests via `ConnectionPool` class reads from `app.config`

## Coding Conventions

- Use `CultureInfo.InvariantCulture` for all numeric formatting/parsing (ASCII protocol)
- Prefer `using` statements for device connections
- Tests that require hardware should skip gracefully when devices unavailable

## License

Unlicense (public domain)

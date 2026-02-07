# Brainboxes.IO Tests

Unit and integration tests for the Brainboxes.IO library.

## Running Tests

```bash
# Run all tests
dotnet test Brainboxes.IO.Tests.csproj

# Run with verbose output
dotnet test Brainboxes.IO.Tests.csproj -v normal

# Run excluding long-running tests
dotnet test --filter "TestCategory!=LongRunning"

# Run specific test class
dotnet test --filter "FullyQualifiedName~ASCIITest"
```

## Configuration

Tests require device configuration in `app.config`. Key format:
- `{Model}IPAddress` for TCP (e.g., `ED588IPAddress`)
- `{Model}ComPort` for serial (e.g., `ED588ComPort`)
- Empty values are ignored
- Multiple devices: append numbers (e.g., `ED588IPAddress1`, `ED588IPAddress2`)

Example:
```xml
<add key="ED588IPAddress" value="192.168.127.254"/>
```

## Test Framework

- MSTest (Microsoft.NET.Test.Sdk)
- coverlet.collector for code coverage
- HdrHistogram for performance testing

## Key Files

- `ConnectionPool.cs` - Reads `app.config` and creates device connection pools
- `UnitTestBase.cs` - Base class with shared `ConnectionPool cp` instance
- `app.config` - Device IP/COM port configuration
- `test.runsettings` - MSTest configuration

## Test Categories

- `[TestCategory("LongRunning")]` - Tests that take several minutes (stress tests, repeated commands)
- Default tests should complete quickly when hardware is available

## Connection Pools

`ConnectionPool` provides dictionaries for different device categories:
- `ValidConnections` - All configured devices
- `EDConnections` - ED series devices
- `ESConnections` - ES series devices
- `DevicesWithDigitalIOConnections` - Devices with digital I/O
- `DevicesWithAnalogueInputsConnections` - Devices with analog inputs
- `DevicesWhichCanBeFactoryReset` - ED and BB devices (not eDAM)

## Notes

- Tests require physical Brainboxes hardware connected to the network
- Tests that cannot connect to devices will skip (empty foreach loops)
- .NET Framework 4.8 tests only run on Windows
- Use `RandomTestOrder()` extension to randomize test iteration order
- Use `AsASCIIProtocol()` / `AsModbusProtocol()` extensions to filter by protocol

## [1.8.0] - 2025-02-01
### Added
- .NET 10 support
- DocFX documentation with Docusaurus-compatible markdown output at https://docs.brainboxes.com/api/dotnet/Brainboxes.IO
### Changed
- License changed to Unlicense (public domain)

## [1.7.2] - 2024-12-13
### Added
- Build for .NET Standard 2.0
## [1.7.1] - 2024-12-05
### Added
- Build for .NET Standard 2.1
- Update package description
## [1.7.0] - 2024-05-13
### Fixed
- Fixed a bug where ED-549 devices with newer firmwares could not have any analogue input lines read when some analogue input lines were disabled.
## [1.6.7] - 2024-04-29
### Fixed
- Fixed rare race condition wherein a disconnect at a precise time could cause an unhandled exception.
## [1.6.6] 2022-02-16
### Updated
- Package meta data was updated
## [1.6.5] 2022-02-09
### Fixed
- Fixed assembly file version
## [1.6.0.0] 2021-10-22
### Added
- Support for ED-582
- Feature to set the data format for analog devices
## [1.5.1.1] 2021-09-24
### Fixed
- Added lock to socket connection background check function
## [1.5.1.0] 2020-03-02
### Updated
- Converted Changelog to follow changelog format: https://keepachangelog.com/en/1.0.0/
### Fixed
- Parse functions should be CultureInvariant
## [1.5.0.9] 2019-11-10
### Fixed
- isConnected would report true when really false
- decimal conversion should always be CultureInvariant
### Added
- support for netstandard2.1, dot net core 3.0, .net framework 4.8
## [1.5.0.7] 2019-03-16
### Added
- Industrial Ethernet to serial ES-5xx classes,
### Updated
- improved default serial protocol to use utf8 encoding instead of ASCII (ASCII is a superset of UTF-8 so still supported fully),
- changed default serial protocol terminating char to /n, linux line ending so give wider out of the box support for linux and windows
## [1.5.0.6] 2018-09-21
### Added
- BB-400 class http://www.brainboxes.com/product/bb-400
## [1.5.0.5] 2018-01-25
### Updated
- comments
## [1.5.0.4] 2017-12-14
### Updated
- updated functions names to be consistent throughout the API
## [1.5.0.3] 2017-12-12
- Updated functions names to be consistent throughout the API.
## [1.5.0.2] 2017-11-30
### Updated
- analog event handler to support a list of IOList objects
## [1.5.0.1] 2016-09-23
### Added
- Added support for .NET Core and .NET Standard
### Fixed
- bug with ED-549 event handling
## [1.5.0.0] 2016-09-15
### Added
- analog functionality including events for support for ED-549 (ASCII only) and ED-560 (ASCII and Modbus)
### Updated
- handling of isAvailble, now only checks for isAvailable if isConnected is false
- handling of isConnected, is now locked so multiple threads cannot access this so it does not send multiple pings
### Fixed
- bug with deviceStatusChangedEvent
## [1.4.0.1] 2016-07-06
### Added
- Universal windows platform dll's to allow compatibility with Windows IoT Core, Windows Store and XBOX
- First official release to coincide with RS DesignSpark Youtube video demo of Brainboxes API's running on Windows IoT Core and Raspberry Pi 3
## [1.2.4.0] 2015-10-28
### Fixed
- Bug with ED-527 event handling reported by Frank Guchelaar and fixed
## [1.2.1.0] 2015-09-20
### Updated
- Updated Ethernet to Serial devices
### Fixed
- many stability and performance fixes
## [1.2.0.0] 2015-04-21
### Added
- Initial Nuget release to include Ethernet to serial
## [1.1.12.0] 2015-09-07
### Updated
- Improved handling of IsAvailable for when a route to the device cannot be found
- Improved handling of IsConnected to determine if the connection is still open
## [1.1.11.0] 2015-08-28
### Fixed
- Corrected ISerialProtocol to implement IClonable so that assigning a protocol to a multiport product was handled correctly
## [1.1.9.0-beta] 2015-03-02
### Added
- Added IsAvailable property to Device and Connection and a call back to be notified on change of device status
- Added factory method to Connection (Connection.Create)
### Updated
- Changed constructor for TCPConnection to include ConnectionTimeout parameter
## [1.0.5.0] 2014-12-22
### Added
- Added counter functionality to API for IOLines and change handlers
## [1.0.4.0]
###Updated
- Changed handling of network and serial streams, by wrapping them into a BBStream to make them work in a consistent way, stream is now flushed before every sendcommand
### Fixed
- Fixed issue with Timeout not being correctly set
- Fixed FactoryRest and Restart command
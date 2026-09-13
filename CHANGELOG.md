# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Removed
- UniTask support (automatic detection via Version Defines)

## [1.0.0] - 2025-12-01

### Added
- Initial stable release
- Wake-on-LAN magic packet transmission functionality
- Synchronous API (`SendMagicPacket`) for simple use cases and editor tools
- Asynchronous API (`SendMagicPacketAsync`) using async/await pattern
- Flexible transmission modes:
  - MAC only: Global broadcast (255.255.255.255) - works for most cases
  - MAC + IP: Unicast to specific IP address
  - MAC + IP + Subnet: Calculated subnet broadcast address
- UniTask support via Version Defines (automatic detection)
- Flexible MAC address format parsing (colon, hyphen, or no separator)
- Comprehensive error handling with specific exception types
- XML documentation comments for IntelliSense support
- Support for Unity 2022.1+
- Cross-platform support (Windows, macOS, Linux, Android, iOS)
- Complete documentation in Korean and English
- Comprehensive README with usage examples, troubleshooting, and platform notes
- MIT License
- Comprehensive unit test suite covering:
  - MAC address parsing and validation
  - Magic packet structure verification
  - Broadcast address calculation
  - Error handling

### Technical Details
- .NET Standard 2.1 compatible
- UDP socket-based implementation
- Automatic broadcast address calculation when subnet mask provided
- Optional subnet mask (not required when IP address is specified)
- 102-byte magic packet generation (6 bytes 0xFF + 16 repetitions of MAC address)
- Zero external dependencies

[Unreleased]: https://github.com/ctrl3d/WakeOnLan/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/ctrl3d/WakeOnLan/releases/tag/v1.0.0

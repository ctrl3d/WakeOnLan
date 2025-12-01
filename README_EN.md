# WakeOnLan for Unity

[![Unity 2022.1+](https://img.shields.io/badge/unity-2022.1%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/github/license/ctrl3d/WakeOnLan)](https://github.com/ctrl3d/WakeOnLan/blob/main/LICENSE)
[![openupm](https://img.shields.io/npm/v/work.ctrl3d.wake-on-lan?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/work.ctrl3d.wake-on-lan/)

English | [한국어](README.md)

A Unity library for sending Wake-on-LAN (WOL) magic packets to wake up remote computers.

## Features

- Asynchronous magic packet transmission (async/await)
- Broadcast or subnet-specific packet transmission
- UniTask support (optional)
- Unity 2022.1+ support

## Installation

### Via Unity Package Manager

1. Open `Window` → `Package Manager` in Unity Editor
2. Click the `+` button in the top left corner
3. Select `Add package from git URL...`
4. Enter the following URL:
   ```
   https://github.com/ctrl3d/WakeOnLan.git?path=Assets/WakeOnLan
   ```

### Via manifest.json

Open `Packages/manifest.json` in your project and add the following to `dependencies`:

```json
{
  "dependencies": {
    "work.ctrl3d.wake-on-lan": "https://github.com/ctrl3d/WakeOnLan.git?path=Assets/WakeOnLan"
  }
}
```

## Usage

### Basic Usage (Asynchronous)

```csharp
using work.ctrl3d;
using UnityEngine;

public class WakeOnLanExample : MonoBehaviour
{
    async void Start()
    {
        try
        {
            // Send magic packet via broadcast
            await WakeOnLan.SendMagicPacketAsync("AA:BB:CC:DD:EE:FF");
            Debug.Log("Magic packet sent successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to send magic packet: {ex.Message}");
        }
    }
}
```

### Synchronous Usage

```csharp
using work.ctrl3d;
using UnityEngine;

public class WakeOnLanSync : MonoBehaviour
{
    void Start()
    {
        try
        {
            // Send magic packet synchronously (blocks main thread)
            WakeOnLan.SendMagicPacket("AA:BB:CC:DD:EE:FF");
            Debug.Log("Magic packet sent successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to send magic packet: {ex.Message}");
        }
    }
}
```

**Note:** The synchronous method blocks the main thread until the packet is sent. It's recommended to use the asynchronous method when possible.

### Advanced Options

#### Unicast to Specific IP

```csharp
// IP address only (no subnet mask) - sends directly to the IP
await WakeOnLan.SendMagicPacketAsync(
    macAddress: "AA:BB:CC:DD:EE:FF",
    ipAddress: "192.168.1.100"
);
```

#### Subnet Broadcast

```csharp
// IP address with subnet mask - calculates subnet broadcast address (e.g., 192.168.1.255)
await WakeOnLan.SendMagicPacketAsync(
    macAddress: "AA:BB:CC:DD:EE:FF",
    ipAddress: "192.168.1.100",
    subnetMask: "255.255.255.0"
);
```

**Summary:**
- **MAC only**: Global broadcast (255.255.255.255) - sufficient for most cases
- **MAC + IP**: Unicast to specific IP
- **MAC + IP + Subnet**: Send to calculated subnet broadcast address

### MAC Address Formats

The following MAC address formats are supported:

- `AA:BB:CC:DD:EE:FF` (colon-separated)
- `AA-BB-CC-DD-EE-FF` (hyphen-separated)
- `AABBCCDDEEFF` (no separator)

### UniTask Support

When using UniTask, the library automatically uses `UniTask` through the Version Defines feature.

#### Automatic Setup (Recommended)

If the UniTask package (`com.cysharp.unitask`) is installed in your project, the Version Defines feature automatically enables the `USE_UNITASK` symbol. No manual configuration is required.

#### Manual Setup

If needed, you can set it up manually:

1. `Project Settings` → `Player` → `Other Settings` → `Scripting Define Symbols`
2. Add `USE_UNITASK`

## API Reference

### SendMagicPacket (Synchronous)

```csharp
public static void SendMagicPacket(
    string macAddress,
    string ipAddress = null,
    string subnetMask = null,
    int port = 9
)
```

**Parameters:**
- `macAddress` (string): Target computer's MAC address
- `ipAddress` (string, optional): IP address
  - Not specified: Global broadcast (255.255.255.255)
  - Specified (no subnet mask): Unicast to the IP
  - Specified (with subnet mask): Send to subnet broadcast address
- `subnetMask` (string, optional): Subnet mask (calculates broadcast address when specified with IP)
- `port` (int, optional): WOL port number (default: 9)

**Exceptions:**
- `FormatException`: Invalid MAC address, IP address, or subnet mask format
- `InvalidOperationException`: Packet transmission failed

**Warning:** This method executes synchronously and blocks the calling thread.

### SendMagicPacketAsync (Asynchronous)

```csharp
public static async Task SendMagicPacketAsync(
    string macAddress,
    string ipAddress = null,
    string subnetMask = null,
    int port = 9
)
```

**Parameters:**
- `macAddress` (string): Target computer's MAC address
- `ipAddress` (string, optional): IP address
  - Not specified: Global broadcast (255.255.255.255)
  - Specified (no subnet mask): Unicast to the IP
  - Specified (with subnet mask): Send to subnet broadcast address
- `subnetMask` (string, optional): Subnet mask (calculates broadcast address when specified with IP)
- `port` (int, optional): WOL port number (default: 9)

**Returns:**
- `Task` (or `UniTask` when USE_UNITASK is defined)

**Exceptions:**
- `FormatException`: Invalid MAC address, IP address, or subnet mask format
- `InvalidOperationException`: Packet transmission failed

## Platform Compatibility

| Platform | Support | Notes |
|----------|---------|-------|
| Windows | ✅ | Full support |
| macOS | ✅ | Full support |
| Linux | ✅ | Full support |
| Android | ✅ | Network permissions required |
| iOS | ✅ | Network permissions required |
| WebGL | ❌ | UDP sockets not supported |

## Advanced Usage Examples

### UI Button Integration

```csharp
using work.ctrl3d;
using UnityEngine;
using UnityEngine.UI;

public class WakeOnLanButton : MonoBehaviour
{
    [SerializeField] private Button wakeButton;
    [SerializeField] private string targetMacAddress = "AA:BB:CC:DD:EE:FF";

    void Start()
    {
        wakeButton.onClick.AddListener(OnWakeButtonClicked);
    }

    async void OnWakeButtonClicked()
    {
        wakeButton.interactable = false;

        try
        {
            await WakeOnLan.SendMagicPacketAsync(targetMacAddress);
            Debug.Log("PC wakeup successful!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed: {ex.Message}");
        }
        finally
        {
            wakeButton.interactable = true;
        }
    }
}
```

### Waking Multiple Computers Sequentially

```csharp
using work.ctrl3d;
using UnityEngine;
using System.Collections.Generic;

public class MultipleWakeOnLan : MonoBehaviour
{
    [System.Serializable]
    public class WakeTarget
    {
        public string name;
        public string macAddress;
        public string ipAddress;
        public string subnetMask;
    }

    [SerializeField] private List<WakeTarget> targets;

    public async void WakeAllComputers()
    {
        foreach (var target in targets)
        {
            try
            {
                Debug.Log($"Waking {target.name}...");

                if (string.IsNullOrEmpty(target.ipAddress))
                {
                    await WakeOnLan.SendMagicPacketAsync(target.macAddress);
                }
                else
                {
                    await WakeOnLan.SendMagicPacketAsync(
                        target.macAddress,
                        target.ipAddress,
                        target.subnetMask
                    );
                }

                Debug.Log($"{target.name} completed!");

                // Wait 1 second between packets
                await System.Threading.Tasks.Task.Delay(1000);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{target.name} failed: {ex.Message}");
            }
        }
    }
}
```

### Error Handling Best Practices

```csharp
using work.ctrl3d;
using UnityEngine;
using System;

public class WakeOnLanWithErrorHandling : MonoBehaviour
{
    async void WakeComputerSafely(string macAddress, string ip = null, string subnet = null)
    {
        try
        {
            await WakeOnLan.SendMagicPacketAsync(macAddress, ip, subnet);
            Debug.Log("Magic packet sent successfully");
        }
        catch (FormatException ex)
        {
            // MAC address or IP address format error
            Debug.LogError($"Address format error: {ex.Message}");
            // Display user-friendly message in UI
        }
        catch (ArgumentNullException ex)
        {
            // IP provided but subnet mask missing
            Debug.LogError($"Required parameter missing: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            // Network transmission failed
            Debug.LogError($"Network error: {ex.Message}");
            // Request network connection check
        }
        catch (Exception ex)
        {
            // Other exceptions
            Debug.LogError($"Unknown error: {ex.Message}");
        }
    }
}
```

## Troubleshooting

### Magic packet is not being sent

1. **Check Firewall**
   - Windows: Allow UDP port 9 in Windows Defender Firewall
   - macOS: System Preferences → Security & Privacy → Firewall
   - Ensure outbound UDP traffic is allowed

2. **Network Permissions (Mobile)**
   - Android: Add internet permission to `AndroidManifest.xml`
     ```xml
     <uses-permission android:name="android.permission.INTERNET" />
     ```
   - iOS: Configure network permissions in `Info.plist`

3. **Router Configuration**
   - Verify you're on the same network as the target computer
   - For remote networks, configure UDP port 9 forwarding on the router

### Doesn't work on WebGL

WebGL does not support UDP sockets due to browser security constraints. You'll need to use a workaround through a server.

### Getting MAC address format errors

Only the following formats are supported:
- `AA:BB:CC:DD:EE:FF` (colon)
- `AA-BB-CC-DD-EE-FF` (hyphen)
- `AABBCCDDEEFF` (no separator)

Case-insensitive.

### Target computer won't wake up

Sending a WOL magic packet alone may not be sufficient:

1. **Target PC BIOS/UEFI Settings**
   - Wake-on-LAN feature must be enabled
   - Enable "Power On by PCI-E/PCIe" option

2. **Network Adapter Settings (Windows)**
   - Device Manager → Network Adapters
   - Properties → Power Management
   - Check "Allow this device to wake the computer" and "Only allow a magic packet to wake the computer"

3. **Shutdown vs Sleep Mode**
   - Windows Fast Startup may not be compatible with WOL
   - Recommended to use Sleep or Hibernate mode on target PC

## Security and Performance Notes

### Security

- **Network Permissions**: This library uses UDP sockets and requires internet permissions
- **Broadcast**: By default uses broadcast, so all devices on the same network can receive the packet
- **No Authentication**: The Wake-on-LAN protocol itself has no authentication mechanism, so implement additional security layers for critical systems

### Performance

- **Lightweight**: Magic packet is only 102 bytes
- **Asynchronous**: async/await pattern prevents main thread blocking
- **Battery Impact**: Network usage on mobile devices has minimal battery impact
- **Network Load**: Single UDP packet transmission causes negligible network load

## Requirements

- Unity 2022.1 or higher
- .NET Standard 2.1

## Testing

This package includes comprehensive unit tests.

### How to Run Tests

1. Open `Window` → `General` → `Test Runner` in Unity Editor
2. Select `PlayMode` tab
3. Click `Run All` to execute all tests

### Test Coverage

- ✅ MAC address parsing (colon, hyphen, no separator)
- ✅ MAC address format validation
- ✅ Magic packet structure validation (102 bytes, 0xFF header, 16 MAC repetitions)
- ✅ Broadcast address calculation (various subnet masks)
- ✅ Error handling (invalid MAC, length errors, etc.)

## License

See the [LICENSE](LICENSE) file for this project's license.

## Author

- **Seungmin Lee**
- Email: ctrl3d@gmail.com
- Website: https://ctrl3d.work

## Contributing

Issues and Pull Requests are always welcome!

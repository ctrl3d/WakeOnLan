#if USE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
#endif
using System;
using System.Net;
using System.Net.Sockets;

namespace work.ctrl3d
{
    public static class WakeOnLan
    {
        private const int MacByteCount = 6;
        private const int MacRepeatCount = 16;
        private const int PacketSize = MacByteCount + MacRepeatCount * MacByteCount; // 102

        /// <summary>
        /// Sends a Wake-on-LAN magic packet synchronously.
        /// Example: <code>WakeOnLan.SendMagicPacket("AA:BB:CC:DD:EE:FF")</code>
        /// Warning: This will block the calling thread until the packet is sent.
        /// </summary>
        /// <param name="macAddress">Target computer's MAC address (e.g., "AA:BB:CC:DD:EE:FF")</param>
        /// <param name="ipAddress">
        /// Optional IP address. If not specified, uses the global broadcast address (255.255.255.255).
        /// If specified without a subnet mask, sends unicast to this IP. Note that unicast only wakes a
        /// machine while the router still holds an ARP entry for it, so prefer supplying
        /// <paramref name="subnetMask"/> to send a directed subnet broadcast instead.
        /// </param>
        /// <param name="subnetMask">Optional subnet mask. When specified with IP address, calculates the subnet broadcast address.</param>
        /// <param name="port">WOL port number (default: 9)</param>
        /// <exception cref="FormatException">Invalid MAC address, IP address, or subnet mask format</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is outside the range 1-65535</exception>
        /// <exception cref="ArgumentException">A subnet mask was given with a non-IPv4 address</exception>
        /// <exception cref="InvalidOperationException">Failed to send magic packet</exception>
        public static void SendMagicPacket(string macAddress, string ipAddress = null, string subnetMask = null, int port = 9)
        {
            var (packet, endPoint) = BuildPacketAndEndpoint(macAddress, ipAddress, subnetMask, port);

            using var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            client.EnableBroadcast = true;

            try
            {
                client.Send(packet, packet.Length, endPoint);
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException($"Failed to send Magic Packet (Code: {ex.SocketErrorCode})", ex);
            }
        }

        /// <summary>
        /// Sends a Wake-on-LAN magic packet asynchronously.
        /// Example: <code>await WakeOnLan.SendMagicPacketAsync("AA:BB:CC:DD:EE:FF")</code>
        /// </summary>
        /// <param name="macAddress">Target computer's MAC address (e.g., "AA:BB:CC:DD:EE:FF")</param>
        /// <param name="ipAddress">
        /// Optional IP address. If not specified, uses the global broadcast address (255.255.255.255).
        /// If specified without a subnet mask, sends unicast to this IP. Note that unicast only wakes a
        /// machine while the router still holds an ARP entry for it, so prefer supplying
        /// <paramref name="subnetMask"/> to send a directed subnet broadcast instead.
        /// </param>
        /// <param name="subnetMask">Optional subnet mask. When specified with IP address, calculates the subnet broadcast address.</param>
        /// <param name="port">WOL port number (default: 9)</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="FormatException">Invalid MAC address, IP address, or subnet mask format</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is outside the range 1-65535</exception>
        /// <exception cref="ArgumentException">A subnet mask was given with a non-IPv4 address</exception>
        /// <exception cref="InvalidOperationException">Failed to send magic packet</exception>
        #if USE_UNITASK
        public static async UniTask SendMagicPacketAsync(string macAddress, string ipAddress = null, string subnetMask = null, int port = 9)
        #else
        public static async Task SendMagicPacketAsync(string macAddress, string ipAddress = null, string subnetMask = null, int port = 9)
        #endif
        {
            var (packet, endPoint) = BuildPacketAndEndpoint(macAddress, ipAddress, subnetMask, port);

            using var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            client.EnableBroadcast = true;

            try
            {
                await client.SendAsync(packet, packet.Length, endPoint).ConfigureAwait(false);
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException($"Failed to send Magic Packet (Code: {ex.SocketErrorCode})", ex);
            }
        }

        /// <summary>
        /// Tries to parse a MAC address into 6 bytes.
        /// ':' and '-' are accepted as optional separators,
        /// so "AA:BB:CC:DD:EE:FF", "AA-BB-CC-DD-EE-FF" and "AABBCCDDEEFF" are all valid.
        /// Trailing separators (e.g., "AA:BB:CC:DD:EE:FF:") are silently ignored.
        /// Leading or embedded whitespace is not trimmed.
        /// </summary>
        /// <param name="macAddress">The MAC address string to parse.</param>
        /// <param name="macBytes">When this method returns, contains the 6-byte MAC address if parsing succeeded; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the MAC address was parsed successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParseMacAddress(string macAddress, out byte[] macBytes)
        {
            try
            {
                macBytes = ParseMacAddress(macAddress);
                return true;
            }
            catch
            {
                macBytes = null;
                return false;
            }
        }

        /// <summary>
        /// Builds the magic packet bytes and target endpoint from the provided arguments.
        /// </summary>
        private static (byte[] packet, IPEndPoint endPoint) BuildPacketAndEndpoint(
            string macAddress, string ipAddress, string subnetMask, int port)
        {
            return (CreateMagicPacket(ParseMacAddress(macAddress)), ResolveEndPoint(ipAddress, subnetMask, port));
        }

        /// <summary>
        /// Parses a MAC address into 6 bytes. ':' and '-' are accepted as optional separators,
        /// so "AA:BB:CC:DD:EE:FF", "AA-BB-CC-DD-EE-FF" and "AABBCCDDEEFF" are all valid.
        /// Trailing separators (e.g., "AA:BB:CC:DD:EE:FF:") are silently ignored.
        /// Leading or embedded whitespace is not trimmed; use <c>macAddress.Trim()</c> beforehand if needed.
        /// </summary>
        /// <exception cref="FormatException">Invalid MAC address format</exception>
        private static byte[] ParseMacAddress(string macAddress)
        {
            if (string.IsNullOrEmpty(macAddress))
                throw new FormatException("MAC address is null or empty.");

            var macBytes = new byte[MacByteCount];
            var byteIndex = 0;

            for (var i = 0; i < macAddress.Length; i++)
            {
                if (macAddress[i] is ':' or '-') continue;

                if (byteIndex >= MacByteCount)
                    throw new FormatException(
                        $"MAC address is too long (more than {MacByteCount} bytes): {macAddress}");

                if (i + 1 >= macAddress.Length)
                    throw new FormatException($"MAC address ends with an incomplete byte: {macAddress}");

                var high = ToHexValue(macAddress, i);
                var low = ToHexValue(macAddress, i + 1);

                macBytes[byteIndex] = (byte)((high << 4) | low);

                byteIndex++;
                i++;
            }

            if (byteIndex != MacByteCount)
                throw new FormatException(
                    $"MAC address is too short ({byteIndex} bytes found, {MacByteCount} bytes required): {macAddress}");

            return macBytes;
        }

        private static int ToHexValue(string macAddress, int position)
        {
            var c = macAddress[position];

            return c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'a' and <= 'f' => c - 'a' + 10,
                >= 'A' and <= 'F' => c - 'A' + 10,
                _ => throw new FormatException(
                    $"MAC address contains a non-hexadecimal character '{c}' at position {position}: {macAddress}")
            };
        }

        private static IPEndPoint ResolveEndPoint(string ipAddress, string subnetMask, int port)
        {
            if (port is < 1 or > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(port), port,
                    $"Port must be between 1 and {IPEndPoint.MaxPort}.");

            // No IP specified: use global broadcast
            if (string.IsNullOrEmpty(ipAddress))
                return new IPEndPoint(IPAddress.Broadcast, port);

            if (!IPAddress.TryParse(ipAddress, out var ip))
                throw new FormatException($"Invalid IP address format: {ipAddress}");

            // IP specified without subnet mask: send unicast to the IP
            if (string.IsNullOrEmpty(subnetMask))
                return new IPEndPoint(ip, port);

            // IP and subnet mask specified: calculate the directed subnet broadcast address
            if (!IPAddress.TryParse(subnetMask, out var mask))
                throw new FormatException($"Invalid subnet mask format: {subnetMask}");

            return new IPEndPoint(GetBroadcastAddress(ip, mask), port);
        }

        private static byte[] CreateMagicPacket(byte[] macBytes)
        {
            // 6 x 0xFF sync stream, followed by the target MAC repeated 16 times.
            var packet = new byte[PacketSize];

            new Span<byte>(packet, 0, MacByteCount).Fill(0xFF);

            var macSpan = new Span<byte>(macBytes);
            var offset = MacByteCount;
            for (var repetition = 0; repetition < MacRepeatCount; repetition++, offset += MacByteCount)
            {
                macSpan.CopyTo(new Span<byte>(packet, offset, MacByteCount));
            }

            return packet;
        }

        private static IPAddress GetBroadcastAddress(IPAddress ipAddress, IPAddress subnetMask)
        {
            // IPv6 has no broadcast address, so a directed broadcast is IPv4-only.
            if (ipAddress.AddressFamily != AddressFamily.InterNetwork ||
                subnetMask.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException(
                    "A subnet broadcast requires an IPv4 address and an IPv4 subnet mask, " +
                    $"but got {ipAddress} / {subnetMask}.");

            // Validate that the subnet mask is a valid contiguous mask (e.g., 255.255.255.0, not 255.255.0.255).
            if (!IsValidSubnetMask(subnetMask))
                throw new FormatException($"Invalid subnet mask (must be contiguous 1-bits followed by 0-bits): {subnetMask}");

            var ipBytes = ipAddress.GetAddressBytes();
            var maskBytes = subnetMask.GetAddressBytes();

            var broadcastBytes = new byte[ipBytes.Length];

            for (var i = 0; i < broadcastBytes.Length; i++)
            {
                broadcastBytes[i] = (byte)(ipBytes[i] ^ 0xFF);
            }

            return new IPAddress(broadcastBytes);
        }

        /// <summary>
        /// Checks whether the given subnet mask is a valid contiguous mask
        /// (a sequence of 1-bits followed by 0-bits, e.g., 255.255.255.0).
        /// </summary>
        private static bool IsValidSubnetMask(IPAddress subnetMask)
        {
            var maskBytes = subnetMask.GetAddressBytes();
            var maskInt = (uint)(maskBytes[0] << 24 | maskBytes[1] << 16 | maskBytes[2] << 8 | maskBytes[3]);

            // A valid subnet mask has the property that ~mask is either 0 or a power of 2 minus 1
            // i.e., ~mask & (~mask + 1) == 0  (lowest bit test)
            var inverted = ~maskInt;
            return (inverted & (inverted + 1)) == 0;
        }
    }
}

#if USE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using System;
using System.Net;
using System.Net.Sockets;

namespace work.ctrl3d
{
    public static class WakeOnLan
    {
        /// <summary>
        /// Sends a Wake-on-LAN magic packet synchronously.
        /// Warning: This will block the calling thread until the packet is sent.
        /// </summary>
        /// <param name="macAddress">Target computer's MAC address (e.g., "AA:BB:CC:DD:EE:FF")</param>
        /// <param name="ipAddress">Optional IP address. If not specified, uses broadcast (255.255.255.255). If specified without subnet mask, sends unicast to this IP.</param>
        /// <param name="subnetMask">Optional subnet mask. When specified with IP address, calculates the subnet broadcast address.</param>
        /// <param name="port">WOL port number (default: 9)</param>
        /// <exception cref="FormatException">Invalid MAC address, IP address, or subnet mask format</exception>
        /// <exception cref="InvalidOperationException">Failed to send magic packet</exception>
        public static void SendMagicPacket(string macAddress, string ipAddress = null, string subnetMask = null,
            int port = 9)
        {
#if USE_UNITASK
            SendMagicPacketAsync(macAddress, ipAddress, subnetMask, port).GetAwaiter().GetResult();
#else
            SendMagicPacketAsync(macAddress, ipAddress, subnetMask, port).GetAwaiter().GetResult();
#endif
        }

        /// <summary>
        /// Sends a Wake-on-LAN magic packet asynchronously.
        /// </summary>
        /// <param name="macAddress">Target computer's MAC address (e.g., "AA:BB:CC:DD:EE:FF")</param>
        /// <param name="ipAddress">Optional IP address. If not specified, uses broadcast (255.255.255.255). If specified without subnet mask, sends unicast to this IP.</param>
        /// <param name="subnetMask">Optional subnet mask. When specified with IP address, calculates the subnet broadcast address.</param>
        /// <param name="port">WOL port number (default: 9)</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="FormatException">Invalid MAC address, IP address, or subnet mask format</exception>
        /// <exception cref="InvalidOperationException">Failed to send magic packet</exception>
#if USE_UNITASK
        public static async UniTask SendMagicPacketAsync(string macAddress, string ipAddress = null, string subnetMask =
 null, int port = 9)
#else
        public static async Task SendMagicPacketAsync(string macAddress, string ipAddress = null,
            string subnetMask = null, int port = 9)
#endif
        {
            var macBytes = new byte[6];
            try
            {
                var byteIndex = 0;
                for (var i = 0; i < macAddress.Length; i++)
                {
                    var c = macAddress[i];

                    if (c is ':' or '-') continue;

                    if (byteIndex >= 6)
                        throw new FormatException("MAC address length is too long.");


                    if (i + 1 >= macAddress.Length)
                        throw new FormatException("Invalid MAC address format (last byte incomplete).");

                    var hexPair = macAddress.Substring(i, 2);
                    macBytes[byteIndex] = Convert.ToByte(hexPair, 16);

                    byteIndex++;
                    i++;
                }

                if (byteIndex != 6)
                    throw new FormatException(
                        $"MAC address length is too short ({byteIndex} bytes found, 6 bytes required).");
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to parse MAC address: {macAddress}", ex);
            }
            
            var packet = CreateMagicPacket(macBytes);

            using var client = new UdpClient();
            client.EnableBroadcast = true;

            IPEndPoint targetEndPoint;

            if (string.IsNullOrEmpty(ipAddress))
            {
                // No IP specified: use global broadcast
                targetEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            }
            else
            {
                if (!IPAddress.TryParse(ipAddress, out var ip))
                    throw new FormatException($"Invalid IP address format: {ipAddress}");

                if (string.IsNullOrEmpty(subnetMask))
                {
                    // IP specified without subnet mask: send unicast to the IP
                    targetEndPoint = new IPEndPoint(ip, port);
                }
                else
                {
                    // IP and subnet mask specified: calculate subnet broadcast address
                    if (!IPAddress.TryParse(subnetMask, out var mask))
                        throw new FormatException($"Invalid subnet mask format: {subnetMask}");

                    var broadcastAddress = GetBroadcastAddress(ip, mask);
                    targetEndPoint = new IPEndPoint(broadcastAddress, port);
                }
            }

            try
            {
                await client.SendAsync(packet, packet.Length, targetEndPoint);
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException($"Failed to send Magic Packet (Code: {ex.SocketErrorCode})", ex);
            }
        }
        
        private static byte[] CreateMagicPacket(byte[] macBytes)
        {
            var packet = new byte[6 + 16 * 6];

            for (var i = 0; i < 6; i++)
            {
                packet[i] = 0xFF;
            }

            for (var i = 6; i < packet.Length; i += macBytes.Length)
            {
                Array.Copy(macBytes, 0, packet, i, macBytes.Length);
            }

            return packet;
        }

        private static IPAddress GetBroadcastAddress(IPAddress ipAddress, IPAddress subnetMask)
        {
            var ipBytes = ipAddress.GetAddressBytes();
            var maskBytes = subnetMask.GetAddressBytes();

            if (ipBytes.Length != maskBytes.Length)
                throw new ArgumentException("IP address and subnet mask versions (IPv4/IPv6) do not match.");

            var broadcastBytes = new byte[ipBytes.Length];
            
            for (var i = 0; i < broadcastBytes.Length; i++)
            {
                broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
            }

            return new IPAddress(broadcastBytes);
        }
    }
}
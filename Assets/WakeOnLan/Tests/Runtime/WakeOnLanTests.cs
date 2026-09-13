using NUnit.Framework;
using System;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace WakeOnLan.Tests
{
    public class WakeOnLanTests
    {
        #region MAC Address Parsing Tests

        [Test]
        public void SendMagicPacket_WithColonSeparatedMAC_ParsesCorrectly()
        {
            // Arrange
            var macAddress = "AA:BB:CC:DD:EE:FF";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                var packet = CreateMagicPacketUsingReflection(macAddress);
                Assert.NotNull(packet);
                Assert.AreEqual(102, packet.Length);
            });
        }

        [Test]
        public void SendMagicPacket_WithHyphenSeparatedMAC_ParsesCorrectly()
        {
            // Arrange
            var macAddress = "AA-BB-CC-DD-EE-FF";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                var packet = CreateMagicPacketUsingReflection(macAddress);
                Assert.NotNull(packet);
                Assert.AreEqual(102, packet.Length);
            });
        }

        [Test]
        public void SendMagicPacket_WithNoSeparatorMAC_ParsesCorrectly()
        {
            // Arrange
            var macAddress = "AABBCCDDEEFF";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                var packet = CreateMagicPacketUsingReflection(macAddress);
                Assert.NotNull(packet);
                Assert.AreEqual(102, packet.Length);
            });
        }

        [Test]
        public void SendMagicPacket_WithLowercaseMAC_ParsesCorrectly()
        {
            // Arrange
            var macAddress = "aa:bb:cc:dd:ee:ff";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                var packet = CreateMagicPacketUsingReflection(macAddress);
                Assert.NotNull(packet);
                Assert.AreEqual(102, packet.Length);
            });
        }

        [Test]
        public void SendMagicPacket_WithMixedCaseMAC_ParsesCorrectly()
        {
            // Arrange
            var macAddress = "Aa:Bb:Cc:Dd:Ee:Ff";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                var packet = CreateMagicPacketUsingReflection(macAddress);
                Assert.NotNull(packet);
                Assert.AreEqual(102, packet.Length);
            });
        }

        [Test]
        public void SendMagicPacket_WithTooShortMAC_ThrowsFormatException()
        {
            // Arrange
            var macAddress = "AA:BB:CC:DD:EE";

            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                CreateMagicPacketUsingReflection(macAddress);
            });
        }

        [Test]
        public void SendMagicPacket_WithTooLongMAC_ThrowsFormatException()
        {
            // Arrange
            var macAddress = "AA:BB:CC:DD:EE:FF:00";

            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                CreateMagicPacketUsingReflection(macAddress);
            });
        }

        [Test]
        public void SendMagicPacket_WithInvalidHexCharacters_ThrowsFormatException()
        {
            // Arrange
            var macAddress = "GG:HH:II:JJ:KK:LL";

            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                CreateMagicPacketUsingReflection(macAddress);
            });
        }

        [Test]
        public void SendMagicPacket_WithEmptyMAC_ThrowsFormatException()
        {
            // Arrange
            var macAddress = "";

            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                CreateMagicPacketUsingReflection(macAddress);
            });
        }

        [Test]
        public void ParseMacAddress_WithNullMAC_ThrowsFormatException()
        {
            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                ParseMacAddressUsingReflection(null);
            });
        }

        [Test]
        public void ParseMacAddress_ReturnsExpectedBytes()
        {
            // Arrange
            var expected = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x23 };

            // Act
            var macBytes = ParseMacAddressUsingReflection("DE:AD:BE:EF:01:23");

            // Assert
            Assert.AreEqual(expected, macBytes);
        }

        [Test]
        public void ParseMacAddress_WithMixedSeparators_ParsesCorrectly()
        {
            // Arrange
            var expected = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

            // Act
            var macBytes = ParseMacAddressUsingReflection("AA:BB-CC:DD-EE:FF");

            // Assert
            Assert.AreEqual(expected, macBytes);
        }

        [Test]
        public void ParseMacAddress_WithIncompleteTrailingByte_ThrowsFormatException()
        {
            // Arrange - the final nibble has no partner
            var macAddress = "AA:BB:CC:DD:EE:F";

            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                ParseMacAddressUsingReflection(macAddress);
            });
        }

        [Test]
        public void ParseMacAddress_WithSeparatorInsideByte_ThrowsFormatException()
        {
            // Arrange - a separator splits the last byte in half
            var macAddress = "AA:BB:CC:DD:EE:F:F";

            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                ParseMacAddressUsingReflection(macAddress);
            });
        }

        #endregion

        #region Magic Packet Structure Tests

        [Test]
        public void CreateMagicPacket_HasCorrectLength()
        {
            // Arrange
            var macAddress = "AA:BB:CC:DD:EE:FF";

            // Act
            var packet = CreateMagicPacketUsingReflection(macAddress);

            // Assert
            Assert.AreEqual(102, packet.Length, "Magic packet should be 102 bytes (6 + 16*6)");
        }

        [Test]
        public void CreateMagicPacket_StartsWithSixFFBytes()
        {
            // Arrange
            var macAddress = "AA:BB:CC:DD:EE:FF";

            // Act
            var packet = CreateMagicPacketUsingReflection(macAddress);

            // Assert
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(0xFF, packet[i], $"Byte {i} should be 0xFF");
            }
        }

        [Test]
        public void CreateMagicPacket_Contains16RepetitionsOfMAC()
        {
            // Arrange
            var macAddress = "AA:BB:CC:DD:EE:FF";
            var expectedMac = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

            // Act
            var packet = CreateMagicPacketUsingReflection(macAddress);

            // Assert - Check 16 repetitions of MAC address
            for (int repetition = 0; repetition < 16; repetition++)
            {
                int offset = 6 + (repetition * 6);
                for (int i = 0; i < 6; i++)
                {
                    Assert.AreEqual(expectedMac[i], packet[offset + i],
                        $"MAC repetition {repetition}, byte {i} should match");
                }
            }
        }

        [Test]
        public void CreateMagicPacket_WithDifferentMAC_HasCorrectBytes()
        {
            // Arrange
            var macAddress = "11:22:33:44:55:66";
            var expectedMac = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 };

            // Act
            var packet = CreateMagicPacketUsingReflection(macAddress);

            // Assert - Check first MAC occurrence after the 0xFF bytes
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(expectedMac[i], packet[6 + i],
                    $"First MAC occurrence, byte {i} should match");
            }
        }

        #endregion

        #region Broadcast Address Calculation Tests

        [Test]
        public void GetBroadcastAddress_WithTypicalSubnet_CalculatesCorrectly()
        {
            // Arrange
            var ip = IPAddress.Parse("192.168.1.100");
            var subnet = IPAddress.Parse("255.255.255.0");
            var expected = IPAddress.Parse("192.168.1.255");

            // Act
            var result = GetBroadcastAddressUsingReflection(ip, subnet);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetBroadcastAddress_WithClassASubnet_CalculatesCorrectly()
        {
            // Arrange
            var ip = IPAddress.Parse("10.0.0.50");
            var subnet = IPAddress.Parse("255.0.0.0");
            var expected = IPAddress.Parse("10.255.255.255");

            // Act
            var result = GetBroadcastAddressUsingReflection(ip, subnet);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetBroadcastAddress_WithClassBSubnet_CalculatesCorrectly()
        {
            // Arrange
            var ip = IPAddress.Parse("172.16.10.50");
            var subnet = IPAddress.Parse("255.255.0.0");
            var expected = IPAddress.Parse("172.16.255.255");

            // Act
            var result = GetBroadcastAddressUsingReflection(ip, subnet);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetBroadcastAddress_WithCustomSubnet_CalculatesCorrectly()
        {
            // Arrange
            var ip = IPAddress.Parse("192.168.10.100");
            var subnet = IPAddress.Parse("255.255.240.0"); // /20
            var expected = IPAddress.Parse("192.168.15.255");

            // Act
            var result = GetBroadcastAddressUsingReflection(ip, subnet);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetBroadcastAddress_WithSingleHostSubnet_ReturnsHostIP()
        {
            // Arrange
            var ip = IPAddress.Parse("192.168.1.100");
            var subnet = IPAddress.Parse("255.255.255.255");
            var expected = IPAddress.Parse("192.168.1.100");

            // Act
            var result = GetBroadcastAddressUsingReflection(ip, subnet);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetBroadcastAddress_WithIPv6Address_ThrowsArgumentException()
        {
            // Arrange - IPv6 has no broadcast address
            var ip = IPAddress.Parse("fe80::1");
            var subnet = IPAddress.Parse("ffff:ffff:ffff:ffff::");

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                GetBroadcastAddressUsingReflection(ip, subnet);
            });
        }

        [Test]
        public void GetBroadcastAddress_WithMismatchedFamilies_ThrowsArgumentException()
        {
            // Arrange
            var ip = IPAddress.Parse("192.168.1.100");
            var subnet = IPAddress.Parse("ffff:ffff:ffff:ffff::");

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                GetBroadcastAddressUsingReflection(ip, subnet);
            });
        }

        #endregion

        #region Endpoint Resolution Tests

        [Test]
        public void ResolveEndPoint_WithNoIP_UsesGlobalBroadcast()
        {
            // Act
            var endPoint = ResolveEndPointUsingReflection(null, null, 9);

            // Assert
            Assert.AreEqual(IPAddress.Broadcast, endPoint.Address);
            Assert.AreEqual(9, endPoint.Port);
        }

        [Test]
        public void ResolveEndPoint_WithIPOnly_UsesUnicast()
        {
            // Act
            var endPoint = ResolveEndPointUsingReflection("192.168.1.42", null, 9);

            // Assert
            Assert.AreEqual(IPAddress.Parse("192.168.1.42"), endPoint.Address);
        }

        [Test]
        public void ResolveEndPoint_WithIPAndSubnetMask_UsesDirectedBroadcast()
        {
            // Act
            var endPoint = ResolveEndPointUsingReflection("192.168.1.42", "255.255.255.0", 7);

            // Assert
            Assert.AreEqual(IPAddress.Parse("192.168.1.255"), endPoint.Address);
            Assert.AreEqual(7, endPoint.Port);
        }

        [Test]
        public void ResolveEndPoint_WithInvalidIP_ThrowsFormatException()
        {
            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                ResolveEndPointUsingReflection("not-an-ip", null, 9);
            });
        }

        [Test]
        public void ResolveEndPoint_WithInvalidSubnetMask_ThrowsFormatException()
        {
            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                ResolveEndPointUsingReflection("192.168.1.42", "not-a-mask", 9);
            });
        }

        [Test]
        public void ResolveEndPoint_WithZeroPort_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ResolveEndPointUsingReflection(null, null, 0);
            });
        }

        [Test]
        public void ResolveEndPoint_WithPortAboveMax_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ResolveEndPointUsingReflection(null, null, 65536);
            });
        }

        #endregion

        #region Helper Methods - Using Reflection to Access Private Methods

        private static readonly Type WakeOnLanType = typeof(work.ctrl3d.WakeOnLan);

        /// <summary>
        /// Invokes a private static method on the production type, re-throwing the original
        /// exception instead of the <see cref="TargetInvocationException"/> reflection wraps it in.
        /// </summary>
        private static object InvokePrivateStatic(string methodName, params object[] args)
        {
            var method = WakeOnLanType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

            if (method == null)
                throw new InvalidOperationException($"{methodName} method not found");

            try
            {
                return method.Invoke(null, args);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw; // Unreachable, but required by the compiler.
            }
        }

        private static byte[] ParseMacAddressUsingReflection(string macAddress)
        {
            return (byte[])InvokePrivateStatic("ParseMacAddress", macAddress);
        }

        private static byte[] CreateMagicPacketUsingReflection(string macAddress)
        {
            var macBytes = ParseMacAddressUsingReflection(macAddress);

            return (byte[])InvokePrivateStatic("CreateMagicPacket", macBytes);
        }

        private static IPAddress GetBroadcastAddressUsingReflection(IPAddress ipAddress, IPAddress subnetMask)
        {
            return (IPAddress)InvokePrivateStatic("GetBroadcastAddress", ipAddress, subnetMask);
        }

        private static IPEndPoint ResolveEndPointUsingReflection(string ipAddress, string subnetMask, int port)
        {
            return (IPEndPoint)InvokePrivateStatic("ResolveEndPoint", ipAddress, subnetMask, port);
        }

        #endregion
    }
}

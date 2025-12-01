using NUnit.Framework;
using System;
using System.Net;
using System.Reflection;
using work.ctrl3d;

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

        #endregion

        #region Helper Methods - Using Reflection to Access Private Methods

        private byte[] CreateMagicPacketUsingReflection(string macAddress)
        {
            // Parse MAC address to get macBytes
            var macBytes = new byte[6];
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

            // Call private CreateMagicPacket method
            var type = typeof(work.ctrl3d.WakeOnLan);
            var method = type.GetMethod("CreateMagicPacket", BindingFlags.NonPublic | BindingFlags.Static);

            if (method == null)
                throw new InvalidOperationException("CreateMagicPacket method not found");

            return (byte[])method.Invoke(null, new object[] { macBytes });
        }

        private IPAddress GetBroadcastAddressUsingReflection(IPAddress ipAddress, IPAddress subnetMask)
        {
            var type = typeof(work.ctrl3d.WakeOnLan);
            var method = type.GetMethod("GetBroadcastAddress", BindingFlags.NonPublic | BindingFlags.Static);

            if (method == null)
                throw new InvalidOperationException("GetBroadcastAddress method not found");

            return (IPAddress)method.Invoke(null, new object[] { ipAddress, subnetMask });
        }

        #endregion
    }
}

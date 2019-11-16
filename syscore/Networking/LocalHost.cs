using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


namespace Sys.Networking
{
    public static class LocalHost
    {
        private static IPAddress[] addressList = null;

        private static readonly NetworkInterfaceType[] NIC_TYPES = new NetworkInterfaceType[]
        {
             NetworkInterfaceType.Wireless80211,
             NetworkInterfaceType.Ethernet,
             NetworkInterfaceType.Wman,
             NetworkInterfaceType.Wwanpp,
             NetworkInterfaceType.Wwanpp2,
        };

        public const string LOCALHOST = "localhost";
        public static bool IgnorevEthernet = true;

        public static string GetLocalIP(int nic)
        {
            IPAddress address = GetIPAddress(nic);
            if (address == null)
                return "127.0.0.1";
            else
                return address.ToString();
        }

        public static IPEndPoint GetIPEndPoint(int nic, int port)
        {
            IPAddress address = GetIPAddress(nic);
            if (address == null)
                return null;

            return new IPEndPoint(address, port);
        }

        public static IPAddress GetIPAddress(int nic)
        {
            IPAddress[] L = GetIPAddressList(IgnorevEthernet);
            if (L.Length == 0)
            {
                return IPAddress.Loopback;
            }

            if (nic >= 0 && nic < L.Length)
                return L[nic];
            else
                return IPAddress.Loopback;
        }


        public static IPAddress[] GetIPAddressList(bool ignorevEthernet)
        {
            if (addressList != null)
            {
                return addressList;
            }

            addressList = getIPAddressList(ignorevEthernet).ToArray();
            return addressList;
        }


        private static IEnumerable<IPAddress> getIPAddressList(bool ignorevEthernet)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return new IPAddress[] { };
            }

            List<IPAddress> addresses = new List<IPAddress>();
            if (ignorevEthernet)
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.Name.StartsWith("vEthernet"))
                        continue;

                    if (!NIC_TYPES.Contains(ni.NetworkInterfaceType))
                        continue;

                    foreach (UnicastIPAddressInformation addressInformation in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (addressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                            addresses.Add(addressInformation.Address);
                    }
                }
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IEnumerable<IPAddress> L = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            if (ignorevEthernet)
                L = L.Where(ip => addresses.Contains(ip));

            return L.ToArray();
        }

    }
}

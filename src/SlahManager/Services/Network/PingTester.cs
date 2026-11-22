using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using SlahManager.Models.Network;

namespace SlahManager.Services.Network;

public sealed class PingTester
{
    private const int Samples = 20;
    private const int TimeoutMilliseconds = 1000;
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(50);
    private static readonly IPAddress InternetTarget = IPAddress.Parse("1.1.1.1");

    public async Task<PingReport> RunAsync()
    {
        var (connection, gateway) = DetectConnection();
        var router = gateway is null ? null : await MeasureAsync(gateway);
        var internet = await MeasureAsync(InternetTarget);
        return new PingReport(connection, router, internet);
    }

    private static (ConnectionKind Kind, IPAddress? Gateway) DetectConnection()
    {
        var routeAddress = LocalAddressTowards(InternetTarget);

        var candidates = NetworkInterface.GetAllNetworkInterfaces()
            .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up
                && adapter.NetworkInterfaceType is not (NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel))
            .OrderByDescending(adapter => routeAddress is not null
                && adapter.GetIPProperties().UnicastAddresses.Any(unicast => unicast.Address.Equals(routeAddress)));

        foreach (var adapter in candidates)
        {

            var gateway = adapter.GetIPProperties().GatewayAddresses
                .Select(address => address.Address)
                .FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork && !address.Equals(IPAddress.Any));

            if (gateway is null)
            {
                continue;
            }

            var kind = adapter.NetworkInterfaceType switch
            {
                NetworkInterfaceType.Wireless80211 => ConnectionKind.WiFi,
                NetworkInterfaceType.Ethernet or NetworkInterfaceType.GigabitEthernet or NetworkInterfaceType.FastEthernetT => ConnectionKind.Cable,
                _ => ConnectionKind.Other
            };

            return (kind, gateway);
        }

        return (ConnectionKind.None, null);
    }

    private static IPAddress? LocalAddressTowards(IPAddress target)
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(target, 53);
            return (socket.LocalEndPoint as IPEndPoint)?.Address;
        }
        catch (SocketException)
        {
            return null;
        }
    }

    private static async Task<PingStats> MeasureAsync(IPAddress target)
    {
        using var ping = new Ping();
        var times = new List<long>();
        var lost = 0;

        for (var index = 0; index < Samples; index++)
        {
            try
            {
                var reply = await ping.SendPingAsync(target, TimeoutMilliseconds);
                if (reply.Status == IPStatus.Success)
                {
                    times.Add(reply.RoundtripTime);
                }
                else
                {
                    lost++;
                }
            }
            catch (PingException)
            {
                lost++;
            }

            await Task.Delay(Interval);
        }

        var loss = lost * 100.0 / Samples;

        if (times.Count == 0)
        {
            return new PingStats(target.ToString(), null, null, loss);
        }

        var jitter = times.Zip(times.Skip(1), (first, second) => (double)Math.Abs(second - first)).DefaultIfEmpty(0).Average();
        return new PingStats(target.ToString(), times.Average(), jitter, loss);
    }
}

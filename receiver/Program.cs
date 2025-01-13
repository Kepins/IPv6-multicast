using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

const string multicastAddress = "ff12::2:D118"; // IPv6 multicast address
const int multicastPort = 4600; // Port to listen on

using var udpClient = new UdpClient(AddressFamily.InterNetworkV6);

// Allow multiple sockets to bind to the same address
udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

// Bind the socket to the multicast port
udpClient.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, multicastPort));

// Set timeout
udpClient.Client.ReceiveTimeout = 15000;

// Join the multicast group
udpClient.JoinMulticastGroup(IPAddress.Parse(multicastAddress));

Console.WriteLine($"[RECEIVER] Listening for multicast messages on [{multicastAddress}]:{multicastPort}");

while (true)
{
    try
    {
        IPEndPoint remoteEndPoint = null;

        // Receive data from the multicast group
        var data = udpClient.Receive(ref remoteEndPoint);

        // Decode the message and print with timestamp
        var message = Encoding.UTF8.GetString(data);
        Console.WriteLine($"[RECEIVER][{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Message from {remoteEndPoint}: {message}");

        // Send an echo response back to the sender
        var echoMessage = $"Echo: {message}";
        var echoData = Encoding.UTF8.GetBytes(echoMessage);
        udpClient.Send(echoData, echoData.Length, remoteEndPoint);

        Console.WriteLine($"[RECEIVER][{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Echo sent.");
    }
    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
    {
        Console.WriteLine($"[RECEIVER][{DateTime.Now:yyyy-MM-dd HH:mm:ss}] No message received for 15 seconds");
        break;
    }
}

udpClient.DropMulticastGroup(IPAddress.Parse(multicastAddress));
udpClient.Close();

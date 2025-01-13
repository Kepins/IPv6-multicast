using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

const string multicastAddress = "ff12::2:D118"; // IPv6 multicast address
const int multicastPort = 4600; // Port to send to

using var udpClient = new UdpClient(AddressFamily.InterNetworkV6);

// Set up the multicast endpoint
var remoteEndPoint = new IPEndPoint(IPAddress.Parse(multicastAddress), multicastPort);

Console.WriteLine($"[SENDER][{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Send a message to multicast group [{multicastAddress}]:{multicastPort}");

// Read a message from the user
Console.Write($"[SENDER][{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Enter your message: ");
string? message = Console.ReadLine();

if (!string.IsNullOrEmpty(message))
{
    // Convert the message to bytes
    var data = Encoding.UTF8.GetBytes(message);

    // Send the message to the multicast group
    udpClient.Send(data, data.Length, remoteEndPoint);
    Console.WriteLine($"[SENDER][{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Message sent.");

    // Receive response
    udpClient.Client.ReceiveTimeout = 3000;

    try
    {
        while (true)
        {
            IPEndPoint senderEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
            var receivedData = udpClient.Receive(ref senderEndPoint);

            string receivedMessage = Encoding.UTF8.GetString(receivedData);
            Console.WriteLine($"[SENDER][{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Response from {senderEndPoint}: {receivedMessage}");
        }
    }
    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
    {
        Console.WriteLine($"[SENDER][{DateTime.Now:yyyy-MM-dd HH:mm:ss}] No more responses received. Exiting.");
    }
}
else
{
    Console.WriteLine($"[SENDER][{DateTime.Now:yyyy-MM-dd HH:mm:ss}] No message entered. Exiting.");
}

udpClient.Close();

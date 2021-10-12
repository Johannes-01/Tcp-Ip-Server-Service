using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AstridServer
{
    class Server
    {
        static TcpListener ServerSocket;
        static bool stop;
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();


        public static void start()
        {
            int count = 1;

            TcpListener ServerSocket = new TcpListener(IPAddress.Parse(GetLocalIPAddress()), 5000);
            ServerSocket.Start();

            while (!stop)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                lock (_lock) list_clients.Add(count, client);
                Console.WriteLine("Someone connected!!");

                Thread t = new Thread(handle_clients);
                t.Start(count);
                count++;
            }
            
        }

        public static void Stop()
        {
           stop = true;
           ServerSocket.Stop();

        }

        public static void handle_clients(object o)
        {
            int id = (int)o;
            TcpClient client;

            lock (_lock) client = list_clients[id];

            while (true)
            {
                try
                {
                    if (client.Connected)
                    {
                        NetworkStream stream = client.GetStream();

                        byte[] buffer = new byte[1024];
                        int byte_count = stream.Read(buffer, 0, buffer.Length);

                        if (byte_count == 0)
                        {
                            break;
                        }

                        string data = Encoding.UTF8.GetString(buffer, 0, byte_count);
                        broadcast(data);
                        Console.WriteLine(data);
                    }
                    else
                    {
                        list_clients.Remove(id);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(GetLocalIPAddress() + " disconnected!");
                }
            }

            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }



        public static void broadcast(string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
                foreach (TcpClient c in list_clients.Values)
                {
                    NetworkStream stream = c.GetStream();

                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }


        #region HELPER
        public static string GetLocalIPAddress()

        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (ip.ToString().StartsWith("192.168.60"))  //You have to be more specific, you need to have tree numbers, not like in the MainForm.
                        return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        #endregion
    }
}

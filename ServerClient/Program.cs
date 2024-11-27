using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace Server
{
    internal class Server
    {
        internal IPAddress? ip;
        internal const int PORT = 1234;
        internal readonly DecryptData DD = new DecryptData(4819, 41, 3881);
        internal Socket? listener;
        static bool running = true;

        static void Main(string[] args)
        {
            Server p = new Server();
            p.Run();
        }

        private void Run()
        {
            Socket? client;
            listener = GetListener();

            do
            {
                client = listener.Accept();
                Task.Run(() => ServeClient(client));
                //FILL IN...


            } while (running);
            //it is not obvious how running is set to false... 
            //we need something advanced later - but not now!!

            try
            {
                client.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                client.Close();
                listener.Close();
            }
        }

        private void ServeClient(Object? c)
        {
            //Pre : c is a connected socket 
            //Post: c has type Object and must first be casted into a Socket variable.
            //      A message from client are read using GetMessage(), it is stored
            //      in variable id. Then public key (N and E) is sent to client. 
            //      Now messages are read, decrypted and written on console until message
            //      equals "STOP", then connection to client is closed
            if (c != null)
            {

                Socket client = (Socket)c;
                String[] id = GetMessage(client);
                client.Send(Encoding.ASCII.GetBytes($"{DD.N}:{DD.E}"));
                while (true)
                {
                    String[] message = GetMessage(client);
                    string decryptedMessage = Decrypt(message);
                    if (decryptedMessage == "STOP")
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine(decryptedMessage);
                    }
                }
                //FILL IN...

                try
                {
                    client.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    client.Close();
                }
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        private String Decrypt(String[] parts)
        {
            //Pre : DD holds values for decrypting (private key). 
            //Post: All strings in parts are decrypted using DD and concatenated into one string
            BigInteger bi;
            List<Char> chars = new List<Char>();

            foreach (String s in parts)
            {
                //decrypt
                bi = BigInteger.Pow(Int32.Parse(s), DD.D) % DD.N;
                char c = Convert.ToChar((int)bi);
                chars.Add(c);
            }
            return new String(chars.ToArray());
        }

        private String[] GetMessage(Socket client)
        {
            //Pre : client holds an active connection
            //Post: A message is read from client and changed into upper case. 
            //      Message can be several parts separated by ":" so it is split into array
            byte[] data = new byte[client.SendBufferSize];
            int receivedBytes = client.Receive(data);
            String receivedMessage = (Encoding.ASCII.GetString(data, 0, receivedBytes).ToUpper());

            return receivedMessage.Split(":");
        }

        private Socket GetListener()
        {
            //Pre : None
            //Post: A socket listening on a chosen IP-addres and port is returned
            ip = GetIPAddress();
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine($"Listening on : {ip} using port: {PORT}");
            listener.Bind(new IPEndPoint(ip, PORT));
            listener.Listen();

            return listener;
        }

        private IPAddress GetIPAddress()
        {
            //Pre : None
            //Post: From a menu showing all IPv4 addresses one is selected and returned
            int indx = 1;

            List<IPAddress> myIP4s = LoadIPv4();
            foreach (IPAddress ip in myIP4s)
            {
                ChColor(ConsoleColor.DarkYellow, ConsoleColor.Black);
                Console.Write($" {indx}: ");
                ChColor(ConsoleColor.Black, ConsoleColor.White);
                Console.WriteLine($" {ip} ");
                indx++;
            }
            Console.Write("\n\n Input number : ");

            if (Int32.TryParse(Console.ReadKey().KeyChar.ToString(), out int choice))
            {
                Console.Clear();
                return myIP4s[choice - 1];
            }
            else
            {
                throw new FormatException();
            }
        }

        private static List<IPAddress> LoadIPv4()
        {
            //Pre : None
            //Post: All IPv4 (= AddressFamily.InterNetwork) addresses
            //      on host is returned in a List
            string hostName = Dns.GetHostName();
            IPAddress[] myIPs = Dns.GetHostEntry(hostName).AddressList;

            List<IPAddress> myIP4s = new List<IPAddress>();
            foreach (IPAddress ip in myIPs)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    myIP4s.Add(ip);
                }
            }
            return myIP4s;
        }

        private static void ChColor(ConsoleColor bg, ConsoleColor fg)
        {
            //Pre : None
            //Post: Changed colors to bg and fg
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }
    }
}


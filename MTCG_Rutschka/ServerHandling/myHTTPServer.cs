using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MTCG_Rutschka {

    public delegate void IncomingEventHandler(object sender, ServerEventArgs e);

    /// <summary> Server to Listen to Incoming requests </summary>
    public sealed class MyHttpServer
    {
        /// <summary> The TCP Listener for the Server </summary>
        private TcpListener _listener;

        /// <summary> EventHandler to handle the Incoming Requests </summary>
        public event IncomingEventHandler Incoming;

        /// <summary>Running flag to Control if the server is Running</summary>
        public bool Running { get; set; }

        /// <summary>Runs the server</summary>
        public void Run() {
            Running = true;

            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 10001);
            _listener.Start();

            byte[] buf = new byte[256];
            int n;
            string data;

            while (Running) {
                TcpClient client = _listener.AcceptTcpClient();

                NetworkStream clientInput = client.GetStream();

                data = "";
                while (clientInput.DataAvailable || (data == "")) {
                    n = clientInput.Read(buf, 0, buf.Length);
                    data += Encoding.ASCII.GetString(buf, 0, n);
                }

                //Invoke a new Event with ServerEventArgs
                Incoming?.Invoke(this, new ServerEventArgs(data, client));
            }

            _listener.Stop();
        }
    }
}

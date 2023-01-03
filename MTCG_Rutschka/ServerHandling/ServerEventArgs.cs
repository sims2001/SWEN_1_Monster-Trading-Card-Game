using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace MTCG_Rutschka {
    /// <summary> Class for the Server Event Arguments (To handle Requests) </summary>
    public class ServerEventArgs : EventArgs {

        private TcpClient _client;

        /// <summary> empty Constructor </summary>
        public ServerEventArgs(){}

        /// <summary> Class to handle server Request Content </summary>
        /// <param name="txt"> complete Message received </param>
        /// <param name="cl"> client to respond to </param>
        public ServerEventArgs(string txt, TcpClient cl) {
            _client = cl;
            CompleteMessage = txt;
            Token = null;
            Payload = null;
            SingleValues = Array.Empty<string>();
            KeyValueJson = new JObject();
            DataJArray = new JArray();

            string[] zeilen = txt.Replace("\r\n", "\n").Replace("\r", "\n").Split("\n");
            bool readingHeaders = true;
            List<HttpHeader> headers = new List<HttpHeader>();

            for (int i = 0; i < zeilen.Length; i++) {
                if (i == 0) {
                    string[] wohin = zeilen[i].Split(" ");
                    Method = wohin[0];
                    Path = wohin[1];
                    continue;
                }

                if (readingHeaders) {
                    if (string.IsNullOrWhiteSpace(zeilen[i]))
                        readingHeaders = false;
                    else 
                        headers.Add(new HttpHeader(zeilen[i]));

                    } else {
                    Payload += zeilen[i] + "\r\n";
                }
            }
            
            foreach (var item in headers) {
                if (item.Name == "Authorization")
                    Token = item.Value;
            };
            if (!string.IsNullOrEmpty(Token))
                Username = Token.Replace("Basic ", "").Replace("-mtcgToken", "").Trim();

            //Payload = Payload.Contains("[") ? Payload.Replace("[", "").Replace("]", "") : Payload;
            Headers = headers.ToArray();

            //ABHÄNGIG VON PFAD MACHEN!!!
            _WorkWithPayload();
        }

        /// <summary> Serialize The RequestPayload </summary>
        private void _WorkWithPayload() {
            if (String.IsNullOrWhiteSpace(Payload))
                return;

            if(Payload.Contains("[") && Payload.Contains("]") && Payload.Contains(":"))
                DataJArray = JArray.Parse(Payload);
            else if (Payload.Contains(":"))
                KeyValueJson = JObject.Parse(Payload);
            else {
                SingleValues = Payload.Replace("[", "").Replace("]","").Replace("\"","").Trim().Split(",").Select(p => p.Trim()).ToArray();
            }
        }

        public string CompleteMessage {
            get; private set;
        }

        public string Method {
            get; protected set;
        }

        public string Path {
            get; protected set;
        }

        public HttpHeader[] Headers {
            get; private set;
        }

        public string Payload { get; private set; }

        public string Token { get; private set; }

        public string Username { get; private set; }

        public JObject KeyValueJson {
            get; private set;
        }

        public JArray DataJArray {
            get; private set;
        }

        public string[] SingleValues {
            get;
            private set;
        }

        /// <summary> Send Response to User </summary>
        /// <param name="status">Integer -> HTTP Status</param>
        /// <param name="statusMessage"> HTTP Status Message </param>
        /// <param name="payload"> Body of response to user (JsonData) </param>
        public virtual void ServerReply(int status, string statusMessage, string payload = null) {
            string data = "HTTP/1.1 " + status + " " + statusMessage + "\n";


            if (string.IsNullOrEmpty(payload)) {
                data += "Content-Length: 0\n";
                data += "Content-Type: text/plain\n\n";
            } else {
                data += "Content-Type: application/json\n\n";
            }

            if (payload != null)
                data += payload;

            byte[] sendBuf = Encoding.ASCII.GetBytes(data);
            _client.GetStream().Write(sendBuf, 0, sendBuf.Length);

            _client.GetStream().Close();
            _client.Dispose();
        }
    }
}

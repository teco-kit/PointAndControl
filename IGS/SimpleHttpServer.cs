using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using System.Diagnostics;

// offered to the public domain for any use with no restriction
// and also with no warranty of any kind, please enjoy. - David Jeske. 

// simple HTTP explanation
// http://www.jmarshall.com/easy/http/

namespace IGS.Server.WebServer
{
    /// <summary>
    ///     Die Klasse enthält die Argumente eines HTTPEvent
    /// </summary>
    public class HttpEventArgs : EventArgs
    {
        /// <summary>
        ///     Konstruktor für das Event
        ///     <param name="clientip">Die IP des Users, der für das Auslösen verantwortlich ist.</param>
        ///     <param name="dev">Die DeviceID des Geräts, das gesteuert werden soll</param>
        ///     <param name="cmd">Die CommandID des Befehls, der ausgeführt werden soll.</param>
        ///     <param name="val">Der Wert, der dem Befehl zugehörig ist.</param>
        ///     <param name="p">Der HttpProcessor, dem der Befehl zugehörig ist.</param>
        /// </summary>
        public HttpEventArgs(string clientip, string dev, string cmd, string val, HttpProcessor p)
        {
            ClientIp = clientip;
            Dev = dev;
            Cmd = cmd;
            Val = val;
            P = p;
        }

        /// <summary>
        ///     Die IP des Klienten.\n
        ///     Mit der "set"-Methode kann die IP gesetzt werden.\n
        ///     Mit der "get"-Methode kann die IP zurückgegeben werden.
        ///     <returns>Gibt die IP des Klienten zurück</returns>
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        ///     Die DeviceID des Gerätes, das gesteuert werden soll.\n
        ///     Mit der "set"-Methode kann die DeviceID gesetzt werden.\n
        ///     Mit der "get"-Methode kann die DeviceID zurückgegeben werden.
        ///     <returns>Gibt die DeviceID zurück</returns>
        /// </summary>
        public string Dev { get; set; }

        /// <summary>
        ///     Die CommandID des Befehls, der ausgeführt werden soll.\n
        ///     Mit der "set"-Methode kann die CommandID gesetzt werden.\n
        ///     Mit der "get"-Methode kann die CommandID zurückgegeben werden.
        ///     <returns>Gibt die CommandID zurück</returns>
        /// </summary>
        public string Cmd { get; set; }

        /// <summary>
        ///     Der übergebene Wert des Befehls, der ausgeführt werden soll.\n
        ///     Mit der "set"-Methode kann der Wert gesetzt werden.\n
        ///     Mit der "get"-Methode kann der Wert zurückgegeben werden.
        ///     <returns>Gibt den Wert des Befehls zurück</returns>
        /// </summary>
        public string Val { get; set; }

        /// <summary>
        ///     Der HTTPProcessor des Klienten.\n
        ///     Mit der "set"-Methode kann der HTTPProcessor gesetzt werden.\n
        ///     Mit der "get"-Methode kann der HTTPProcessor zurückgegeben werden.
        ///     <returns>Gibt den HTTPProcessor des Klienten zurück</returns>
        /// </summary>
        public HttpProcessor P { get; set; }
    }


    /// <summary>
    ///     Delegat für ein HTTP Event
    ///     <param name="sender">der Sender</param>
    ///     <param name="e">Die EventArgs</param>
    /// </summary>
    public delegate void HttpEventHandler(object sender, HttpEventArgs e);


    /// <summary>
    ///     Übernimmt die Verwaltung eines Clients.
    ///     Die Klasse HttpProcessor läuft in einem gesonderten Thread, der bei Ankunft eines neuen Client erstellt wird.
    ///     Die Klasse nimmt die Anfragen des Clients entgegen und deligiert sie an die Klasse HttpServer weiter.
    ///     Dort wird die Anfrage interpretiert und die Aktionen ausgeführt, die für die jeweilige Anfrage definiert sind.
    ///     @author Christopher Baumgärtner(edited)
    /// </summary>
    public class HttpProcessor
    {
        private const int BufSize = 4096;
        private const int MaxPostSize = 10*1024*1024; // 10MB

        /// <summary>
        ///     Die Header
        /// </summary>
        private Hashtable _httpHeaders = new Hashtable();

        /// <summary>
        ///     Konstruktor für HttpProcessor
        /// </summary>
        /// <param name="s">Der TCP Client</param>
        /// <param name="srv">Der HTTP Server</param>
        public HttpProcessor(TcpClient s, HttpServer srv)
        {
            Socket = s;
            Srv = srv;
        }


        /// <summary>
        ///     Der Socket der Verbindung.
        ///     Mit der "set"-Methode kann der Socket gesetzt werden.
        ///     Mit der "get"-Methode kann der Socket zurückgegeben werden.
        ///     <returns>Gibt den Socket der Verbindung zurück</returns>
        /// </summary>
        public TcpClient Socket { get; set; }

        /// <summary>
        ///     Der HTTP Server.
        ///     Mit der "set"-Methode kann der HTTP Server gesetzt werden.
        ///     Mit der "get"-Methode kann der HTTP Server zurückgegeben werden.
        ///     <returns>Gibt den HTTP Server zurück</returns>
        /// </summary>
        public HttpServer Srv { get; set; }

        /// <summary>
        ///     Der InputStream.
        ///     Mit der "set"-Methode kann der InputStream gesetzt werden.
        ///     Mit der "get"-Methode kann der InputStream zurückgegeben werden.
        ///     <returns>Gibt den InputStream zurück</returns>
        /// </summary>
        public Stream InputStream { get; set; }

        /// <summary>
        ///     Der OutputStream.
        ///     Mit der "set"-Methode kann der OutputStream gesetzt werden.
        ///     Mit der "get"-Methode kann der OutputStream zurückgegeben werden.
        ///     <returns>Gibt den OutputStream zurück</returns>
        /// </summary>
        public StreamWriter OutputStream { get; set; }

        /// <summary>
        ///     Die HTTP-Methode.
        ///     Mit der "set"-Methode kann die HTTP-Methode gesetzt werden.
        ///     Mit der "get"-Methode kann die HTTP-Methode zurückgegeben werden.
        ///     <returns>Gibt die HTTP-Methode zurück</returns>
        /// </summary>
        public string HttpMethod { get; set; }


        /// <summary>
        ///     Die HTTP-Methode.
        ///     Mit der "set"-Methode kann die HTTP-Methode gesetzt werden.
        ///     Mit der "get"-Methode kann die HTTP-Methode zurückgegeben werden.
        ///     <returns>Gibt die HTTP-Methode zurück</returns>
        /// </summary>
        public string HttpUrl { get; set; }

        /// <summary>
        ///     Der Versionsstring des Protokols.
        ///     Mit der "set"-Methode kann der Versionsstring des Protokols gesetzt werden.
        ///     Mit der "get"-Methode kann der Versionsstring des Protokols zurückgegeben werden.
        ///     <returns>Gibt den Versionsstring des Protokols zurück</returns>
        /// </summary>
        public string HttpProtocolVersionstring { get; set; }

        /// <summary>
        ///     Die HTTP-Header.
        ///     Mit der "set"-Methode können die HTTP-Header gesetzt werden.
        ///     Mit der "get"-Methode können die HTTP-Header zurückgegeben werden.
        ///     <returns>Gibt die HTTP-Header zurück</returns>
        /// </summary>
        public Hashtable HttpHeaders
        {
            get { return _httpHeaders; }
            set { _httpHeaders = value; }
        }

        private string StreamReadLine(Stream inputStream)
        {
            string data = "";
            while (true)
            {
                int nextChar = inputStream.ReadByte();
                if (nextChar == '\n')
                {
                    break;
                }
                if (nextChar == '\r')
                {
                    continue;
                }
                if (nextChar == -1)
                {
                    Thread.Sleep(1);
                    continue;
                }
                data += Convert.ToChar(nextChar);
            }
            return data;
        }

        /// <summary>
        ///     Nimmt die Anfrage entgegen, liest den Header aus und gibt die Anfrage an handleGETRequest() oder handlePOSTRequest() weiter.
        /// </summary>
        public void Process()
        {
            // we can't use a StreamReader for input, because it buffers up extra data on us inside it's
            // "processed" view of the world, and we want the data raw after the headers
            InputStream = new BufferedStream(Socket.GetStream());

            // we probably shouldn't be using a streamwriter for all output from handlers either
            OutputStream = new StreamWriter(new BufferedStream(Socket.GetStream()));
            try
            {
                ParseRequest();
                ReadHeaders();
                if (HttpMethod.Equals("GET"))
                {
                    HandleGetRequest();
                }
                else if (HttpMethod.Equals("POST"))
                {
                    HandlePostRequest();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
                WriteFailure();
            }
            
            OutputStream.Flush();
            // bs.Flush(); // flush any remaining output
            InputStream = null;
            OutputStream = null; // bs = null;            
            Socket.Close();
        }

        /// <summary>
        ///     Empfängt die Http Anfrage und splittet diese in die Bestandteile auf (http_method, http_url, http_protocol_versionstring).
        /// </summary>
        private void ParseRequest()
        {
            String request = StreamReadLine(InputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            HttpMethod = tokens[0].ToUpper();
            HttpUrl = tokens[1].Trim('/');
            HttpProtocolVersionstring = tokens[2];

            Debug.WriteLine("starting: " + request);
        }

        /// <summary>
        ///     Empfängt und verarbeitet die Http Header.
        ///     Für jeden Header wird Name und Wert ausgelesen und in httpHeaders[] gespeichert.
        /// </summary>
        private void ReadHeaders()
        {
            Debug.WriteLine("readHeaders()");
            String line;
            while ((line = StreamReadLine(InputStream)) != null)
            {
                if (line.Equals(""))
                {
                    Debug.WriteLine("got headers");
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // strip any spaces
                }

                String value = line.Substring(pos, line.Length - pos);
                Debug.WriteLine("header: {0}:{1}", name, value);
                HttpHeaders[name] = value;
            }
        }

        /// <summary>
        ///     Leitet die GET-Anfrage an den Server weiter (HttpServer.handleGetRequest).
        /// </summary>
        public void HandleGetRequest()
        {
            Srv.HandleGetRequest(this);
        }

        /// <summary>
        ///     Liest die Daten der Post-Anfrage ein und leitet die Anfrage an den Server weiter (HttpServer.handlePostRequest).
        /// </summary>
        public void HandlePostRequest()
        {
            Srv.HandlePostRequest(this, null);
        }

        /// <summary>
        ///     Antwortet, dass das Lesen erfolgreich war (200).
        /// </summary>
        public void WriteSuccess(string content_type)
        {
            OutputStream.WriteLine("HTTP/1.0 200 OK");
            OutputStream.WriteLine("Content-Type: " + content_type);
            OutputStream.WriteLine("Connection: close");
            OutputStream.WriteLine("");
        }

        /// <summary>
        ///     Antwortet, dass das Lesen fehlerhaft war (404).
        /// </summary>
        public void WriteFailure()
        {
            OutputStream.WriteLine("HTTP/1.0 404 File not found");
            OutputStream.WriteLine("Connection: close");
            OutputStream.WriteLine("");
        }
    }

    /// <summary>
    ///     Teil des Entwurfmusters: Subjekt(HttpEvent)
    ///     Die abstrakte Klasse eines HttpServers.
    ///     Die Klassen, die HttpServer implementieren werden von einem HttpProcessor aufgerufen und interpretiert die übergebenen Anfragen.
    ///     Die Methoden handleGETRequest und handlePOSTRequest übernehmen die weitere Verarbeitung der Anfrage.
    ///     @author Christopher Baumgärtner(edited)
    /// </summary>
    public abstract class HttpServer
    {
        /// <summary>
        ///     Die lokale Ip-Adresse.
        /// </summary>
        protected IPAddress LocalIp;

        private bool is_active = true;
        private TcpListener _listener;

        /// <summary>
        ///     Der Port, an dem der Server liegt.
        /// </summary>
        protected int Port;

        /// <summary>
        ///     Konstruktor für HttpServer.
        ///     <param name="port">der Port, an dem der Server liegt</param>
        ///     <param name="localIp">die Ip-Adresse des Servers</param>
        /// </summary>
        protected HttpServer(int port, IPAddress localIp)
        {
            Port = port;
            LocalIP = localIp;
        }

        /// <summary>
        ///     Die Ip-Adresse des Servers.
        ///     Mit der "set"-Methode kann die Geräte-Liste gesetzt werden.
        ///     Mit der "get"-Methode kann die Geräte-Liste zurückgegeben werden.
        ///     <returns>Gibt die Geräte-Liste zurück</returns>
        /// </summary>
        public IPAddress LocalIP
        {
            get { return LocalIp; }
            set { LocalIp = value; }
        }

        /// <summary>
        ///     Teil des Entwurfmusters: Beobachter(HttpEvent)
        /// </summary>
        public virtual event HttpEventHandler Request;

        /// <summary>
        ///     Wartet auf ankommende Clients und startet dann einen neuen Thread mit HttpProcessor und dem TCP Client.
        /// </summary>
        public void Listen()
        {
            _listener = new TcpListener(LocalIP, Port);
            _listener.Start();
            while (is_active)
            {
                TcpClient s = _listener.AcceptTcpClient();
                HttpProcessor processor = new HttpProcessor(s, this);
                Thread thread = new Thread(new ThreadStart(processor.Process));
                thread.Start();
                Thread.Sleep(1);
            }
        }

        /// <summary>
        ///     Wird aufgerufen, wenn ein Event auftritt
        ///     Teil des Entwurfmusters: Beobachter(HttpEvent)
        ///     Übernimmt die Funktion der Notify-Methode im Beobachter Entwurfsmuster.
        /// </summary>
        /// <param name="e">die EventArgs</param>
        public abstract void OnRequest(HttpEventArgs e);
       


        /// <summary>
        ///     Verarbeitet eine GET-Anfrage.
        ///     <param name="p">der HttpProcessor</param>
        /// </summary>
        public abstract void HandleGetRequest(HttpProcessor p);

        /// <summary>
        ///     Verarbeitet eine POST-Anfrage.
        ///     <param name="p">der HttpProcessor</param>
        ///     <param name="inputData">die Daten</param>
        /// </summary>
        public abstract void HandlePostRequest(HttpProcessor p, StreamReader inputData);

        /// <summary>
        ///     Sendet eine Antwort an den Client.
        /// </summary>
        /// <param name="p">der HttpProcessor, der die Verbindung mit dem Client verarbeitet.</param>
        /// <param name="msg">die Antwort</param>
        public abstract void SendResponse(HttpProcessor p, string msg);
    }


    /// <summary>
    ///     Teil des Entwurfmusters: Subjekt(HttpEvent)
    ///     Die Klasse MyHttpServers implementiert HttpServer.
    ///     Die Klasse MyHttpServer wird von einem HttpProcessor aufgerufen und interpretiert die übergebenen Anfragen.
    ///     Die Methoden handleGETRequest und handlePOSTRequest übernehmen die weitere Verarbeitung der Anfrage.
    ///     @author Christopher Baumgärtner(edited)
    /// </summary>
    public class MyHttpServer : HttpServer
    {
        /// <summary>
        ///     Konstruktor für MyHttpServer.
        ///     <param name="port">der Port, an dem der Server liegt</param>
        ///     <param name="localIp">die Ip-Adresse des Servers</param>
        /// </summary>
        public MyHttpServer(int port, IPAddress localIp)
            : base(port, localIp)
        {
        }

        /// <summary>
        ///     Teil des Entwurfmusters: Beobachter(HttpEvent)
        /// </summary>
        public override event HttpEventHandler Request;

        /// <summary>
        ///     Wird aufgerufen, wenn ein Event auftritt
        ///     Teil des Entwurfmusters: Beobachter(HttpEvent)
        ///     Übernimmt die Funktion der Notify-Methode im Beobachter Entwurfsmuster.
        /// </summary>
        /// <param name="e">die EventArgs</param>
        public override void OnRequest(HttpEventArgs e)
        {
            if (Request != null) Request(this, e);
        }

        /// <summary>
        ///     Verarbeitet eine GET-Anfrage.
        ///     Entweder wird die angeforderte Datei an den Client gesendet, oder die übergebenen Parameter verarbeitet.
        ///     <param name="p">der HttpProcessor</param>
        /// </summary>
        public override void HandleGetRequest(HttpProcessor p)
        {
            if (p.HttpUrl.EndsWith(".html")) {
                p.WriteSuccess("text/html");
                sendData(p);
            } 
            else if (p.HttpUrl.EndsWith(".css")) {
                p.WriteSuccess("text/css");
                sendData(p);
            } 
            else if (p.HttpUrl.EndsWith(".js")) {
                p.WriteSuccess("text/javascript");
                sendData(p);
            } 
            else if (p.HttpUrl.EndsWith(".jpg")) {
                p.WriteSuccess("image/jpg");
                sendData(p);
            } 
            else if (p.HttpUrl.EndsWith(".gif")) {
                p.WriteSuccess("image/gif");
                sendData(p);
            } 
            else if (p.HttpUrl.EndsWith(".png")) {
                p.WriteSuccess("image/png");
                sendData(p);
            }
            else if (p.HttpUrl.EndsWith(".ico"))
            {
                p.WriteSuccess("image/x-icon");
                sendData(p);
            } 
            else {
                p.WriteSuccess("text/html");
                NameValueCollection col = HttpUtility.ParseQueryString(p.HttpUrl);
                string device = col["dev"];
                string temp = col["cmd"];
                if (temp != null)
                {
                    string[] cmdval = temp.Split('_');
                    string command = cmdval[0];
                    string value = "";
                    if (cmdval.Length > 1) value = cmdval[1];
                    string clientIp = ((IPEndPoint)p.Socket.Client.RemoteEndPoint).Address.ToString();
                    OnRequest(new HttpEventArgs(clientIp, device, command, value, p));
                    
                }
            }

        }

        /// <summary>
        ///     Sendet die angeforderte Datei an den Client
        ///     <param name="p">der HttpProcessor</param>
        /// </summary>
        private void sendData(HttpProcessor p)
        {
            using (Stream fs = File.Open(AppDomain.CurrentDomain.BaseDirectory + "\\HttpRoot\\" + p.HttpUrl, FileMode.Open)) {
                fs.CopyTo(p.OutputStream.BaseStream);
                p.OutputStream.BaseStream.Flush();
            }
            //Stream fs = File.Open(AppDomain.CurrentDomain.BaseDirectory + "\\HttpRoot\\" + p.HttpUrl, FileMode.Open);
            //fs.CopyTo(p.OutputStream.BaseStream);
            //p.OutputStream.BaseStream.Flush();
        }

        /// <summary>
        ///     Sendet eine Antwort an den Client.
        /// </summary>
        /// <param name="p">der HttpProcessor, der die Verbindung mit dem Client verarbeitet.</param>
        /// <param name="msg">die Antwort</param>
        public override void SendResponse(HttpProcessor p, string msg)
        {
            p.OutputStream.Write(msg);
        }

        /// <summary>
        ///     Verarbeitet eine POST-Anfrage.
        ///     <param name="p">der HttpProcessor</param>
        ///     <param name="inputData">die Daten</param>
        /// </summary>
        public override void HandlePostRequest(HttpProcessor p, StreamReader inputData)
        {
        }
    }
}
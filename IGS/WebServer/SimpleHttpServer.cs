using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
    ///     This class contains the Arguments of a HTTP event     /// </summary>
    public class HttpEventArgs : EventArgs
    {
        /// <summary>
        ///     Constructor for the event 
        ///     <param name="clientip">The IP of the User, who is responsible for the triggering</param>
        ///     <param name="dev">The ID of the Device which should be controlled</param>
        ///     <param name="cmd">The ID of the command which should be executed.</param>
        ///     <param name="val">The Value belonging to the command.</param>
        ///     <param name="p">The HTTpProcessor belonging to the command.</param>
        /// </summary>
        public HttpEventArgs(String clientip, String dev, String cmd, String val,String language, HttpProcessor p)
        {
            ClientIp = clientip;
            Dev = dev;
            Cmd = cmd;
            Val = val;
            Language = language;
            P = p;
        }

        public HttpEventArgs(String clientip, String PostString, HttpProcessor p)
        {
            POSTString = PostString;
            P = p;
        }

        public String POSTString { get; set; }
        /// <summary>
        ///     The IP of the client.\n
        ///     With the "set"-method the ip can be set.\n
        ///     With the "get"-method the ip can be returned. 
        ///     <returns>Returns the IP of the client</returns>
        /// </summary>
        public String ClientIp { get; set; }

        /// <summary>
        ///     The ID of the device which will be controlled. \n
        ///     With the "set"-method the DeviceID can be set.\n
        ///     With the "get"-method the DeviceID can be returned. 
        ///     <returns>Returns the DeviceID</returns>
        /// </summary>
        public String Dev { get; set; }

        public String Language { get; set; }
        /// <summary>
        ///     The Command ID of the command which should be executed.\n
        ///     With the "set"-method the CommandID can be set.\n
        ///     With the "get"-method the CommandID can be returned. 
        ///     <returns>Returns the CommandID</returns>
        /// </summary>
        public String Cmd { get; set; }

        /// <summary>
        ///     The passed Value of the command, which should be executed.\n
        ///     With the "set"-method the value can be set.\n
        ///     With the "get"-method the value can be returned. 
        ///     <returns>Returns the value of the command</returns>
        /// </summary>
        public String Val { get; set; }

        /// <summary>
        ///     The HTTPProcessor of the client.\n
        ///     With the "set"-method the HTTPProcessor can be set.\n
        ///     With the "get"-method the HTTPProcessor can be returned. 
        ///     <returns>Returns the HTTPProcessor of the client.</returns>
        /// </summary>
        public HttpProcessor P { get; set; }
    }


    /// <summary>
    ///     delegate for a HTTP  event
    ///     <param name="sender">the sender</param>
    ///     <param name="e">the eventArgs</param>
    /// </summary>
    public delegate void HttpEventHandler(object sender, HttpEventArgs e);


    /// <summary>
    ///     Is responsible for the management of a client.
    ///     The class HttpProcessor runs in a seperate thread which will be created at arrival of a new client.
    ///     The class receives the requests of the client and passes them on to the class HttpServer.
    ///     The request will be interpreted there and the action for the corresponding request will be executed
    ///     @author Christopher Baumgärtner(edited)
    /// </summary>
    public class HttpProcessor
    {
        /// <summary>
        ///     The header
        /// </summary>
        private Hashtable _httpHeaders = new Hashtable();
        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
        /// <summary>
        ///     constructor for the HTTPProcessor 
        /// </summary>
        /// <param name="s">The TCP client</param>
        /// <param name="srv">The HTTP server</param>
        public HttpProcessor(TcpClient s, HttpServer srv)
        {
            Socket = s;
            Srv = srv;
        }


        /// <summary>
        ///     The socket of the connection.
        ///     With the "set"-method the HTTPProcessor can be set.\n
        ///     With the "get"-method the HTTPProcessor can be returned. 
        ///     <returns>Returns the socket of the connection</returns>
        /// </summary>
        public TcpClient Socket { get; set; }

        /// <summary>
        ///     The HTTP server.
        ///     With the "set"-method the HTTP server can be set.\n
        ///     With the "get"-method the HTTP server can be returned. 
        ///     <returns>Returns the HTTP server</returns>
        /// </summary>
        public HttpServer Srv { get; set; }

        /// <summary>
        ///     The InputStream
        ///     With the "set"-method the InputStream can be set.\n
        ///     With the "get"-method the InputStream can be returned. 
        ///     <returns>returns the InputStream</returns>
        /// </summary>
        public Stream InputStream { get; set; }

        /// <summary>
        ///     The OutputStream
        ///     With the "set"-method the OutputStream can be set.\n
        ///     With the "get"-method the OutputStream can be returned. 
        ///     <returns>Returns the OutputStream</returns>
        /// </summary>
        public StreamWriter OutputStream { get; set; }

        /// <summary>
        ///     The HTTP-method.
        ///     With the "set"-method the HTTP-method can be set.\n
        ///     With the "get"-method the HTTP-method can be returned. 
        ///     <returns>Returns the HTTP-method</returns>
        /// </summary>
        public String HttpMethod { get; set; }


        /// <summary>
        ///     The HTTP-URL.
        ///     With the "set"-method the HTTP-URL can be set.\n
        ///     With the "get"-method the HTTP-URL can be returned. 
        ///     <returns>Returns the HTTP-URL</returns>
        /// </summary>
        public String HttpUrl { get; set; }

        /// <summary>
        ///     The versionstring of the protokol.
        ///     With the "set"-method the versionstring of the protokolr can be set.\n
        ///     With the "get"-method the versionstring of the protokol can be returned. 
        ///     <returns>Gibt den Versionsstring des Protokols zurück</returns>
        /// </summary>
        public String HttpProtocolVersionstring { get; set; }

        /// <summary>
        ///     The HTTP-Header.
        ///     With the "set"-method the HTTP-Header can be set.\n
        ///     With the "get"-method the HTTP-Header can be returned. 
        ///     <returns>Returns the HTTP-Header</returns>
        /// </summary>
        public Hashtable HttpHeaders
        {
            get { return _httpHeaders; }
            set { _httpHeaders = value; }
        }

        private String StreamReadLine(Stream inputStream)
        {
            String data = "";
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
        ///     Receivs the request, reads the header and passes the request on to handleGETRequest() or handlePOSTRequest().
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
                OutputStream.Flush();
            }
            catch (Exception e)
            {
                
                Console.WriteLine(e.StackTrace);
                WriteFailure();
            }



            InputStream = null;
            OutputStream = null;
            Socket.Close();
        }

        /// <summary>
        ///     Receivs the http request and splits it in its parts (http_method, http_url, http_protocol_versionstring).
        /// </summary>
        private void ParseRequest()
        {
            String request = StreamReadLine(InputStream);
            
            String[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            HttpMethod = tokens[0].ToUpper();
            HttpUrl = tokens[1].Trim('/');

            Debug.WriteLine("HttpUrl: {0}", HttpUrl);
            HttpProtocolVersionstring = tokens[2];

            Debug.WriteLine("starting: " + request);
        }

        /// <summary>
        ///     Receivs and processes the Http header.
        ///     For every header the name and the value will be read and saved in httpHeader[].
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
        ///     Passes the GET-request on to the server (HttpServer.handleGetRequest). 
        /// </summary>
        public void HandleGetRequest()
        {
            Srv.HandleGetRequest(this);
        }

        private const int BUF_SIZE = 4096;
        public void HandlePostRequest()
        {
            // this post data processing just reads everything into a memory stream.
            // this is fine for smallish things, but for large stuff we should really
            // hand an input stream to the request processor. However, the input stream 
            // we hand him needs to let him see the "end of the stream" at this content 
            // length, because otherwise he won't know when he's seen it all! 

            int content_len = 0;
            MemoryStream ms = new MemoryStream();

            if (HttpHeaders.ContainsKey("Content-Length"))
            {
                content_len = Convert.ToInt32(this.HttpHeaders["Content-Length"]);
                if (content_len > MAX_POST_SIZE)
                {
                    throw new Exception(
                        String.Format("POST Content-Length({0}) too big for this simple server",
                          content_len));
                }
                byte[] buf = new byte[BUF_SIZE];
                int to_read = content_len;
                while (to_read > 0)
                {
                    Console.WriteLine("starting Read, to_read={0}", to_read);

                    int numread = this.InputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                    Console.WriteLine("read finished, numread={0}", numread);
                    if (numread == 0)
                    {
                        if (to_read == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("client disconnected during post");
                        }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }
            Srv.HandlePostRequest(this, new StreamReader(ms));
        }

        /// <summary>
        ///     Responses that the reading was successful(200).
        /// </summary>
        public void WriteSuccess(String content_type)
        {
            OutputStream.WriteLine("HTTP/1.0 200 OK");
            OutputStream.WriteLine("Content-Type: " + content_type);
            OutputStream.WriteLine("Connection: close");
            OutputStream.WriteLine("");
            OutputStream.Flush();
        }

        /// <summary>
        ///     Responses that the reading was erroneus(404).
        /// </summary>
        public void WriteFailure()
        {
            OutputStream.WriteLine("HTTP/1.0 404 File not found");
            OutputStream.WriteLine("Connection: close");
            OutputStream.WriteLine("");
        }

        /// <summary>
        ///     Responses to redirect to another URL.
        ///     Error Number 301 (Moved Permanently) - Cache - UA MUST NOT Autmatically Redirect
        ///     Error Number 302 (Found) - Only Cachable if indicated by Cache-Control or Expires header fiel. UA See 301
        ///     Error Number 303 (See Other) - MUST NOT be Cached - Should be answered with a GET method. 
        ///     Default: 301 
        /// </summary>
        public void WriteRedirect(String new_location, int errorNumber)
        {
            String error = "";

            switch (errorNumber)
            {
                case 301:
                    error = "Moved Permanently";
                    break;
                case 302:
                    error = "Found";
                    break;
                case 303:
                    error = "See Other";
                    break;
                case 307:
                    error = "Moved Temporary";
                    break;
                default:
                    error = "Found";
                    errorNumber = 302;
                    break;
            }

            OutputStream.WriteLine("HTTP/1.1 " + errorNumber + " " + error);
            OutputStream.WriteLine("Location: " + new_location);
            OutputStream.WriteLine("Cache - Control: no - cache, no - store");
            OutputStream.WriteLine("Pragma: no-cache");
            OutputStream.WriteLine("Expires: 0");
            OutputStream.WriteLine("");
            OutputStream.Flush();
            

        }


    }

    /// <summary>
    ///     Part of the design pattern: subject(HttpEvent)
    ///     the abstract class of a HttpServer.
    ///     The classes implementing HTTPServer are called by a HttpProcessor and interpretes the passed request.
    ///     The methood hadnleGETRequest and handlePOSTRequest are responsible for the follwing procession of the request.
    ///     @author Christopher Baumgärtner(edited)
    /// </summary>
    public abstract class HttpServer
    {
        /// <summary>
        ///     The local IP
        /// </summary>
        protected IPAddress LocalIp;

        private const bool is_active = true;
        private TcpListener _listener;

        /// <summary>
        ///     The port the server uses.
        /// </summary>
        protected int Port;

        /// <summary>
        ///     Constructor for the HttpServer.
        ///     <param name="port">the port which is used by the server</param>
        ///     <param name="localIp">the IP-adress of the server</param>
        /// </summary>
        protected HttpServer(int port, IPAddress localIp)
        {
            Port = port;
            LocalIP = localIp;
        }

        /// <summary>
        ///     The IP-adress of the server.
        ///     With the "set"-method the OutputStream can be set.\n
        ///     With the "get"-method the OutputStream can be returned. 
        ///     <returns>Returns the Ip-adress of the server</returns>
        /// </summary>
        public IPAddress LocalIP { get; set; }

        /// <summary>
        ///     Part of the design pattern: observer(HttpEvent)
        /// </summary>
        public virtual event HttpEventHandler Request;

        public virtual event HttpEventHandler postRequest;

        /// <summary>
        ///     Waits for arriving clients and starts a new thread with HttpProcessor and the TCP Client at arrival.
        /// </summary>
        public void Listen()
        {
            Console.WriteLine(String.Format(Properties.Resources.StartingHTTPServer, LocalIP.ToString(),Port));

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
        ///     Will be called when an event occurs
        ///     Part of the design pattern: observerTeil des Entwurfmusters: Beobachter(HttpEvent)
        ///     Inhabits the function of the notify method in the observer design pattern.
        /// </summary>
        /// <param name="e">the eventArgs</param>
        public abstract void OnRequest(HttpEventArgs e);

        public abstract void OnPOSTRequest(HttpEventArgs e);

        /// <summary>
        ///     processes a GET-request.
        ///     <param name="p">der HttpProcessor</param>
        /// </summary>
        public abstract void HandleGetRequest(HttpProcessor p);

        public abstract void HandlePostRequest(HttpProcessor p, StreamReader inputdata);

        /// <summary>
        ///     Sends a response to the client.
        /// </summary>
        /// <param name="p">the HttpProcessor processing the connection with the client.</param>
        /// <param name="msg">the response</param>
        public abstract void SendResponse(HttpProcessor p, String msg);

        //public abstract void SendDataDirect(HttpProcessor p, String msg);
    }


    /// <summary>
    ///     Part of the design pattern: subject(HttpEvent)
    ///     The class MyHttpServer implements a http server.
    ///     The class MyHttpServer is called by a HttpProcesser and implements the provided request.
    ///     The method handleGETRequest and handlePOSTRequest is responsible for the following processing of the request.
    ///     @author Christopher Baumgärtner(edited)
    /// </summary>
    public class MyHttpServer : HttpServer
    {
        /// <summary>
        ///     Constructor for MyHttpServer.
        ///     <param name="port">the port belonging to the server</param>
        ///     <param name="localIp">the ip-adress of the server</param>
        /// </summary>
        public MyHttpServer(int port, IPAddress localIp)
            : base(port, localIp)
        {
        }

        /// <summary>
        ///     Part of the design pattern: observer(HttpEvent)
        /// </summary>
        public override event HttpEventHandler Request;

        public override event HttpEventHandler postRequest;

        /// <summary>
        ///     Is called when an event occurs
        ///     Part of the design pattern: observer(HttpEvent)
        ///     Takes the place of the notify-method in the observer deisgn pattern.
        /// </summary>
        /// <param name="e">die EventArgs</param>
        public override void OnRequest(HttpEventArgs e)
        {
            if (Request != null) Request(this, e);
        }

        public override void OnPOSTRequest(HttpEventArgs e)
        {
            if (postRequest != null) postRequest(this, e);

        }

        /// <summary>
        ///     Processes the GET-request.
        ///     Either the requested file will be sent to the client or the provided parameters will be processed.
        ///     <param name="p">the HttpProcessor</param>
        /// </summary>
        public override void HandleGetRequest(HttpProcessor p)
        {

            string querystring = null;
            string pathstring = null;
            // check query part of string

            int iqs = p.HttpUrl.IndexOf('?');
            // If query string variables exist, put them in a string.
            //if(p.HttpUrl == "")
            //{
            //    p.WriteRedirect("Http://" + LocalIP + ":" + Port + "/index.html",302);
            //}
            if (iqs >= 0)
            {
                querystring = (iqs < p.HttpUrl.Length - 1) ? p.HttpUrl.Substring(iqs + 1) : String.Empty;
                pathstring = p.HttpUrl.Substring(0, iqs);
            }
            else
            {
                pathstring = p.HttpUrl;
            }


            
            if (isFileEnding(pathstring))
            {
                sendData(p);
            }
            else if (querystring != null && querystring.Length > 0) //TODO: currently we either serve files or process query parameters
            {
                NameValueCollection col = HttpUtility.ParseQueryString(querystring);
                String device = col["dev"];
                String command = col["cmd"];
                String language = p.HttpHeaders["Accept-Language"].ToString();
                language = language.Split(',')[0];

                if (device != null && command != null)
                {
                    String value = (col["val"] != null)? col["val"] : "";
                    String clientIp = ((IPEndPoint)p.Socket.Client.RemoteEndPoint).Address.ToString();
                    OnRequest(new HttpEventArgs(clientIp, device, command, value, language, p));
                } else {
                    p.WriteFailure();
                }
            }
            else
            {
                p.WriteRedirect(String.Format("Http://{0}:{1}/index.html", LocalIP, Port), 302);
            }

        }

        public override void HandlePostRequest(HttpProcessor p, StreamReader inputData)
        {
            Console.WriteLine("POST request: {0}", p.HttpUrl);
            string data = inputData.ReadToEnd();

            p.WriteSuccess("text/html");
            p.OutputStream.WriteLine("<html><body><h1>test server</h1>");
            p.OutputStream.WriteLine("<a href=/test>return</a><p>");
            p.OutputStream.WriteLine("postbody: <pre>{0}</pre>", data);
            String clientIp = ((IPEndPoint)p.Socket.Client.RemoteEndPoint).Address.ToString();

            OnPOSTRequest(new HttpEventArgs(clientIp, data, p));
        }

      
        /// <summary>
        ///     Sends the requested file to the client
        ///     <param name="p">the HttpProcessor</param>
        /// </summary>
        private void sendData(HttpProcessor p)
        {
            //TODO: Write Flexible Header usage
            String pathstring = null;
            int iqs = p.HttpUrl.IndexOf("?");
            if (iqs == -1)
            {
                pathstring = p.HttpUrl;
            }
            else if (iqs > 0)
            {
                pathstring = p.HttpUrl.Substring(0, iqs);
            }
            Debug.WriteLine("send Data: " + pathstring);

            bool continueSending = responseToFileRequest(p, pathstring);

            if (!continueSending)
            {
                return;
            }

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\HttpRoot\\" + pathstring))
            {
                using (
                    Stream fs = File.Open(AppDomain.CurrentDomain.BaseDirectory + "\\HttpRoot\\" + pathstring,
                                          FileMode.Open)
                    )
                {
                    fs.CopyTo(p.OutputStream.BaseStream);
                    p.OutputStream.BaseStream.Flush();
                }
            }
            else
            {
                p.WriteFailure();
            }
        }

        /// <summary>
        ///     Sends a response to the client.
        /// </summary>
        /// <param name="p">the HttpProcessor processing the connection with the client.</param>
        /// <param name="msg">the response</param>
        public override void SendResponse(HttpProcessor p, String msg)
        { 
            p.OutputStream.Write(msg);
        }


        public bool isFileEnding(string pathstring)
        {
            string ending = getFileEnding(pathstring);

            switch (ending)
            {
                case ".html":
                    return true;
                case ".css":
                    return true;
                case ".js":
                    return true;
                case ".json":
                    return true;
                case ".map":
                    return true;
                case ".jpg":
                    return true;
                case ".gif":
                    return true;
                case ".png":
                    return true;
                case ".ico":
                    return true;
                case ".svg":
                    return true;
                default:
                    return false;
            }
        }

        private string getFileEnding(string s)
        {
            string[] pathDotSplit = s.Split('.');
            return "." + pathDotSplit[pathDotSplit.Length - 1];
        }

        private bool responseToFileRequest(HttpProcessor p, string pathString)
        {
            string fileEnding = getFileEnding(pathString);

            switch (fileEnding)
            {
                case ".html":
                    p.WriteSuccess("text/html");
                    return true;
                case ".css":
                    p.WriteSuccess("text/css");
                    return true;
                case ".js":
                    if (pathString.Equals("js/globalize/globalize.js"))
                    {
                        string gloablizeRerout = getLocalizationPath(p, pathString);
                        p.WriteRedirect(gloablizeRerout, 302);
                        return false;
                    }
                    p.WriteSuccess("text/javascript");
                    return true;
                case ".json":
                    p.WriteSuccess("application/json");
                    return true;
                case ".map":
                    p.WriteSuccess("application/octet-stream");
                    return true;
                case ".jpg":
                    p.WriteSuccess("image/jpg");
                    return true;
                case ".gif":
                    p.WriteSuccess("image/gif");
                    return true;
                case ".png":
                    p.WriteSuccess("image/png");
                    return true;
                case ".ico":
                    p.WriteSuccess("image/x-icon");
                    return true;
                case ".svg":
                    p.WriteSuccess("image/svg+xml");
                    return true;
                default:
                    p.WriteFailure();
                    return false;
            }
        }

        public string getLocalizationPath(HttpProcessor p, string pathString)
        {
            string language = p.HttpHeaders["Accept-Language"].ToString();
            language = language.Split(',')[0];
           
            if(pathString == "js/globalize/globalize.js")
            {
                string baseServerGlobalize = "http://" + LocalIP + ":" + Port + "/js/globalize/";
                string globalizeServerPath = AppDomain.CurrentDomain.BaseDirectory + "\\HttpRoot\\js\\globalize\\" + language + "\\globalize.js";
               
                if (File.Exists(globalizeServerPath))
                { 
                    return baseServerGlobalize + language + "/globalize.js";
                } else
                {
                    return baseServerGlobalize + "default" + "/globalize.js"; 
                }
            } else
            {
                return "";
            }

        }

       
    }
}
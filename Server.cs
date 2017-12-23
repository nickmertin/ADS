using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ADS
{
    public class Server
    {
        EndPoint ep;
        ServerType type;
        ClientCallback cb;
        dynamic listener;
        public Server(EndPoint endPoint, ServerType serverType, ClientCallback callback)
        {
            ep = endPoint;
            type = serverType;
            cb = callback;
            switch (type)
            {
                case ServerType.Custom:
                    TcpListener listenerCustom = new TcpListener((IPEndPoint)ep);
                    listenerCustom.Start();
                    listenerCustom.BeginAcceptTcpClient(callbackCustom, listenerCustom);
                    listener = listenerCustom;
                    break;
                case ServerType.File:
                    TcpListener listenerFile = new TcpListener((IPEndPoint)ep);
                    listenerFile.Start();
                    listenerFile.BeginAcceptTcpClient(callbackFile, listenerFile);
                    listener = listenerFile;
                    break;
                case ServerType.HTTP:
                    HttpListener listenerHttp = new HttpListener();
                    listenerHttp.Prefixes.Add("http://" + ep.ToString() + "/");
                    listenerHttp.Start();
                    listenerHttp.BeginGetContext(callBackHTTP,listenerHttp);
                    listener = listenerHttp;
                    break;
                case ServerType.HTTPS:
                    HttpListener listenerHttps = new HttpListener();
                    listenerHttps.Prefixes.Add("https://" + ep.ToString() + "/");
                    listenerHttps.Start();
                    listenerHttps.BeginGetContext(callBackHTTP, listenerHttps);
                    listener = listenerHttps;
                    break;
            }
        }
        public EndPoint EndPoint
        {
            get
            {
                return ep;
            }
        }
        public ServerType Type
        {
            get
            {
                return type;
            }
        }
        void callbackCustom(IAsyncResult result)
        {
            cb(((TcpListener)result.AsyncState).EndAcceptTcpClient(result));
        }
        void callbackFile(IAsyncResult result)
        {
            NetworkStream stream = ((TcpListener)result.AsyncState).EndAcceptTcpClient(result).GetStream();
            StreamReader r = new StreamReader(stream);
            StreamWriter w = new StreamWriter(stream);
            w.Write(File.ReadAllText((string)cb(r.ReadToEnd())));
        }
        void callBackHTTP(IAsyncResult result)
        {
            HttpListenerContext context = ((HttpListener)result.AsyncState).EndGetContext(result);
            StreamWriter w = new StreamWriter(context.Response.OutputStream);
            w.Write((string)cb(context.Request));
        }
        public void Dispose()
        {
            listener.Stop();
        }
    }
    public delegate object ClientCallback(object info);
    public enum ServerType { Custom, HTTP, File, HTTPS }
}
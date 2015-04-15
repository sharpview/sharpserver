using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

//-----------------------------------------------------------------
//
//    Sharpserver - Embedded Webserver
//             http://github.com/muis/Sharpserver
//
//    This library is free software; you can redistribute it 
//    and modify it under the terms of the GNU General Public
//    License as published by the Free Software Foundation.
//
//-----------------------------------------------------------------

namespace HTTP.Net
{
    public enum Library
    {
        Mono, // Mono listener on Mono framework
        Hybrid, // Mono listener on MS framework
        Microsoft, // MS listener on MS framework
    }

    public class HttpListener : IHttpListener
    {
        IHttpListener zListen = new NativeListener();

        public void Start()
        {
            List<string> sPrefixes = GetPrefixes(zListen.Prefixes);

            Close();

            try
            {
                zListen = new NativeListener();
                CopyPrefixes(sPrefixes, zListen.Prefixes);

                // Call below will throw 'Access denied' when called as an 'User'.
                // We avoid this by using a Mono fallback, but it can be fixed by
                // running the following command with 'Administrator' privileges:
                //
                // 'netsh http add urlacl http://+:[PORT]/ user=\Everyone'
                //

                zListen.Start();

            }
            catch
            {
                try
                {
                    // When the MS listener throws the error
                    // we use the Mono listener as a fallback,
                    // since it doesn't require Admin privileges.

                    zListen = new MonoListener();
                    CopyPrefixes(sPrefixes, zListen.Prefixes);

                    zListen.Start();
                }

                catch (System.Net.HttpListenerException hex)
                {
                    throw new Exception(hex.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        ~HttpListener()
        {
            Close();
        }

        public void Abort()
        {
            if (zListen != null)
            {
                try { zListen.Abort(); }
                catch { }
            }
        }

        public void Stop()
        {
            if (zListen != null)
            {
                try { zListen.Stop(); }
                catch { }
            }
        }

        public void Close()
        {
            if (zListen != null)
            {
                Abort();
                try { zListen.Close(); }
                catch { }
                zListen = null;
            }
        }

        private void CopyPrefixes(List<string> zFrom, HttpPrefixes zTo)
        {
            zTo.Clear();
            foreach (string pp in zFrom) { zTo.Add(pp); }
        }

        private List<string> GetPrefixes(HttpPrefixes zIn)
        {
            List<string> zOut = new List<string>();
            foreach (string pp in zIn) { zOut.Add(pp); }
            return zOut;
        }

        public Library Library
        {
            get { return zListen.Library; }
        }

        public HttpPrefixes Prefixes
        {
            get { return zListen.Prefixes; }
        }

        public HTTP.Net.HttpContext GetContext
        {
            get { return zListen.GetContext; }
        }

        public HTTP.Net.HttpContext EndGetContext(IAsyncResult asyncResult)
        {
            return zListen.EndGetContext(asyncResult);
        }

        public IAsyncResult BeginGetContext(AsyncCallback callback, Object state)
        {
            return zListen.BeginGetContext(callback, state);
        }
    }

    public interface IHttpListener
    {
        void Stop();
        void Start();
        void Abort();
        void Close();

        HTTP.Net.Library Library { get; }
        HTTP.Net.HttpPrefixes Prefixes { get; }
        HTTP.Net.HttpContext GetContext { get; }
        HTTP.Net.HttpContext EndGetContext(IAsyncResult asyncResult);
        IAsyncResult BeginGetContext(AsyncCallback callback, Object state);
    }

    public interface HttpContext
    {
        DateTime Timestamp { get; }
        HTTP.Net.HttpRequest Request { get; }
        HTTP.Net.HttpResponse Response { get; }
        HTTP.Net.HttpSessionState Session { get; }
    }

    public interface HttpRequest
    {
        bool IsLocal { get; }
        bool KeepAlive { get; }
        bool IsAuthenticated { get; }
        bool IsSecureConnection { get; }

        string ContentType { get; }
        string HttpMethod { get; }
        long ContentLength64 { get; }

        Uri Url { get; }
        Uri UrlReferrer { get; }
        Stream InputStream { get; }
        Encoding ContentEncoding { get; }
        NameValueCollection Form { get; }
        NameValueCollection Headers { get; }
        NameValueCollection QueryString { get; }
        NameValueCollection ServerVariables { get; }
        HTTP.Net.HttpFileCollection Files { get; }

        string Path { get; set; }
        string PhysicalPath { get; set; }

        string UserAgent { get; }
        string UserHostAddress { get; }
        string UserHostName { get; }
        string[] UserLanguages { get; }
        string[] AcceptTypes { get; }
        string RequestType { get; }
        int ClientCertificateError { get; }
        Version ProtocolVersion { get; }
        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }
        System.Net.CookieCollection Cookies { get; }
    }

    public interface HttpResponse
    {
        void Abort();
        void Close();
        void Redirect(string url);

        bool KeepAlive { get; set; }
        bool SendChunked { get; set; }

        TextWriter Output { get; }
        int Expires { get; set; }
        int StatusCode { get; set; }
        Stream Filter { get; set; }
        Stream OutputStream { get; }
        string ContentType { get; set; }
        long ContentLength64 { get; set; }
        string StatusDescription { get; set; }
        System.Net.WebHeaderCollection Headers { get; }
        Encoding ContentEncoding { get; set; }
        string[] FileDependencies { get; }
        DateTime ExpiresAbsolute { get; set; }
        HTTP.Net.CustomCachePolicy Cache { get; set; }

        string RedirectLocation { get; set; }
        Version ProtocolVersion { get; set; }

        void SetCookie(System.Net.Cookie cookie);
        void AppendCookie(System.Net.Cookie cookie);
        void AddHeader(string name, string value);
        void AppendHeader(string name, string value);
    }

    internal interface IExtReponse
    {
        int Expires { get; set; }
        TextWriter Output { get; }
        Stream Filter { get; set; }
        string[] FileDependencies { get; }
        DateTime ExpiresAbsolute { get; set; }
        HTTP.Net.CustomCachePolicy Cache { get; set; } 
    }

    internal interface IExtContext
    {
        DateTime Timestamp { get; }
        HTTP.Net.HttpSessionState Session { get; }
    }

    public interface HttpPrefixes
    {
        void Clear();
        int Count { get; }
        bool IsReadOnly { get; }
        bool IsSynchronized { get; }
        void Add(string uriPrefix);
        bool Contains(string uriPrefix);
        void CopyTo(string[] array, int offset);
        IEnumerator<string> GetEnumerator();
        bool Remove(string uriPrefix);
    }

    internal class ExtReponse : IExtReponse
    {
        TextWriter zWriter;
        HTTP.Net.HttpResponse zResp;
        string[] zFiles = null; // TODO
        HTTP.Net.CustomCachePolicy zCache = new CustomCachePolicy();
        HTTP.Net.InputFilterStream zFilter = new HTTP.Net.InputFilterStream();

        internal ExtReponse(HTTP.Net.HttpResponse hResp)
        {
            zResp = hResp;
        }

        public int Expires
        {
            get { return Cache.ExpireMinutes(); }
            set { Cache.SetExpires(DateTime.Now + new TimeSpan(0, value, 0)); }
        }

        public TextWriter Output 
        {
            get {
                if ((zWriter == null) && (zResp != null))
                {
                    StreamWriter sw = new StreamWriter(zResp.OutputStream);
                    sw.AutoFlush = true;
                    zWriter = sw;
                }               
                return zWriter;
            } 
        }

        public string[] FileDependencies { get { return zFiles; } }
        public HTTP.Net.CustomCachePolicy Cache { get { return zCache; } set { zCache = value; } }
        public Stream Filter { get { return zFilter; } set { zFilter = (HTTP.Net.InputFilterStream)value; } }
        public DateTime ExpiresAbsolute { get { return Cache.Expires; } set { Cache.SetExpires(value); } }
    }

    internal interface IExtRequest
    {
        string Path { get; set; }
        string PhysicalPath { get; set; }

        NameValueCollection Form { get; }
        HTTP.Net.HttpFileCollection Files { get; }
        NameValueCollection ServerVariables { get; }
    }

    internal class ExtRequest : IExtRequest
    {
        string zSelf = null; 
        string zPath = null;

        HTTP.Net.HttpRequest zReq;
        HTTP.Net.ServerVariablesCollection zVariables;
        HTTP.Net.WebROCollection zForm = new HTTP.Net.WebROCollection();
        HTTP.Net.HttpFileCollection zFiles = new HTTP.Net.HttpFileCollection();

        public ExtRequest(HTTP.Net.HttpRequest hReq)
        {
            zReq = hReq;
            zVariables = new HTTP.Net.ServerVariablesCollection(zReq);
        }

        public string Path { get { return zSelf; } set { zSelf = value; } }
        public string PhysicalPath { get { return zPath; } set { zPath = value; } }

        public NameValueCollection Form { get { return zForm; } }
        public HTTP.Net.HttpFileCollection Files { get { return zFiles; } }
        public NameValueCollection ServerVariables { get { return zVariables; } }
    }

    internal class ExtContext : IExtContext
    {
        private HTTP.Net.HttpContext zContext;
        private HTTP.Net.HttpSessionState zSession;
        private DateTime TimeStamp = DateTime.Now;

        internal ExtContext(HTTP.Net.HttpContext hContext)
        {
            zContext = hContext;
        }
        
        public HTTP.Net.HttpSessionState Session { get { return zSession; } }
        public DateTime Timestamp { get { return TimeStamp.ToLocalTime(); } }
    }

    internal class NativeResponse : HTTP.Net.HttpResponse, IExtReponse
    {
        ExtReponse eResp;
        System.Net.HttpListenerResponse zResp;

        internal NativeResponse(System.Net.HttpListenerResponse hResp)
        {
            zResp = hResp;
            eResp = new ExtReponse(this);
        }

        public void Abort() { zResp.Abort(); }
        public void Close() { zResp.Close(); }

        public void Redirect(string url) { zResp.Redirect(url); }
        public void SetCookie(System.Net.Cookie cookie) { zResp.SetCookie(cookie); }
        public Stream OutputStream { get { return zResp.OutputStream; } }
        public System.Net.WebHeaderCollection Headers { get { return zResp.Headers; } }
        public void AppendCookie(System.Net.Cookie cookie) { zResp.AppendCookie(cookie); }
        public void AddHeader(string name, string value) { zResp.AddHeader(name, value); }
        public void AppendHeader(string name, string value) { zResp.AppendHeader(name, value); }
        public bool KeepAlive { get { return zResp.KeepAlive; } set { zResp.KeepAlive = value; } }
        public int StatusCode { get { return zResp.StatusCode; } set { zResp.StatusCode = value; } }
        public bool SendChunked { get { return zResp.SendChunked; } set { zResp.SendChunked = value; } }
        public string ContentType { get { return zResp.ContentType; } set { zResp.ContentType = value; } }
        public long ContentLength64 { get { return zResp.ContentLength64; } set { zResp.ContentLength64 = value; } }
        public Encoding ContentEncoding { get { return zResp.ContentEncoding; } set { zResp.ContentEncoding = value; } }
        public Version ProtocolVersion { get { return zResp.ProtocolVersion; } set { zResp.ProtocolVersion = value; } }
        public string RedirectLocation { get { return zResp.RedirectLocation; } set { zResp.RedirectLocation = value; } }
        public string StatusDescription { get { return zResp.StatusDescription; } set { zResp.StatusDescription = value; } }

        public TextWriter Output { get { return eResp.Output; } }
        public string[] FileDependencies { get { return eResp.FileDependencies; } }
        public int Expires { get { return eResp.Expires; } set { eResp.Expires = value; } }
        public HTTP.Net.CustomCachePolicy Cache { get { return eResp.Cache; } set { eResp.Cache = value; } }
        public Stream Filter { get { return eResp.Filter; } set { eResp.Filter = (HTTP.Net.InputFilterStream)value; } }
        public DateTime ExpiresAbsolute { get { return eResp.Cache.Expires; } set { eResp.Cache.SetExpires(value); } }
    }

    internal class NativeRequest : HTTP.Net.HttpRequest, IExtRequest
    {
        ExtRequest eReq;
        System.Net.HttpListenerRequest zReq;

        internal NativeRequest(System.Net.HttpListenerRequest hReq)
        {
            zReq = hReq;
            eReq = new ExtRequest(this);
        }

        public Uri Url { get { return zReq.Url; } }
        public bool IsLocal { get { return zReq.IsLocal; } }
        public bool KeepAlive { get { return zReq.KeepAlive; } }
        public string UserAgent { get { return zReq.UserAgent; } }
        public string HttpMethod { get { return zReq.HttpMethod; } }
        public Uri UrlReferrer { get { return zReq.UrlReferrer; } }
        public string RequestType { get { return zReq.HttpMethod; } }
        public Stream InputStream { get { return zReq.InputStream; } }
        public string ContentType { get { return zReq.ContentType; } }
        public string UserHostName { get { return zReq.UserHostName; } }
        public string[] AcceptTypes { get { return zReq.AcceptTypes; } }
        public NameValueCollection Headers { get { return zReq.Headers; } }
        public string[] UserLanguages { get { return zReq.UserLanguages; } }
        public string UserHostAddress { get { return zReq.UserHostAddress; } }
        public long ContentLength64 { get { return zReq.ContentLength64; } }
        public IPEndPoint LocalEndPoint { get { return zReq.LocalEndPoint; } }
        public bool IsAuthenticated { get { return zReq.IsAuthenticated; } }
        public Version ProtocolVersion { get { return zReq.ProtocolVersion; } }
        public Encoding ContentEncoding { get { return zReq.ContentEncoding; } }
        public IPEndPoint RemoteEndPoint { get { return zReq.RemoteEndPoint; } }
        public bool IsSecureConnection { get { return zReq.IsSecureConnection; } }
        public NameValueCollection QueryString { get { return zReq.QueryString; } }
        public System.Net.CookieCollection Cookies { get { return zReq.Cookies; } }
        public int ClientCertificateError { get { return zReq.ClientCertificateError; } }

        public NameValueCollection Form { get { return eReq.Form; } }
        public HTTP.Net.HttpFileCollection Files { get { return eReq.Files; } }
        public NameValueCollection ServerVariables { get { return eReq.ServerVariables; } }

        public string Path { get { return eReq.Path; } set { eReq.Path = value; } }
        public string PhysicalPath { get { return eReq.PhysicalPath; } set { eReq.PhysicalPath = value; } }
    }

    internal class NativeContext : HTTP.Net.HttpContext, IExtContext
    {
        private ExtContext eContext;
        private HTTP.Net.HttpRequest zRequest;
        private HTTP.Net.HttpResponse zResponse;

        private System.Net.HttpListenerContext zContext;

        internal NativeContext(System.Net.HttpListenerContext hContext)
        {
            zContext = hContext;

            eContext = new ExtContext(this);
            zRequest = new NativeRequest(zContext.Request);
            zResponse = new NativeResponse(zContext.Response);
        }

        public DateTime Timestamp { get { return eContext.Timestamp; } }
        public HTTP.Net.HttpRequest Request { get { return zRequest; } }
        public HTTP.Net.HttpResponse Response { get { return zResponse; } }
        public HTTP.Net.HttpSessionState Session { get { return eContext.Session; } }
    }

    internal class NativeListener : IHttpListener
    {
        private System.Net.HttpListener hListen;

        internal NativeListener()
        {
            hListen = new System.Net.HttpListener();
        }

        public void Start() { hListen.Start(); }
        public void Stop() { hListen.Stop(); }
        public void Abort() { hListen.Abort(); }
        public void Close() { hListen.Close(); }

        public HTTP.Net.Library Library
        {
            get
            {
                Type t = Type.GetType("Mono.Runtime");
                if (t != null) { return Library.Mono; }
                return Library.Microsoft;
            }
        }

        public HttpPrefixes Prefixes 
        { 
            get { return new HttpListenerPrefixes(hListen.Prefixes); } 
        }

        public HTTP.Net.HttpContext GetContext 
        { 
            get  { return new NativeContext(hListen.GetContext()); } 
        }

        public HTTP.Net.HttpContext EndGetContext(IAsyncResult asyncResult) 
        {
            return new NativeContext(hListen.EndGetContext(asyncResult)); 
        }

        public IAsyncResult BeginGetContext(AsyncCallback callback, Object state) 
        { 
            return hListen.BeginGetContext(callback, state); 
        }
    } // <X1x56ahbTLY>

    internal class MonoListener : IHttpListener
    {
        private HTTP.Net.HttpListen hListen = new HttpListen();

        public void Start() { hListen.Start(); }
        public void Stop() { hListen.Stop(); }
        public void Abort() { hListen.Abort(); }
        public void Close() { hListen.Close(); }

        internal MonoListener()
        {
        }

        public HTTP.Net.Library Library
        {
            get
            {
                Type t = Type.GetType("Mono.Runtime");
                if (t != null) { return Library.Mono; }
                return Library.Hybrid;
            }
        }

        public HttpPrefixes Prefixes 
        { 
            get { return hListen.Prefixes; } 
        }
        
        public HTTP.Net.HttpContext GetContext 
        { 
            get 
            {
                MonoContext mContext = hListen.GetContext();
                return mContext;
            } 
        }
        
        public IAsyncResult BeginGetContext(AsyncCallback callback, Object state) 
        { 
            return hListen.BeginGetContext(callback, state); 
        }
        
        public HTTP.Net.HttpContext EndGetContext(IAsyncResult asyncResult) 
        { 
            MonoContext mContext = hListen.EndGetContext(asyncResult);
            return mContext;
        }
    } 

    internal class HttpListenerPrefixes : ICollection<string>, IEnumerable<string>, IEnumerable, HttpPrefixes 
    {
        private System.Net.HttpListenerPrefixCollection zPref;

        internal HttpListenerPrefixes(System.Net.HttpListenerPrefixCollection hPref)
        {
            zPref = hPref;
        }

        public void Clear() { zPref.Clear(); }
        public int Count { get { return zPref.Count; } }
        public bool IsReadOnly { get { return zPref.IsReadOnly; } }
        public bool IsSynchronized { get { return zPref.IsSynchronized; } }
        public void Add(string uriPrefix) { zPref.Add(uriPrefix); }
        public bool Contains(string uriPrefix) { return zPref.Contains(uriPrefix); }
        public void CopyTo(string[] array, int offset) { zPref.CopyTo(array, offset); }
        public IEnumerator<string> GetEnumerator() { return zPref.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return zPref.GetEnumerator(); }
        public bool Remove(string uriPrefix) { return zPref.Remove(uriPrefix); }
    }

    internal class HttpPrefixesList : ICollection<string>, IEnumerable<string>, IEnumerable, HttpPrefixes
    {
        private List<string> zPref = new List<string>();
        public void Clear() { zPref.Clear(); }
        public int Count { get { return zPref.Count; } }
        public bool IsReadOnly { get { return false; } }
        public bool IsSynchronized { get { return false; } }
        public void Add(string uriPrefix) { zPref.Add(uriPrefix); }
        public bool Contains(string uriPrefix) { return zPref.Contains(uriPrefix); }
        public void CopyTo(string[] array, int offset) { zPref.CopyTo(array, offset); }
        public IEnumerator<string> GetEnumerator() { return zPref.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return zPref.GetEnumerator(); }
        public bool Remove(string uriPrefix) { return zPref.Remove(uriPrefix); }
    }

    internal static class MIME
    {
        public static string FindType(string Extension)
        {
            string cType;

            switch (Extension)
            {
                case "htm":
                case "html":
                case "php":
                case "asp":
                case "aspx":
                    cType = "text/html";
                    break;
                case "css":
                    cType = "text/css";
                    break;
                case "txt":
                case "inc":
                    cType = "text/plain";
                    break;
                case "xml":
                    cType = "text/xml";
                    break;
                case "bmp":
                    cType = "image/bmp";
                    break;
                case "png":
                    cType = "image/png";
                    break;
                case "gif":
                    cType = "image/gif";
                    break;
                case "jpg":
                case "jpeg":
                    cType = "image/jpeg";
                    break;               
                case "rss":
                    cType = "application/rss+xml";
                    break;
                case "js":
                    cType = "application/javascript";
                    break;
                case "zip":
                    cType = "application/zip";
                    break;
                case "pdf":
                    cType = "application/pdf";
                    break;
                case "swf":
                    cType = "application/x-shockwave-flash";
                    break;
                case "xaml":
                    cType = "application/xaml+xml";
                    break;
                case "xap":
                    cType = "application/x-silverlight-app";
                    break;
                case "xbap":
                    cType = "application/x-ms-xbap";
                    break;
                default:
                    cType = "text/plain";
                    break;
            }
            return cType;
        }
    }
} // <VzDKNZKY8Mg>

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using HTTP.Net;

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

namespace Sharpserver
{
    public interface Webhandler
    {
        string VirtualDirectory { get; }
        bool Process(HTTP.Net.HttpContext context);
    }

    public class Webserver
    {
        private string sRoot;
        private int iPort = 80;

        private HTTP.Net.HttpListener Listener;
        private CancellationTokenSource vToken;

        private Webhandler CustomHandler = null;
        private Encoding Latin = Encoding.GetEncoding("iso-8859-1");

        ~Webserver()
        {
            Close();
        }

        public void Start()
        {
            Listener = new HTTP.Net.HttpListener();
            vToken = new CancellationTokenSource();

            Listener.Prefixes.Add("http://+:" + iPort + "/");
            Listener.Start();

            IAsyncResult result = Listener.BeginGetContext(new AsyncCallback(WebRequestCallback), Listener);
        }

        protected internal void WebRequestCallback(IAsyncResult result)
        {
            if (this.Listener == null) { return; }
            if (this.vToken.IsCancellationRequested) { return; }

            HTTP.Net.HttpContext context = this.Listener.EndGetContext(result);
            this.Listener.BeginGetContext(new AsyncCallback(WebRequestCallback), this.Listener);

            Action PR = (Action)(() => HandleRequest(context));
            Task tPR = new Task(PR, vToken.Token, TaskCreationOptions.None);

            tPR.Start();
        }

        internal void ProcessRequest(HTTP.Net.HttpContext context)
        {
            string sFile = "";
            string sDir = "";
            string sHeader = "";
            bool bCustom = false;

            Uri uri = context.Request.Url;
            StringBuilder sb = new StringBuilder();

            if ((uri == null) || (uri.LocalPath.Length < 2))
            {
                sFile = sRoot + "\\index.htm"; // default
            }
            else
            {
                sDir = Path.GetDirectoryName(uri.LocalPath);
                if (sDir.Length > 1) { sDir = sDir + Path.DirectorySeparatorChar; }
                sFile = Path.GetFullPath(sRoot + sDir + Path.GetFileName(uri.LocalPath));
            }

            if ((Custom != null) && (Custom.VirtualDirectory != null))
            {
                bCustom = (Custom.VirtualDirectory == sDir.Replace("\\", ""));
                if (!(bCustom)) { bCustom = (Custom.VirtualDirectory == Path.GetFileName(uri.LocalPath)); }
            }

            if ((!(bCustom)) && (!File.Exists(sFile)))
            {
                HTTP.Net.HttpResponse Resp = context.Response;

                sHeader = "404 Not Found";
                Resp.StatusCode = (int)HttpStatusCode.NotFound;
                sb.Append("The requested URL " + sDir + Path.GetFileName(uri.LocalPath) + " was not found.");
                ShowError(404, sHeader, sb.ToString(), new HttpException(404, sHeader), context);
                return;
            }

            Webhandler wh;
            string sError = "";
            Exception le = null;

            try
            {
                if (bCustom)
                {
                    wh = CustomHandler;
                    if (wh.Process(context)) { return; }
                    throw new Exception(sFile);
                }

                HTTP.Net.HttpRequest Req = context.Request;
                HTTP.Net.HttpResponse Resp = context.Response;

                Req.PhysicalPath = sFile;
                Req.Path = sDir + Path.GetFileName(uri.LocalPath);

                Resp.ContentEncoding = Latin;
                Resp.Headers.Add("Server", "Sharpserver");
                Resp.ContentType = MIME.FindType(Path.GetExtension(sFile).Substring(1));

                switch (Path.GetExtension(sFile).Substring(1))
                {
                    case "asp":
                    case "aspx":
                        wh = new ASPHandler();
                        break;

                    case "php":
                        wh = new PHPHandler();
                        break;

                    default:
                        wh = new StaticFile();
                        break;
                }

                if (wh.Process(context)) { return; }

                sError = "Unknown error occured";
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    le = e;
                    sError = sError + e.Message + "<br>";
                }
            }
            catch (Exception ex)
            {
                le = ex;
                sError = ex.Message;
            }

            sb.Append(sError);
            sHeader = "Internal Error";
            context.Response.StatusCode = 500;
            ShowError(context.Response.StatusCode, sHeader, sb.ToString(), le, context);

        }

        public Webserver(string RootFolder = "")
        {
            sRoot = RootFolder;            

            if (sRoot.Length > 0)
            {
                if ((sRoot.EndsWith("/") || sRoot.EndsWith("\\")))
                { sRoot = sRoot.Substring(0, sRoot.Length - 1); }
                if (!(Directory.Exists(sRoot))) { sRoot = ""; }
            }
        }

        public void Close()
        {
            try
            {
                if (vToken != null)
                {
                    if (!(vToken.IsCancellationRequested))
                    {
                        vToken.Cancel(true);
                    }
                    vToken = null;
                }
            }
            catch { }

            try
            {
                if (Listener != null)
                {
                    Listener.Close();
                    Listener = null;
                }
            }
            catch { }
        }

        public int Port
        {
            get { return iPort; }
            set { iPort = value; }
        }

        public string URL
        {
            get { return "http://" + Environment.MachineName + ":" + iPort + "/"; }
        }

        internal void HandleRequest(HTTP.Net.HttpContext context)
        {
            string sErr = "";

            try
            {
                Action PR = (Action)(() => ProcessRequest(context));
                Task tPR = new Task(PR, vToken.Token, TaskCreationOptions.None);

                tPR.Start();
                tPR.Wait(vToken.Token);
                return;
            }
            catch (AggregateException ae)
            {
                Exception le = null;
                foreach (var e in ae.InnerExceptions)
                {
                    le = e;
                    sErr = sErr + e.Message + "<P>";
                }
                ShowError(500, "Error handling request", sErr, le, context);
            }
            catch (Exception ex)
            {
                ShowError(500, "Error handling request", ex.Message, ex, context);
            }
        }

        public Webhandler Custom
        {
            get { return CustomHandler; }
            set { CustomHandler = value; }
        }
       
        internal void ShowError(int Code, string sHeader, string sError, Exception ex, HTTP.Net.HttpContext context)
        {
            try
            {
                HttpException he = new HttpException(Code, sHeader, ex, sError);
                byte[] bOut = Latin.GetBytes(he.GetHtmlErrorMessage(context, Listener));

                HTTP.Net.HttpResponse Resp = context.Response;

                try
                {
                    Resp.Headers.Add("Expires", "0");
                    Resp.Headers.Add("Pragma", "no-cache");
                    Resp.Headers.Add("Cache-Control", "no-cache, must-revalidate");
                    Resp.Headers.Add("Cache-Control", "pre-check=0, post-check=0, max-age=0");
                }
                catch { }

                try
                {
                    Resp.ContentLength64 = bOut.Length;
                    Resp.StatusDescription = sHeader + ": " + sError;
                }
                catch { }

                try
                {
                    if (Resp.StatusCode != (int)HttpStatusCode.NotFound) { Resp.StatusCode = (int)HttpStatusCode.InternalServerError; };
                }
                catch { }

                Resp.OutputStream.Write(bOut, 0, bOut.Length);
                Resp.OutputStream.Flush();

            }
            catch (Exception x)
            {
                //Console.Write(ex.Message); 
            }

            try
            {
                context.Response.OutputStream.Close();
            }
            catch { }
        }
    }
} // <3UzxTIYdX40>


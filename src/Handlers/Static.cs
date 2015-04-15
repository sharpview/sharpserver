using System;
using System.IO;
using System.Net;
using System.Text;
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

using HTTP.Net;

namespace Sharpserver
{
    internal class StaticFile : Webhandler 
    {
        public StaticFile() { }

        public string VirtualDirectory
        {
            get { return null; }
        }

        public bool Process(HttpContext context)
        {
            HttpResponse Resp = context.Response;
            NameValueCollection nv = context.Request.Headers;
            FileInfo fi = new FileInfo(context.Request.PhysicalPath);

            foreach (string key in nv.AllKeys)
            {
                switch (key.ToLower())
                {
                    case "if-modified-since":
                        
                        long mDate = DateTime.Parse(nv.GetValues(key)[0]).ToFileTimeUtc();

                        if (mDate >= fi.LastWriteTimeUtc.AddSeconds(-1).ToFileTimeUtc())
                        {
                            Resp.StatusCode = (int)HttpStatusCode.NotModified;
                            Resp.OutputStream.Close();
                            return true;
                        }

                        break;

                    //case "content-range": 
                    //    // (first-byte-pos "-" last-byte-pos)
                    //    break;

                    //case "if-range":
                    //    break;
                }
            }

            Resp.Headers.Add("Cache-Control", "must-revalidate");
            Resp.Headers.Add("Last-Modified", fi.LastWriteTimeUtc.ToString("s"));

            FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);
           
            byte[] b = new byte[fs.Length];

            Resp.StatusCode = (int)HttpStatusCode.OK;
            Resp.ContentLength64 = b.Length;
            
            fs.Read(b, 0, (int)(fs.Length));
            fs.Close();

            Resp.OutputStream.Write(b, 0, b.Length);
            Resp.OutputStream.Flush();
            Resp.OutputStream.Close();

            return true;
        }
    }
} // <lVL-zZnD3VU>
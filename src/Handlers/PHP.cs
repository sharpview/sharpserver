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

#if PHP
    using PHP.Core;
#endif

namespace Sharpserver
{
    internal class PHPHandler : Webhandler 
    {
        internal  PHPHandler() { }

        public bool Process(HTTP.Net.HttpContext context)
        {
            HTTP.Net.HttpResponse Resp = context.Response;
            NameValueCollection cParam = context.Request.QueryString;

#if PHP
            // --------------------------------------------
            // Dependency: Phalanger
            // --------------------------------------------

            Resp.StatusCode = 200;

            try
            {
                PHP.Core.PageFactory pf = new PageFactory();
                IHttpHandler ih = pf.GetHandler(context, context.Request.RequestType, context.Request.Url.AbsoluteUri, context.Request.PhysicalPath);
                
                ih.ProcessRequest(context);
                Resp.Close();

            }
            catch (PhpException pex)
            {
                throw new Exception(pex.Message + " in file " + pex.DebugInfo.File + " on line " + pex.DebugInfo.Line);
            }
            catch (Exception ex)
            {
                throw;
            }

            return true;
#else
            throw new Exception("PHP support disabled");
#endif
        }

        public string VirtualDirectory
        {
            get { return null; }
        }
    }
} // <nMn2cCBwH18>

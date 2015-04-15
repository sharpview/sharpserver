using System;
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

namespace Sharpserver
{
    internal class ASPHandler : Webhandler 
    {
        public ASPHandler() { }

        public bool Process(HTTP.Net.HttpContext context)
        {
            HTTP.Net.HttpResponse Resp = context.Response;
            NameValueCollection cParam = context.Request.QueryString;

#if ASP
            // --------------------------------------------
            // Todo: Cassini 
            // --------------------------------------------

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
} // <T-bF7cOtc7M>

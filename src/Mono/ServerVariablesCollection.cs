//
// System.Web.ServerVariablesCollection
//
// Authors:
//   	Alon Gazit (along@mainsoft.com)
//   	Miguel de Icaza (miguel@novell.com)
//   	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// (c) 2004 Mainsoft, Inc. (http://www.mainsoft.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Globalization;
using System.Security.Permissions;
using System.Collections.Generic;

namespace HTTP.Net
{
	class ServerVariablesCollection : BaseParamsCollection
	{
        HttpRequest request;

        public ServerVariablesCollection(HttpRequest request) : base(request)
        {
            this.request = request;
            IsReadOnly = false;
        }      

		void AppendKeyValue (StringBuilder sb, string key, string value, bool standard)
		{
			//
			// Standard has HTTP_ prefix, everything is uppercase, has no space
			// after colon, - is changed to _
			//
			// Raw is header, colon, space, values, raw.
			//
			if (standard){
				sb.Append ("HTTP_");
				sb.Append (key.ToUpper (System.Globalization.CultureInfo.InvariantCulture).Replace ('-', '_'));
				sb.Append (":");
			} else {
				sb.Append (key);
				sb.Append (": ");
			}
			sb.Append (value);
			sb.Append ("\r\n");
		}
				     
		void loadServerVariablesCollection()
		{
            try
            {
                Add("SERVER_ADDR", request.LocalEndPoint.Address.ToString());
                Add("SERVER_NAME", Environment.MachineName + ":" + request.Url.Port.ToString());
                Add("SERVER_PORT", request.Url.Port.ToString());

                Add("SERVER_SOFTWARE", "Sharpserver");
                Add("SERVER_PROTOCOL", "HTTP/" + request.ProtocolVersion.ToString());
                if (request.IsSecureConnection) { Add("SERVER_HTTPS", "on"); } else { Add("SERVER_HTTPS", "off"); }

                Add("LOCAL_ADDR", request.LocalEndPoint.Address.ToString());
                Add("REMOTE_ADDR", request.UserHostAddress);
                Add("REMOTE_HOST", request.UserHostName.Split(':')[0]);
                Add("REMOTE_PORT", request.RemoteEndPoint.Port.ToString());

                Add("HTTP_HOST", request.Url.Host + ":" + request.Url.Port.ToString());
                Add("HTTP_USER_AGENT", request.UserAgent);
                Add("REQUEST_METHOD", request.HttpMethod);

                Add("URL", request.Url.LocalPath);
                Add("QUERY_STRING", qString(request.QueryString));

                Add("CONTENT_TYPE", request.ContentType);
                Add("CONTENT_ENCODING", request.ContentEncoding.EncodingName);
                Add("CONTENT_LENGTH", request.ContentLength64.ToString());
            }
            catch
            {
                throw new Exception("Error loading server variables");
            }
			IsReadOnly = true;
		}

        internal String qString(NameValueCollection param)
        {
            List<String> items = new List<String>();

            foreach (String n in param)
                items.Add(String.Concat(n, "=", HttpUtility.UrlEncode(param[n])));

            return String.Join("&", items.ToArray());
        }

        protected override void InsertInfo ()
		{
			loadServerVariablesCollection ();
		}

		protected override string InternalGet (string name)
		{
            return null;
		}
	}
}

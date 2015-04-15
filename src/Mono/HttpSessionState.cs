//
// System.Web.SessionState.HttpSessionState
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace HTTP.Net {

    public enum SessionStateMode
    {
	    Off = 0,
	    InProc = 1,
	    StateServer = 2,
	    SQLServer = 3,
	    Custom = 4,
    }

    public enum HttpCookieMode
    {
        UseUri = 0,
        UseCookies = 1,
        AutoDetect = 2,
        UseDeviceProfile = 3,
    }

	public interface IHttpSessionState
	{
		///methods
		void Abandon ();
		void Add (string itemName, object itemValue);
		void Clear ();
		void CopyTo (Array sessionValues, int index);
		IEnumerator GetEnumerator ();
		void Remove (string itemName);
		void RemoveAll ();
		void RemoveAt (int index);
	
		///properties
		int CodePage { get; set; }
		HttpCookieMode CookieMode { get; }
		int Count { get; }		
		bool IsCookieless { get; }
		bool IsNewSession { get; }
		bool IsReadOnly { get; }
		bool IsSynchronized { get; }
		object this [int index] { get; set; }
		object this [string name] { get; set;}
		NameObjectCollectionBase.KeysCollection Keys { get; }
		int LCID { get; set; }
		SessionStateMode Mode { get; }
		string SessionID { get; }
		HttpStaticObjectsCollection StaticObjects { get; }
		object SyncRoot { get; }
		int Timeout { get; set; }			
	}

    public sealed class HttpSessionState : ICollection, IEnumerable    
    {
	    IHttpSessionState container;
	
	    internal HttpSessionState (IHttpSessionState container)
	    {
		    this.container = container;
	    }

	    internal IHttpSessionState Container {
		    get { return container; }
	    }
	
	    public int CodePage {
		    get { return container.CodePage; }
		    set { container.CodePage = value; }
	    }

	    public HttpSessionState Contents {
		    get { return this; }
	    }

	    public HttpCookieMode CookieMode {
		    get {
			    if (IsCookieless)
				    return HttpCookieMode.UseUri;
			    else
				    return HttpCookieMode.UseCookies;
		    }
	    }

	    public int Count {
		    get { return container.Count; }
	    }

	    public bool IsCookieless {
		    get { return container.IsCookieless; }
	    }

	    public bool IsNewSession {
		    get { return container.IsNewSession; }
	    }

	    public bool IsReadOnly {
		    get { return container.IsReadOnly; }
	    }

	    public bool IsSynchronized {
		    get { return container.IsSynchronized; }
	    }

	    public object this [string key] {
		    get { return container [key]; }
		    set { container [key] = value; }
	    }

	    public object this [int index] {
		    get { return container [index]; }
		    set { container [index] = value; }
	    }

	    public NameObjectCollectionBase.KeysCollection Keys {
		    get { return container.Keys; }
	    }

	    public int LCID {
		    get { return container.LCID; }
		    set { container.LCID = value; }
	    }

	    public SessionStateMode Mode {
		    get { return container.Mode; }
	    }

	    public string SessionID {
		    get { return container.SessionID; }
	    }

	    public HttpStaticObjectsCollection StaticObjects {
		    get { return container.StaticObjects; }
	    }

	    public object SyncRoot {
		    get { return container.SyncRoot; }
	    }

	    public int Timeout {
		    get { return container.Timeout; }
		    set { container.Timeout = value; }
	    }

	    public void Abandon ()
	    {
		    container.Abandon ();
	    }

	    public void Add (string name, object value)
	    {
		    container.Add (name, value);
	    }

	    public void Clear ()
	    {
		    container.Clear ();
	    }
	
	    public void CopyTo (Array array, int index)
	    {
		    container.CopyTo (array, index);
	    }

	    public IEnumerator GetEnumerator ()
	    {
		    return container.GetEnumerator ();
	    }
	
	    public void Remove (string name)
	    {
		    container.Remove (name);
	    }

	    public void RemoveAll ()
	    {
		    container.Clear ();
	    }

	    public void RemoveAt (int index)
	    {
		    container.RemoveAt (index);
	    }
    }
}


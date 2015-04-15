using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using Sharpserver;

//-----------------------------------------------------------------
//
//    Sharpserver - Embedded Webserver
//
//    This library is free software; you can redistribute it 
//    and modify it under the terms of the GNU General Public
//    License as published by the Free Software Foundation.
//
//-----------------------------------------------------------------

namespace Example
{
    public partial class Example : Form
    {
        Webserver Server = null;

        public Example()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ButtonStart.Enabled = false;            

            Server = new Webserver(Path.GetTempPath());
            Server.Port = 8083;

            try
            {
                Server.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                ButtonStart.Enabled = true;
                return;
            }

            string file = CreateTestFile();
            System.Diagnostics.Process.Start(Server.URL + Path.GetFileName(file));
            ButtonStop.Enabled = true;           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ButtonStop.Enabled = false;
            Stop();
            ButtonStart.Enabled = true;
        }

        ~Example()
        {
            Stop();
            Server = null;
        }

        private string CreateTestFile()
        {
            #if PHP
                string sExt = "php";
            #else
                string sExt = "php";
            #endif

            string file = Path.GetTempFileName().Replace(".tmp","") + "." + sExt;
            Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Example.test." + sExt);
            StreamWriter sw = new StreamWriter(new FileStream(file, FileMode.Create));
            s.CopyTo(sw.BaseStream);
            sw.Close();
            return file;
        }

        private void Stop()
        {
            if (Server != null)
            {
                Server.Close ();
            }
        }
    }
}

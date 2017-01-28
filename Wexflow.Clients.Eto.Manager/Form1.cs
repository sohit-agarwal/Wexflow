using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Forms;
using Eto.Drawing;

namespace Wexflow.Clients.Eto.Manager
{
    public class Form1 : Form
    {
        public Form1()
        {
            // sets the client (inner) size of the window for your content
            this.ClientSize = new Size(600, 400);

            this.Title = "Hello, Eto.Forms";
        }
    }
}

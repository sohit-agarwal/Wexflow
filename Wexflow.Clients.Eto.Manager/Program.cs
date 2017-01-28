using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Forms;

namespace Wexflow.Clients.Eto.Manager
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application().Run(new Form1());
        }
    }
}

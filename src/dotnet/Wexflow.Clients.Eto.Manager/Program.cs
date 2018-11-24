using System;
using Eto.Forms;

namespace Wexflow.Clients.Eto.Manager
{
    internal class Program
    {
        [STAThread]
        public static void Main()
        {
            new Application().Run(new Form1());
        }
    }
}

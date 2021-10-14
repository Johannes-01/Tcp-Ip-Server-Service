using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Runtime.InteropServices;

namespace AstridServer
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Thread s = new Thread(Server.start);
            s.Start();
        }

        protected override void OnStop()
        {
            System.Console.WriteLine("Service stop");
        }

    }
}

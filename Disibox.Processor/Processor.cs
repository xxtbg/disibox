using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Disibox.Data;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Disibox.Processor
{
    public class Processor : RoleEntryPoint
    {
        private DataSource _dataSource;

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("Disibox.Processor entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.WriteLine("Working", "Information");

//                var procReq = _dataSource.DequeueProcessingRequest();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            _dataSource = new DataSource();

            return base.OnStart();
        }

        private void ProcessFile(string file, string toolName)
        {
            object tool;
            
            //if (!_procTools.TryGetValue(toolName, out tool))
                throw new ArgumentException(toolName + " does not exist.", "toolName");


        }
    }
}

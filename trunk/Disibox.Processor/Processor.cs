using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Disibox.Data;
using Disibox.Processing;
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

                var procReq = DataSource.DequeueProcessingRequest();
                if (procReq != null)
                    ProcessRequest(procReq);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            _dataSource = new DataSource();

            return base.OnStart();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procReq"></param>
        private void ProcessRequest(ProcessingMessage procReq)
        {
            var tool = ToolsManifest.GetTool(procReq.ToolName);
            if (tool == null)
                throw new ArgumentException(procReq.ToolName + " does not exist.", "procReq");

            var file = _dataSource.GetFile(procReq.FileUri);

            tool.ProcessFile(file, procReq.FileContentType);
        }
    }
}

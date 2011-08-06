using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Disibox.Processor
{
    public class Processor : RoleEntryPoint
    {
        private const string ProcAssemblyPath = @"c:\Test.dll";

        private Dictionary<string, object> _procTools;
 
        public Processor()
        {
            InitProcTools();
        }

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("Disibox.Processor entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.WriteLine("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        private void ProcessFile(string file, string toolName)
        {
            object tool;
            
            if (!_procTools.TryGetValue(toolName, out tool))
                throw new ArgumentException(toolName + " does not exist.", "toolName");


        }

        private void InitProcTools()
        {
            _procTools = new Dictionary<string, object>();

            var procAssembly = Assembly.LoadFile(ProcAssemblyPath);
            var procTypes = procAssembly.GetTypes();

            foreach (var procType in procTypes)
            {
                var procObject = Activator.CreateInstance(procType);
                _procTools.Add(procType.ToString(), procObject);
            }
        }
    }
}

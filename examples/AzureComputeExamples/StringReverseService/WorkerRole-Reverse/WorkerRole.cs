using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.ServiceModel;
using Reverse;

namespace WorkerRole_Reverse
{
    public class WorkerRole : RoleEntryPoint {
        private static ServiceHost serviceHost;
        private const string serviceName = "Reverse";

        public override void Run() {
            Trace.WriteLine("WorkerRole-Reverse entry point called", "Information");

            StartStringReverseService();

            while (true) {
                Thread.Sleep(300000);
                Trace.TraceInformation("Working....");
            }
        }

        public override bool OnStart() {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        private static void StartStringReverseService() {
            serviceHost = new ServiceHost(typeof (Reverse.Reverse));

            var binding = new NetTcpBinding(SecurityMode.None);
            var externalEndPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["ReverseStringEndpoint"];

            serviceHost.AddServiceEndpoint(typeof (IReverse), binding,
                                            String.Format("net.tcp://{0}/{1}",
                                                            externalEndPoint.IPEndpoint, serviceName));

            try {
                serviceHost.Open();
                Trace.TraceInformation("Service started with success.");
            } catch (Exception e) {
                Trace.WriteLine("Cannot start StringReverseService - " + e.Message);
            }
        }
    }
}

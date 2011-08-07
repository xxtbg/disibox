using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Disibox.Processing;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Disibox.Dispatcher
{
    public class Dispatcher : RoleEntryPoint {
        private readonly AutoResetEvent _connectionHandler = new AutoResetEvent(false);

        public override void Run()
        {
            Trace.WriteLine("Disibox.Dispatcher entry point called", "Information");

            TcpListener tcpListener;

            try {
                tcpListener = new TcpListener(RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["EndpointDispatcher"].IPEndpoint)
                        {ExclusiveAddressUse = false};
                tcpListener.Start();
            } catch(SocketException) {
                Trace.Write("Echo server could not start.", "Error");
                return;
            }

            Trace.WriteLine("Disibox.Dispatcher waiting for connections on port 2345", "Information");

            while (true) {
                var result = tcpListener.BeginAcceptTcpClient(HandleAssincConnection, tcpListener);
                _connectionHandler.WaitOne();

                Trace.WriteLine("Working", "Information");
            }
        }

        private void HandleAssincConnection(IAsyncResult result) {
            // accepted connection
            var listener = (TcpListener) result.AsyncState;
            var client = listener.EndAcceptTcpClient(result);
            _connectionHandler.Set();

            var clientId = Guid.NewGuid();
            Trace.Write("Accepted connection with ID " + clientId, "Information");

            var reader = new StreamReader(client.GetStream());
            var writer = new StreamWriter(client.GetStream()) {AutoFlush = true};

            var user = reader.ReadLine(); //user
            var password = reader.ReadLine(); //password
            var mime = reader.ReadLine(); //mime
            var uriFile = reader.ReadLine(); //uri of the file to process

            Trace.WriteLine("user: " + user, "Information");
            Trace.WriteLine("password: " + password, "Information");
            Trace.WriteLine("mime: " + mime, "Information");
            Trace.WriteLine("uri file: " + uriFile, "Information");


            var processingTools = Manifest.GetAvailableTools(mime);


            var numberOfTools = processingTools.Count;
            //communicating the number of tools
            writer.WriteLine(numberOfTools);

            for (var i=0; i<numberOfTools; ++i)
                writer.WriteLine(processingTools[i]);


            //quale operazione voglio fare
            var operation = reader.ReadLine();

            Trace.WriteLine("operazione da compiere: " + operation, "Information");
            //processo il file con l'operazione operation e ritorno con il uri del file processato
//            uriProcessedFile = proces(uriFile, operation);

            writer.WriteLine("uriProcessedFile");

            client.Close();

        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Disibox.Data;
using Disibox.Data.Exceptions;
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
            StreamReader reader;
            StreamWriter writer;
            string user = null, password = null, mime = null, uriFile = null;
            DataSource datasource;

            // accepted connection
            var listener = (TcpListener) result.AsyncState;
            var client = listener.EndAcceptTcpClient(result);
            _connectionHandler.Set();

            var clientId = Guid.NewGuid();
            Trace.Write("Accepted connection with ID " + clientId, "Information");

            try
            {
                reader = new StreamReader(client.GetStream());
                writer = new StreamWriter(client.GetStream()) {AutoFlush = true};
            } catch(Exception)
            {
                Trace.WriteLine("An error during initialization of reader and writer of connection: " + clientId + ". Closing comunication.", "Information");
                client.Close(); ;
                return;
            }

            try
            {
                user = reader.ReadLine();
                password = reader.ReadLine();
                mime = reader.ReadLine();
                uriFile = reader.ReadLine();
            } catch(Exception)
            {
                Trace.WriteLine("An error reading user, password, mime and uro file of connection: " + clientId + ". Closing comunication.", "Information");
                client.Close(); ;
                return;
            }

            if (user == null || password == null || mime == null || uriFile == null)
            {
                Trace.WriteLine("User or password or mime or uri file are null (connection: " + clientId + "). Closing comunication.", "Information");
                client.Close(); ;
                return;
            }

            Trace.WriteLine("user: " + user, "Information");
            Trace.WriteLine("password: " + password, "Information");
            Trace.WriteLine("mime: " + mime, "Information");
            Trace.WriteLine("uri file: " + uriFile, "Information");

            datasource = new DataSource();

            try
            {
                datasource.Login(user, password);
            } catch (UserNotExistingException)
            {
                writer.WriteLine("KO");
                client.Close();
            }

            writer.WriteLine("OK");

            IList<string> processingTools = null;
            var numberOfTools = 0;
            try
            {
                processingTools = ToolsManifest.GetAvailableTools(mime);
                numberOfTools = processingTools.Count;
            } catch
            {
                Trace.WriteLine("Error getting available tools (connection: " + clientId + ")", "Information");
            }

            try
            {
                //communicating the number of tools
                writer.WriteLine(numberOfTools);
            }
            catch (Exception)
            {
                Trace.WriteLine("Error sending the number of tools to processed file (connection: " + clientId + "). Closing comunication.", "Information");
                client.Close();
                return;
            }

            try
            {
                for (var i = 0; i < numberOfTools; ++i)
                    writer.WriteLine(processingTools[i]);
            } catch(Exception)
            {
                Trace.WriteLine("Error sending the tools to processed file (connection: " + clientId + "). Closing comunication.", "Information");
                client.Close();
                return;
            }

            string operation;
            try
            {
                //quale operazione voglio fare
                operation = reader.ReadLine();
            } catch(Exception)
            {
                Trace.WriteLine("Error reading the operation (connection: " + clientId + "). Closing comunication.", "Information");
                client.Close();
                return;
            }

            if (operation==null)
            {
                Trace.WriteLine("Error reading the operation (connection: " + clientId + "). Closing comunication.", "Information");
                client.Close();
                return;
            }


            datasource.EnqueueProcessingRequest(new ProcessingMessage(uriFile, mime, operation));


            Trace.WriteLine("operazione da compiere: " + operation, "Information");
            //processo il file con l'operazione operation e ritorno con il uri del file processato
//            uriProcessedFile = proces(uriFile, operation);

            try
            {
                writer.WriteLine("uriProcessedFile");
            }
            catch (Exception)
            {
                Trace.WriteLine("Error sending the uri of processed file (connection: " + clientId + "). Closing comunication.", "Information");
                client.Close();
                return;
            }

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

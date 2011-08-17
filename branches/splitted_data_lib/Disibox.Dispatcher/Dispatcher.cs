//
// Copyright (c) 2011, University of Genoa
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the University of Genoa nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

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
    public class Dispatcher : RoleEntryPoint 
    {
        private readonly AutoResetEvent _connectionHandler = new AutoResetEvent(false);

        public override void Run() {
            TcpListener tcpListener;

            Trace.WriteLine("Disibox.Dispatcher entry point called", "Information");

            try {
                tcpListener =
                    new TcpListener(
                        RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["EndpointDispatcher"].IPEndpoint)
                    {ExclusiveAddressUse = false};
                tcpListener.Start();
            } catch (SocketException) {
                Trace.Write("Echo server could not start.", "Error");
                return;
            }

            Trace.WriteLine("Disibox.Dispatcher waiting for connections on port 2345", "Information");

            while (true) {
                tcpListener.BeginAcceptTcpClient(HandleAssincConnection, tcpListener);
                _connectionHandler.WaitOne();

                Trace.WriteLine("Working", "Information");
            }
        }

        private void HandleAssincConnection(IAsyncResult result) {
            StreamReader reader;
            StreamWriter writer;
            string user, password, mime, uriFile;

            // accepted connection
            var listener = (TcpListener) result.AsyncState;
            var client = listener.EndAcceptTcpClient(result);
            _connectionHandler.Set();

            var clientId = Guid.NewGuid();
            Trace.Write("Accepted connection with ID " + clientId, "Information");

            try {
                reader = new StreamReader(client.GetStream());
                writer = new StreamWriter(client.GetStream()) {AutoFlush = true};
            } catch (Exception) {
                Trace.WriteLine(
                    "An error during initialization of reader and writer of connection: " + clientId +
                    ". Closing comunication.", "Information");
                client.Close();
                return;
            }

            try {
                user = reader.ReadLine();
                password = reader.ReadLine();
                mime = reader.ReadLine();
                uriFile = reader.ReadLine();
            } catch (Exception) {
                Trace.WriteLine(
                    "An error reading user, password, mime and uro file of connection: " + clientId +
                    ". Closing comunication.", "Information");
                client.Close();
                return;
            }

            if (user == null || password == null || mime == null || uriFile == null) {
                Trace.WriteLine(
                    "User or password or mime or uri file are null (connection: " + clientId +
                    "). Closing comunication.", "Information");
                client.Close();
                return;
            }


//            Trace.WriteLine("user: " + user, "Information");
//            Trace.WriteLine("password: " + password, "Information");
//            Trace.WriteLine("mime: " + mime, "Information");
//            Trace.WriteLine("uri file: " + uriFile, "Information");

            var datasource = new DataSource();

            try {
                datasource.Login(user, password);
            } catch (UserNotExistingException) {
                writer.WriteLine("KO");
                client.Close();
                return;
            }

            writer.WriteLine("OK");

            IList<BaseTool> processingTools = null;
            var numberOfTools = 0;
            try {
                processingTools = ToolsManifest.GetAvailableTools(mime);
                numberOfTools = processingTools.Count;
            } catch {
                Trace.WriteLine("Error getting available tools (connection: " + clientId + ")", "Information");
            }

            try {
                //communicating the number of tools
                writer.WriteLine(numberOfTools);
            } catch (Exception) {
                Trace.WriteLine(
                    "Error sending the number of tools to processed file (connection: " + clientId +
                    "). Closing comunication.", "Information");
                client.Close();
                return;
            }

            try {
                for (var i = 0; i < numberOfTools; ++i)
                    writer.WriteLine(processingTools[i].ToString());
            } catch (Exception) {
                Trace.WriteLine(
                    "Error sending the tools to processed file (connection: " + clientId + "). Closing comunication.",
                    "Information");
                client.Close();
                return;
            }

            string operation;
            try {
                operation = reader.ReadLine();
            } catch (Exception) {
                Trace.WriteLine("Error reading the operation (connection: " + clientId + "). Closing comunication.",
                                "Information");
                client.Close();
                return;
            }

            if (operation == null) {
                Trace.WriteLine("Error reading the operation (connection: " + clientId + "). Closing comunication.",
                                "Information");
                client.Close();
                return;
            }

            Trace.WriteLine("operazione da compiere: " + operation, "Information");

            datasource.EnqueueProcessingRequest(new ProcessingMessage(uriFile, mime, operation));
            var returnMessage = datasource.DequeueProcessingCompletion();

            Trace.WriteLine("File uri: " + returnMessage.FileUri, "Information");

            try {
                writer.WriteLine(returnMessage.FileUri);
            } catch (Exception) {
                Trace.WriteLine(
                    "Error sending the uri of processed file (connection: " + clientId + "). Closing comunication.",
                    "Information");
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

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
// DISCLAIMED. IN NO EVENT SHALL UNIVERSITY OF GENOA BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.Diagnostics;
using System.Net;
using Disibox.Data.Server;
using Disibox.Processing;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Disibox.Processor
{
    public class Processor : RoleEntryPoint
    {
        private readonly ServerDataSource _dataSource = new ServerDataSource();

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("Disibox.Processor entry point called", "Information");

            while (true)
            {
                Trace.WriteLine("Waiting for a processing request...", "Information");

                var procReq = _dataSource.DequeueProcessingRequest();
                ProcessRequest(procReq);

                Trace.WriteLine("Processing completed.", "Information");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            return base.OnStart();
        }

        private void ProcessRequest(ProcessingMessage procReq)
        {
            var tool = ToolsManifest.GetTool(procReq.ToolName);
            if (tool == null)
                throw new ArgumentException(procReq.ToolName + " does not exist.", "procReq");

            var file = _dataSource.GetFile(procReq.FileUri);
            var output = tool.ProcessFile(file, procReq.FileContentType);
            var outputUri = _dataSource.AddOutput(procReq.ToolName, output.ContentType, output.Content);

            var procCompl = new ProcessingMessage(outputUri, output.ContentType, procReq.ToolName);
            _dataSource.EnqueueProcessingCompletion(procCompl);
        }
    }
}

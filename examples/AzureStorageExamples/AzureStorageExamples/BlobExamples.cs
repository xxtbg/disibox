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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AzureStorageExamples.Data;
using AzureStorageExamples.Properties;
using Microsoft.WindowsAzure;

namespace AzureStorageExamples
{
    public static class BlobExamples
    {
        private const int StreamCount = 9;
        private const int StreamLength = 16;

        public static void RunAll()
        {
            Console.WriteLine(" * Manipulate block blob");
            ManipulateBlockBlob();

            Console.WriteLine(" * Manipulate page blob");
            ManipulatePageBlob();

            Console.WriteLine(" * Use Azure drive");
            UseAzureDrive();

            Console.WriteLine(" * Use custom container");
            UseCustomContainer();
        }

        private static void ManipulateBlockBlob()
        {
            
        }

        private static void ManipulatePageBlob()
        {
            
        }

        private static void UseAzureDrive()
        {
            
        }

        private static void UseCustomContainer()
        {
            var container = CreateCustomContainer("container");
            var streams = CreateSimpleStreams();

            for (var i = 0; i < streams.Count; ++i)
                container.AddBlob(i + ".txt", "text/plain", streams[i]);

            var blobs = container.GetBlobs();
            Debug.Assert(blobs.Count() == streams.Count);

            container.Delete();
        }

        private static AzureContainer CreateCustomContainer(string containerName)
        {
            var connectionString = Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobEndpointUri = storageAccount.BlobEndpoint.ToString();
            var credentials = storageAccount.Credentials;
            AzureContainer.Create(containerName, blobEndpointUri, credentials);
            return AzureContainer.Connect(containerName, blobEndpointUri, credentials);
        }

        private static IList<Stream> CreateSimpleStreams()
        {
            var streams = new List<Stream>();
            var bytes = new byte[StreamLength];
            for (var i = 0; i < StreamCount; ++i)
            {
                for (var j = 0; j < StreamLength; ++j)
                    bytes[j] = (byte) i;
                streams.Add(new MemoryStream(bytes));
            }
            return streams;
        }
    }
}

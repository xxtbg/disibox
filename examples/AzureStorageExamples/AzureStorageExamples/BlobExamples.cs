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
using Microsoft.WindowsAzure.StorageClient;

namespace AzureStorageExamples
{
    public static class BlobExamples
    {
        private const int StreamCount = 8;

        public static void RunAll()
        {
            Console.WriteLine(" * Manipulate generic blob");
            ManipulateGenericBlob();

            Console.WriteLine(" * Manipulate block blob");
            ManipulateBlockBlob();

            Console.WriteLine(" * Manipulate page blob");
            ManipulatePageBlob();

            Console.WriteLine(" * Use Azure drive");
            UseAzureDrive();

            Console.WriteLine(" * Use custom container");
            UseCustomContainer();
        }

        private static void ManipulateGenericBlob()
        {
            var container = CreateContainer("container");

            var blob = container.GetBlobReference(container.Uri + "/blob");
            var streams = CreateSimpleStreams(128);
            blob.UploadFromStream(streams[0]);
            Debug.Assert(blob.Properties.Length == streams[0].Length);

            blob.Metadata.Add("Author", "Pino");
            blob.Metadata.Add("Date", "Duemilamai");
            Debug.Assert(blob.Metadata.Count == 2);
            Debug.Assert(blob.Metadata.Get("Author") == "Pino");
            Debug.Assert(blob.Metadata.Get("Date") == "Duemilamai");

            container.Delete();
        }

        private static void ManipulateBlockBlob()
        {
            var container = CreateContainer("container");

            var blockBlob = container.GetBlockBlobReference(container.Uri + "/blockblob");
            var streams = CreateSimpleStreams(256);
            var blockIds = new List<string>();
            for (var i = 0; i < streams.Count; ++i)
            {
                var blockId = EncodeTo64("b" + i);
                blockBlob.PutBlock(blockId, streams[i], null);
                blockIds.Add(blockId);
            }
            blockBlob.PutBlockList(blockIds);

            blockBlob.FetchAttributes();
            Debug.Assert(blockBlob.Properties.Length == streams.Count*streams[0].Length);

            var blocks = blockBlob.DownloadBlockList();
            Debug.Assert(blocks.Count() == streams.Count);
            Debug.Assert(blocks.Count(b => !b.Committed) == 0);

            container.Delete();
        }

        private static void ManipulatePageBlob()
        {
            var container = CreateContainer("container");

            var pageBlob = container.GetPageBlobReference(container.Uri + "/pageblob");
            var streams = CreateSimpleStreams(1024 * 1024); // 1MB
            pageBlob.Create(streams.Count * streams[0].Length);
            for (var i = 0; i < streams.Count; ++i)
                pageBlob.WritePages(streams[i], i * streams[0].Length);

            pageBlob.FetchAttributes();
            Debug.Assert(pageBlob.Properties.Length == streams.Count * streams[0].Length);

            //var pages = pageBlob.GetPageRanges();
            //Debug.Assert(pages.Count() == 2);

            container.Delete();
        }

        private static void UseAzureDrive()
        {
            var container = CreateContainer("container");

            var pageBlob = container.GetPageBlobReference(container.Uri + "/pageblob");
            //pageBlob.Create(128 * 1024 * 1024); // 128MB

            var drive = CreateCloudDrive(pageBlob.Uri); // REFERENZA
            drive.Create(64); // Must be called once in a VHD lifetime.
            var driveLetter = drive.Mount(32, DriveMountOptions.None);

            var testFilename = driveLetter + "\\test.txt";
            var testContent = "We are testing Azure Drive!";
            File.WriteAllText(testFilename, testContent);
            var readContent = File.ReadAllText(testFilename);
            Debug.Assert(readContent == testContent);

            drive.Unmount();
            container.Delete();
        }

        private static void UseCustomContainer()
        {
            var container = CreateCustomContainer("container");
            var streams = CreateSimpleStreams(128);

            for (var i = 0; i < streams.Count; ++i)
                container.AddBlob(i + ".txt", "text/plain", streams[i]);

            var blobs = container.GetBlobs();
            Debug.Assert(blobs.Count() == streams.Count);

            container.Delete();
        }

        private static CloudBlobContainer CreateContainer(string containerName)
        {
            var connectionString = Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobEndpointUri = storageAccount.BlobEndpoint.ToString();
            var credentials = storageAccount.Credentials;
            var container = new CloudBlobContainer(blobEndpointUri + "/" + containerName, credentials);
            container.CreateIfNotExist();
            return container;
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

        private static CloudDrive CreateCloudDrive(Uri pageBlobUri)
        {
            var connectionString = Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var credentials = storageAccount.Credentials;
            return new CloudDrive(pageBlobUri, credentials);
        }

        private static IList<Stream> CreateSimpleStreams(int streamLength)
        {
            var streams = new List<Stream>();
            var bytes = new byte[streamLength];
            for (var i = 0; i < StreamCount; ++i)
            {
                for (var j = 0; j < streamLength; ++j)
                    bytes[j] = (byte) i;
                streams.Add(new MemoryStream(bytes));
            }
            return streams;
        }

        private static string EncodeTo64(string toEncode)
        {
            var toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }
    }
}

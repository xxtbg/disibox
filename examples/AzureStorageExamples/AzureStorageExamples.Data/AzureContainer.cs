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
using System.IO;
using System.Linq;
using AzureStorageExamples.Data.Exceptions;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace AzureStorageExamples.Data
{
    public class AzureContainer : IStorage
    {
        private readonly CloudBlobClient _blobClient;
        private readonly CloudBlobContainer _container;

        private AzureContainer(CloudBlobClient blobClient, CloudBlobContainer container)
        {
            _blobClient = blobClient;
            _container = container;
        }

        public string Name
        {
            get { return _container.Name; }
        }

        public BlobContainerPermissions Permissions
        {
            get { return _container.GetPermissions(); }
            set { _container.SetPermissions(value); }
        }

        public Uri Uri
        {
            get { return _container.Uri; }
        }

        public static AzureContainer Connect(string containerName, string blobEndpointUri,
                                             StorageCredentials credentials)
        {
            // Requirements
            Require.NotEmpty(containerName, "containerName");
            Require.NotEmpty(blobEndpointUri, "blobEndpointUri");
            Require.NotNull(credentials, "credentials");

            var blobClient = CreateBlobClient(blobEndpointUri, credentials);
            var container = CreateContainer(containerName, blobClient);
            return new AzureContainer(blobClient, container);
        }

        public static void Create(string containerName, string blobEndpointUri, StorageCredentials credentials)
        {
            // Requirements
            Require.NotEmpty(containerName, "containerName");
            Require.NotEmpty(blobEndpointUri, "blobEndpointUri");
            Require.NotNull(credentials, "credentials");

            var blobClient = CreateBlobClient(blobEndpointUri, credentials);
            var container = CreateContainer(containerName, blobClient);
            container.CreateIfNotExist();
        }

        public string AddBlob(string blobName, string blobContentType, Stream blobContent)
        {
            // Requirements
            Require.NotEmpty(blobName, "blobName");
            Require.NotEmpty(blobContentType, "blobContentType");
            Require.NotNull(blobContent, "blobContent");
            RequireExistingContainer();

            var oldPosition = blobContent.Position;
            blobContent.Seek(0, SeekOrigin.Begin);
            var blob = _container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = blobContentType;
            blob.UploadFromStream(blobContent);
            blobContent.Seek(oldPosition, SeekOrigin.Begin);
            return blob.Uri.ToString();
        }

        public bool DeleteBlob(string blobUri)
        {
            // Requirements
            RequireValidUri(blobUri, "blobUri");
            RequireExistingContainer();

            var blob = _container.GetBlobReference(blobUri);
            return blob.DeleteIfExists();
        }

        public CloudBlob GetBlob(string blobUri)
        {
            // Requirements
            RequireValidUri(blobUri, "blobUri");
            RequireExistingContainer();

            return _container.GetBlobReference(blobUri);
        }

        public Stream GetBlobData(string blobUri)
        {
            // Requirements
            RequireValidUri(blobUri, "blobUri");
            RequireExistingContainer();

            return GetBlob(blobUri).OpenRead();
        }

        public IEnumerable<CloudBlob> GetBlobs()
        {
            // Requirements
            RequireExistingContainer();

            var options = new BlobRequestOptions {UseFlatBlobListing = true};
            return _container.ListBlobs(options).Select(b => (CloudBlob) b).ToList();
        }

        public IEnumerable<Stream> GetBlobsData()
        {
            // Requirerements
            RequireExistingContainer();

            return GetBlobs().Select(b => b.OpenRead());
        }

        public void Clear()
        {
            // Requirements
            RequireExistingContainer();

            _container.Delete();
            _container.Create();
        }

        public void Delete()
        {
            // Requirements
            RequireExistingContainer();

            _container.Delete();
        }

        public bool Exists()
        {
            var containers = _blobClient.ListContainers().Select(c => c.Name);
            return containers.Contains(_container.Name);
        }

        private static CloudBlobClient CreateBlobClient(string blobEndpointUri, StorageCredentials credentials)
        {
            return new CloudBlobClient(blobEndpointUri, credentials);
        }

        private static CloudBlobContainer CreateContainer(string containerName, CloudBlobClient blobClient)
        {
            return blobClient.GetContainerReference(containerName);
        }

        private void RequireExistingContainer()
        {
            if (Exists()) return;
            throw new ContainerNotExistingException(_container.Name);
        }

        private void RequireValidUri(string uri, string argName)
        {
            // Requirements
            Require.NotEmpty(uri, argName);

            if (uri.StartsWith(Uri.ToString())) return;
            throw new ArgumentException("Invalid uri.", argName);
        }
    }
}
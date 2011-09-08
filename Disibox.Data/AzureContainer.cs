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
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class AzureContainer
    {
        private readonly CloudBlobContainer _container;

        private AzureContainer(CloudBlobContainer container)
        {
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
            var container = CreateContainer(containerName, blobEndpointUri, credentials);
            return new AzureContainer(container);
        }

        public static AzureContainer Create(string containerName, string blobEndpointUri, StorageCredentials credentials)
        {
            var container = CreateContainer(containerName, blobEndpointUri, credentials);
            container.CreateIfNotExist();
            return new AzureContainer(container);
        }

        public string AddBlob(string blobName, string blobContentType, Stream blobContent)
        {
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
            var blob = _container.GetBlobReference(blobUri);
            return blob.DeleteIfExists();
        }

        public Stream GetBlob(string blobUri)
        {
            var blob = _container.GetBlockBlobReference(blobUri);
            return blob.OpenRead();
        }

        public IEnumerable<CloudBlob> GetBlobs()
        {
            var options = new BlobRequestOptions {UseFlatBlobListing = true};
            return _container.ListBlobs(options).Select(b => (CloudBlob) b).ToList();
        }

        public void Clear()
        {
            _container.Delete();
            _container.Create();
        }

        private static CloudBlobContainer CreateContainer(string containerName, string blobEndpointUri,
                                                          StorageCredentials credentials)
        {
            return new CloudBlobContainer(blobEndpointUri + "/" + containerName, credentials);
        }
    }
}
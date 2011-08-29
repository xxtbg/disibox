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

using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class BlobContainer : CloudBlobContainer
    {
        public BlobContainer(string containerName, string blobEndpointUri, StorageCredentials credentials)
            : base(blobEndpointUri + "/" + containerName, credentials)
        {
            // Empty
        }

        /// <summary>
        /// Uploads given stream to blob storage.
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="blobContentType"></param>
        /// <param name="blobContent"></param>
        /// <returns></returns>
        public string AddBlob(string blobName, string blobContentType, Stream blobContent)
        {
            blobContent.Seek(0, SeekOrigin.Begin);
            var blob = GetBlockBlobReference(blobName);
            blob.Properties.ContentType = blobContentType;
            blob.UploadFromStream(blobContent);
            return blob.Uri.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blobUri"></param>
        /// <returns></returns>
        public bool DeleteBlob(string blobUri)
        {
            var blob = GetBlobReference(blobUri);
            return blob.DeleteIfExists();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blobUri"></param>
        /// <returns></returns>
        public Stream GetBlob(string blobUri)
        {
            var blob = GetBlockBlobReference(blobUri);
            return blob.OpenRead();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CloudBlob> GetBlobs()
        {
            var options = new BlobRequestOptions {UseFlatBlobListing = true};
            return (IEnumerable<CloudBlob>) ListBlobs(options);
        }
    }
}
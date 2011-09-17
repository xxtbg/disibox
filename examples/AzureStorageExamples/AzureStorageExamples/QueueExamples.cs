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
using System.Diagnostics;
using System.Linq;
using System.Text;
using AzureStorageExamples.Data;
using AzureStorageExamples.Messages;
using AzureStorageExamples.Properties;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace AzureStorageExamples
{
    public static class QueueExamples
    {
        private static readonly IList<int> Integers = new List<int> {6, 2, 8, 4, 0};

        public static void RunAll()
        {
            IntegerSortQueue();
            IntegerSortCustomQueue();
        }

        private static void IntegerSortQueue()
        {
            var inputs = SetupIntSortQueue("inputs");
            var outputs = SetupIntSortQueue("outputs");

            inputs.AddMessage(new CloudQueueMessage(ListToString(Integers)));
            var integersToSort = StringToList(inputs.GetMessage().AsString).ToList();
            integersToSort.Sort();
            outputs.AddMessage(new CloudQueueMessage(ListToString(integersToSort)));

            var sortedOutput = StringToList(outputs.GetMessage().AsString);
            Debug.Assert(sortedOutput.Count == integersToSort.Count);
            for (var i = 0; i < integersToSort.Count; ++i)
                Debug.Assert(sortedOutput[i] == integersToSort[i]);

            inputs.Delete();
            outputs.Delete();
        }

        private static void IntegerSortCustomQueue()
        {
            var inputs = SetupIntSortCustomQueue("inputs");
            var outputs = SetupIntSortCustomQueue("outputs");

            inputs.EnqueueMessage(IntSortMessage.FromList(Integers));
            var integersToSort = inputs.DequeueMessage().Integers.ToList();
            integersToSort.Sort();
            outputs.EnqueueMessage(IntSortMessage.FromList(integersToSort));

            var sortedMsg = outputs.DequeueMessage();
            Debug.Assert(sortedMsg.Integers.Count == integersToSort.Count);
            for (var i = 0; i < integersToSort.Count; ++i)
                Debug.Assert(sortedMsg.Integers[i] == integersToSort[i]);

            inputs.Delete();
            outputs.Delete();
        }

        private static CloudQueue SetupIntSortQueue(string queueName)
        {
            var connectionString = Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var queueEndpointUri = storageAccount.QueueEndpoint.ToString();
            var credentials = storageAccount.Credentials;
            var queue = new CloudQueue(queueEndpointUri + "/" + queueName, credentials);
            queue.CreateIfNotExist();
            return queue;
        }

        private static AzureQueue<IntSortMessage> SetupIntSortCustomQueue(string queueName)
        {
            var connectionString = Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var queueEndpointUri = storageAccount.QueueEndpoint.ToString();
            var credentials = storageAccount.Credentials;
            AzureQueue<IntSortMessage>.Create(queueName, queueEndpointUri, credentials);
            return AzureQueue<IntSortMessage>.Connect(queueName, queueEndpointUri, credentials);
        }

        private static string ListToString(IList<int> integers)
        {
            if (integers.Count == 0) return "";
            var builder = new StringBuilder();
            builder.Append(integers[0]);
            for (var i = 1; i < integers.Count; ++i)
                builder.Append("," + integers[i]);
            return builder.ToString();
        }

        private static IList<int> StringToList(string integers)
        {
            return integers.Split(',').Select(int.Parse).ToList();
        }
    }
}

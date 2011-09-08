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
using System.Data.Services.Client;
using System.Linq;
using Disibox.Data.Entities;
using Disibox.Data.Exceptions;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class AzureTable<TEntity> where TEntity : BaseEntity
    {
        private readonly CloudTableClient _tableClient;
        private readonly TableServiceContext _tableContext;
        private readonly string _tableName;

        private AzureTable(string tableName, CloudTableClient tableClient)
        {
            _tableClient = tableClient;
            _tableContext = tableClient.GetDataServiceContext();
            _tableName = tableName;           
        }

        public IQueryable<TEntity> Entities
        {
            get { return _tableContext.CreateQuery<TEntity>(_tableName); }
        }

        public static AzureTable<TEntity> Connect(string tableName, string tableEndpointUri, StorageCredentials credentials)
        {
            // Requirements
            Require.NotEmpty(tableName, "tableName");
            Require.NotEmpty(tableEndpointUri, "tableEndpointUri");

            var tableClient = CreateTableClient(tableEndpointUri, credentials);
            return new AzureTable<TEntity>(tableName, tableClient);
        }

        public static AzureTable<TEntity> Create(string tableName, string tableEndpointUri, StorageCredentials credentials)
        {
            // Requirements
            Require.NotEmpty(tableName, "tableName");
            Require.NotEmpty(tableEndpointUri, "tableEndpointUri");

            var tableClient = CreateTableClient(tableEndpointUri, credentials);
            tableClient.CreateTableIfNotExist(tableName);
            return new AzureTable<TEntity>(tableName, tableClient);
        }

        public void AddEntity(TEntity entity)
        {
            Require.NotNull(entity, "entity");
            _tableContext.AddObject(_tableName, entity);
        }

        public void DeleteEntity(TEntity entity)
        {
            Require.NotNull(entity, "entity");
            _tableContext.DeleteObject(entity);
        }

        public void UpdateEntity(TEntity entity)
        {
            Require.NotNull(entity, "entity");
            _tableContext.UpdateObject(entity);
        }

        public void SaveChanges(SaveChangesOptions options = SaveChangesOptions.None)
        {
            RequireExistingTable();
            _tableContext.SaveChanges(options);
        }

        public void Clear()
        {
            foreach (var entity in Entities)
                DeleteEntity(entity);
            SaveChanges();
        }

        public void Delete()
        {
            RequireExistingTable();
            _tableClient.DeleteTableIfExist(_tableName);
        }

        private static CloudTableClient CreateTableClient(string tableEndpointUri, StorageCredentials credentials)
        {
            var tableClient = new CloudTableClient(tableEndpointUri, credentials);
            tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
            return tableClient;
        }

        private void RequireExistingTable()
        {
            if (_tableClient.DoesTableExist(_tableName)) return;
            throw new TableNotExistingException(_tableName);
        }
    }
}
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
using System.Linq;
using AzureStorageExamples.Data;
using AzureStorageExamples.Entities;
using AzureStorageExamples.Properties;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace AzureStorageExamples
{
    public static class TableExamples
    {
        public static void RunAll()
        {
            Console.WriteLine(" * Use table");
            UseTable();

            Console.WriteLine(" * Use table without type safety");
            UseTableWithoutTypeSafety();

            Console.WriteLine(" * Use custom table");
            UseCustomTable();

            Console.WriteLine(" * Use custom table with type safety");
            UseCustomTableWithTypeSafety();

            Console.WriteLine(" * Use custom table with inheritance");
            UseCustomTableWithInheritance();
        }

        private static void UseTable()
        {
            const string tableName = "devices";
            var devices = SetupDeviceTable(tableName);

            var pinoMouse = new Device("m1", "PinoMouse");
            var ginoCam = new Device("c7", "GinoCam");

            devices.AddObject(tableName, pinoMouse);
            devices.AddObject(tableName, ginoCam);
            devices.SaveChanges();

            pinoMouse.Name = "ExtremePinoMouse";
            devices.UpdateObject(pinoMouse);
            devices.SaveChanges();

            var query = devices.CreateQuery<Device>(tableName).Where(d => d.RowKey == "m1");
            Debug.Assert(query.First().Name == pinoMouse.Name);

            DeleteDeviceTable(tableName);
        }

        private static void UseTableWithoutTypeSafety()
        {
            const string tableName = "devices";
            var devices = SetupDeviceTable(tableName);

            var pinoMouse = new Device("m1", "PinoMouse");
            var ginoPeach = new Fruit("p6", "GinoPeach");

            devices.AddObject(tableName, pinoMouse);
            devices.AddObject(tableName, ginoPeach);
            devices.SaveChanges();

            var query = devices.CreateQuery<Fruit>(tableName).Where(d => d.RowKey == "p6");
            Debug.Assert(query.First().Name == ginoPeach.Name);

            DeleteDeviceTable(tableName);
        }

        private static void UseCustomTable()
        {
            var devices = SetupCustomDeviceTable();

            var pinoMouse = new Device("m1", "PinoMouse");
            var ginoCam = new Device("c7", "GinoCam");

            devices.AddEntity(pinoMouse);
            devices.AddEntity(ginoCam);
            devices.SaveChanges();

            pinoMouse.Name = "ExtremePinoMouse";
            devices.UpdateEntity(pinoMouse);
            devices.SaveChanges();

            var query = devices.Entities.Where(d => d.RowKey == "m1");
            Debug.Assert(query.First().Name == pinoMouse.Name);

            devices.Delete();
        }

        private static void UseCustomTableWithTypeSafety()
        {
            var devices = SetupCustomDeviceTable();

            var pinoMouse = new Device("m1", "PinoMouse");
            var ginoPeach = new Fruit("p6", "GinoPeach"); // Unused, see below.

            devices.AddEntity(pinoMouse);
            // devices.AddEntity(ginoPeach); -> Blocked by the compiler itself. 
            devices.SaveChanges();

            var query = devices.Entities.Where(d => d.RowKey == "m1");
            Debug.Assert(query.First().Name == pinoMouse.Name);

            devices.Delete();
        }

        private static void UseCustomTableWithInheritance()
        {
            var devices = SetupCustomDeviceTable();

            var pinoMouse = new Device("m1", "PinoMouse");
            var ginoDrive = new UsbDrive("u5", "GinoDrive", 512);
            var bobScreen = new Screen("s3", "BobScreen", 800, 600);

            devices.AddEntity(pinoMouse);
            devices.AddEntity(ginoDrive);
            devices.AddEntity(bobScreen);
            devices.SaveChanges();

            var query = devices.Entities.Where(d => d.Name == "GinoDrive");
            var entity = (UsbDrive)query.First();
            Debug.Assert(entity.CapacityInMb == ginoDrive.CapacityInMb);

            devices.Delete();
        }

        private static TableServiceContext SetupDeviceTable(string tableName)
        {
            var connectionString = Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableEndpointUri = storageAccount.TableEndpoint.ToString();
            var credentials = storageAccount.Credentials;
            var tableClient = new CloudTableClient(tableEndpointUri, credentials);
            tableClient.CreateTableIfNotExist(tableName);
            return tableClient.GetDataServiceContext();
        }

        private static void DeleteDeviceTable(string tableName)
        {
            var connectionString = Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableEndpointUri = storageAccount.TableEndpoint.ToString();
            var credentials = storageAccount.Credentials;
            var tableClient = new CloudTableClient(tableEndpointUri, credentials);
            tableClient.DeleteTable(tableName);
        }

        private static AzureTable<Device> SetupCustomDeviceTable()
        {
            var connectionString = Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableEndpointUri = storageAccount.TableEndpoint.ToString();
            var credentials = storageAccount.Credentials;
            AzureTable<Device>.Create(tableEndpointUri, credentials);
            return AzureTable<Device>.Connect(tableEndpointUri, credentials);
        }
    }
}

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

package jase;

import java.util.List;

import jase.entities.Device;

import org.soyatec.windowsazure.table.ITable;
import org.soyatec.windowsazure.table.ITableServiceEntity;
import org.soyatec.windowsazure.table.TableServiceContext;
import org.soyatec.windowsazure.table.TableStorageClient;

final class TableExamples 
{
	public static void runAll() 
	{
		useTable();
	}
	
	private static void useTable()
	{
		// The context is bound to a specific table, unlike the .NET version.
		TableServiceContext devices = createTable(Device.TABLE_NAME);
		
		Device pinoMouse = Device.create("m1", "PinoMouse");
        Device ginoCam = Device.create("c7", "GinoCam");
		
		devices.insertEntity(pinoMouse);
		devices.insertEntity(ginoCam);
		// There is no need to call a method like "SaveChanges".
		
		pinoMouse.setName("ExtremePinoMouse");
		devices.updateEntity(pinoMouse);
		
		List<ITableServiceEntity> query = devices.retrieveEntitiesByKey(Device.TABLE_NAME, "m1", Device.class);
		Device first = (Device) query.get(0);
		assert(first.getName().equals(pinoMouse.getName()));
		
		deleteTable(Device.TABLE_NAME);
	}
	
	private static TableServiceContext createTable(String tableName)
	{
		TableStorageClient client = createTableClient();
		ITable table = client.getTableReference(tableName);
		if (table.isTableExist())
			table.deleteTable();
		table.createTable();
		return table.getTableServiceContext();
	}
	
	private static void deleteTable(String tableName)
	{
		TableStorageClient client = createTableClient();
		ITable table = client.getTableReference(tableName);
		if (table.isTableExist())
			table.deleteTable();
	}
	
	private static TableStorageClient createTableClient()
	{
		return TableStorageClient.create(
			Settings.TABLE_ENDPOINT, 
			true, 
			Settings.DEV_ACCOUNT_NAME, 
			Settings.DEV_ACCOUNT_KEY
		);
	}
}

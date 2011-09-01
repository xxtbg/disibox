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
using System.Web.UI;
using System.Web.UI.WebControls;
using Disibox.Data.Client;

namespace Disibox.WebUI
{
    public partial class _Default : Page
    {
        public readonly ClientDataSource DataSource = new ClientDataSource();

        private readonly string[] _filesTableHeader = {"Name", "ContentType", "Owner", "Size"};

        private const string AdminEmail = "admin@disibox.com";
        private const string AdminPwd = "roottoor";

        protected void Page_Load(object sender, EventArgs e)
        {
            FillFilesTable();
        }

        protected void UploadButton_Click(object sender, EventArgs e)
        {
            DataSource.Login(AdminEmail, AdminPwd);
            DataSource.AddFile(FileUpload.FileName, FileUpload.FileContent); 
            DataSource.Logout();

            FillFilesTable();
        }

        /*=============================================================================
            Private methods
        =============================================================================*/

        private void FillFilesTable()
        {
            DataSource.Login(AdminEmail, AdminPwd);

            FilesTable.Rows.Clear();

            AddHeaderToFilesTable();

            var metadata = DataSource.GetFileMetadata();
            foreach (var fileMetadata in metadata)
            {
                var fileRow = new TableRow();
                fileRow.Cells.Add(CreateTableCell(fileMetadata.Name));
                fileRow.Cells.Add(CreateTableCell(fileMetadata.ContentType));
                fileRow.Cells.Add(CreateTableCell(fileMetadata.Owner));
                fileRow.Cells.Add(CreateTableCell(fileMetadata.Size.ToString()));
                FilesTable.Rows.Add(fileRow);
            }

            DataSource.Logout();
        }

        private void AddHeaderToFilesTable()
        {
            var header = new TableRow();
            foreach (var columnHeader in _filesTableHeader)
                header.Cells.Add(CreateTableCell(columnHeader));
            FilesTable.Rows.Add(header);
        }

        private static TableCell CreateTableCell(string text)
        {
            var cell = new TableCell();
            cell.Controls.Add(new LiteralControl(text));
            return cell;
        }
    }
}

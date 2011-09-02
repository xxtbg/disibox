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
using Disibox.Data.Client;

namespace Disibox.WebUI
{
    public partial class _Default : Page
    {
        private readonly ClientDataSource _dataSource = new ClientDataSource();

        private const string AdminEmail = "admin@disibox.com";
        private const string AdminPwd = "roottoor";

        /*=============================================================================
            Event handlers
        =============================================================================*/

        protected void Page_Load(object sender, EventArgs e)
        {
            RefreshFilesTable();
            RefreshAdminUsersTable();
            RefreshCommonUsersTable();
        }

        protected void DeleteAdminUsersButton_Click(object sender, EventArgs e)
        {
            RefreshAdminUsersTable();
        }

        protected void DeleteCommonUsersButton_Click(object sender, EventArgs e)
        {
            RefreshCommonUsersTable();
        }

        protected void DeleteFilesButton_Click(object sender, EventArgs e)
        {
            var selectedFiles = FilesTable.GetSelectedItems();
            _dataSource.Login(AdminEmail, AdminPwd);
            foreach (var selectedFile in selectedFiles)
                _dataSource.DeleteFile(selectedFile.Uri);
            _dataSource.Logout();

            RefreshFilesTable();
        }

        protected void UploadButton_Click(object sender, EventArgs e)
        {
            _dataSource.Login(AdminEmail, AdminPwd);
            _dataSource.AddFile(FileUpload.FileName, FileUpload.FileContent); 
            _dataSource.Logout();

            RefreshFilesTable();
        }

        /*=============================================================================
            Private methods
        =============================================================================*/

        private void RefreshFilesTable()
        {
            _dataSource.Login(AdminEmail, AdminPwd);
            FilesTable.Refresh(_dataSource.GetFileMetadata());
            _dataSource.Logout();
        }

        private void RefreshAdminUsersTable()
        {
            _dataSource.Login(AdminEmail, AdminPwd);
            AdminUsersTable.Refresh(_dataSource.GetAdminUsersEmails());
            _dataSource.Logout();
        }

        private void RefreshCommonUsersTable()
        {
            _dataSource.Login(AdminEmail, AdminPwd);
            CommonUsersTable.Refresh(_dataSource.GetCommonUsersEmails());
            _dataSource.Logout();
        }
    }
}

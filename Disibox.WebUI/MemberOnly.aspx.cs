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
using System.IO;
using System.Web.UI;
using Disibox.Data.Client;
using Disibox.Data.Client.Exceptions;
using Disibox.Data.Exceptions;

namespace Disibox.WebUI
{
    public partial class MemberOnly : Page {
        private ClientDataSource _dataSource;
        /*=============================================================================
            Event handlers
        =============================================================================*/

        protected void Page_Load(object sender, EventArgs e)
        {
            var loggedUser = (String)Session["UserEmail"];
            _dataSource = (ClientDataSource) Session["ClientDataSource"];
            if (loggedUser == null)
                Response.Redirect("Account/Login.aspx");

            Literal_Logged.Text = loggedUser;

            RefreshFilesTable();
            RefreshAdminUsersTable();
            RefreshCommonUsersTable();
        }

        protected void DeleteAdminUsersButton_Click(object sender, EventArgs e) {
            var adminsSelected = AdminUsersTable.GetSelectedItems();

            var msg = "There was an error: ";
            try {
                foreach (var admin in adminsSelected) {
                    _dataSource.DeleteUser(admin);
                }
            } catch (UserNotAdminException) {
                msg += "only users with admin priviledges can delete other users!";
            } catch (ArgumentNullException) {
                msg += "one or more users are null!";
            } catch (UserNotExistingException) {
                msg += "one or more users that you selected not exists!";
            } catch (CannotDeleteLastAdminException) {
                msg += "the last admin cannot be deleted!";
            } catch (UserNotLoggedInException) {
                msg = "This is rather impossible... but you are not logged in!";
            } finally {
                DeleteAdminMessage.Text = msg;
            }

            RefreshAdminUsersTable();
        }

        protected void DeleteCommonUsersButton_Click(object sender, EventArgs e) {
            var commonUsersSelected = CommonUsersTable.GetSelectedItems();
            var msg = "There was an error: ";

            try {
                foreach (var commonUser in commonUsersSelected) {
                    _dataSource.DeleteUser(commonUser);
                }
            } catch (UserNotAdminException) {
                msg += "only users with admin priviledges can delete other users!";
            } catch (ArgumentNullException) {
                msg += "one or more users are null!";
            } catch (UserNotExistingException) {
                msg += "one or more users that you selected not exists!";
            } catch (CannotDeleteLastAdminException) {
                msg += "the last admin cannot be deleted!";
            } catch (UserNotLoggedInException) {
                msg = "This is rather impossible... but you are not logged in!";
            } finally {
                DeleteUserCommonMessage.Text = msg;
            }

            RefreshCommonUsersTable();
        }

        protected void DeleteFilesButton_Click(object sender, EventArgs e)
        {
            var selectedFiles = FilesTable.GetSelectedItems();

            var msg = "";
            try {
                foreach (var selectedFile in selectedFiles)
                    _dataSource.DeleteFile(selectedFile.Uri);
            } catch (ArgumentNullException) {
                msg = "One or more files that you selected are null!";
            } catch (FileNotFoundException) {
                msg = "One or more files that you selected ware not found!";
            } catch (FileNotOwnedException) {
                msg = "One or more files that you selected are not owned by you!";
            } catch (InvalidUriException) {
                msg = "One or more files that you selected have not valid uri!";
            } catch (UserNotLoggedInException) {
                msg = "This is rather impossible... but you are not logged in!";
            } finally {
                UploadMessage.Text = msg;
            }

            RefreshFilesTable();
        }

        protected void UploadButton_Click(object sender, EventArgs e) {
            var msg = "";
            try {
                _dataSource.AddFile(FileUpload.FileName, FileUpload.FileContent);
            } catch (ArgumentNullException) {
                msg = "The file that you want to upload is null!";
            } catch (FileExistingException) {
                msg = "The file that you want to upload already exists!";
            } catch (InvalidFileNameException) {
                msg = "The file that you want to upload have an invalid filename!";
            } catch (UserNotLoggedInException) {
                msg = "This is rather impossible... but you are not logged in!";
            } finally {
                UploadMessage.Text = msg;
            }

            if (msg=="")
                RefreshFilesTable();
        }

        /*=============================================================================
            Private methods
        =============================================================================*/

        private void RefreshFilesTable()
        {
            FilesTable.Refresh(_dataSource.GetFileMetadata());
        }

        private void RefreshAdminUsersTable()
        {
            try {
                AdminUsersTable.Refresh(_dataSource.GetAdminUsersEmails());
            } catch {}
        }

        private void RefreshCommonUsersTable()
        {
            try {
                CommonUsersTable.Refresh(_dataSource.GetCommonUsersEmails());
            } catch {}
        }
    }
}

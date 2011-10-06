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
using Disibox.Utils;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data.Entities
{
    /// <summary>
    /// Table entity representing a user.
    /// </summary>
    public class User : TableServiceEntity
    {
        private static readonly string TableName = (typeof (User)).Name.ToLower();

        /// <summary>
        /// Creates a User entity according to given parameters.
        /// In particular, it takes care of storing the hashed password.
        /// </summary>
        /// <param name="userId">User unique identifier.</param>
        /// <param name="userEmail">User email address.</param>
        /// <param name="userPwd">User password (NOT hashed).</param>
        /// <param name="userType">The type of user (admin or common).</param>
        public User(string userId, string userEmail, string userPwd, UserType userType)
            : base(TableName, userId)
        {
            Email = userEmail;
            HashedPassword = Hash.ComputeMD5(userPwd);
            IsAdmin = (userType == UserType.AdminUser);
        }

        /// <summary>
        /// Seems to be required for serialization sake.
        /// </summary>
        [Obsolete]
        public User()
        {
            // Empty
        }

        /// <summary>
        /// User email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Hashed user password.
        /// </summary>
        public string HashedPassword { get; set; }

        /// <summary>
        /// Indicates whether user is administrator.
        /// </summary>
        public bool IsAdmin { get; set; }
    }

    public enum UserType
    {
        AdminUser, CommonUser
    }
}
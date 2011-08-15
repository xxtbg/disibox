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
    internal sealed class User : TableServiceEntity
    {
        public const string UserPartitionKey = "users";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userEmail"></param>
        /// <param name="userPwd"></param>
        /// <param name="userIsAdmin"></param>
        public User(string userId, string userEmail, string userPwd, bool userIsAdmin)
        {
            // TableServiceEntity properties
            PartitionKey = UserPartitionKey;
            RowKey = userId;
            
            // Custom properties
            Email = userEmail;
            HashedPassword = Hash.ComputeMD5(userPwd);
            IsAdmin = userIsAdmin;
        }

        /// <summary>
        /// Seems to be required for serialization sake.
        /// </summary>
        [Obsolete]
        public User()
        {
            // TableServiceEntity properties
            PartitionKey = UserPartitionKey;
            RowKey = UserPartitionKey;
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
        /// 
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}

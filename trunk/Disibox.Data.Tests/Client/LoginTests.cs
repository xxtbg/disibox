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
using Disibox.Data.Entities;
using Disibox.Data.Exceptions;
using NUnit.Framework;

namespace Disibox.Data.Tests.Client
{
    public class LoginTests : BaseClientTests
    {
        private const string InvalidEmail = "PINO";
        private const string ValidEmail = "pino@gino.com";

        private const string EmptyPwd = "";
        private const string ShortPwd = "bau";
        private const string SlightlyShortPwd = "carmela";
        private const string RightPwd = "ottomane";
        private const string SlightlyLongPwd = "pancrazio";
        private const string LongPwd = "pinoginopinoginopinoginopinogino";

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();
        }

        [TearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }

        /*=============================================================================
            Valid calls
        =============================================================================*/

        [Test]
        public void ValidAdminUserLogin()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.Logout();
        }

        [Test]
        public void ValidCommonUserLoginWithRightPwd()
        {
            ValidAddAndLogCommonUser(ValidEmail, RightPwd);
        }

        [Test]
        public void ValidCommonUserLoginWithSlightlyLongPwd()
        {
            ValidAddAndLogCommonUser(ValidEmail, SlightlyLongPwd);
        }

        [Test]
        public void ValidCommonUserLoginWithLongPwd()
        {
            ValidAddAndLogCommonUser(ValidEmail, LongPwd);
        }

        /*=============================================================================
            ArgumentNullException
        =============================================================================*/

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvalidLoginWithNullEmail()
        {
            DataSource.Login(null, RightPwd);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvalidLoginWithNullPwd()
        {
            DataSource.Login(ValidEmail, null);
        }

        /*=============================================================================
            InvalidEmailException
        =============================================================================*/

        [Test]
        [ExpectedException(typeof(InvalidEmailException))]
        public void InvalidLoginWithInvalidEmail()
        {
            DataSource.Login(InvalidEmail, RightPwd);
        }

        /*=============================================================================
            InvalidPasswordException
        =============================================================================*/

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidLoginWithEmptyPwd()
        {
            DataSource.Login(ValidEmail, EmptyPwd);
        }

        [Test]
        [ExpectedException(typeof(InvalidPasswordException))]
        public void InvalidLoginWithShortPwd()
        {
            DataSource.Login(ValidEmail, ShortPwd);
        }

        [Test]
        [ExpectedException(typeof(InvalidPasswordException))]
        public void InvalidLoginWithSlightlyShortPwd()
        {
            DataSource.Login(ValidEmail, SlightlyShortPwd);
        }

        /*=============================================================================
            Private methods
        =============================================================================*/

        private void ValidAddAndLogCommonUser(string userEmail, string userPwd)
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(userEmail, userPwd, UserType.CommonUser);
            DataSource.Logout();

            DataSource.Login(userEmail, userPwd);
            DataSource.Logout();
        }
    }
}

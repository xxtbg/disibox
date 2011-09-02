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
using NUnit.Framework;

namespace Disibox.Data.Tests.Server
{
    public class EnqueueProcMsgTests : BaseProcMsgTests
    {
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
        public void EnqueueOneRequest()
        {
            EnqueueOneMessage(DataSource.EnqueueProcessingRequest, DataSource.PeekProcessingRequests);
        }

        [Test]
        public void EnqueueOneCompletion()
        {
            EnqueueOneMessage(DataSource.EnqueueProcessingCompletion, DataSource.PeekProcessingCompletions);
        }

        [Test]
        public void EnqueueManyRequests()
        {
            EnqueueManyMessages(DataSource.EnqueueProcessingRequest, DataSource.PeekProcessingRequests);
        }

        [Test]
        public void EnqueueManyCompletions()
        {
            EnqueueManyMessages(DataSource.EnqueueProcessingCompletion, DataSource.PeekProcessingCompletions);
        }

        /*=============================================================================
            ArgumentNullException
        =============================================================================*/

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnqueueNullRequest()
        {
            EnqueueNullMessage(DataSource.EnqueueProcessingRequest);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnqueueNullCompletion()
        {
            EnqueueNullMessage(DataSource.EnqueueProcessingCompletion);
        }

        /*=============================================================================
            Private methods
        =============================================================================*/

        private void EnqueueOneMessage(EnqueueMethod enqueueMethod, PeekMethod peekMethod)
        {
            enqueueMethod(Messages[0]);
            var messages = peekMethod();
            Assert.True(messages.Count == 1);
            Assert.True(messages.Contains(Messages[0]));
        }

        private void EnqueueManyMessages(EnqueueMethod enqueueMethod, PeekMethod peekMethod)
        {
            foreach (var message in Messages)
                enqueueMethod(message);
            var messages = peekMethod();
            Assert.True(messages.Count == Messages.Count);
            foreach (var message in Messages)
                Assert.True(messages.Contains(message));
        }

        private static void EnqueueNullMessage(EnqueueMethod enqueueMethod)
        {
            enqueueMethod(null);
        }
    }
}

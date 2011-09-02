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

using System.Collections.Generic;
using Disibox.Data.Server;
using NUnit.Framework;

namespace Disibox.Data.Tests.Server
{
    public abstract class BaseProcMsgTests : BaseServerTests
    {
        private const int MessageCount = 5;
        private const int SuffixLength = 5;

        protected readonly IList<ProcessingMessage> Messages = new List<ProcessingMessage>();

        protected delegate ProcessingMessage DequeueMethod();

        protected delegate void EnqueueMethod(ProcessingMessage msg);

        protected delegate IList<ProcessingMessage> PeekMethod();

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            const string baseUri = "uri-";
            const string baseCType = "ctype-";
            const string baseTName = "tname-";

            for (var i = 0; i < MessageCount; ++i)
            {
                var tmpSuffix = new string((char)('a'+i), SuffixLength);
                Messages.Add(new ProcessingMessage(baseUri + tmpSuffix, baseCType + tmpSuffix, baseTName + tmpSuffix));
            }
        }

        [TearDown]
        protected override void TearDown()
        {
            Messages.Clear();
            base.TearDown();
        }
    }
}

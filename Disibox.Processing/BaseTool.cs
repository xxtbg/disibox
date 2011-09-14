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
using System.Collections.Generic;
using System.IO;

namespace Disibox.Processing
{
    public abstract class BaseTool : IEquatable<BaseTool>
    {
        protected BaseTool(string name, string briefDescription, string longDescription)
        {
            Name = name;
            BriefDescription = briefDescription;
            LongDescription = longDescription;
            ProcessableTypes = new HashSet<string>();
        }

        public string Name { get; private set; }

        public string BriefDescription { get; private set; }

        public string LongDescription { get; private set; }

        /// <summary>
        /// If this set is empty, the tool is taken as multi purpose.
        /// Otherwise, only files having a content type which is in this set
        /// are passed as parameter to the <see cref="ProcessFile"/> method.
        /// </summary>
        public ISet<string> ProcessableTypes { get; private set; }

        public abstract ProcessingOutput ProcessFile(Stream file, string fileContentType);

        public bool Equals(BaseTool other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (BaseTool)) return false;
            return Equals((BaseTool) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(BaseTool left, BaseTool right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseTool left, BaseTool right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", Name, BriefDescription, LongDescription);
        }
    }
}

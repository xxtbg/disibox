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

namespace Disibox.Data.Server
{
    public class ProcessingMessage : IMessage, IEquatable<ProcessingMessage>
    {
        public ProcessingMessage()
        {
            // Empty
        }

        public ProcessingMessage(string fileUri, string fileContentType, string processingToolName)
        {
            FileUri = fileUri;
            FileContentType = fileContentType;
            ToolName = processingToolName;
        }

        public string FileUri { get; private set; }

        public string FileContentType { get; private set; }

        public string ToolName { get; private set; }

        public void FromString(string req)
        {
            var reqParts = req.Split(new[] {','});

            FileUri = reqParts[0];
            FileContentType = reqParts[1];
            ToolName = reqParts[2];
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", FileUri, FileContentType, ToolName);
        }

        public bool Equals(ProcessingMessage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.FileUri, FileUri) && Equals(other.FileContentType, FileContentType) && Equals(other.ToolName, ToolName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ProcessingMessage)) return false;
            return Equals((ProcessingMessage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = FileUri.GetHashCode();
                result = (result*397) ^ FileContentType.GetHashCode();
                result = (result*397) ^ ToolName.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(ProcessingMessage left, ProcessingMessage right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProcessingMessage left, ProcessingMessage right)
        {
            return !Equals(left, right);
        }
    }
}
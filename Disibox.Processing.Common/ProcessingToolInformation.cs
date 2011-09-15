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

using System.Text.RegularExpressions;

namespace Disibox.Processing.Common
{
    public class ProcessingToolInformation
    {
        private ProcessingToolInformation(string name, string briefDescr, string longDescr)
        {
            Name = name;
            BriefDescription = briefDescr;
            LongDescription = longDescr;
        }

        public string Name { get; private set; }

        public string BriefDescription { get; private set; }

        public string LongDescription { get; private set; }
        
        public static ProcessingToolInformation FromString(string info)
        {
            var splittedInfo = Regex.Split(info, "\", \"");
            var name = splittedInfo[0].Substring(1); // To avoid initial '"'
            var briefDescr = splittedInfo[1];
            var longDescr = splittedInfo[2].Substring(0, splittedInfo[2].Length-1); // To avoid final '"'
            return new ProcessingToolInformation(name, briefDescr, longDescr);
        }
    }
}

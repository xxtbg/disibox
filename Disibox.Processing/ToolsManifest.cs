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
//     * Neither the name of the <organization> nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Disibox.Processing.Exceptions;
using Disibox.Utils;

namespace Disibox.Processing
{
    public static class ToolsManifest
    {
        /// <summary>
        /// Contains all available tools.
        /// </summary>
        private static readonly ISet<BaseTool> ProcTools = new HashSet<BaseTool>();

        /// <summary>
        /// Contains the tools that work with all content types.
        /// </summary>
        private static readonly ISet<BaseTool> MultiPurposeTools = new HashSet<BaseTool>();

        /// <summary>
        /// The key is the content type, the value is the tool.
        /// </summary>
        private static readonly ISet<Pair<string, BaseTool>> SpecificTools = new HashSet<Pair<string, BaseTool>>();

        static ToolsManifest()
        {
            InitTools();
        }

        public static void Main()
        {
            Console.WriteLine("Available tools:");
            foreach (var tool in ProcTools)
                Console.WriteLine("* " + tool.Name);

            // Empty line
            Console.WriteLine();

            Console.WriteLine("Multipurpose tools:");
            foreach (var tool in MultiPurposeTools)
                Console.WriteLine("* " + tool.Name);

            // Empty line
            Console.WriteLine();

            Console.WriteLine("Specific tools:");
            foreach (var entry in SpecificTools)
                Console.WriteLine(string.Format("* {0} ({1})", entry.Second.Name, entry.First));
            
            // Empty line
            Console.WriteLine();

            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        public static IList<BaseTool> GetAvailableTools(string fileContentType)
        {
            var availableTools = SpecificTools.Where(e => e.First == fileContentType).Select(e => e.Second);
            return MultiPurposeTools.Concat(availableTools).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toolName"></param>
        /// <returns></returns>
        public static BaseTool GetTool(string toolName)
        {
            var query = ProcTools.Where(t => t.Name == toolName);
            if (query.Count() == 0)
                throw new ToolNotExistingException();
            return query.First();
        }

        private static void InitTools()
        {
            var toolTypes = LoadToolsAssembly();

            foreach (var toolType in toolTypes)
            {
                // We require the tool to implement BaseTool abstract class.
                if (!toolType.IsSubclassOf(typeof(BaseTool))) continue;

                // We need a concrete class in order to create an instance of it.
                if (toolType.IsAbstract) continue;

                var tool = (BaseTool) Activator.CreateInstance(toolType);

                ProcTools.Add(tool);

                if (tool.ProcessableTypes.Count() == 0) // True for a multi purpose tool
                {
                    MultiPurposeTools.Add(tool);
                    continue;
                }

                foreach (var contentType in tool.ProcessableTypes)
                {
                    var entry = new Pair<string, BaseTool>(contentType, tool);
                    SpecificTools.Add(entry);
                }
            }
        }

        private static IEnumerable<Type> LoadToolsAssembly()
        {
            var toolsAssemblyPath = Properties.Settings.Default.ToolsAssemblyPath;
            var procAssembly = Assembly.LoadFile(toolsAssemblyPath);
            return procAssembly.GetTypes();
        }
    }
}

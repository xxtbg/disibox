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
        /// 
        /// </summary>
        private static readonly IDictionary<string, BaseTool> ProcTools = new Dictionary<string, BaseTool>();

        /// <summary>
        /// Contains the tools that work with all content types.
        /// </summary>
        private static readonly IList<BaseTool> MultiPurposeTools = new List<BaseTool>();

        /// <summary>
        /// The key is the content type, the value is the tool.
        /// </summary>
        private static ILookup<string, BaseTool> _specificTools;

        static ToolsManifest()
        {
            InitTools();
        }

        public static void Main()
        {
            Console.WriteLine("Available tools:");
            foreach (var entry in ProcTools)
                Console.WriteLine("* " + entry.Key);

            // Empty line
            Console.WriteLine();

            Console.WriteLine("Multipurpose tools:");
            foreach (var tool in MultiPurposeTools)
                Console.WriteLine("* " + tool.Name);

            // Empty line
            Console.WriteLine();

            Console.WriteLine("Specific tools:");
            foreach (var entry in _specificTools)
            {
                Console.WriteLine("# " + entry.Key + ":");
                foreach (var tool in entry)
                    Console.WriteLine(" * " + tool.Name);
            }
            
            // Empty line
            Console.WriteLine();

            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        public static IList<BaseTool> GetAvailableTools(string fileContentType)
        {
            var availableTools = _specificTools[fileContentType];
            return MultiPurposeTools.Concat(availableTools).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toolName"></param>
        /// <returns></returns>
        public static BaseTool GetTool(string toolName)
        {
            BaseTool tool;
            if (ProcTools.TryGetValue(toolName, out tool)) return tool;
            throw new ToolNotExistingException();
        }

        private static void InitTools()
        {
            var tmpAvailableTools = new List<Pair<string, BaseTool>>();

            var toolsAssemblyPath = Properties.Settings.Default.ToolsAssemblyPath;
            var procAssembly = Assembly.LoadFile(toolsAssemblyPath);
            var procTypes = procAssembly.GetTypes();

            foreach (var toolType in procTypes)
            {
                // We require the tool to implement BaseTool abstract class.
                if (!toolType.IsSubclassOf(typeof(BaseTool))) continue;

                var tool = (BaseTool) Activator.CreateInstance(toolType);

                ProcTools.Add(tool.Name, tool);

                if (tool.ProcessableTypes.Count() == 0) // True for a multi purpose tool
                {
                    MultiPurposeTools.Add(tool);
                    continue;
                }

                foreach (var contentType in tool.ProcessableTypes)
                {
                    var tmpPair = new Pair<string, BaseTool>(contentType, tool);
                    tmpAvailableTools.Add(tmpPair);
                }
            }

            _specificTools = tmpAvailableTools.ToLookup(p => p.First, p => p.Second);
        }
    }
}

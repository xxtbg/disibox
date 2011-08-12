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
            BaseTool tool;
            if (ProcTools.TryGetValue(toolName, out tool)) return tool;
            throw new ToolNotExistingException();
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

                ProcTools.Add(tool.Name, tool);

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

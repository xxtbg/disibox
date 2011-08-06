using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Disibox.Utils;

namespace Disibox.Processing
{
    public static class Manifest
    {
        private static IDictionary<string, ITool> _procTools;

        /// <summary>
        /// Contains the names of the tools that work with all content types.
        /// </summary>
        private static IList<string> _multiPurposeTools;

        /// <summary>
        /// The key is the content type, the value is the tool name.
        /// </summary>
        private static ILookup<string, string> _availableTools;

        static Manifest()
        {
            InitTools();
        }

        public static IEnumerable<string> GetAvailableTools(string fileContentType)
        {
            var availableTools = _availableTools[fileContentType];
            return _multiPurposeTools.Concat(availableTools);
        }

        private static void InitTools()
        {
            _procTools = new Dictionary<string, ITool>();
            _multiPurposeTools = new List<string>();
            var tmpAvailableTools = new List<Pair<string, string>>();

            var toolsAssemblyPath = Properties.Settings.Default.ToolsAssemblyPath;
            var procAssembly = Assembly.LoadFile(toolsAssemblyPath);
            var procTypes = procAssembly.GetTypes();

            var iToolType = Type.GetType("ITool");

            foreach (var toolType in procTypes)
            {
                // We require the tool to implement the ITool interface.
                if (!toolType.GetInterfaces().Contains(iToolType)) continue;

                var tool = (ITool) Activator.CreateInstance(toolType);
                var toolId = toolType.ToString();

                _procTools.Add(toolId, tool);

                if (tool.ProcessableTypes.Count() == 0)
                {
                    _multiPurposeTools.Add(toolId);
                    continue;
                }

                foreach (var contentType in tool.ProcessableTypes)
                {
                    var tmpPair = new Pair<string, string>(contentType, toolId);
                    tmpAvailableTools.Add(tmpPair);
                }

                _availableTools = tmpAvailableTools.ToLookup(p => p.First, p => p.Second);
            }
        }
    }
}

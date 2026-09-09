#if BUILD_UPLOADER_WIKI && BUILD_UPLOADER_PIPELINE
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Unity.Pipeline.Commands;

namespace Wireframe
{
    [InitializeOnLoad]
    internal static class PipelineWiki
    {
        private const string CLIHeader = "## Commands";

        static PipelineWiki()
        {
            Wiki.ExportAdditionalPages += WriteCLICommands;
        }

        private class CommandData
        {
            public MethodInfo methodInfo;
            public CliCommandAttribute commandAttribute;
            public CommandArg[] args;
        }
        
        private class CommandArg
        {
            public CliArgAttribute CliArg;
            public ParameterInfo Parameter;

        }

        private static List<CommandData> GetCommands()
        {
            var list = new List<CommandData>();
            
            IEnumerable<MethodInfo> methodInfos = typeof(PipelineCommands)
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(a=>a.GetCustomAttribute<CliCommandAttribute>() != null);
            foreach (MethodInfo methodInfo in methodInfos)
            {
                CliCommandAttribute commandAttribute = methodInfo.GetCustomAttribute<CliCommandAttribute>();
                var parameters = methodInfo.GetParameters().Where(a=>a.GetCustomAttribute<CliArgAttribute>() != null);

                CommandData commandData = new CommandData();
                commandData.methodInfo = methodInfo;
                commandData.commandAttribute = commandAttribute;
                commandData.args = parameters.Select((a, b)=>
                {
                    return new CommandArg()
                    {
                        CliArg = a.GetCustomAttribute<CliArgAttribute>(),
                        Parameter = a,
                    };
                }).ToArray();
                list.Add(commandData);
            }
            
            return list;
        }

        private static void WriteCLICommands()
        {
            string filePath = Path.Combine(Application.dataPath, "../Wiki/CLI.md");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Command Line Interface");
            sb.AppendLine();
            sb.AppendLine("Run Build Uploader operations from a terminal, a CI job or an AI agent.");
            sb.AppendLine();
            sb.AppendLine($"**The Unity CLI** drives an Editor that is *already open*, which makes it the one to use while you are working - and the one an agent should reach for. It requires Unity's [Pipeline package](https://github.com/Unity-Technologies/com.unity.pipeline) (`com.unity.pipeline`), which exposes the running Editor over a local HTTP server: install the `unity` CLI, run `unity pipeline install` in your project, then open it in the Editor. The package is completely optional - if it is not installed the `{PipelineCommands.COMMAND}` command simply is not registered and nothing else changes.");
            sb.AppendLine();
            sb.AppendLine(CLIHeader);

            // One table per group, groups and arguments both in declaration order.
            bool isFirst = true;
            foreach (CommandData group in GetCommands().OrderBy(a=>a.commandAttribute.Name))
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.AppendLine();
                }
                
                sb.AppendLine("```");
                sb.AppendLine($"unity command {group.commandAttribute.Name} --<argument> <value>");
                sb.AppendLine($"unity command {group.commandAttribute.Name} --<argument> <value1>,<value2>");
                sb.AppendLine("```");
                sb.AppendLine();
                sb.AppendLine("### Arguments");
                
                if (group.args.Length == 0)
                {
                    sb.AppendLine("No args");
                    continue;
                }
                
                sb.AppendLine("| Argument | Required | Type | Description |");
                sb.AppendLine("| --- | --- | --- | --- |");
                
                foreach (CommandArg arg in group.args)
                {
                    sb.AppendLine($"| `{arg.CliArg.Name}` | {arg.CliArg.Required} | {arg.Parameter.ParameterType.Name} | {arg.CliArg.Description} |");
                }
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
#endif

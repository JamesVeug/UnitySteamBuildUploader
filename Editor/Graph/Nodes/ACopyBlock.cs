#if BUILD_UPLOADER_GRAPHTOOLKIT
using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Shared base for self-contained copy operations: take a full Source path, copy (optionally zipped) to a full
    /// Destination path. Compiles to one complete <see cref="UploadConfig"/> via the existing Serialize/Deserialize
    /// contracts — no new setters or reflection into runtime types.
    ///
    /// Paths are plain string options for now (a "..." browse button needs an in-node UI hook that Graph Toolkit
    /// 0.4-exp.2 does not expose — see NodeGraph-Design.md "API limitations").
    /// </summary>
    [Serializable]
    public abstract class ACopyBlock : BlockNode, ICopyBlock
    {
        public const string OptSourcePath = "sourcePath";
        public const string OptDestinationPath = "destinationPath";
        public const string OptZip = "zip";
        public const string OptZipName = "zipName";
        public const string OptDuplicates = "duplicates";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptSourcePath).WithDisplayName("Source Path").Build();
            context.AddOption<string>(OptDestinationPath).WithDisplayName("Destination Path").Build();
            context.AddOption<bool>(OptZip).WithDisplayName("Zip Content").Build();
            context.AddOption<string>(OptZipName).WithDisplayName("Zip Name").Build();
            context.AddOption<Utils.FileExistHandling>(OptDuplicates).WithDisplayName("Duplicate Files").Build();
        }

        /// <summary>The runtime source this operation reads from (folder vs file).</summary>
        protected abstract AUploadSource CreateSourceInstance();

        public UploadConfig CompileConfig(GraphCompileLog log)
        {
            GetNodeOptionByName(OptSourcePath).TryGetValue(out string sourcePath);
            GetNodeOptionByName(OptDestinationPath).TryGetValue(out string destinationPath);
            GetNodeOptionByName(OptZip).TryGetValue(out bool zip);
            GetNodeOptionByName(OptZipName).TryGetValue(out string zipName);
            GetNodeOptionByName(OptDuplicates).TryGetValue(out Utils.FileExistHandling duplicates);

            if (string.IsNullOrEmpty(sourcePath))
            {
                log.Error($"{GetType().Name}: no Source Path set.");
            }

            if (string.IsNullOrEmpty(destinationPath))
            {
                log.Error($"{GetType().Name}: no Destination Path set.");
            }

            if (zip && string.IsNullOrEmpty(zipName))
            {
                log.Error($"{GetType().Name}: Zip Content is on but no Zip Name set.");
            }

            AUploadSource source = CreateSourceInstance();
            source.Deserialize(new Dictionary<string, object>
            {
                { "enteredFilePath", sourcePath ?? string.Empty },
                { "pathType", 0L }, // ABrowsePathSource.PathType.Absolute
            });

            LocalPathDestination destination = new LocalPathDestination();
            destination.Deserialize(new Dictionary<string, object>
            {
                { "m_localPath", destinationPath ?? string.Empty },
                { "m_fileName", zipName ?? string.Empty },
                { "m_zipContent", zip },
                // Box as long: LocalPathDestination.Deserialize gates this on `is long`.
                { "m_duplicateFileHandling", (long)duplicates },
            });

            UploadConfig config = new UploadConfig();
            config.AddSource(source);
            config.AddDestination(destination);
            return config;
        }
    }
}
#endif

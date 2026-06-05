#if BUILD_UPLOADER_GRAPHTOOLKIT
using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace Wireframe
{
    /// <summary>
    /// Sends a Discord channel message. Compiles to a post-action attached to the nearest preceding copy block's
    /// config in the same group, so it fires after that operation completes.
    ///
    /// App / Server / Channel are raw IDs for now (no service dropdowns — that needs an in-node UI hook Graph
    /// Toolkit 0.4-exp.2 doesn't expose; see NodeGraph-Design.md).
    /// </summary>
    [Serializable]
    [UseWithContext(typeof(SequentialGroupNode), typeof(ParallelGroupNode))]
    public class DiscordMessageBlock : BlockNode, IActionBlock
    {
        public const string OptAppId = "appId";
        public const string OptServerId = "serverId";
        public const string OptChannelId = "channelId";
        public const string OptText = "text";
        public const string OptWhen = "when";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(OptAppId).WithDisplayName("App Id").Build();
            context.AddOption<int>(OptServerId).WithDisplayName("Server Id").Build();
            context.AddOption<int>(OptChannelId).WithDisplayName("Channel Id").Build();
            context.AddOption<string>(OptText).WithDisplayName("Text").Build();
            context.AddOption<UploadConfig.UploadActionData.UploadCompleteStatus>(OptWhen)
                .WithDisplayName("When To Execute").Build();
        }

        public UploadConfig.UploadActionData CompileAction(GraphCompileLog log)
        {
            GetNodeOptionByName(OptAppId).TryGetValue(out int appId);
            GetNodeOptionByName(OptServerId).TryGetValue(out int serverId);
            GetNodeOptionByName(OptChannelId).TryGetValue(out int channelId);
            GetNodeOptionByName(OptText).TryGetValue(out string text);
            GetNodeOptionByName(OptWhen).TryGetValue(out UploadConfig.UploadActionData.UploadCompleteStatus when);

            if (string.IsNullOrEmpty(text))
            {
                log.Error($"{nameof(DiscordMessageBlock)}: no message text set.");
            }

            DiscordMessageChannelAction action = new DiscordMessageChannelAction();
            action.Deserialize(new Dictionary<string, object>
            {
                { "app", (long)appId },
                { "serverId", (long)serverId },
                { "channelId", (long)channelId },
                { "text", text ?? string.Empty },
                { "embeds", new List<object>() },
            });

            return new UploadConfig.UploadActionData(action, when,
                UploadConfig.UploadActionData.UploadTrigger.OnTaskFinished);
        }
    }
}
#endif

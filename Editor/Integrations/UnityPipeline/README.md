# Optional Unity Pipeline integration

This editor assembly is enabled only when `com.unity.pipeline` is installed. Its package version define and assembly constraint keep the core Build Uploader usable without Unity Pipeline.

`PipelineCommands` registers the Build Uploader CLI command. `PipelineWiki` adds CLI documentation to wiki exports when `BUILD_UPLOADER_WIKI` is enabled. The core editor assembly must not reference this assembly or Unity Pipeline.

Installing or removing Unity Pipeline automatically enables or disables this integration after Unity recompiles.

using System;

[Serializable]
public class UploadDestinationStringWrapper
{
    public string DisplayName;
    public string InternalName;
    public string CliArg;
    public string Value;
    public bool ShowFormatted;
    public bool required;
    public bool skip;

    public UploadDestinationStringWrapper(string displayName, string internalName, string cliArg, bool required = true, bool skip = false)
    {
        DisplayName = displayName;
        InternalName = internalName;
        CliArg = cliArg;
        Value = string.Empty;
        this.required = required;
        this.skip = skip;
    }
}
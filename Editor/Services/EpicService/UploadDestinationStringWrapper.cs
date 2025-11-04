public class UploadDestinationStringWrapper
{
    private string dictionaryName;
    private string displayName;
    private string argName;
    public bool enabled { get; set; }
    public string value { get; set; }

    public UploadDestinationStringWrapper(string dictionaryName, string displayName, string argName)
    {
        this.dictionaryName = dictionaryName;
        this.displayName = displayName;
    }

    public string GetDicName()
    {
        return dictionaryName;
    }

    public string GetArg()
    {
        return $"{argName} \"{value}\"";
    }
}
public class UploadDestinationStringWrapper
{
    private string dictionaryName;
    private string displayName;
    public bool enabled { get; set; }

    public string GetDicName()
    {
        return dictionaryName;
    }
}
namespace Wireframe
{
    public partial class LocalPathDestination : StringFormatter.IContextModifier
    {
        public bool ReplaceString(string key, out string value, StringFormatter.Context ctx)
        {
            if (key == StringFormatter.DESTINATION_LOCAL_PATH_KEY)
            {
                value = FullPath(ctx);
                return true;
            }
            
            value = "";
            return false;
        }
    }
}
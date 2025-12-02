namespace Wireframe
{
    public partial class UploadConfig : StringFormatter.IContextModifier
    {
        public bool ReplaceString(string key, out string value, StringFormatter.Context ctx)
        {
            foreach (SourceData source in Sources)
            {
                if (!source.Enabled)
                {
                    continue;
                }

                if (source.Source is StringFormatter.IContextModifier modifier)
                {
                    if (modifier.ReplaceString(key, out value, ctx))
                    {
                        return true;
                    }
                }
            }

            foreach (DestinationData destination in Destinations)
            {
                if (!destination.Enabled)
                {
                    continue;
                }

                if (destination.Destination is StringFormatter.IContextModifier modifier)
                {
                    if (modifier.ReplaceString(key, out value, ctx))
                    {
                        return true;
                    }
                }
            }
            
            value = "";
            return false;
        }
    }
}
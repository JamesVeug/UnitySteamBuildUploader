namespace Wireframe
{
    public partial class UploadConfig
    {
        private class UploadConfigContext : Context
        {
            private UploadConfig config;

            public UploadConfigContext(UploadConfig config)
            {
                this.config = config;
            }

            public override bool TryFormatKeyLocally(string key, out string value)
            {
                if (base.TryFormatKeyLocally(key, out value))
                {
                    return true;
                }

                foreach (SourceData source in config.Sources)
                {
                    if (!source.Enabled || source.Source == null)
                    {
                        continue;
                    }

                    if (source.Source.Context.TryFormatKeyLocally(key, out value))
                    {
                        return true;
                    }
                }

                foreach (ModifierData modifier in config.Modifiers)
                {
                    if (!modifier.Enabled || modifier.Modifier == null)
                    {
                        continue;
                    }

                    if (modifier.Modifier.Context.TryFormatKeyLocally(key, out value))
                    {
                        return true;
                    }
                }

                foreach (DestinationData destination in config.Destinations)
                {
                    if (!destination.Enabled || destination.Destination == null)
                    {
                        continue;
                    }

                    if (destination.Destination.Context.TryFormatKeyLocally(key, out value))
                    {
                        return true;
                    }
                }

                value = "";
                return false;
            }
        }
    }
}
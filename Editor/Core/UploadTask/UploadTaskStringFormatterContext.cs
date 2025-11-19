namespace Wireframe
{
    public class UploadTaskStringFormatterContext : StringFormatter.Context, StringFormatter.IContextModifier
    {
        private readonly UploadTask task;

        public UploadTaskStringFormatterContext(UploadTask task)
        {
            this.task = task;
            AddModifier(this);
        }

        public bool ReplaceString(string key, out string value)
        {
            foreach (UploadConfig config in task.UploadConfigs)
            {
                if (config.ReplaceString(key, out value))
                {
                    return true;
                }
            }
            
            value = null;
            return false;
        }
    }
}
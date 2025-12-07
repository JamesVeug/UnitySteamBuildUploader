namespace Wireframe
{
    public class UploadTaskStringFormatterContext : Context
    {
        private readonly IContextContainer _container;

        public UploadTaskStringFormatterContext(IContextContainer container)
        {
            this._container = container;
        }

        public override bool TryFormatKeyLocally(string key, out string value)
        {
            if (base.TryFormatKeyLocally(key, out value))
            {
                return true;
            }
            
            foreach (UploadConfig config in _container.UploadConfigs)
            {
                if (config.Context.TryFormatKeyLocally(key, out value))
                {
                    return true;
                }
            }
            
            foreach (UploadConfig.UploadActionData action in _container.PreUploadActions)
            {
                if (action.WhenToExecute != UploadConfig.UploadActionData.UploadCompleteStatus.Never &&
                    action.UploadAction != null)
                {
                    if (action.UploadAction.Context.TryFormatKeyLocally(key, out value))
                    {
                        return true;
                    }
                }
            }
            
            foreach (UploadConfig.UploadActionData action in _container.PostUploadActions)
            {
                if (action.WhenToExecute != UploadConfig.UploadActionData.UploadCompleteStatus.Never &&
                    action.UploadAction != null)
                {
                    if (action.UploadAction.Context.TryFormatKeyLocally(key, out value))
                    {
                        return true;
                    }
                }
            }
            
            value = null;
            return false;
        }
    }
}
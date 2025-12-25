namespace Wireframe
{
    public abstract partial class AUploadModifer
    {
        /// <summary>
        /// Draw the full GUI of the source so everything can be modified
        /// Executed when the Upload Config is expanded
        /// </summary>
        /// <param name="isDirty">Set to true when something is changed to save the config</param>
        /// <param name="ctx">Context for formatting strings such as {version}</param>
        protected internal abstract void OnGUIExpanded(ref bool isDirty, Context ctx);
        
        /// <summary>
        /// Returns a short summary of the different things for this that can be changed
        /// eg: Compress: Compressing 'C:/MyFiles/ThatImportantFile'
        /// Used for {taskStatus}
        /// </summary>
        public abstract string Summary();
    }
}
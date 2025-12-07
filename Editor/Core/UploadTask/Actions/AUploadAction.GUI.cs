namespace Wireframe
{
    public abstract partial class AUploadAction
    {
        /// <summary>
        /// Draw a 1 line summary of the source 
        /// Executed when the Upload Config is collapsed
        /// </summary>
        /// <param name="isDirty">Set to true when something is changed to save the config</param>
        /// <param name="maxWidth">How wide the UI can be</param>
        public abstract void OnGUICollapsed(ref bool isDirty, float maxWidth);

        /// <summary>
        /// Draw the full GUI of the source so everything can be modified
        /// Executed when the Upload Config is expanded
        /// </summary>
        /// <param name="isDirty">Set to true when something is changed to save the config</param>
        public abstract void OnGUIExpanded(ref bool isDirty);
    }
}
namespace Wireframe
{
    public abstract partial class AExcludePathsByRegex_BuildModifier
    {
        protected internal override void OnGUIExpanded(ref bool isDirty)
        {
            isDirty |= m_reorderableList.OnGUI();
        }
    }
}
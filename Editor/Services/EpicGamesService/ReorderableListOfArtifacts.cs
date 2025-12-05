using UnityEditor;
using UnityEngine;

namespace Wireframe
{
    public class ReorderableListOfArtifacts : InternalReorderableList<EpicGamesArtifact>
    {
        protected override void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EpicGamesArtifact element = list[index];
                
                float width = 50;
                Rect rect0 = new Rect(rect.x, rect.y, width, rect.height);
                GUI.Label(rect0, "Name");

                width = 200;
                Rect rect1 = new Rect(rect0.x + rect0.width, rect0.y, width, rect0.height);
                string n = GUI.TextField(rect1, element.Name);
                if (n != element.Name)
                {
                    element.Name = n.Trim();
                    dirty = true;
                }
                
                width = 25;
                Rect rect2 = new Rect(rect1.x + rect1.width, rect1.y, width, rect1.height);
                GUI.Label(rect2, "ID");
                
                width = rect.width - (50 + 200 + 25);
                Rect rect3 = new Rect(rect2.x + rect2.width, rect2.y, width, rect2.height);
                string n2 = GUI.TextField(rect3, element.ArtifactID);
                if (n2 != element.ArtifactID)
                {
                    element.ArtifactID = n2.Trim();
                    dirty = true;
                }
            }
        }

        protected override EpicGamesArtifact CreateItem(int index)
        {
            EpicGamesArtifact artifact = new EpicGamesArtifact();
            artifact.ID = index + 1;
            return artifact;
        }
        
        protected override int CompareTo(EpicGamesArtifact a, EpicGamesArtifact b)
        {
            return string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
        }
    }
}
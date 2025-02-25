using System.Collections.Generic;

namespace Wireframe
{
    internal static class UnityCloudAPIEditorUtil
    {
        internal class UnityCloudTargetPopup : CustomDropdown<UnityCloudTarget>
        {
            protected override List<UnityCloudTarget> FetchAllData()
            {
                return UnityCloudAPI.CloudBuildTargets;
            }
        }

        internal class UnityCloudBuildPopup : CustomMultiDropdown<UnityCloudTarget, UnityCloudBuild>
        {
            public override List<(UnityCloudTarget, List<UnityCloudBuild>)> GetAllData()
            {
                return UnityCloudAPI.CurrentBuilds;
            }

            public override bool IsItemValid(UnityCloudBuild y)
            {
                return y.IsSuccessful;
            }

            public override int SortByName(UnityCloudBuild a, UnityCloudBuild b)
            {
                return b.build.CompareTo(a.build);
            }
        }

        public static UnityCloudTargetPopup TargetPopup => m_targetPopup ?? (m_targetPopup = new UnityCloudTargetPopup());

        private static UnityCloudTargetPopup m_targetPopup;

        public static UnityCloudBuildPopup BuildPopup => m_buildPopup ?? (m_buildPopup = new UnityCloudBuildPopup());
        private static UnityCloudBuildPopup m_buildPopup;

    }
}
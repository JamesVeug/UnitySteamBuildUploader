using System.Collections.Generic;

namespace Wireframe
{
    public static class UnityCloudAPIEditorUtil
    {
        public class UnityCloudTargetPopup : CustomDropdown<UnityCloudTarget>
        {
            public override List<UnityCloudTarget> GetAllData()
            {
                return UnityCloudAPI.CloudBuildTargets;
            }
        }

        public class UnityCloudBuildPopup : CustomMultiDropdown<UnityCloudTarget, UnityCloudBuild>
        {
            public override List<(UnityCloudTarget, List<UnityCloudBuild>)> GetAllData()
            {
                return UnityCloudAPI.CurrentBuilds;
            }

            public override string ItemDisplayName(UnityCloudBuild y)
            {
                return y.CreateBuildName();
            }

            public override bool IsItemValid(UnityCloudBuild y)
            {
                return y.IsSuccessful;
            }

            public override int CompareTo(UnityCloudBuild a, UnityCloudBuild b)
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
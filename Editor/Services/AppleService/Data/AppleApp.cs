using System;
using System.Collections.Generic;

namespace Wireframe
{
    public partial class AppleConfig
    {
        /// <summary>
        /// An App Store Connect application
        /// </summary>
        [Serializable]
        public class AppleApp : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Name;

            /// <summary>The bundle identifier (e.g. com.example.MyGame).</summary>
            public string BundleID;

            /// <summary>
            /// The App Store Connect "app" resource ID. Numeric string returned by
            /// GET /v1/apps. Required for any REST call that references this app.
            /// </summary>
            public string AppStoreConnectID;

            /// <summary>iOS / tvOS / macOS / visionOS. Passed as --type to xcrun altool.</summary>
            public ApplePlatform Platform = ApplePlatform.iOS;

            public List<AppleBetaGroup> betaGroups;

            private int m_id;

            public AppleApp()
            {
                m_id = 0;
                Name = "Template";
                BundleID = "";
                AppStoreConnectID = "";
                betaGroups = new List<AppleBetaGroup>(2);
            }

            public AppleApp(int id, string displayName, string bundleId)
            {
                m_id = id;
                Name = displayName;
                BundleID = bundleId;
                AppStoreConnectID = "";
                betaGroups = new List<AppleBetaGroup>(2);
            }
        }

        /// <summary>
        /// A TestFlight beta group within an App. Mirrors the Slack "Channel" tier.
        /// </summary>
        [Serializable]
        public class AppleBetaGroup : DropdownElement
        {
            public int Id
            {
                get => m_id;
                set => m_id = value;
            }

            public string DisplayName => Name;

            public string Name;

            /// <summary>App Store Connect "betaGroup" resource ID.</summary>
            public string BetaGroupID;

            private int m_id;

            public AppleBetaGroup()
            {
                m_id = 0;
                Name = "Template";
                BetaGroupID = "";
            }

            public AppleBetaGroup(int id, string displayName, string betaGroupId)
            {
                m_id = id;
                Name = displayName;
                BetaGroupID = betaGroupId;
            }
        }
    }

    public enum ApplePlatform
    {
        iOS = 0,
        tvOS = 1,
        macOS = 2,
        visionOS = 3,
    }
}

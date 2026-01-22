using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Profile;

namespace Wireframe
{
    internal static class BuildProfileUIUtils
    {
        public class BuildProfilePopup : CustomDropdown<BuildProfileWrapper>
        {
            public override string FirstEntryText => "Choose Build Profile";

            protected override List<BuildProfileWrapper> FetchAllData()
            {
                GetBuildProfiles();
                return data;
            }
        }

        private static List<BuildProfileWrapper> data;

        public static List<BuildProfileWrapper> GetBuildProfiles()
        {
            if (data == null)
            {
                LoadAll();
            }
            return data;
        }

		[MenuItem("Window/Build Uploader/Other/Reload Build Profiles")]
        private static void LoadAll()
        {
            data = new List<BuildProfileWrapper>();
            
            Type type = Type.GetType("UnityEditor.Build.Profile.BuildProfileModuleUtil, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            MethodInfo getProfiles = type.GetMethod("GetAllBuildProfiles", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            List<BuildProfile> profiles = (List<BuildProfile>)getProfiles.Invoke(null, null);
            for (var i = 0; i < profiles.Count; i++)
            {
                var profile = profiles[i];
                BuildProfileWrapper wrapper = new BuildProfileWrapper(profile, i + 1);
                data.Add(wrapper);
            }
        }

        public static void Clear()
        {
            data.Clear();
        }

        public static BuildProfilePopup BuildProfilesPopup => m_buildProfilePopup ?? (m_buildProfilePopup = new BuildProfilePopup());
        private static BuildProfilePopup m_buildProfilePopup;
    }
}
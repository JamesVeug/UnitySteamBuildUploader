#if UNITY_2020_2_OR_NEWER
using UnityEditor;
#endif

namespace Wireframe
{
    public static class ProgressUtils
    {
        public static bool Exists(int progressId)
        {
#if UNITY_2020_2_OR_NEWER
            return Progress.Exists(progressId);
#else
            return false;
#endif
        }

        public static void Remove(int progressId)
        {
#if UNITY_2020_2_OR_NEWER
            Progress.Remove(progressId);
#endif
        }

        public static int Start(string title, string desc)
        {
#if UNITY_2020_2_OR_NEWER
            return Progress.Start(title, desc);
#else
            return -1;
#endif
        }

        public static void Report(int progressId, float progress, string description)
        {
#if UNITY_2020_2_OR_NEWER
            Progress.Report(progressId, progress, description);
#endif
        }
    }
}
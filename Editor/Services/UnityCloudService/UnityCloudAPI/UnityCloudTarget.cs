using System;
using System.Collections.Generic;

namespace Wireframe
{
   public enum Platform
   {
      Windows,
      Mac,
   }

   internal static class UnityCloudConfigExtensions
   {
      public static string ToPlatformString(this Platform p)
      {
         switch (p)
         {
            case Platform.Windows:
               return "standalonewindows64";
            case Platform.Mac:
               return "standaloneosxuniversal";
            default:
               throw new ArgumentOutOfRangeException(nameof(p), p, null);
         }
      }

      public static Platform ToPlatformEnum(this string p)
      {
         switch (p)
         {
            case "standalonewindows64":
               return Platform.Windows;
            case "standaloneosx":
            case "standaloneosxuniversal":
               return Platform.Mac;
            default:
               throw new ArgumentOutOfRangeException(nameof(p), p, null);
         }
      }
   }

   [Serializable]
   public class UnityCloudTarget : DropdownElement
   {
      [Serializable]
      public class LastBuilt
      {
         public string unityVersion;
      }

      public int Id => ID;
      public string DisplayName => name;

      public int ID;
      public string name;
      public string platform;
      public string buildtargetid;
      public bool enabled;
      public LastBuilt lastBuilt;
      public Dictionary<string, object> links;
   }
}
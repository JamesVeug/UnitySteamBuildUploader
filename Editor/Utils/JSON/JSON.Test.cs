using System.Collections;
using System.Reflection;
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
#endif

namespace Wireframe
{
    internal static partial class JSON
    {
        [MenuItem("Tools/Wireframe/JSON Test")]
        public static void Test()
        {
        //     UnityCloudBuild cloudBuild = new UnityCloudBuild();
        //     cloudBuild.changeset = new List<UnityCloudBuild.ArtifactChange>();
        //     UnityCloudBuild.ArtifactChange change = new UnityCloudBuild.ArtifactChange();
        //     change.commitId = "commitId";
        //     change.message = "message";
        //     change.timestamp = "timestamp";
        //     change._id = "_id";
        //     change.author = new UnityCloudBuild.ArtifactChangeAuthor();
        //     change.author.fullName = "fullName";
        //     change.author.absoluteUrl = "absoluteUrl";
        //     change.numAffectedFiles = 0;
        //     cloudBuild.changeset.Add(change);
        //     UnityCloudBuild.ArtifactChange change2 = new UnityCloudBuild.ArtifactChange();
        //     change2.commitId = "2";
        //     change2.message = "1";
        //     change2.timestamp = "now";
        //     change2._id = "_id";
        //     change2.author = new UnityCloudBuild.ArtifactChangeAuthor();
        //     change2.author.fullName = "big name '1'";
        //     change2.author.absoluteUrl = "absoluteUrl";
        //     change2.numAffectedFiles = 0;
        //     cloudBuild.changeset.Add(change2);
        //     cloudBuild.build = 0;
        //     cloudBuild.buildtargetid = null;
        //     cloudBuild.buildTargetName = "buildTargetName";
        //     cloudBuild.buildStatus = "buildStatus";
        //     cloudBuild.platform = "platform";
        //     cloudBuild.created = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        //     cloudBuild.finished = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        //     cloudBuild.buildStartTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        //     cloudBuild.links = new Dictionary<string, object>();
        //     cloudBuild.links.Add("facebook", "https://www.facebook.com/");
        //
        //     string cloudBuildText = SerializeObject(cloudBuild);
        //     File.WriteAllText(Application.persistentDataPath + "/BuildUploader/TESTcloudBuildText.json", cloudBuildText);
        //     //
        //     UnityCloudTarget target = new UnityCloudTarget();
        //     target.buildtargetid = "buildtargetid";
        //     target.links = null;
        //     target.enabled = true;
        //     string targetText = SerializeObject(target);
        //     File.WriteAllText(Application.persistentDataPath + "/BuildUploader/TESTtargetText.json", targetText);
        //
        //
        //     WindowUploadTab.UploadTabData buildTarget = new WindowUploadTab.UploadTabData();
        //     buildTarget.Data = new List<Dictionary<string, object>>();
        //     Dictionary<string, object> data = new Dictionary<string, object>();
        //     data.Add("key", "value");
        //     data.Add("key2", "value2");
        //     data.Add("key3", "value3");
        //     buildTarget.Data.Add(data);
        //     buildTarget.Data.Add(data);
        //
        //     string buildTargetText = SerializeObject(buildTarget);
        //     File.WriteAllText(Application.persistentDataPath + "/BuildUploader/TESTbuildTargetText.json",
        //         buildTargetText);
        //
        //     Debug.Log("Uploaded to " + Application.persistentDataPath + "/BuildUploader/");
        //
        //     // Deserialize
        //
        //     UnityCloudBuild cloudBuild2 = DeserializeObject<UnityCloudBuild>(cloudBuildText);
        //     Debug.Log("cloudBuild2: " + cloudBuild2.build);
        //
        //
        //     WindowUploadTab.UploadTabData buildTarget2 =
        //         DeserializeObject<WindowUploadTab.UploadTabData>(buildTargetText);
        //     Debug.Log("buildTarget2: " + buildTarget2.Data.Count);
        //
        //     UnityCloudTarget target2 = DeserializeObject<UnityCloudTarget>(targetText);
        //     Debug.Log("target2: " + target2.buildtargetid);
        //
        //     // Test are equal
        //
        //     CompareObjects(cloudBuild, cloudBuild2, typeof(UnityCloudBuild));
        //     CompareObjects(buildTarget, buildTarget2, typeof(WindowUploadTab.UploadTabData));
        //     CompareObjects(target, target2, typeof(UnityCloudTarget));
        //
        //     Debug.Log("Done!");
        // }
        //
        // private static bool CompareObjects(object a, object b, Type type)
        // {
        //     // get all fields
        //     FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        //     
        //     foreach (FieldInfo field in fields)
        //     {
        //         object valueA = field.GetValue(a);
        //         object valueB = field.GetValue(b);
        //         if (valueA == null && valueB == null)
        //         {
        //             continue;
        //         }
        //         if (valueA == null || valueB == null)
        //         {
        //             Debug.LogError("Field " + field.Name + " is null in one of the objects of type " + field.FieldType.Name);
        //             return false;
        //         }
        //         
        //         // List
        //         if(valueA is IList listA && valueB is IList listB)
        //         {
        //             if (listA.Count != listB.Count)
        //             {
        //                 Debug.LogError("Field " + field.Name + " is not equal of type " + field.FieldType.Name);
        //                 return false;
        //             }
        //             for (int i = 0; i < listA.Count; i++)
        //             {
        //                 if (!CompareObjects(listA[i], listB[i], field.FieldType.GetGenericArguments()[0]))
        //                 {
        //                     Debug.LogError("Field " + field.Name + " is not equal of type " + field.FieldType.Name);
        //                     return false;
        //                 }
        //             }
        //             continue;
        //         }
        //         
        //         // Dictionary
        //         if(valueA is IDictionary dictA && valueB is IDictionary dictB)
        //         {
        //             if (dictA.Count != dictB.Count)
        //             {
        //                 Debug.LogError("Field " + field.Name + " is not equal of type " + field.FieldType.Name);
        //                 return false;
        //             }
        //             foreach (DictionaryEntry entry in dictA)
        //             {
        //                 if (!dictB.Contains(entry.Key))
        //                 {
        //                     Debug.LogError("Field " + field.Name + " is not equal of type " + field.FieldType.Name);
        //                     return false;
        //                 }
        //                 else if (!CompareObjects(entry.Value, dictB[entry.Key], field.FieldType.GetGenericArguments()[1]))
        //                 {
        //                     Debug.LogError("Field " + field.Name + " is not equal of type " + field.FieldType.Name);
        //                     return false;
        //                 }
        //             }
        //             continue;
        //         }
        //         
        //         // Class
        //         if (field.FieldType.IsClass && !field.FieldType.IsPrimitive)
        //         {
        //             if (!CompareObjects(valueA, valueB, field.FieldType))
        //             {
        //                 Debug.LogError("Field " + field.Name + " is not equal of type " + field.FieldType.Name);
        //                 return false;
        //             }
        //             continue;
        //         }
        //         
        //         if (!valueA.Equals(valueB))
        //         {
        //             Debug.LogError("Field " + field.Name + " is not equal of type " + field.FieldType.Name);
        //             return false;
        //         }
        //     }
        //     
        //     Debug.Log(type.Name + " Objects are equal!");
        //     return true;
        }
    }
}
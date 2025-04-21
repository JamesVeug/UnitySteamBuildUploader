// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Networking;
//
// namespace Wireframe
// {
//     /// <summary>
//     /// Plan is to show images in the UI so its prettier and exciting to upload builds
//     /// </summary>
//     public class SteamPageInfo
//     {
//         // [MenuItem("Steam/Save Header Image")]
//         // public static void SaveHeaderImage()
//         // {
//         //     int id = 1141030;
//         //     string path = Application.dataPath + "/" + id + "_headerImage.png";
//         //     GetHeaderImage(id, texture =>
//         //     {
//         //         if (texture == null)
//         //         {
//         //             return;
//         //         }
//         //         
//         //         byte[] bytes = texture.EncodeToPNG();
//         //         
//         //         Debug.Log("Saving image to " + path);
//         //         System.IO.File.WriteAllBytes(path, bytes);
//         //     });
//         // }
//         
//         public static void GetHeaderImage(int id, Action<Texture2D> callback = null)
//         {
//             FromAppID(id, info =>
//             {
//                 if (info == null)
//                 {
//                     callback?.Invoke(null);
//                     return;
//                 }
//                 
//                 string url = info.header_image.Replace("\\/", "/"); // Urls come escaped
//                 Debug.Log("Downloading image from " + url);
//                 UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
//                 request.SendWebRequest().completed += operation =>
//                 {
//                     if (request.isNetworkError || request.isHttpError)
//                     {
//                         callback?.Invoke(null);
//                         return;
//                     }
//                     
//                     callback?.Invoke(DownloadHandlerTexture.GetContent(request));
//                 };
//             });
//         }
//         
//         public static void FromAppID(int id, Action<SteamPageInfo> callback = null)
//         {
//             string url = $"https://store.steampowered.com/api/appdetails?appids={id}";
//             Debug.Log("Pinging app page: " + url);
//             
//             UnityWebRequest request = UnityWebRequest.Get(url);
//             request.SendWebRequest().completed += operation =>
//             {
//                 if (request.responseCode == 200)
//                 {
//                     Debug.Log("Pinged app page successfully");
//                     string json = request.downloadHandler.text;
//                     Dictionary<string, object> dict = JSON.DeserializeObject<Dictionary<string, object>>(json);
//                     if (dict.TryGetValue(id.ToString(), out object appDataObject))
//                     {
//                         Dictionary<string, object> appData = appDataObject as Dictionary<string, object>;
//                         if ((bool)appData["success"] == true)
//                         {
//                             Debug.Log("Found data for ID " + id);
//                             SteamPageInfo data = (SteamPageInfo)JSON.JSONDeserializer.ConvertType(appData["data"], typeof(SteamPageInfo));
//                             callback?.Invoke(data);
//                             return;
//                         }
//                         else
//                         {
//                             Debug.LogError("Pinged Steam for ID " + id + " but no data was found");
//                         }
//                     }
//                     else
//                     {
//                         Debug.LogError("Pinged Steam for ID " + id + " but no data was found");
//                     }
//                     
//                     callback?.Invoke(null);
//                 }
//                 else
//                 {
//                     Debug.LogError("Failed to ping app page. Error: " + request.responseCode + " " + request.error);
//                     callback?.Invoke(null);
//                 }
//             };
//             
//         }
//         
//         public string type;
//         public string name;
//         public int steam_appid;
//         public int required_age;
//         public bool is_free;
//         public string detailed_description;
//         public string about_the_game;
//         public string short_description;
//         public string supported_languages;
//         public string header_image;
//         public string capsule_image;
//         public string capsule_imagev5;
//         public object website;
//         public Pc_requirements pc_requirements;
//         public Mac_requirements mac_requirements;
//         public Linux_requirements linux_requirements;
//         public string[] developers;
//         public string[] publishers;
//         public object[] package_groups;
//         public Platforms platforms;
//         public Categories[] categories;
//         public Genres[] genres;
//         public Screenshots[] screenshots;
//         public Movies[] movies;
//         public Achievements achievements;
//         public Release_date release_date;
//         public Support_info support_info;
//         public string background;
//         public string background_raw;
//         public Content_descriptors content_descriptors;
//         public object ratings;
//     }
//
//     public class Pc_requirements
//     {
//         public string minimum;
//         public string recommended;
//     }
//
//     public class Mac_requirements
//     {
//         public string minimum;
//         public string recommended;
//     }
//
//     public class Linux_requirements
//     {
//         public string minimum;
//         public string recommended;
//     }
//
//     public class Platforms
//     {
//         public bool windows;
//         public bool mac;
//         public bool linux;
//     }
//
//     public class Categories
//     {
//         public int id;
//         public string description;
//     }
//
//     public class Genres
//     {
//         public string id;
//         public string description;
//     }
//
//     public class Screenshots
//     {
//         public int id;
//         public string path_thumbnail;
//         public string path_full;
//     }
//
//     public class Movies
//     {
//         public int id;
//         public string name;
//         public string thumbnail;
//         public Webm webm;
//         public Mp4 mp4;
//         public bool highlight;
//     }
//
//     public class Webm
//     {
//         public string _80;
//         public string max;
//     }
//
//     public class Mp4
//     {
//         public string _80;
//         public string max;
//     }
//
//     public class Achievements
//     {
//         public int total;
//         public Highlighted[] highlighted;
//     }
//
//     public class Highlighted
//     {
//         public string name;
//         public string path;
//     }
//
//     public class Release_date
//     {
//         public bool coming_soon;
//         public string date;
//     }
//
//     public class Support_info
//     {
//         public string url;
//         public string email;
//     }
//
//     public class Content_descriptors
//     {
//         public int[] ids;
//         public string notes;
//     }
// }
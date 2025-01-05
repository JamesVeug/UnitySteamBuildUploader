using System;
using System.Collections.Generic;

namespace Wireframe
{
    [Serializable]
    internal class DepotList : VdfMap<int, string>
    {
        public DepotList()
        {
            
        }
        
        public DepotList(DepotList depots)
        {
            foreach (var depot in depots.GetData())
            {
                Add(depot.Key, depot.Value);
            }
        }
    }

    public interface IVdfMap
    {

    }

    [Serializable]
   internal abstract class VdfMap<T, Y> : IVdfMap
    {
        internal class MapData
        {
            public T Key;
            public Y Value;

            public MapData(T key, Y value)
            {
                Key = key;
                Value = value;
            }
        }

        public int Count
        {
            get { return Data.Count; }
        }

        public List<MapData> GetData()
        {
            return Data;
        }

        private List<MapData> Data;

        public VdfMap()
        {
            Data = new List<MapData>();
        }

        public void Add(T key, Y value)
        {
            Data.Add(new MapData(key, value));
        }

        public void RemoveAt(int i)
        {
            Data.RemoveAt(i);
        }

        public T GetKey(int i)
        {
            return Data[i].Key;
        }

        public Y GetValue(int i)
        {
            return Data[i].Value;
        }

        public void Clear()
        {
            Data.Clear();
        }
    }

    [Serializable]
    internal class AppVDFFile : VDFFile
    {
        public override string FileName => "appbuild";

        // Set the app ID that this script will upload.
        public int appid;

        // The description for this build.
        // The description is only visible to you in the 'Your Builds' section of the App Admin panel.
        // This can be changed at any time after uploading a build on the 'Your Builds' page.
        public string desc;

        // The following paths can be absolute or relative to location of the script.

        // This directory will be the location for build logs, chunk cache, and intermediate output.
        // The cache stored within this causes future SteamPipe uploads to complete quicker by using diffing.
        //
        // NOTE: for best performance, use a separate disk for your build output. This splits the disk IO workload, letting your content root
        // disk handle the read requests and your output disk handle the write requests. 
        public string buildoutput = "..\\output\\";

        // The root of the content folder.
        public string contentroot = "..\\content\\";

        // Branch name to automatically set live after successful build, none if empty.
        // Note that the 'default' branch can not be set live automatically. That must be done through the App Admin panel.
        public string setlive = "";

        // Enable/Disable whether this a preview build.
        // It's highly recommended that you use preview builds while doing the initially setting up SteamPipe to
        // ensure that the depot manifest contains the correct files.
        public bool preview = false;

        // File path of the local content server if it's enabled.
        public string local = "";

        // The list of depots included in this build.
        public DepotList depots = new DepotList();

        public AppVDFFile()
        {
            
        }
        
        public AppVDFFile(AppVDFFile currentConfigApp)
        {
            appid = currentConfigApp.appid;
            desc = currentConfigApp.desc;
            buildoutput = currentConfigApp.buildoutput;
            contentroot = currentConfigApp.contentroot;
            setlive = currentConfigApp.setlive;
            preview = currentConfigApp.preview;
            local = currentConfigApp.local;
            depots = new DepotList(currentConfigApp.depots);
        }
    }
}
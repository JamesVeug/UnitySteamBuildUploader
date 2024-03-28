using System;

namespace Wireframe
{
    [Serializable]
    public class DepotFileMapping : VDFFile
    {
        public override string FileName => "FileMapping";

        // This can be a full path, or a path relative to ContentRoot
        public string LocalPath;

        // This is a path relative to the install folder of your game
        public string DepotPath;

        // If LocalPath contains wildcards, setting this means that all
        // matching files within subdirectories of LocalPath will also
        // be included.
        public bool recursive;
    }

    [Serializable]
    public class DepotVDFFile : VDFFile
    {
        public override string FileName => "DepotBuildConfig";

        // Set your assigned depot ID here
        public int DepotID;

        // include all files recursively
        public DepotFileMapping FileMapping = new DepotFileMapping();

        // but exclude all symbol files  
        // This can be a full path, or a path relative to ContentRoot
        public string FileExclusion;
    }
}
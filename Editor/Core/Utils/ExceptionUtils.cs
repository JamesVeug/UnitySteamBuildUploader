using System;

namespace Wireframe
{
    public class FilePathTooIsLongException : Exception
    {
        public FilePathTooIsLongException() : base($"It seems that the file path has exceeded {Utils.MaxFilePath} characters and is too long. " +
                                                   "Try changing the Cache Build folder in Edit->Preferences->Build Uploader to a shorter path such as C:/CacheBuilds")
        {
        }
    }
}
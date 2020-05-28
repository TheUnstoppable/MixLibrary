/*  
    MIX Package/File Parser
    Copyright (c) 2020 Unstoppable
    You can redistribute or modify this code under GNU General Public License v3.0.
    The permission given to run this code in a closed source project modified.
    But, you have to release the source code using this library must be released.
    Or, you have to add original owner's name into your project.
*/


using System;
using System.IO;

namespace MixLibrary
{
    public struct MixPackageClass
    {
        /// <summary>
        /// Collection of files stored in this package.
        /// </summary>
        public MixFileCollection Files { get; internal set; }

        /// <summary>
        /// Total files stored by this package.
        /// </summary>
        public int FileCount { get => Files.Count; }

        /// <summary>
        /// Adds the specified file into MIX.
        /// </summary>
        /// <param name="FileLoc">Location to source file.</param>
        public void AddFile(string FileLoc)
        {
            if (File.Exists(FileLoc))
            {
                MixFileClass Data = new MixFileClass
                {
                    FileName = Path.GetFileName(FileLoc),
                    Data = File.ReadAllBytes(FileLoc)
                };
                Files.Add(Data);
            }
        }

        /// <summary>
        /// Extract all files in this package to a folder.
        /// </summary>
        /// <param name="Folder">Destionation folder to extract all files.</param>
        public void Extract(string Folder)
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            foreach(MixFileClass Entry in Files)
                File.WriteAllBytes(Path.Combine(Folder, Entry.FileName), Entry.Data);
        }
    }
}

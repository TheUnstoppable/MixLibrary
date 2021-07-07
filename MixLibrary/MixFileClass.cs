/*  
    MIX Package/File Parser
    Copyright (c) 2021 Unstoppable
    You can redistribute or modify this code under GNU General Public License v3.0.
    The permission given to run this code in a closed source project modified.
    But, you have to release the source code using this library must be released.
    Or, you have to add original owner's name into your project.
*/


using System.IO;

namespace MixLibrary
{
    /// <summary>
    /// A piece of file contained in <see cref="MixPackageClass"/>
    /// </summary>
    public class MixFileClass
    {
        /// <summary>
        /// File name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Array of bytes containing the file content.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// CRC of File Name.
        /// </summary>
        public string FileCRC { get => CRC.Calculate(FileName.ToUpper(new System.Globalization.CultureInfo("en-US"))).ToString("X"); }

        /// <summary>
        /// CRC of File Data.
        /// </summary>
        public string DataCRC { get => CRC.Calculate(Data).ToString("X"); }

        /// <summary>
        /// Save this file to a folder with optionally different name.
        /// </summary>
        /// <param name="Folder">Folder to save this file.</param>
        /// <param name="File">Optional file name. Leave null to use default.</param>
        public void Save(string Folder, string File = null)
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            System.IO.File.WriteAllBytes(Path.Combine(Folder, File ?? FileName), Data);
        }

        internal uint ContentOffset; //Temp variable to read from MIX file.
        internal uint ContentLength; //Temp variable to read from MIX file.
        internal string MixCRC; //Temp variable to read from MIX file.
    }
}

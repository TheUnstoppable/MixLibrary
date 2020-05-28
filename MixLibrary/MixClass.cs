/*  
    MIX Package/File Parser
    Copyright (c) 2020 Unstoppable
    You can redistribute or modify this code under GNU General Public License v3.0.
    The permission given to run this code in a closed source project modified.
    But, you have to release the source code using this library must be released.
    Or, you have to add original owner's name into your project.
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace MixLibrary
{
    /// <summary>
    /// Actions to save and load MIX packages.
    /// </summary>
    public static class MixClass
    {
        /// <summary>
        /// Create an empty MIX package.
        /// </summary>
        /// <returns>Fresh instance of <see cref="MixPackageClass"/></returns>
        public static MixPackageClass CreateMIX()
        {
            return new MixPackageClass()
            {
                Files = new MixFileCollection()
            };
        }

        /// <summary>
        /// Load MIX package from a remote host.
        /// </summary>
        /// <param name="url">Remote URL to download package.</param>
        /// <param name="IgnoreCRCMismatches">Ignore integrity checks when parsing file datas.</param>
        /// <returns>Downloaded MIX package.</returns>
        public static MixPackageClass Load(Uri url,             bool IgnoreCRCMismatches = false) { using (WebClient wc = new WebClient()) return Load(wc.DownloadData(url), IgnoreCRCMismatches); }

        /// <summary>
        /// Load MIX package from a directory.
        /// </summary>
        /// <param name="FileLocation">Location to MIX package.</param>
        /// <param name="IgnoreCRCMismatches">Ignore integrity checks when parsing file datas.</param>
        /// <returns>MIX Package loaded from specified location.</returns>
        public static MixPackageClass Load(string FileLocation, bool IgnoreCRCMismatches = false) => Load(File.ReadAllBytes(FileLocation), IgnoreCRCMismatches);

        /// <summary>
        /// Load MIX package from a stream.
        /// </summary>
        /// <param name="Stream"><see cref="Stream"/> object containing MIX package.</param>
        /// <param name="IgnoreCRCMismatches">Ignore integrity checks when parsing file datas.</param>
        /// <returns>MIX Package loaded from stream.</returns>
        public static MixPackageClass Load(Stream Stream,       bool IgnoreCRCMismatches = false) { using (MemoryStream Str = new MemoryStream()) { Stream.CopyTo(Str); return Load(Str.ToArray(), IgnoreCRCMismatches); } }

        /// <summary>
        /// Load MIX package from array of bytes.
        /// </summary>
        /// <param name="Data">Byte array containing the bytes of a MIX package.</param>
        /// <param name="IgnoreCRCMismatches">Ignore integrity checks when parsing file datas.</param>
        /// <returns>MIX Package loaded from byte array.</returns>
        public static MixPackageClass Load(byte[] Data,         bool IgnoreCRCMismatches = false) 
        {
            MixPackageClass Package = new MixPackageClass();
            Package.Files = new MixFileCollection();

            int FileNamesOffset = 0;
            int FileDataOffset = 0;
            int FileCount = 0;

            try
            {
                using (MemoryStream Stream = new MemoryStream(Data))
                {
                    if (BitConverter.ToInt32(Stream.Read(4), 0) == 0x3158494D)
                    {
                        FileDataOffset = BitConverter.ToInt32(Stream.Read(4), 0);
                        FileNamesOffset = BitConverter.ToInt32(Stream.Read(4), 0);

                        Stream.Position = FileNamesOffset;
                        FileCount = BitConverter.ToInt32(Stream.Read(4), 0);

                        for (int i = 0; i < FileCount; i++)
                        {
                            MixFileClass File = new MixFileClass();
                            File.FileName = Stream.ReadString((int)Stream.Read(1)[0]);
                            File.FileName = File.FileName.Substring(0, File.FileName.Length - 1);
                            Package.Files.Add(File);
                        }

                        Stream.Position = FileDataOffset;
                        Stream.Read(4); //Skipping because we already got file count from File Names section.
                        for (int i = 0; i < FileCount; i++)
                        {
                            MixFileClass File = Package.Files[i];
                            File.MixCRC = BitConverter.ToUInt32(Stream.Read(4), 0).ToString("X");
                            File.ContentOffset = BitConverter.ToUInt32(Stream.Read(4), 0);
                            File.ContentLength = BitConverter.ToUInt32(Stream.Read(4), 0);

                            if (File.MixCRC != File.FileCRC && !IgnoreCRCMismatches)
                            {
                                throw new MixParserException($"MIX CRC is mismatching with API calculated CRC.\nFile Name: {File.FileName}");
                            }

                            Package.Files[i] = File;
                        }

                        foreach (MixFileClass File in Package.Files.OrderByDescending(x => x.ContentOffset))
                        {
                            Stream.Position = (int)File.ContentOffset;
                            int Index = Package.Files.FindIndex(x => x.ContentOffset == File.ContentOffset);

                            var tmpfile = Package.Files[Index];
                            tmpfile.Data = Stream.Read((int)File.ContentLength);
                            Package.Files[Index] = tmpfile;
                        }
                    }
                    else
                    {
                        throw new MixFormatException("Invalid MIX file.");
                    }
                }
            }
            catch (Exception ex)
            {
                if(!(ex is MixFormatException || ex is MixParserException))
                {
                    throw new MixParserException("Failed to parse MIX file.", ex);
                }
                else
                {
                    throw;
                }
            }

            return Package;
        }

        /// <summary>
        /// Save the specified MIX package to the specified file.
        /// </summary>
        /// <param name="Package">Instance of <see cref="MixPackageClass"/> to write into a file.</param>
        /// <param name="FileLocation">Location to destination file.</param>
        public static void            Save(MixPackageClass Package, string FileLocation) => File.WriteAllBytes(FileLocation, Save(Package));

        /// <summary>
        /// Save the specified MIX package as a array of bytes
        /// </summary>
        /// <param name="Package">Instance of <see cref="MixPackageClass"/> to write into a byte array.</param>
        /// <returns>MIX Package data as array of bytes.</returns>
        public static byte[]          Save(MixPackageClass Package                     ) { Save(Package, out Stream str); using (str) using (MemoryStream memstr = new MemoryStream()) {str.CopyTo(memstr); return memstr.ToArray();}}

        /// <summary>
        /// Save the specified MIX package to a Stream.
        /// </summary>
        /// <param name="Package">Instance of <see cref="MixPackageClass"/> to write into the stream.</param>
        /// <param name="Stream">Output stream containing MIX data.</param>
        public static void Save(MixPackageClass Package, out Stream Stream)
        {
            MemoryStream MS = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(MS);
            MS.WriteString("MIX1");

            Package.Files.Sort(
                (x, y) => CRC.Calculate(x.FileName.ToUpper())
                           .CompareTo(CRC.Calculate(y.FileName.ToUpper()))
            );

            uint FileDataOffset = 0;
            uint FileNameOffset = 0;
            MS.Position = 16;
            for (int i = 0; i < Package.Files.Count; i++)
            {
                var File = Package.Files[i];

                File.ContentOffset = (uint)MS.Position;
                File.ContentLength = (uint)File.Data.Length;
                MS.Write(File.Data);

                Package.Files[i] = File;
                MS.Position += -MS.Position & 7;
            }

            FileDataOffset = (uint)MS.Position;

            MS.Write(BitConverter.GetBytes(Package.FileCount));
            for (int i = 0; i < Package.Files.Count; i++)
            {
                var File = Package.Files[i];
                bw.Write(CRC.Calculate(File.FileName.ToUpper(new System.Globalization.CultureInfo("en-US"))));
                bw.Write(File.ContentOffset);
                bw.Write(File.ContentLength);
            }

            FileNameOffset = (uint)MS.Position;
            bw.Write(Package.FileCount);
            for (int i = 0; i < Package.Files.Count; i++)
            {
                var File = Package.Files[i];
                bw.Write((byte)(File.FileName.Length + 1));
                MS.WriteString(File.FileName + "\0");
                Package.Files[i] = File;
            }

            MS.Position = 4;
            bw.Write(FileDataOffset);
            bw.Write(FileNameOffset);

            MS.Position = 0;
            Stream = MS;
        }
    }
}

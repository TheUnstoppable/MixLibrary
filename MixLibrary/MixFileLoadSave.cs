/*  
    MIX Package/File Parser
    Copyright (c) 2021 Unstoppable
    You can redistribute or modify this code under GNU General Public License v3.0.
    The permission given to run this code in a closed source project modified.
    But, you have to release the source code using this library must be released.
    Or, you have to add original owner's name into your project.
*/


using System;
using System.IO;
using System.Linq;
using System.Net;

namespace MixLibrary
{
    public partial class MixPackageClass
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
        public static MixPackageClass Load(Uri url, bool IgnoreCRCMismatches = false)
        {
            using (WebClient wc = new WebClient()) return Load(wc.DownloadData(url), IgnoreCRCMismatches);
        }

        /// <summary>
        /// Load MIX package from a directory.
        /// </summary>
        /// <param name="FileLocation">Location to MIX package.</param>
        /// <param name="IgnoreCRCMismatches">Ignore integrity checks when parsing file datas.</param>
        /// <returns>MIX Package loaded from specified location.</returns>
        public static MixPackageClass Load(string FileLocation, bool IgnoreCRCMismatches = false) =>
            Load(File.ReadAllBytes(FileLocation), IgnoreCRCMismatches);

        /// <summary>
        /// Load MIX package from a stream.
        /// </summary>
        /// <param name="Stream"><see cref="Stream"/> object containing MIX package.</param>
        /// <param name="IgnoreCRCMismatches">Ignore integrity checks when parsing file datas.</param>
        /// <returns>MIX Package loaded from stream.</returns>
        public static MixPackageClass Load(Stream Stream, bool IgnoreCRCMismatches = false)
        {
            using (MemoryStream Str = new MemoryStream())
            {
                Stream.CopyTo(Str);
                return Load(Str.ToArray(), IgnoreCRCMismatches);
            }
        }

        /// <summary>
        /// Load MIX package from array of bytes.
        /// </summary>
        /// <param name="Data">Byte array containing the bytes of a MIX package.</param>
        /// <param name="IgnoreCRCMismatches">Ignore integrity checks when parsing file datas.</param>
        /// <returns>MIX Package loaded from byte array.</returns>
        public static MixPackageClass Load(byte[] Data, bool IgnoreCRCMismatches = false)
        {
            MixPackageClass Package = new MixPackageClass
            {
                Files = new MixFileCollection()
            };

            try
            {
                using (MemoryStream Stream = new MemoryStream(Data))
                using (BinaryReader Reader = new BinaryReader(Stream))
                {
                    if (Reader.ReadInt32() == 0x3158494D)
                    {
                        var FileDataOffset = Reader.ReadInt32();
                        var FileNamesOffset = Reader.ReadInt32();

                        Stream.Position = FileNamesOffset;
                        var FileCount = Reader.ReadInt32();

                        for (int i = 0; i < FileCount; i++)
                        {
                            MixFileClass File = new MixFileClass
                            {
                                FileName = Stream.ReadString(Reader.ReadByte()).TrimEnd('\0')
                            };
                            Package.Files.Add(File);
                        }

                        Stream.Position = FileDataOffset;
                        Stream.Read(4); //Skipping because we already got file count from File Names section.
                        for (int i = 0; i < FileCount; i++)
                        {
                            MixFileClass File = Package.Files[i];
                            File.MixCRC = Reader.ReadUInt32().ToString("X");
                            File.ContentOffset = Reader.ReadUInt32();
                            File.ContentLength = Reader.ReadUInt32();

                            if (File.MixCRC != File.FileCRC && !IgnoreCRCMismatches)
                            {
                                throw new MixParserException(
                                    $"MIX CRC is mismatching with API calculated CRC.\nFile Name: {File.FileName}");
                            }

                            var pos = Stream.Position;
                            Stream.Position = (int)File.ContentOffset;
                            File.Data = Stream.Read((int)File.ContentLength);
                            Stream.Position = pos;

                            Package.Files[i] = File;
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
                if (!(ex is MixFormatException || ex is MixParserException))
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
        /// <param name="FileLocation">Location to destination file.</param>
        public void Save(string FileLocation) =>
            File.WriteAllBytes(FileLocation, Save());

        /// <summary>
        /// Save the specified MIX package as a array of bytes
        /// </summary>
        /// <returns>MIX Package data as array of bytes.</returns>
        public byte[] Save()
        {
            Save(out Stream str);
            using (str)
            using (MemoryStream memstr = new MemoryStream())
            {
                str.CopyTo(memstr);
                return memstr.ToArray();
            }
        }

        /// <summary>
        /// Save the specified MIX package to a Stream.
        /// </summary>
        /// <param name="Stream">Output stream containing MIX data.</param>
        public void Save(out Stream Stream)
        {
            MemoryStream MS = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(MS);
            MS.WriteString("MIX1");

            Files.Sort(
                (x, y) => CRC.Calculate(x.FileName.ToUpper())
                    .CompareTo(CRC.Calculate(y.FileName.ToUpper()))
            );

            MS.Position = 16;
            for (int i = 0; i < Files.Count; i++)
            {
                var File = Files[i];

                File.ContentOffset = (uint) MS.Position;
                File.ContentLength = (uint) File.Data.Length;
                MS.Write(File.Data);

                Files[i] = File;
                MS.Position += -MS.Position & 7;
            }

            var FileDataOffset = (uint) MS.Position;
            MS.Write(BitConverter.GetBytes(FileCount));
            foreach (var File in Files)
            {
                bw.Write(CRC.Calculate(File.FileName.ToUpper(new System.Globalization.CultureInfo("en-US"))));
                bw.Write(File.ContentOffset);
                bw.Write(File.ContentLength);
            }

            var FileNameOffset = (uint) MS.Position;
            bw.Write(FileCount);
            for (int i = 0; i < Files.Count; i++)
            {
                var File = Files[i];
                bw.Write((byte) (File.FileName.Length + 1));
                MS.WriteString(File.FileName + "\0");
                Files[i] = File;
            }

            MS.Position = 4;
            bw.Write(FileDataOffset);
            bw.Write(FileNameOffset);

            MS.Position = 0;
            Stream = MS;
        }
    }
}

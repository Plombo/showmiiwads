/* This file is part of ShowMiiWads (Original code by Ben Wilson, Thanks!)
 * Copyright (C) 2009 Ben Wilson
 * 
 * ShowMiiWads is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ShowMiiWads is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class NandExtract
{
    /*
     * Class for fst entries.
     */
    public class fst_t
    {
        public byte[] filename = new byte[0x0B];
        public byte mode;
        public byte attr;
        public UInt16 sub;
        public UInt16 sib;
        public UInt32 size;
        public UInt32 uid;
        public UInt16 gid;
        public UInt32 x3;
    }

    /*
     * Globals.
     */
    public static byte[] key = new byte[16];
    public static byte[] iv = new byte[16];
    public static Int32 loc_super;
    public static Int32 loc_fat;
    public static Int32 loc_fst;
    public static string extractPath;
    public static string nandFile;
    public static BinaryReader rom;
    public static int type = -1;

    /*
     * Extraction functions.
     */
    public static void extractNAND(string nandfile, string destionationpath)
    {
        extractPath = destionationpath;
        nandFile = nandfile;

        rom = new BinaryReader(File.Open(nandfile,
                                                FileMode.Open,
                                                FileAccess.Read,
                                                FileShare.Read),
                                            Encoding.ASCII);

        if (initNand() == true)
        {

            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            extractFST(0, "");
        }
        else
        {
            throw new Exception("This is not a valid BootMii NAND Dump!");
        }

        rom.Close();
    }

    private static bool initNand()
    {
        type = getDumpType(rom.BaseStream.Length);

        if (getKey(type))
        {
            try
            {
                loc_super = findSuperblock();
            }
            catch
            {
                //statusText("Invalid or non-ECC NAND dump");
                //msg_Error("Can't find superblock.\nAre you sure this is a Full (with ECC) or BootMii NAND dump?");
                throw new Exception("This is not a valid BootMii NAND Dump!");
            }

            Int32[] n_fatlen = { 0x010000, 0x010800, 0x010800 };
            loc_fat = loc_super;
            loc_fst = loc_fat + 0x0C + n_fatlen[type];
            return true;

        }
        return false;
    }

    private static int getDumpType(Int64 FileSize)
    {
        Int64[] sizes = { 536870912,    // type 0 | 536870912 == no ecc
                              553648128,    // type 1 | 553648128 == ecc
                              553649152 };  // type 2 | 553649152 == old bootmii
        for (int i = 0; i < sizes.Length; i++)
            if (sizes[i] == FileSize)
                return i;
        throw new Exception("This is not a valid BootMii NAND Dump!");
    }

    private static bool getKey(int type)
    {
        switch (type)
        {
            case 0:
                key = readKeyfile(Directory.GetCurrentDirectory() + "\\keys.bin");
                if (key != null)
                    return true;
                break;

            case 1:
                key = readKeyfile(Directory.GetCurrentDirectory() + "\\keys.bin");
                if (key != null)
                    return true;
                break;

            case 2:
                rom.BaseStream.Seek(0x21000158, SeekOrigin.Begin);
                if (rom.Read(key, 0, 16) > 0)
                    return true;
                break;
        }

        throw new Exception("An error occured. The key can't be found...");
    }

    public static byte[] readKeyfile(string path)
    {
        byte[] retval = new byte[16];

        if (File.Exists(path))
        {
            try
            {
                BinaryReader br = new BinaryReader(File.Open(path,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read),
                            Encoding.ASCII);
                br.BaseStream.Seek(0x158, SeekOrigin.Begin);
                br.Read(retval, 0, 16);
                br.Close();
                return retval;
            }
            catch
            {
                //msg_Error(string.Format("Can't open keys.bin:\n{0}\n" +
                //                        "Try closing any program(s) that may be accessing it.",
                //                        path));
                throw new Exception("Can't open keys.bin!");
            }
        }
        else
        {
            //msg_Error(string.Format("You tried to open a file that doesn't exist:\n{0}", path));
            throw new Exception(string.Format("File doesn't exist: {0}", path));
        }
    }

    private static Int32 findSuperblock()
    {
        Int32 loc, current, last = 0;
        Int32[] n_start = { 0x1FC00000, 0x20BE0000, 0x20BE0000 },
                n_end = { 0x20000000, 0x21000000, 0x21000000 },
                n_len = { 0x40000, 0x42000, 0x42000 };

        rom.BaseStream.Seek(n_start[type] + 4, SeekOrigin.Begin);

        for (loc = n_start[type]; loc < n_end[type]; loc += n_len[type])
        {
            current = (int)bswap(rom.ReadUInt32());

            if (current > last)
                last = current;
            else
                return loc - n_len[type];

            rom.BaseStream.Seek(n_len[type] - 4, SeekOrigin.Current);
        }

        return -1;
    }

    private static void extractFST(UInt16 entry, string parent)
    {
        fst_t fst = getFST(entry);

        if (fst.sib != 0xffff)
            extractFST(fst.sib, parent);

        switch (fst.mode)
        {
            case 0:
                extractDir(fst, entry, parent);
                break;
            case 1:
                extractFile(fst, entry, parent);
                break;
            default:
                break;
        }
    }

    private static void extractDir(fst_t fst, UInt16 entry, string parent)
    {
        string filename = ASCIIEncoding.ASCII.GetString(fst.filename).Replace("\0", string.Empty);

        if (filename != "/")
        {
            if (parent != "/" && parent != "")
                filename = parent + "\\" + filename;

            Directory.CreateDirectory(extractPath + "\\" + filename);
        }

        if (fst.sub != 0xffff)
            extractFST(fst.sub, filename);
    }

    private static void extractFile(fst_t fst, UInt16 entry, string parent)
    {
        UInt16 fat;
        int cluster_span = (int)(fst.size / 0x4000) + 1;
        byte[] cluster = new byte[0x4000],
               data = new byte[cluster_span * 0x4000];

        string filename = "\\" + parent + "\\" +
                        ASCIIEncoding.ASCII.GetString(fst.filename).
                        Replace("\0", string.Empty).
                        Replace(":", "-");

        try
        {
            BinaryWriter bw = new BinaryWriter(File.Open(extractPath + filename,
                                                            FileMode.Create,
                                                            FileAccess.Write,
                                                            FileShare.Read),
                                                        Encoding.ASCII);
            fat = fst.sub;
            for (int i = 0; fat < 0xFFF0; i++)
            {
                Buffer.BlockCopy(getCluster(fat), 0, data, i * 0x4000, 0x4000);
                fat = getFAT(fat);
            }

            bw.Write(data, 0, (int)fst.size);
            bw.Close();
        }
        catch
        {
            //msg_Error(string.Format("Can't open file for writing:\n{0}",
            //                            extractPath + filename));
            throw new Exception(string.Format("Can't open file: {0}", extractPath + filename));
        }
    }

    private static fst_t getFST(UInt16 entry)
    {
        fst_t fst = new fst_t();

        // compensate for 64 bytes of ecc data every 64 fst entries
        Int32[] n_fst = { 0, 2, 2 };
        int loc_entry = (((entry / 0x40) * n_fst[type]) + entry) * 0x20;

        rom.BaseStream.Seek(loc_fst + loc_entry, SeekOrigin.Begin);

        fst.filename = rom.ReadBytes(0x0C);
        fst.mode = rom.ReadByte();
        fst.attr = rom.ReadByte();
        fst.sub = bswap(rom.ReadUInt16());
        fst.sib = bswap(rom.ReadUInt16());
        fst.size = bswap(rom.ReadUInt32());
        fst.uid = bswap(rom.ReadUInt32());
        fst.gid = bswap(rom.ReadUInt16());
        fst.x3 = bswap(rom.ReadUInt32());

        fst.mode &= 1;

        return fst;
    }

    private static byte[] getCluster(UInt16 cluster_entry)
    {
        Int32[] n_clusterlen = { 0x4000, 0x4200, 0x4200 };
        Int32[] n_pagelen = { 0x800, 0x840, 0x840 };

        byte[] cluster = new byte[0x4000];
        byte[] page = new byte[n_pagelen[type]];

        rom.BaseStream.Seek(cluster_entry * n_clusterlen[type], SeekOrigin.Begin);

        for (int i = 0; i < 8; i++)
        {
            page = rom.ReadBytes(n_pagelen[type]);
            Buffer.BlockCopy(page, 0, cluster, i * 0x800, 0x800);
        }

        return aesDecrypt(key, iv, cluster);
    }

    private static UInt16 getFAT(UInt16 fat_entry)
    {
        /*
         * compensate for "off-16" storage at beginning of superblock
         * 53 46 46 53   XX XX XX XX   00 00 00 00
         * S  F  F  S     "version"     padding?
         *   1     2       3     4       5     6
         */
        fat_entry += 6;

        // location in fat of cluster chain
        Int32[] n_fat = { 0, 0x20, 0x20 };
        int loc = loc_fat + ((((fat_entry / 0x400) * n_fat[type]) + fat_entry) * 2);

        rom.BaseStream.Seek(loc, SeekOrigin.Begin);
        return bswap(rom.ReadUInt16());
    }

    private static byte[] aesDecrypt(byte[] key, byte[] iv, byte[] enc_data)
    {
        // zero out any remaining iv bytes
        byte[] iv16 = new byte[16];
        Buffer.BlockCopy(iv, 0, iv16, 0, iv.Length);

        RijndaelManaged aes = new RijndaelManaged();
        aes.Padding = PaddingMode.None;
        aes.Mode = CipherMode.CBC;

        ICryptoTransform decryptor = aes.CreateDecryptor(key, iv16);
        MemoryStream memoryStream = new MemoryStream(enc_data);
        CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                  decryptor,
                                                  CryptoStreamMode.Read);

        byte[] dec_data = new byte[enc_data.Length];

        int decryptedByteCount = cryptoStream.Read(dec_data, 0,
                                               dec_data.Length);

        memoryStream.Close();
        cryptoStream.Close();
        return dec_data;
    }

    public static UInt16 bswap(UInt16 value)
    {
        return (UInt16)((0x00FF & (value >> 8))
                         | (0xFF00 & (value << 8)));
    }

    public static UInt32 bswap(UInt32 value)
    {
        UInt32 swapped = ((0x000000FF) & (value >> 24)
                         | (0x0000FF00) & (value >> 8)
                         | (0x00FF0000) & (value << 8)
                         | (0xFF000000) & (value << 24));
        return swapped;
    }
}

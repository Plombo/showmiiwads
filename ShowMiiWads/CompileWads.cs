/* Copyright (C) 2010 Bryan Cain (Plombo)
 * 
 * CompileWads is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * CompileWads is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Data;
using System.IO;
using Wii;

namespace CompileWads
{
	public class CompileWads
	{
        public static void LoadNand(string NandPath, string WadDirectory)
        {
            if (Directory.Exists(NandPath + "/ticket"))
			{
                string[] tiks = Directory.GetFiles(NandPath + "/ticket/", "*.tik", SearchOption.AllDirectories);

                foreach (string tik in tiks)
                {
                    bool exists = false;

                    if (exists == false)
                    {
                        string[] Infos = new string[11];

                        try
                        {
                            string path1 = tik.Remove(tik.LastIndexOf('/'));
                            path1 = path1.Remove(0, path1.LastIndexOf('/') + 1);
                            string path2 = tik.Remove(0, tik.LastIndexOf('/') + 1);
                            path2 = path2.Remove(path2.LastIndexOf('.'));

                            byte[] tikarray = Wii.Tools.LoadFileToByteArray(tik);
                            if (File.Exists(NandPath + "/title/" + path1 + "/" + path2 + "/content/title.tmd"))
                            {
                                byte[] tmd = Wii.Tools.LoadFileToByteArray(NandPath + "/title/" + path1 + "/" + path2 + "/content/title.tmd");
                                string[,] continfo = Wii.WadInfo.GetContentInfo(tmd);
                                string cid = "00000000";

                                for (int j = 0; j < continfo.GetLength(0); j++)
                                {
                                    if (continfo[j, 1] == "00000000")
                                        cid = continfo[j, 0];
                                }

                                byte[] nullapp = Wii.Tools.LoadFileToByteArray(NandPath + "/title/" + path1 + "/" + path2 + "/content/" + cid + ".app");

                                Infos[0] = tik.Remove(0, tik.LastIndexOf('/') + 1);
                                Infos[1] = Wii.WadInfo.GetTitleID(tikarray, 0);
                                //Infos[2] = Wii.WadInfo.GetNandBlocks(tmd);
                                //Infos[3] = Wii.WadInfo.GetNandSize(tmd, true);
                                Infos[4] = Wii.WadInfo.GetIosFlag(tmd);
                                Infos[5] = Wii.WadInfo.GetRegionFlag(tmd);
                                //Infos[6] = Wii.WadInfo.GetContentNum(tmd).ToString();
                                Infos[7] = Wii.WadInfo.GetNandPath(tikarray, 0);
                                Infos[8] = Wii.WadInfo.GetChannelType(tikarray, 0);
                                Infos[9] = Wii.WadInfo.GetTitleVersion(tmd).ToString();
								
								string language = "English";

                                switch (language)
                                {
                                    case "Dutch":
                                        Infos[10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[6];
                                        break;
                                    case "Italian":
                                        Infos[10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[5];
                                        break;
                                    case "Spanish":
                                        Infos[10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[4];
                                        break;
                                    case "French":
                                        Infos[10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[3];
                                        break;
                                    case "German":
                                        Infos[10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[2];
                                        break;
                                    case "Japanese":
                                        Infos[10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[0];
                                        break;
                                    default:
                                        Infos[10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[1];
                                        break;
                                }

                                string[] titlefiles = Directory.GetFiles(NandPath + "/title/" + path1 + "/" + path2, "*", SearchOption.AllDirectories);
                                Infos[6] = (titlefiles.Length - 1).ToString();
                                int nandsize = 0;

                                foreach (string titlefile in titlefiles)
                                {
                                    FileInfo fi = new FileInfo(titlefile);
                                    nandsize += (int)fi.Length;
                                }

                                FileInfo fitik = new FileInfo(tik);
                                nandsize += (int)fitik.Length;

                                double blocks = (double)((Convert.ToDouble(nandsize) / 1024) / 128);
                                Infos[2] = Math.Ceiling(blocks).ToString();

                                string size = Convert.ToString(Math.Round(Convert.ToDouble(nandsize) * 0.0009765625 * 0.0009765625, 2));
                                if (size.Length > 4) { size = size.Remove(4); }
                                Infos[3] = size.Replace(",", ".") + " MB";
								
                                //lvNand.Items.Add(new ListViewItem(Infos));
								PackVCWads(NandPath, WadDirectory, Infos);
                            }
                        }
                        catch { }
                    }
                }
            }

            //pbProgress.Value = 100;
            //SaveListNand();
            //lbFileCount.Text = lvNand.Items.Count.ToString();
        }
		
		private static void PackVCWads(string NandPath, string dir, string[] Infos)
        {
            string filename = "";

            switch (Infos[8])
            {
                case "System: IOS":
                    filename = Infos[1] + "-v" + Infos[9];
                    break;
                case "System: Boot2":
                    filename = "Boot2-v" + Infos[9];
                    break;
                case "System: MIOS":
                    filename = "MIOS-v" + Infos[9];
                    break;
                case "System: BC":
                    filename = "BC-v" + Infos[9];
                    break;
                case "Hidden Channel":
                    filename = "Hidden Channel - " + Infos[1];
                    break;
                default:
                    if (!string.IsNullOrEmpty(Infos[10]))
                        filename = Infos[10] + " - " + Infos[1];
                    else
                        filename = Infos[8] + " - " + Infos[1];
                    break;
            }
			
			// part of the extension - denotes the platform
			string platext = "";
			
			switch(Infos[8])
			{
				case "NES":
					platext = ".nes";
					break;
				case "SNES":
					platext = ".snes";
					break;
				case "Sega Genesis":
					platext = ".genesis";
					break;
				case "Turbografx":
					platext = ".turbografx";
					break;
				case "Nintendo 64":
					platext = ".n64";
					break;
				default:
					return;
			}
			
			// print the info
			Console.WriteLine(Infos[8] + ": " + Infos[10]);

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                filename = filename.Replace(invalidChar.ToString(), string.Empty);

            string path = Infos[7];
            string result = dir + "/" + filename + platext + ".wad";

            try { Wii.WadPack.PackWadFromNand(NandPath, path, result); }
            catch (Exception ex) { Console.WriteLine(filename + "\n" + ex.Message); }
        }
		
		public static int Main(string[] args)
		{
			string appVersion = "v0.1";
			
			Console.WriteLine("CompileWads " + appVersion);
			Console.WriteLine("Copyright (c) 2010 Bryan Cain (Plombo)");
			Console.WriteLine("Code from ShowMiiWads is copyright (c) 2009 Leathl");
			Console.WriteLine();
			
			if(args.Length != 2)
			{
				Console.WriteLine("Usage: compilewads nandpath destdir");
				Console.WriteLine("\tnandpath: path to an extracted NAND dump");
				Console.WriteLine("\tdestdir: location to save compiled Virtual Console WADs");
				return 1;
			}
			
			LoadNand(args[0], args[1]);
			
			Console.WriteLine();
			Console.WriteLine("Done!");
			return 0;
		}
	}
}


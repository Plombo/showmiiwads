/* This file is part of ShowMiiWads
 * Copyright (C) 2009 Leathl
 * 
 * ShowMiiWads is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * ShowMiiWads is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using InputBoxes;
using ChannelNameBox;
using System.Net;

namespace ShowMiiWads
{
    public partial class Main : Form
    {
        //Define global variables
        public const string version = "1.0b";
        private string language = "English";
        private string langfile = "";
        private string oldlang = "";
        private string[] Messages;
        private int langcount;
        private Sorter lvSorter = new Sorter();
        private string autosize = "true";
        private delegate void DelegateAddWads(string path);
        private delegate void DelegateAddWadsSub(string path);
        private DelegateAddWads AddWadsDel;
        private DelegateAddWadsSub AddWadsSubDel;
        private string wwidth = "930";
        private string wheight = "396";
        private string locx;
        private string locy;
        private string wstate = "Normal";
        public static string accepted = "false";
        private string savefolders = "true";
        private string copyfile = "";
        private string copyaction = "";
        private string nandpath = "";
        private string showpath = "true";
        private string addsub = "false";
        private string backup = "false";
        private string ckey = Application.StartupPath + "\\common-key.bin";
        private string key = Application.StartupPath + "\\key.bin";
        private string[] mru = new string[5] {"", "", "", "", ""};
        public static string ImageTempPath = Path.GetTempPath() + "\\smwtemp0000images\\";

        public Main()
        {
            InitializeComponent();
            //Display WaitCursor
            Cursor.Current = Cursors.WaitCursor;
            //Set Caption
            this.Text = "ShowMiiWads " + version + " by Leathl";
            //Define Sorter for lvWads
            lvWads.ListViewItemSorter = lvSorter;
            lvNand.ListViewItemSorter = lvSorter;
            //Define Delegate for Drag & Drop action
            AddWadsDel = new DelegateAddWads(this.AddWads);
            AddWadsSubDel = new DelegateAddWadsSub(this.AddWadsSub);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //Count Lines in Language File to get the size for the Messages Array
            CountLang();
            //Define Messages Array
            Messages = new string[langcount];
            //Load Settings from XML
            LoadSettings();
            //Define EventHandler for Wad Processes
            Wii.Tools.ProgressChanged += new EventHandler<Wii.ProgressChangedEventArgs>(WadProgressChanged);
            //Check for Updates
            UpdateCheck(true);
            //Cursor back to default
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Sets the Value of the ProgressBar to the current state.
        /// Set the Tag of the ProgressBar to "NoProgress" to disable this
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WadProgressChanged(object sender, Wii.ProgressChangedEventArgs e)
        {
            if ((string)pbProgress.Tag != "NoProgress")
            pbProgress.Value = e.PercentProgress;
        }

        /// <summary>
        /// Counts lines in English.txt
        /// </summary>
        private void CountLang()
        {
            Assembly _assembly;
            _assembly = Assembly.GetExecutingAssembly();
            StreamReader countstream = new StreamReader(_assembly.GetManifestResourceStream("ShowMiiWads.English.txt"));
            langcount = 0;
            string thisline = "";

            while ((thisline = countstream.ReadLine()) != null)
            {
                if (!thisline.StartsWith("//"))
                    langcount++;
            }
        }

        /// <summary>
        /// Adds the titles from Nand Backup to lvNand
        /// </summary>
        private void AddNand()
        {
            lvNand.Items.Clear();

            if (Directory.Exists(nandpath) &&
                Directory.Exists(nandpath + "\\ticket") &&
                Directory.Exists(nandpath + "\\title"))
            {
                string[] tickets = Directory.GetFiles(nandpath + "\\ticket", "*.tik", SearchOption.AllDirectories);
                
                if (tickets.Length > 0)
                {
                    string[,] Infos = new string[tickets.Length, 11];
                    Cursor.Current = Cursors.WaitCursor;
                    
                    for (int i = 0; i < tickets.Length; i++)
                    {
                        try
                        {
                            pbProgress.Value = (i + 1) * 100 / tickets.Length;
                            string path1 = tickets[i].Remove(tickets[i].LastIndexOf('\\'));
                            path1 = path1.Remove(0, path1.LastIndexOf('\\') + 1);
                            string path2 = tickets[i].Remove(0, tickets[i].LastIndexOf('\\') + 1);
                            path2 = path2.Remove(path2.LastIndexOf('.'));

                            byte[] tik = Wii.Tools.LoadFileToByteArray(tickets[i]);
                            if (File.Exists(nandpath + "\\title\\" + path1 + "\\" + path2 + "\\content\\title.tmd"))
                            {
                                byte[] tmd = Wii.Tools.LoadFileToByteArray(nandpath + "\\title\\" + path1 + "\\" + path2 + "\\content\\title.tmd");
                                string[,] continfo = Wii.WadInfo.GetContentInfo(tmd);
                                string cid = "00000000";

                                for (int j = 0; j < Wii.WadInfo.GetContentNum(tmd); j++)
                                {
                                    if (continfo[j, 1] == "00000000")
                                        cid = continfo[j, 0];
                                }

                                byte[] nullapp = Wii.Tools.LoadFileToByteArray(nandpath + "\\title\\" + path1 + "\\" + path2 + "\\content\\" + cid + ".app");

                                Infos[i, 0] = tickets[i].Remove(0, tickets[i].LastIndexOf('\\') + 1);
                                Infos[i, 1] = Wii.WadInfo.GetTitleID(tik, 0);
                                //Infos[i, 2] = Wii.WadInfo.GetNandBlocks(tmd);
                                //Infos[i, 3] = Wii.WadInfo.GetNandSize(tmd, true);
                                Infos[i, 4] = Wii.WadInfo.GetIosFlag(tmd);
                                Infos[i, 5] = Wii.WadInfo.GetRegionFlag(tmd);
                                //Infos[i, 6] = Wii.WadInfo.GetContentNum(tmd).ToString();
                                Infos[i, 7] = Wii.WadInfo.GetNandPath(tik, 0);
                                Infos[i, 8] = Wii.WadInfo.GetChannelType(tik, 0);
                                Infos[i, 9] = Wii.WadInfo.GetTitleVersion(tmd).ToString();

                                switch (language)
                                {
                                    case "Dutch":
                                        Infos[i, 10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[6];
                                        break;
                                    case "Italian":
                                        Infos[i, 10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[5];
                                        break;
                                    case "Spanish":
                                        Infos[i, 10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[4];
                                        break;
                                    case "French":
                                        Infos[i, 10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[3];
                                        break;
                                    case "German":
                                        Infos[i, 10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[2];
                                        break;
                                    default:
                                        Infos[i, 10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[1];
                                        break;
                                    case "Japanese":
                                        Infos[i, 10] = Wii.WadInfo.GetChannelTitlesFromApp(nullapp)[0];
                                        break;
                                }

                                string[] titlefiles = Directory.GetFiles(nandpath + "\\title\\" + path1 + "\\" + path2, "*", SearchOption.AllDirectories);
                                Infos[i, 6] = (titlefiles.Length - 1).ToString();
                                int nandsize = 0;

                                foreach (string titlefile in titlefiles)
                                {
                                    FileInfo fi = new FileInfo(titlefile);
                                    nandsize += (int)fi.Length;
                                }

                                FileInfo fitik = new FileInfo(tickets[i]);
                                nandsize += (int)fitik.Length;

                                double blocks = (double)((Convert.ToDouble(nandsize) / 1024) / 128);
                                Infos[i, 2] = Math.Ceiling(blocks).ToString();

                                string size = Convert.ToString(Math.Round(Convert.ToDouble(nandsize) * 0.0009765625 * 0.0009765625, 2));
                                if (size.Length > 4) { size = size.Remove(4); }
                                Infos[i, 3] = size + " MB";
                            }
                        }
                        catch //(Exception ex)
                        {
                            //MessageBox.Show(ex.Message, Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    for (int x = 0; x < Infos.GetLength(0); x++)
                    {
                        if (!string.IsNullOrEmpty(Infos[x, 0]))
                        {
                            lvNand.Items.Add(new ListViewItem(new string[] { Infos[x, 0], Infos[x, 1], Infos[x, 2], Infos[x, 3], Infos[x, 4], Infos[x, 5], Infos[x, 6], Infos[x, 7], Infos[x, 8], Infos[x, 9], Infos[x, 10] }));
                        }
                    }
                }
            }

            lbFileCount.Text = lvNand.Items.Count.ToString();
            Cursor.Current = Cursors.Default;
            pbProgress.Value = 100;
        }

        /// <summary>
        /// Adds a folder and all subfolders including one ore more wad files to lvWads
        /// </summary>
        /// <param name="path"></param>
        private void AddWadsSub(string path)
        {
            string[] wadfiles = Directory.GetFiles(path, "*.wad", SearchOption.AllDirectories);
            string[] added = new string[wadfiles.Length];

            for (int i = 0; i < wadfiles.Length; i++)
            {
                bool exists = false;

                for (int j = 0; j < added.Length; j++)
                {
                    if (wadfiles[i].Remove(wadfiles[i].LastIndexOf('\\')) == added[j]) exists = true;
                }

                if (exists == false)
                {
                    AddWads(wadfiles[i].Remove(wadfiles[i].LastIndexOf('\\')));
                    added[i] = wadfiles[i].Remove(wadfiles[i].LastIndexOf('\\'));
                }
            }
        }

        /// <summary>
        /// Adds a folder to lvWads
        /// </summary>
        /// <param name="path">Path to Add to Listview</param>
        private void AddWads(string path)
        {
            bool thisexists = false;

            for (int f = 0; f < lvWads.Groups.Count; f++)
            {
                if (lvWads.Groups[f].Tag.ToString() == path)
                {
                    thisexists = true;
                }
            }

            string[] wadfiles = Directory.GetFiles(path, "*.wad", SearchOption.TopDirectoryOnly);

            if (wadfiles.Length > 0 && thisexists == false)
            {

                Cursor.Current = Cursors.WaitCursor;

                string[,] Infos = new string[wadfiles.Length, 11];
                int i = 0;

                if (showpath == "true")
                {
                    lvWads.Groups.Add(new ListViewGroup(path, path));
                }
                else
                {
                    lvWads.Groups.Add(new ListViewGroup(path, path.Remove(0, path.LastIndexOf('\\') + 1)));
                }
                lvWads.Groups[lvWads.Groups.Count - 1].Tag = path;

                foreach (string thisFile in wadfiles)
                {
                    try
                    {
                        pbProgress.Value = ((i + 1) * 100) / (wadfiles.Length);

                        byte[] wadfile = Wii.Tools.LoadFileToByteArray(thisFile);

                        Infos[i, 0] = thisFile.Remove(0, thisFile.LastIndexOf('\\') + 1);
                        Infos[i, 1] = Wii.WadInfo.GetTitleID(wadfile, 0);
                        Infos[i, 2] = Wii.WadInfo.GetNandBlocks(wadfile);
                        Infos[i, 3] = Wii.WadInfo.GetNandSize(wadfile, true);
                        Infos[i, 4] = Wii.WadInfo.GetIosFlag(wadfile);
                        Infos[i, 5] = Wii.WadInfo.GetRegionFlag(wadfile);
                        Infos[i, 6] = Wii.WadInfo.GetContentNum(wadfile).ToString();
                        Infos[i, 7] = Wii.WadInfo.GetNandPath(wadfile, 0);
                        Infos[i, 8] = Wii.WadInfo.GetChannelType(wadfile, 0);
                        Infos[i, 9] = Wii.WadInfo.GetTitleVersion(wadfile).ToString();

                        switch (language)
                        {
                            case "Dutch":
                                Infos[i, 10] = Wii.WadInfo.GetChannelTitles(wadfile)[6];
                                break;
                            case "Italian":
                                Infos[i, 10] = Wii.WadInfo.GetChannelTitles(wadfile)[5];
                                break;
                            case "Spanish":
                                Infos[i, 10] = Wii.WadInfo.GetChannelTitles(wadfile)[4];
                                break;
                            case "French":
                                Infos[i, 10] = Wii.WadInfo.GetChannelTitles(wadfile)[3];
                                break;
                            case "German":
                                Infos[i, 10] = Wii.WadInfo.GetChannelTitles(wadfile)[2];
                                break;
                            default:
                                Infos[i, 10] = Wii.WadInfo.GetChannelTitles(wadfile)[1];
                                break;
                            case "Japanese":
                                Infos[i, 10] = Wii.WadInfo.GetChannelTitles(wadfile)[0];
                                break;
                        }

                        i++;
                    }
                    catch { }
                }


                for (int j = 0; j < wadfiles.Length; j++)
                {
                    lvWads.Items.Insert(lvWads.Items.Count, new ListViewItem(new String[] { Infos[j, 0], Infos[j, 1], Infos[j, 2], Infos[j, 3], Infos[j, 4], Infos[j, 5], Infos[j, 6], Infos[j, 7], Infos[j, 8], Infos[j, 9], Infos[j, 10] })).Group = lvWads.Groups[path];
                }

                for (int x = 0; x < lvWads.Items.Count; x++)
                {
                    if (lvWads.Items[x].Group == null)
                    {
                        lvWads.Items.Remove(lvWads.Items[x]);
                    }

                    if (string.IsNullOrEmpty(lvWads.Items[x].Text))
                    {
                        lvWads.Items.Remove(lvWads.Items[x]);
                    }
                }

                lvWads.Groups[lvWads.Groups.Count - 1].Header = lvWads.Groups[lvWads.Groups.Count - 1].Header + " (" + lvWads.Groups[lvWads.Groups.Count - 1].Items.Count + ")";
            }

            Cursor.Current = Cursors.Default;
            pbProgress.Value = 100;

            lbFileCount.Text = lvWads.Items.Count.ToString();
            lbFolderCount.Text = lvWads.Groups.Count.ToString();
        }

        /// <summary>
        /// Saves all settings to ShowMiiWads.cfg
        /// </summary>
        private void SaveSettings()
        {
            DataTable settings = new DataTable("Settings");
            DataTable window = new DataTable("Window");
            DataTable folders = new DataTable("Folders");

            settings.Columns.Add("Version");
            settings.Columns.Add("Language");
            settings.Columns.Add("LangFile");
            settings.Columns.Add("AutoSize");
            settings.Columns.Add("NandPath");
            settings.Columns.Add("ShowPath");
            settings.Columns.Add("AddSub");
            window.Columns.Add("WindowWidth");
            window.Columns.Add("WindowHeight");
            window.Columns.Add("LocationX");
            window.Columns.Add("LocationY");
            window.Columns.Add("WindowState");
            settings.Columns.Add("Accepted");
            settings.Columns.Add("SaveFolders");
            folders.Columns.Add("MRU0");
            folders.Columns.Add("MRU1");
            folders.Columns.Add("MRU2");
            folders.Columns.Add("MRU3");
            folders.Columns.Add("MRU4");
            settings.Columns.Add("CreateBackups");

            folders.Columns.Add("Foldercount");

            if (savefolders != "false")
            {
                for (int i = 0; i < lvWads.Groups.Count; i++)
                {
                    folders.Columns.Add("Folder" + i.ToString());
                }
            }

            DataRow settingsrow = settings.NewRow();
            DataRow windowrow = window.NewRow();
            DataRow foldersrow = folders.NewRow();

            settingsrow["Version"] = version;
            settingsrow["AddSub"] = addsub;
            settingsrow["NandPath"] = nandpath;
            settingsrow["Language"] = language;
            settingsrow["LangFile"] = langfile;
            settingsrow["AutoSize"] = autosize;
            settingsrow["ShowPath"] = showpath;
            settingsrow["CreateBackups"] = backup;
            windowrow["WindowWidth"] = wwidth;
            windowrow["WindowHeight"] = wheight;
            windowrow["LocationX"] = locx;
            windowrow["LocationY"] = locy;
            windowrow["WindowState"] = wstate;
            settingsrow["Accepted"] = accepted;
            settingsrow["SaveFolders"] = savefolders;
            foldersrow["MRU0"] = mru[0];
            foldersrow["MRU1"] = mru[1];
            foldersrow["MRU2"] = mru[2];
            foldersrow["MRU3"] = mru[3];
            foldersrow["MRU4"] = mru[4];

            if (savefolders != "false")
            {
                foldersrow["Foldercount"] = lvWads.Groups.Count.ToString();

                for (int j = 0; j < lvWads.Groups.Count; j++)
                {
                    foldersrow["Folder" + j.ToString()] = lvWads.Groups[j].Tag.ToString();
                }
            }
            else
            {
                foldersrow["Foldercount"] = "0";
            }

            settings.Rows.Add(settingsrow);
            window.Rows.Add(windowrow);
            folders.Rows.Add(foldersrow);

            DataSet ds = new DataSet("ShowMiiWads");
            ds.Tables.Add(settings);
            ds.Tables.Add(window);
            ds.Tables.Add(folders);

            ds.WriteXml(Application.StartupPath + "\\ShowMiiWads.cfg");
        }

        /// <summary>
        /// Loads all settings from ShowMiiWads.cfg
        /// </summary>
        private void LoadSettings()
        {
            if (File.Exists(@Application.StartupPath + "\\ShowMiiWads.cfg"))
            {
                DataSet ds = new DataSet("ShowMiiWads");

                ds.ReadXmlSchema(Application.StartupPath + "\\ShowMiiWads.cfg");
                ds.ReadXml(Application.StartupPath + "\\ShowMiiWads.cfg");

                try
                {
                    //if (ds.Tables["Settings"].Rows[0]["Version"].ToString() == version)
                    //{
                        langfile = ds.Tables["Settings"].Rows[0]["LangFile"].ToString();
                        addsub = ds.Tables["Settings"].Rows[0]["AddSub"].ToString();
                        savefolders = ds.Tables["Settings"].Rows[0]["SaveFolders"].ToString();
                        language = ds.Tables["Settings"].Rows[0]["Language"].ToString();
                        autosize = ds.Tables["Settings"].Rows[0]["AutoSize"].ToString();
                        accepted = ds.Tables["Settings"].Rows[0]["Accepted"].ToString();
                        wwidth = ds.Tables["Window"].Rows[0]["WindowWidth"].ToString();
                        wheight = ds.Tables["Window"].Rows[0]["WindowHeight"].ToString();
                        locx = ds.Tables["Window"].Rows[0]["LocationX"].ToString();
                        locy = ds.Tables["Window"].Rows[0]["LocationY"].ToString();
                        wstate = ds.Tables["Window"].Rows[0]["WindowState"].ToString();
                        nandpath = ds.Tables["Settings"].Rows[0]["NandPath"].ToString();
                        showpath = ds.Tables["Settings"].Rows[0]["ShowPath"].ToString();
                        mru[0] = ds.Tables["Folders"].Rows[0]["MRU0"].ToString();
                        mru[1] = ds.Tables["Folders"].Rows[0]["MRU1"].ToString();
                        mru[2] = ds.Tables["Folders"].Rows[0]["MRU2"].ToString();
                        mru[3] = ds.Tables["Folders"].Rows[0]["MRU3"].ToString();
                        mru[4] = ds.Tables["Folders"].Rows[0]["MRU4"].ToString();
                        backup = ds.Tables["Settings"].Rows[0]["CreateBackups"].ToString();

                        int foldercount = Convert.ToInt32(ds.Tables["Folders"].Rows[0]["Foldercount"]);

                        if (foldercount > 0)
                        {
                            for (int x = 0; x < foldercount; x++)
                            {
                                if (Directory.Exists(ds.Tables["Folders"].Rows[0]["Folder" + x.ToString()].ToString()))
                                {
                                    AddWads(ds.Tables["Folders"].Rows[0]["Folder" + x.ToString()].ToString());
                                }
                            }
                        }

                        LoadLanguage();

                        switch (autosize)
                        {
                            case "false":
                                btnAutoResize.Checked = false;
                                break;
                            default:
                                btnAutoResize.Checked = true;
                                break;
                        }

                        switch (backup)
                        {
                            case "true":
                                btnCreateBackups.Checked = true;
                                break;
                            default:
                                btnCreateBackups.Checked = false;
                                break;
                        }

                        switch (savefolders)
                        {
                            case "false":
                                btnSaveFolders.Checked = false;
                                break;
                            default:
                                btnSaveFolders.Checked = true;
                                break;
                        }

                        switch (showpath)
                        {
                            case "false":
                                btnShowPath.Checked = false;
                                break;
                            default:
                                btnShowPath.Checked = true;
                                break;
                        }

                        SetWindowProperties(wwidth, wheight, locx, locy, wstate);

                        switch (accepted)
                        {
                            case "true":
                                accepted = "true";
                                EditFeatures(true);
                                break;
                            case "nouse":
                                EditFeatures(false);
                                break;
                            default:
                                Disclaimer dcl = new Disclaimer();
                                dcl.firststart = true;
                                dcl.ShowDialog();
                                if (accepted == "true")
                                {
                                   EditFeatures(true);
                                }
                                else
                                {
                                    EditFeatures(false);
                                }
                                break;
                        }

                        btnRefresh_Click(null, null);
                    //}
                    //else
                    //{
                    //    File.Delete(Application.StartupPath + "\\ShowMiiWads.cfg");
                    //    LoadSettings();
                    //}
                }
                catch
                {
                    File.Delete(Application.StartupPath + "\\ShowMiiWads.cfg");
                    LoadSettings();
                }
            }
            else
            {
                LoadLanguage();

                SetWindowProperties(wwidth, wheight, "", "", "Normal");

                Disclaimer dcl = new Disclaimer();
                dcl.firststart = true;
                dcl.ShowDialog();
                if (accepted == "true")
                {
                    EditFeatures(true);
                }
                else
                {
                    EditFeatures(false);
                }
            }
        }

        /// <summary>
        /// Sets the WindowProperties to the given values
        /// </summary>
        /// <param name="width">Window Width</param>
        /// <param name="height">Window Height</param>
        /// <param name="x">Window Location X</param>
        /// <param name="y">Window Location Y</param>
        /// <param name="state">Window State</param>
        private void SetWindowProperties(string width, string height, string x, string y, string state)
        {
            try
            {
                this.Size = new Size(Convert.ToInt32(width), Convert.ToInt32(height));

                if (Convert.ToInt32(x) < 0 || Convert.ToInt32(y) < 0)
                {
                    x = "";
                    y = "";
                }

                if (x == "" && y == "")
                {
                    CenterToScreen();
                }
                else
                {
                    this.Location = new Point(Convert.ToInt32(x), Convert.ToInt32(y));
                }

                switch (state)
                {
                    case "Maximized":
                        this.WindowState = FormWindowState.Maximized;
                        break;
                    default:
                        this.WindowState = FormWindowState.Normal;
                        break;
                }
            }
            catch { }
        }

        /// <summary>
        /// Loads the Language which is set in the global variable "language" to the Messages String Array
        /// </summary>
        private void LoadLanguage()
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            int counter = -1;
            string thisline = "";

            switch (language)
            {
                case "German":
                    StreamReader germanstream = new StreamReader(_assembly.GetManifestResourceStream("ShowMiiWads.German.txt"));

                    while ((thisline = germanstream.ReadLine()) != null)
                    {
                        if (!thisline.StartsWith("//"))
                            Messages[++counter] = thisline;
                    }

                    btnGerman.Checked = true;
                    btnEnglish.Checked = false;
                    btnFrench.Checked = false;
                    btnFromFile.Checked = false;
                    germanstream.Close();
                    break;
                case "French":
                    StreamReader frenchstream = new StreamReader(_assembly.GetManifestResourceStream("ShowMiiWads.French.txt"));

                    while ((thisline = frenchstream.ReadLine()) != null)
                    {
                        if (!thisline.StartsWith("//"))
                            Messages[++counter] = thisline;
                    }

                    btnFrench.Checked = true;
                    btnEnglish.Checked = false;
                    btnGerman.Checked = false;
                    btnFromFile.Checked = false;
                    frenchstream.Close();
                    break;
                case "File":
                    if (File.Exists(langfile))
                    {
                        int count = 0;
                        using (StreamReader reader = new StreamReader(langfile))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (!line.StartsWith("//"))
                                    count++;
                            }
                        }

                        if (langcount == count)
                        {
                            StreamReader filestream = new StreamReader(langfile);

                            while ((thisline = filestream.ReadLine()) != null)
                            {
                                if (!thisline.StartsWith("//"))
                                    Messages[++counter] = thisline;
                            }

                            btnEnglish.Checked = false;
                            btnGerman.Checked = false;
                            btnFrench.Checked = false;
                            btnFromFile.Checked = true;
                            filestream.Close();
                        }
                        else
                        {
                            //Zu viele oder zu wenig Zeilen
                            MessageBox.Show(Messages[64], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                            btnFromFile.Checked = false;
                            if (oldlang != "") { language = oldlang; }
                            else { language = "English"; }

                            langfile = "";
                            LoadLanguage();
                        }
                    }
                    else
                    {
                        btnFromFile.Checked = false;
                        if (oldlang != "") { language = oldlang; }
                        else { language = "English"; }

                        langfile = "";
                        LoadLanguage();
                    }
                    break;
                default:
                    StreamReader englishstream = new StreamReader(_assembly.GetManifestResourceStream("ShowMiiWads.English.txt"));

                    while ((thisline = englishstream.ReadLine()) != null)
                    {
                        if (!thisline.StartsWith("//"))
                            Messages[++counter] = thisline;
                    }

                    btnEnglish.Checked = true;
                    btnGerman.Checked = false;
                    btnFrench.Checked = false;
                    btnFromFile.Checked = false;
                    englishstream.Close();
                    break;
            }

            RefreshTexts();
        }

        /// <summary>
        /// Refreshes all Texts on the GUI
        /// </summary>
        private void RefreshTexts()
        {
            btnAbout.Text = Messages[7];
            btnEnglish.Text = Messages[5];
            btnGerman.Text = Messages[6];
            btnFrench.Text = Messages[94];
            btnRefresh.Text = Messages[4];
            btnChangeID.Text = Messages[22];
            btnRename.Text = Messages[23];
            btnExit.Text = Messages[26];
            btnDelete.Text = Messages[30];
            btnAutoResize.Text = Messages[34];
            btnSaveFolders.Text = Messages[3];
            btnOpen.Text = Messages[45];
            btnCopy.Text = Messages[46];
            btnCut.Text = Messages[47];
            btnPaste.Text = Messages[48];
            btnNandPath.Text = Messages[61];
            btnToNand.Text = Messages[60];
            btnToFolder.Text = Messages[55];
            btnFolderFree.Text = Messages[70];
            btnExport.Text = Messages[71];
            btnShowPath.Text = Messages[72];
            btnChannelName.Text = Messages[73];
            btnFromFile.Text = Messages[77];
            btnPackWad.Text = Messages[80];
            btnPackWadWithoutTrailer.Text = Messages[81];
            btnAddSub.Text = Messages[93];
            btnPreview.Text = Messages[99];
            btnUnpackU8.Text = Messages[100];
            btnConvertTpl.Text = Messages[101];
            btnCreateKey.Text = Messages[110];
            btnUpdateCheck.Text = Messages[114];
            btnCreateBackups.Text = Messages[117];
            btnRestore.Text = Messages[118];

            cmRestore.Text = Messages[118];
            cmNandPreview.Text = Messages[99];
            cmNandPackWad.Text = Messages[80];
            cmPreview.Text = Messages[99];
            cmChangeID.Text = Messages[22];
            cmRename.Text = Messages[23];
            cmDelete.Text = Messages[30];
            cmRemoveFolder.Text = Messages[38];
            cmCopy.Text = Messages[46];
            cmCut.Text = Messages[47];
            cmPaste.Text = Messages[48];
            cmToNand.Text = Messages[60];
            cmToFolder.Text = Messages[55];
            cmExtract.Text = Messages[40];
            cmFolderFree.Text = Messages[70];
            cmChannelName.Text = Messages[73];
            cmNandDelete.Text = Messages[30];
            cmInstall.Text = Messages[84];
            cmInstallWad.Text = Messages[90];
            cmInstallFolder.Text = Messages[91];

            tsFile.Text = Messages[0];
            tsHelp.Text = Messages[2];
            tsLanguage.Text = Messages[1];
            tsEdit.Text = Messages[21];
            tsOptions.Text = Messages[25];
            tsChangeRegion.Text = Messages[24];
            tsExtract.Text = Messages[40];
            tsMru.Text = Messages[76];
            tsTools.Text = Messages[78];
            tsView.Text = Messages[79];

            if (lvWads.Visible == true)
            {
                lbFiles.Text = Messages[16] + ":";
                lbFolders.Text = Messages[37] + ":";
            }
            else { lbFiles.Text = Messages[86] + ":"; }

            lbQueue.Text = Messages[83] + ":";
            lbQueueInstall.Text = Messages[84];
            lbQueueDiscard.Text = Messages[85];

            lvName.Text = Messages[8];
            lvID.Text = Messages[9];
            lvBlocks.Text = Messages[10];
            lvData.Text = Messages[14];
            lvFilesize.Text = Messages[11];
            lvIOS.Text = Messages[12];
            lvPath.Text = Messages[15];
            lvRegion.Text = Messages[13];
            lvVersion.Text = Messages[87];
            lvTitle.Text = Messages[89];
            lvType.Text = Messages[88];
        }

        /// <summary>
        /// Changes the Title ID of the given Wad file
        /// </summary>
        /// <param name="wadfile">The Wad file to edit</param>
        /// <param name="oldid">The old Title ID of the Wad file</param>
        /// <returns></returns>
        private bool ChangeTitleID(string wadfile, string oldid)
        {
            if (File.Exists(ckey) || File.Exists(key))
            {
                InputBoxDialog ib = new InputBoxDialog();
                ib.FormCaption = Messages[41];
                ib.FormPrompt = Messages[42];
                ib.DefaultValue = oldid;
                ib.btnCancel.Text = Messages[27];
                ib.MaxLength = 4;

                if (ib.ShowDialog() == DialogResult.OK)
                {
                    string newid = ib.InputResponse.ToUpper();
                    if (newid.Length == 4)
                    {
                        if (newid != oldid)
                        {
                            Regex reg = new Regex("^[0-9A-Za-z]*$");

                            if (reg.IsMatch(newid))
                            {
                                Cursor.Current = Cursors.WaitCursor;
                                CreateBackup(wadfile);

                                try { Wii.WadEdit.ChangeTitleID(wadfile, newid); }
                                catch (Exception ex) { MessageBox.Show(ex.Message, Messages[53], MessageBoxButtons.OK, MessageBoxIcon.Information); }

                                Cursor.Current = Cursors.Default;
                                return true;
                            }
                            else
                            {
                                MessageBox.Show(Messages[43], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show(Messages[44], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show(Messages[52], Messages[53], MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }

        /// <summary>
        /// Changes the region of the given Wad file
        /// </summary>
        /// <param name="wadfile">The Wad file to edit</param>
        /// <param name="region">p for PAL, u for USA, j for JAPAN, f for FREE</param>
        /// <returns></returns>
        private bool ChangeRegion(string wadfile, char region)
        {
            Cursor.Current = Cursors.WaitCursor;

            int intregion;

            switch (region)
            {
                case 'p':
                    intregion = 2;
                    break;
                case 'u':
                    intregion = 1;
                    break;
                case 'j':
                    intregion = 0;
                    break;
                default:
                    intregion = 3;
                    break;
            }

            CreateBackup(wadfile);

            Wii.WadEdit.ChangeRegion(wadfile, intregion);

            Cursor.Current = Cursors.Default;

            return true;
        }

        /// <summary>
        /// Re-Sorts the Listview, depending on the clicked Column
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvWads_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            if (e.Column == lvSorter.SortColumn)
            {
                if (lvSorter.Order == SortOrder.Ascending)
                {
                    lvSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                lvSorter.Order = SortOrder.Ascending;
            }

            lvSorter.Column = e.Column;
            lvSorter.SortColumn = e.Column;
            lvWads.Sort();
        }

        /// <summary>
        /// Resizes the Columns, when the Form is resized (if autosize is true)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvWads_Resize(object sender, EventArgs e)
        {
            if (autosize == "true")
            {
                lvWads.Columns[0].Width = Convert.ToInt32(lvWads.Width * ((double)145 / 914)); //Filename
                lvWads.Columns[1].Width = Convert.ToInt32(lvWads.Width * ((double)56 / 914)); //Title ID
                lvWads.Columns[2].Width = Convert.ToInt32(lvWads.Width * ((double)60 / 914)); //Blocks
                lvWads.Columns[3].Width = Convert.ToInt32(lvWads.Width * ((double)82 / 914)); //Filesize
                lvWads.Columns[4].Width = Convert.ToInt32(lvWads.Width * ((double)53 / 914)); //IOS Flag
                lvWads.Columns[5].Width = Convert.ToInt32(lvWads.Width * ((double)70 / 914)); //Region Flag
                lvWads.Columns[6].Width = Convert.ToInt32(lvWads.Width * ((double)49 / 914)); //Content
                lvWads.Columns[8].Width = Convert.ToInt32(lvWads.Width * ((double)78 / 914)); //Type
                lvWads.Columns[9].Width = Convert.ToInt32(lvWads.Width * ((double)47 / 914)); //Version
                lvWads.Columns[10].Width = Convert.ToInt32(lvWads.Width * ((double)148 / 914)); //Channel Title
                lvWads.Columns[7].Width = Convert.ToInt32(lvWads.Width -                //Path
                    lvWads.Columns[0].Width -
                    lvWads.Columns[1].Width -
                    lvWads.Columns[2].Width -
                    lvWads.Columns[3].Width -
                    lvWads.Columns[4].Width -
                    lvWads.Columns[5].Width -
                    lvWads.Columns[6].Width -
                    lvWads.Columns[8].Width -
                    lvWads.Columns[9].Width -
                    lvWads.Columns[10].Width);
            }
        }

        /// <summary>
        /// Renames the given Wad file
        /// </summary>
        /// <param name="Wadfile">The Wad file to rename</param>
        private void RenameWad(string Wadfile)
        {
            if (File.Exists(Wadfile))
            {
                string thispath = Wadfile.Remove(Wadfile.LastIndexOf("\\"));
                string wadname = Wadfile.Remove(Wadfile.Length - 4);
                wadname = wadname.Remove(0, wadname.LastIndexOf("\\") + 1);
                InputBoxDialog ib = new InputBoxDialog();
                ib.FormPrompt = Messages[29] + "\r\n\r\n" + Wadfile;
                ib.btnCancel.Text = Messages[27];
                ib.FormCaption = Messages[28];
                ib.DefaultValue = wadname;
                ib.ShowDialog();

                if (ib.DialogResult == DialogResult.OK)
                {
                    string newname = ib.InputResponse;
                    FileInfo fiWad = new FileInfo(@Wadfile);

                    if (!newname.EndsWith(".wad"))
                    {
                        newname = newname + ".wad";
                    }

                    try
                    {
                        fiWad.MoveTo(@thispath + "\\" + newname);
                        btnRefresh_Click(null, null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(Messages[31], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Delete the given Wad file
        /// </summary>
        /// <param name="Wadfile">The Wad file to delete</param>
        private void DeleteWad(string Wadfile)
        {
            if (File.Exists(Wadfile))
            {
                if (MessageBox.Show(Messages[33], Messages[32], MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Delete(Wadfile);
                    btnRefresh_Click(null, null);
                }
            }
            else
            {
                MessageBox.Show(Messages[31], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Deletes all groups in Listview and Re-Adds them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (lvWads.Visible == true)
            {
                string[] groups = new string[lvWads.Groups.Count];
                int groupcount = lvWads.Groups.Count;

                for (int i = 0; i < groupcount; i++)
                {
                    groups[i] = lvWads.Groups[i].Tag.ToString();
                }

                lvWads.Groups.Clear();
                lvWads.Items.Clear();

                for (int j = 0; j < groups.Length; j++)
                {
                    AddWads(groups[j]);
                }

                if (groups.Length == 0)
                {
                    lbFileCount.Text = "0";
                    lbFolderCount.Text = "0";
                }
            }
            else if (lvNand.Visible == true)
            {
                AddNand();
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Text = Messages[17];
            about.lbSMW.Text = "ShowMiiWads " + version + " by Leathl";
            about.ShowDialog();
        }

        private void btnEnglish_Click(object sender, EventArgs e)
        {
            if (btnEnglish.Checked == true)
            {
                btnGerman.Checked = false;
                btnFrench.Checked = false;
                btnFromFile.Checked = false;
                language = "English";
                langfile = "";
                LoadLanguage();
                SaveSettings();
                btnRefresh_Click(null, null);
            }
            else
            {
                btnEnglish.Checked = true;
            }
        }

        private void btnGerman_Click(object sender, EventArgs e)
        {
            if (btnGerman.Checked == true)
            {
                btnEnglish.Checked = false;
                btnFrench.Checked = false;
                btnFromFile.Checked = false;
                language = "German";
                langfile = "";
                LoadLanguage();
                SaveSettings();
                btnRefresh_Click(null, null);
            }
            else
            {
                btnGerman.Checked = true;
            }
        }

        private void btnFrench_Click(object sender, EventArgs e)
        {
            if (btnFrench.Checked == true)
            {
                btnEnglish.Checked = false;
                btnGerman.Checked = false;
                btnFromFile.Checked = false;
                language = "French";
                langfile = "";
                LoadLanguage();
                SaveSettings();
                btnRefresh_Click(null, null);
            }
            else
            {
                btnFrench.Checked = true;
            }
        }

        private void btnFromFile_Click(object sender, EventArgs e)
        {
            if (btnFromFile.Checked == true)
            {
                OpenFileDialog opendlg = new OpenFileDialog();
                opendlg.Filter = Messages[63] + " (*.slang) | *.slang";
                opendlg.InitialDirectory = Application.StartupPath;

                if (opendlg.ShowDialog() == DialogResult.OK)
                {
                    oldlang = language;
                    language = "File";
                    langfile = opendlg.FileName;
                    LoadLanguage();
                    SaveSettings();
                    btnRefresh_Click(null, null);
                }
                else
                {
                    btnFromFile.Checked = false;
                }
            }
            else
            {
                btnFromFile.Checked = true;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Shows ContextMenu, if an Item was Right-Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvWads_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (lvWads.SelectedItems.Count != 0)
                {
                    cmWads.Show(lvWads, e.Location);
                }
            }
        }

        private void cmRename_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                RenameWad(lvWads.SelectedItems[0].Group.Tag.ToString() + "\\" + lvWads.SelectedItems[0].Text);
            }
        }

        private void cmDelete_Click(object sender, EventArgs e)
        {
            if (lvWads.Visible == true)
            {
                if (lvWads.SelectedItems.Count == 1)
                {
                    DeleteWad(lvWads.SelectedItems[0].Group.Tag.ToString() + "\\" + lvWads.SelectedItems[0].Text);
                }
            }
            else
            {
                if (lvNand.SelectedItems.Count == 1)
                {
                    if (File.Exists(nandpath + "\\ticket\\" + lvNand.SelectedItems[0].SubItems[7].Text.Remove(8) + "\\" + lvNand.SelectedItems[0].Text))
                        File.Delete(nandpath + "\\ticket\\" + lvNand.SelectedItems[0].SubItems[7].Text.Remove(8) + "\\" + lvNand.SelectedItems[0].Text);

                    if (Directory.Exists(nandpath + "\\title\\" + lvNand.SelectedItems[0].SubItems[7].Text.Replace('/', '\\')))
                        Directory.Delete(nandpath + "\\title\\" + lvNand.SelectedItems[0].SubItems[7].Text.Replace('/', '\\'), true);

                    btnRefresh_Click(null, null);
                }
            }
        }

        private void btnAutoResize_Click(object sender, EventArgs e)
        {
            if (btnAutoResize.Checked == true)
            {
                autosize = "true";

                lvWads.Columns[0].Width = Convert.ToInt32(lvWads.Width * ((double)145 / 914)); //Filename
                lvWads.Columns[1].Width = Convert.ToInt32(lvWads.Width * ((double)56 / 914)); //Title ID
                lvWads.Columns[2].Width = Convert.ToInt32(lvWads.Width * ((double)60 / 914)); //Blocks
                lvWads.Columns[3].Width = Convert.ToInt32(lvWads.Width * ((double)82 / 914)); //Filesize
                lvWads.Columns[4].Width = Convert.ToInt32(lvWads.Width * ((double)53 / 914)); //IOS Flag
                lvWads.Columns[5].Width = Convert.ToInt32(lvWads.Width * ((double)70 / 914)); //Region Flag
                lvWads.Columns[6].Width = Convert.ToInt32(lvWads.Width * ((double)49 / 914)); //Content
                lvWads.Columns[8].Width = Convert.ToInt32(lvWads.Width * ((double)78 / 914)); //Type
                lvWads.Columns[9].Width = Convert.ToInt32(lvWads.Width * ((double)47 / 914)); //Version
                lvWads.Columns[10].Width = Convert.ToInt32(lvWads.Width * ((double)148 / 914)); //Channel Title
                lvWads.Columns[7].Width = Convert.ToInt32(lvWads.Width -                //Path
                    lvWads.Columns[0].Width -
                    lvWads.Columns[1].Width -
                    lvWads.Columns[2].Width -
                    lvWads.Columns[3].Width -
                    lvWads.Columns[4].Width -
                    lvWads.Columns[5].Width -
                    lvWads.Columns[6].Width -
                    lvWads.Columns[8].Width -
                    lvWads.Columns[9].Width -
                    lvWads.Columns[10].Width);

                lvNand.Columns[0].Width = Convert.ToInt32(lvNand.Width * ((double)145 / 914)); //Filename
                lvNand.Columns[1].Width = Convert.ToInt32(lvNand.Width * ((double)56 / 914)); //Title ID
                lvNand.Columns[2].Width = Convert.ToInt32(lvNand.Width * ((double)60 / 914)); //Blocks
                lvNand.Columns[3].Width = Convert.ToInt32(lvNand.Width * ((double)82 / 914)); //Filesize
                lvNand.Columns[4].Width = Convert.ToInt32(lvNand.Width * ((double)53 / 914)); //IOS Flag
                lvNand.Columns[5].Width = Convert.ToInt32(lvNand.Width * ((double)70 / 914)); //Region Flag
                lvNand.Columns[6].Width = Convert.ToInt32(lvNand.Width * ((double)49 / 914)); //Content
                lvNand.Columns[8].Width = Convert.ToInt32(lvNand.Width * ((double)78 / 914)); //Type
                lvNand.Columns[9].Width = Convert.ToInt32(lvNand.Width * ((double)47 / 914)); //Version
                lvNand.Columns[10].Width = Convert.ToInt32(lvNand.Width * ((double)148 / 914)); //Channel Title
                lvNand.Columns[7].Width = Convert.ToInt32(lvNand.Width -                //Path
                    lvNand.Columns[0].Width -
                    lvNand.Columns[1].Width -
                    lvNand.Columns[2].Width -
                    lvNand.Columns[3].Width -
                    lvNand.Columns[4].Width -
                    lvNand.Columns[5].Width -
                    lvNand.Columns[6].Width -
                    lvNand.Columns[8].Width -
                    lvNand.Columns[9].Width -
                    lvNand.Columns[10].Width);
            }
            else
            {
                autosize = "false";
            }

            SaveSettings();
        }

        /// <summary>
        /// Adds dropped folders to Listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvWads_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] drop = (string[])e.Data.GetData(DataFormats.FileDrop);
                string[] added = new string[drop.Length];

                for (int i = 0; i < drop.Length; i++)
                {
                    if (Directory.Exists(drop[i]))
                    {
                        bool doesexist = false;

                        for (int j = 0; j < lvWads.Groups.Count; j++)
                        {
                            if (drop[i] == lvWads.Groups[j].Tag.ToString())
                            {
                                doesexist = true;
                            }
                        }

                        if (doesexist == false)
                        {
                            if (Directory.GetFiles(drop[i], "*.wad").Length > 0)
                            {
                                NewMRU(drop[i]);

                                //Wads aus Verzeichnis adden
                                if (addsub != "true") { this.BeginInvoke(AddWadsDel, new object[] { drop[i] }); }
                                else { this.BeginInvoke(AddWadsSubDel, new object[] { drop[i] }); }
                                this.Activate();
                            }
                        }
                    }
                    else if (File.Exists(drop[i]) && drop[i].Remove(0, drop[i].LastIndexOf('.')) == ".wad")
                    {
                        bool doesexist = false;

                        for (int j = 0; j < lvWads.Groups.Count; j++)
                        {
                            if (drop[i].Remove(drop[i].LastIndexOf('\\')) == lvWads.Groups[j].Tag.ToString())
                            {
                                doesexist = true;
                            }
                        }

                        for (int k = 0; k < drop.Length; k++)
                        {
                            if (drop[i].Remove(drop[i].LastIndexOf('\\')).ToUpper() == added[k])
                            {
                                doesexist = true;
                            }
                        }

                        if (doesexist == false)
                        {
                            if (Directory.GetFiles(drop[i].Remove(drop[i].LastIndexOf('\\')), "*.wad").Length > 0)
                            {
                                NewMRU(drop[i].Remove(drop[i].LastIndexOf('\\')));

                                //Verzeichnis, in dem die Datei liegt, hinzufügen
                                this.BeginInvoke(AddWadsDel, new object[] { drop[i].Remove(drop[i].LastIndexOf('\\')) });
                                this.Activate();
                                added[i] = drop[i].Remove(drop[i].LastIndexOf('\\')).ToUpper();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Sets DragEffect to Copy, if folders or Wad files are dragged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvWads_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drop = (string[])e.Data.GetData(DataFormats.FileDrop);
                bool right = true;

                for (int i = 0; i < drop.Length; i++)
                {
                    if (Directory.Exists(drop[i]) == false && File.Exists(drop[i]) == false)
                    {
                        right = false;
                    }
                    else if (File.Exists(drop[i]) && drop[i].Remove(0, drop[i].LastIndexOf('.')) != ".wad")
                    {
                        right = false;
                    }
                }

                if (right == true)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            } 
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.Description = Messages[20];
            DialogResult thisresult = path.ShowDialog();

            if (thisresult == DialogResult.OK)
            {
                bool thisexists = false;

                for (int i = 0; i < lvWads.Groups.Count; i++)
                {
                    if (lvWads.Groups[i].Tag.ToString() == path.SelectedPath)
                    {
                        thisexists = true;
                    }
                }

                if (thisexists == false)
                {
                    if (Directory.GetFiles(path.SelectedPath, "*.wad").Length > 0)
                    {
                        NewMRU(path.SelectedPath);

                        if (addsub != "true") { AddWads(path.SelectedPath); }
                        else { AddWadsSub(path.SelectedPath); }
                    }
                }
            }
        }

        private void Main_LocationChanged(object sender, EventArgs e)
        {
            if (this.Location.X != -8 && this.Location.Y != -8)
            {
                locx = this.Location.X.ToString();
                locy = this.Location.Y.ToString();
            }
        }

        private void Main_ResizeEnd(object sender, EventArgs e)
        {
            wwidth = this.Size.Width.ToString();
            wheight = this.Size.Height.ToString();

        }

        private void Main_SizeChanged(object sender, EventArgs e)
        {
            wstate = this.WindowState.ToString();
        }

        private void cmRemoveFolder_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                ListViewGroup thisgroup = lvWads.SelectedItems[0].Group;
                string thisheader = thisgroup.Tag.ToString();

                for (int i = lvWads.Groups[thisgroup.Name].Items.Count; i > 0; i--)
                {
                    lvWads.Groups[thisgroup.Name].Items.RemoveAt(i - 1);
                }

                lvWads.Groups.Remove(thisgroup);

                for (int j = lvWads.Items.Count; j > 0; j--)
                {
                    if (lvWads.Items[j - 1].Group == null || lvWads.Items[j - 1].Group.Tag.ToString() == thisheader)
                    {
                        lvWads.Items.RemoveAt(j - 1);
                    }
                }

                lvWads.Groups.Remove(thisgroup);

                lbFolderCount.Text = lvWads.Groups.Count.ToString();
                lbFileCount.Text = lvWads.Items.Count.ToString();
            }
        }

        private void cmChangeID_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                string path = lvWads.SelectedItems[0].Group.Tag.ToString();
                string file = lvWads.SelectedItems[0].Text;
                string oldid = lvWads.SelectedItems[0].SubItems[1].Text;

                if (lvWads.SelectedItems[0].SubItems[8].Text.Contains("System"))
                {
                    MessageBox.Show(Messages[62], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (ChangeTitleID(path + "\\" + file, oldid) == true)
                    {
                        btnRefresh_Click(null, null);
                    }
                }
            }
        }

        private void cmRegionFree_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                if (cmRegionFree.Checked == false)
                {
                    string path = lvWads.SelectedItems[0].Group.Tag.ToString();
                    string file = lvWads.SelectedItems[0].Text;

                    if (lvWads.SelectedItems[0].SubItems[8].Text.Contains("System"))
                    {
                        MessageBox.Show(Messages[62], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (ChangeRegion(path + "\\" + file, 'f') == true)
                        {
                            btnRefresh_Click(null, null);
                        }
                    }
                }
            }
        }

        private void cmPal_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                if (cmPal.Checked == false)
                {
                    string path = lvWads.SelectedItems[0].Group.Tag.ToString();
                    string file = lvWads.SelectedItems[0].Text;

                    if (lvWads.SelectedItems[0].SubItems[8].Text.Contains("System"))
                    {
                        MessageBox.Show(Messages[62], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (ChangeRegion(path + "\\" + file, 'p') == true)
                        {
                            btnRefresh_Click(null, null);
                        }
                    }
                }
            }
        }

        private void cmNtscU_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                if (cmNtscU.Checked == false)
                {
                    string path = lvWads.SelectedItems[0].Group.Tag.ToString();
                    string file = lvWads.SelectedItems[0].Text;

                    if (lvWads.SelectedItems[0].SubItems[8].Text.Contains("System"))
                    {
                        MessageBox.Show(Messages[62], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (ChangeRegion(path + "\\" + file, 'u') == true)
                        {
                            btnRefresh_Click(null, null);
                        }
                    }
                }
            }
        }

        private void cmNtscJ_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                if (cmNtscJ.Checked == false)
                {
                    string path = lvWads.SelectedItems[0].Group.Tag.ToString();
                    string file = lvWads.SelectedItems[0].Text;

                    if (lvWads.SelectedItems[0].SubItems[8].Text.Contains("System"))
                    {
                        MessageBox.Show(Messages[62], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        if (ChangeRegion(path + "\\" + file, 'j') == true)
                        {
                            btnRefresh_Click(null, null);
                        }
                    }
                }
            }
        }

        private void btnDisclaimer_Click(object sender, EventArgs e)
        {
            Disclaimer dcl = new Disclaimer();

            switch (accepted)
            {
                case "true":
                    dcl.rbAccept.Checked = true;
                    dcl.rbNoUse.Checked = false;
                    break;
                default:
                    dcl.rbAccept.Checked = false;
                    dcl.rbNoUse.Checked = true;
                    break;
            }

            dcl.ShowDialog();
            if (accepted == "true")
            {
                EditFeatures(true);
            }
            else
            {
                EditFeatures(false);
            }
        }

        private void btnSaveFolders_Click(object sender, EventArgs e)
        {
            if (btnSaveFolders.Checked == true)
            {
                savefolders = "true";
            }
            else
            {
                savefolders = "false";
            }
            
            SaveSettings();
        }

        private void cmCopy_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                copyfile = lvWads.SelectedItems[0].Group.Tag.ToString() + "\\" + lvWads.SelectedItems[0].Text;
                copyaction = "copy";
            }
        }

        private void cmCut_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                copyfile = lvWads.SelectedItems[0].Group.Tag.ToString() + "\\" + lvWads.SelectedItems[0].Text;
                copyaction = "cut";
            }
        }

        private void cmPaste_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                if (copyfile != "")
                {
                    string newfolder = lvWads.SelectedItems[0].Group.Tag.ToString();
                    string filename = copyfile.Remove(0, copyfile.LastIndexOf('\\') + 1);

                    if (File.Exists(copyfile))
                    {
                        bool over = false;
                        bool exists = false;

                        if (File.Exists(newfolder + "\\" + filename))
                        {
                            exists = true;
                            if (MessageBox.Show(Messages[49], Messages[51], MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                over = true;
                            }
                        }

                        if (over == true || exists == false)
                        {
                            try
                            {
                                File.Copy(copyfile, newfolder + "\\" + filename);

                                if (copyaction == "cut")
                                {
                                    File.Delete(copyfile);
                                    copyfile = "";
                                }

                                btnRefresh_Click(null, null);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error occured (004): " + ex.Message + "\r\r" + ex.Source, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else
                    {
                        //Sourcefile does not exist
                        MessageBox.Show(Messages[50], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void cmWads_Opening(object sender, CancelEventArgs e)
        {
            cmPal.Checked = false;
            cmNtscU.Checked = false;
            cmNtscJ.Checked = false;
            cmRegionFree.Checked = false;

            switch (lvWads.SelectedItems[0].SubItems[5].Text)
            {
                case "Europe":
                    cmPal.Checked = true;
                    break;
                case "Japan":
                    cmNtscJ.Checked = true;
                    break;
                case "USA":
                    cmNtscU.Checked = true;
                    break;
                default:
                    cmRegionFree.Checked = true;
                    break;
            }
        }

        private void tsEdit_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                btnPal.Checked = false;
                btnNtscU.Checked = false;
                btnNtscJ.Checked = false;
                btnRegionFree.Checked = false;

                switch (lvWads.SelectedItems[0].SubItems[5].Text)
                {
                    case "Europe":
                        btnPal.Checked = true;
                        break;
                    case "Japan":
                        btnNtscJ.Checked = true;
                        break;
                    case "USA":
                        btnNtscU.Checked = true;
                        break;
                    default:
                        btnRegionFree.Checked = true;
                        break;
                }
            }
        }

        private void cmToNAND_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                string wadfile = lvWads.SelectedItems[0].Group.Tag.ToString() + "\\" + lvWads.SelectedItems[0].Text;

                if (lvWads.SelectedItems[0].SubItems[8].Text.Contains("System"))
                {
                    //Systemtitel können nicht in den NAND entpackt werden
                    MessageBox.Show(Messages[62], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    CopyToNand(wadfile);
                }
            }
        }

        /// <summary>
        /// Extracts given Wad file to NAND Path
        /// </summary>
        /// <param name="wadfile">Wad file to extract</param>
        /// <param name="path1">First Part of Path</param>
        /// <param name="path2">Second Part of Path</param>
        private void CopyToNand(string wadfile)
        {
            if (File.Exists(ckey) || File.Exists(key))
            {
                if (nandpath != "")
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Wii.WadUnpack.UnpackToNand(wadfile, nandpath);
                    Cursor.Current = Cursors.Default;
                }
                else
                {
                    MessageBox.Show(Messages[57], Messages[58], MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(Messages[52], Messages[53], MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnNandPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            fb.Description = Messages[56];

            if (nandpath != "")
            {
                fb.SelectedPath = nandpath;
            }

            if (fb.ShowDialog() == DialogResult.OK)
            {
                nandpath = fb.SelectedPath;
                SaveSettings();

                if (lvNand.Visible == true)
                { btnRefresh_Click(null, null); }
            }
        }

        private void cmToFolder_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                string wadfile = lvWads.SelectedItems[0].Group.Tag + "\\" + lvWads.SelectedItems[0].Text;
                string wadname = lvWads.SelectedItems[0].Text.Remove(lvWads.SelectedItems[0].Text.Length - 4);

                FolderBrowserDialog fd = new FolderBrowserDialog();
                fd.Description = string.Format(Messages[39], wadname);
                if (!lvWads.SelectedItems[0].SubItems[8].Text.Contains("System"))
                {
                    if (File.Exists(ckey) || File.Exists(key))
                    {
                        if (fd.ShowDialog() == DialogResult.OK)
                        {
                            Cursor.Current = Cursors.WaitCursor;

                            try { Wii.WadUnpack.UnpackWad(wadfile, fd.SelectedPath + "\\" + wadname); }
                            catch (Exception ex) { MessageBox.Show(ex.Message); }

                            Cursor.Current = Cursors.Default;
                        }
                    }
                    else
                    {
                        //Common-Key fehlt
                        MessageBox.Show(Messages[52], Messages[53], MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    //Systemtitles can't be extracted
                    MessageBox.Show(Messages[62], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cmFolderFree_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                string path = lvWads.SelectedItems[0].Group.Tag.ToString();
                int tochange = 0;

                for (int i = 0; i < lvWads.Groups[path].Items.Count; i++)
                {
                    if (!lvWads.Groups[path].Items[i].SubItems[8].Text.Contains("System"))
                    {
                        if (!lvWads.Groups[path].Items[i].SubItems[5].Text.Contains("Free"))
                        {
                            tochange++;
                        }
                    }
                }

                if (tochange > 0)
                {
                    if (MessageBox.Show(Messages[67], Messages[68], MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        string[] wads = new string[lvWads.Groups[path].Items.Count];

                        for (int i = 0; i < lvWads.Groups[path].Items.Count; i++)
                        {
                            if (!lvWads.Groups[path].Items[i].SubItems[8].Text.Contains("System"))
                            {
                                if (!lvWads.Groups[path].Items[i].SubItems[5].Text.Contains("Free"))
                                {
                                    wads[i] = lvWads.Groups[path].Items[i].Text;
                                }
                            }
                        }

                        int failed = 0;
                        int counter = 0;
                        int changed = 0;

                        foreach (string thiswad in wads)
                        {
                            pbProgress.Value = (counter * 100) / wads.Length;
                            counter++;

                            try
                            {
                                if (thiswad.Remove(0, thiswad.LastIndexOf('.')) == ".wad")
                                {
                                    if (ChangeRegion(path + "\\" + thiswad, 'f') == false)
                                    {
                                        failed++;
                                    }
                                    else
                                    {
                                        changed++;
                                    }
                                }
                            }
                            catch { }
                        }

                        if (changed > 0)
                        {
                            btnRefresh_Click(null, null);
                        }

                        if (failed > 0)
                        {
                            MessageBox.Show(Messages[66], Messages[65], MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        pbProgress.Value = 100;
                    }
                }
                else
                {
                    MessageBox.Show(Messages[69], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnShowPath_Click(object sender, EventArgs e)
        {
            if (btnShowPath.Checked == true)
            {
                showpath = "true";
                btnRefresh_Click(null, null);
            }
            else
            {
                showpath = "false";
                btnRefresh_Click(null, null);
            }
        }

        /// <summary>
        /// Exports Listview to File
        /// </summary>
        private void ExportList()
        {
            SaveFileDialog savethis = new SaveFileDialog();
            savethis.Filter = "*.txt|*.txt|*.csv|*.csv";
            savethis.InitialDirectory = Application.StartupPath;

            if (savethis.ShowDialog() == DialogResult.OK)
            {
                StreamWriter write = new StreamWriter(savethis.FileName);

                write.WriteLine("Filename, Type, Channel Title, Title ID, Version, Blocks, Filesize, IOS Flag, Region Flag, Content, Path");
                write.WriteLine("");

                if (lvWads.Visible == true)
                {
                    for (int i = 0; i < lvWads.Groups.Count; i++)
                    {
                        write.WriteLine(lvWads.Groups[i].Tag.ToString());

                        for (int j = 0; j < lvWads.Groups[i].Items.Count; j++)
                        {
                            write.WriteLine("     " + lvWads.Groups[i].Items[j].Text + ", " +
                                lvWads.Groups[i].Items[j].SubItems[8].Text + ", " +
                                lvWads.Groups[i].Items[j].SubItems[10].Text + ", " +
                                lvWads.Groups[i].Items[j].SubItems[1].Text + ", " +
                                lvWads.Groups[i].Items[j].SubItems[9].Text + ", " +
                                lvWads.Groups[i].Items[j].SubItems[2].Text + ", " +
                                lvWads.Groups[i].Items[j].SubItems[3].Text + ", " +
                                lvWads.Groups[i].Items[j].SubItems[4].Text + ", " +
                                lvWads.Groups[i].Items[j].SubItems[5].Text + ", " +
                                lvWads.Groups[i].Items[j].SubItems[6].Text + ", " +
                                lvWads.Groups[i].Items[j].SubItems[7].Text);
                        }

                        if (i != lvWads.Groups.Count - 1)
                        {
                            write.WriteLine("");
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < lvNand.Items.Count; j++)
                    {
                        write.WriteLine("     " + lvNand.Items[j].Text + ", " +
                            lvNand.Items[j].SubItems[8].Text + ", " +
                            lvNand.Items[j].SubItems[10].Text + ", " +
                            lvNand.Items[j].SubItems[1].Text + ", " +
                            lvNand.Items[j].SubItems[9].Text + ", " +
                            lvNand.Items[j].SubItems[2].Text + ", " +
                            lvNand.Items[j].SubItems[3].Text + ", " +
                            lvNand.Items[j].SubItems[4].Text + ", " +
                            lvNand.Items[j].SubItems[5].Text + ", " +
                            lvNand.Items[j].SubItems[6].Text + ", " +
                            lvNand.Items[j].SubItems[7].Text);
                    }
                }

                write.Close();
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportList();
        }

        private void cmChannelName_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                if (!lvWads.SelectedItems[0].SubItems[8].Text.Contains("System"))
                {
                    if (File.Exists(ckey) || File.Exists(key))
                    {
                        string wadfile = lvWads.SelectedItems[0].Group.Tag.ToString() + "\\" + lvWads.SelectedItems[0].Text;
                        string[] oldtitles = Wii.WadInfo.GetChannelTitles(lvWads.SelectedItems[0].Group.Tag.ToString() + "\\" + lvWads.SelectedItems[0].Text);
                        
                        if (oldtitles[1].Length != 0)
                        {
                            string[] oldvalues = new string[7] { oldtitles[0], oldtitles[1], oldtitles[2], oldtitles[3], oldtitles[4], oldtitles[5], oldtitles[6] };
                            ChannelNameDialog cld = new ChannelNameDialog();
                            cld.FormCaption = Messages[73];
                            cld.btnCancelText = Messages[27];
                            cld.Titles = oldtitles;
                            cld.ShowDialog();

                            if (cld.DialogResult == DialogResult.OK)
                            {
                                string[] newtitles = cld.Titles;
                                bool[] samesame = new bool[7] { true, true, true, true, true, true, true };

                                for (int z = 0; z < 7; z++)
                                {
                                    if (oldvalues[z] != newtitles[z])
                                    {
                                        samesame[z] = false;
                                    }
                                }

                                if (samesame[0] != true || samesame[1] != true || samesame[2] != true || samesame[3] != true || samesame[4] != true || samesame[5] != true || samesame[6] != true)
                                {
                                    Cursor.Current = Cursors.WaitCursor;
                                    CreateBackup(wadfile);

                                    Wii.WadEdit.ChangeChannelTitle(wadfile, newtitles[0], newtitles[1], newtitles[2], newtitles[3], newtitles[4], newtitles[5], newtitles[6]);
                                    btnRefresh_Click(null, null);

                                    Cursor.Current = Cursors.Default;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(Messages[75], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show(Messages[52], Messages[53], MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show(Messages[62], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                pbProgress.Value = 100;
            }
        }

        private void msMain_MouseEnter(object sender, EventArgs e)
        {
            tsMru.DropDownItems.Clear();

            for (int i = 0; i < 5; i++)
            {
                if (mru[i] != string.Empty)
                {
                    tsMru.DropDownItems.Add(new ToolStripMenuItem(mru[i], null, mruItems_Click));
                }
                else
                {
                    break;
                }
            }

            if (lvWads.Visible == true)
            {
                btnCopy.Enabled = true;
                btnCut.Enabled = true;
                btnPaste.Enabled = true;
                btnRename.Enabled = true;
                btnOpen.Enabled = true;
                btnExport.Enabled = true;
                tsMru.Enabled = true;

                if (accepted == "true") EditFeatures(true);
                else EditFeatures(false);
                
            }
            else
            {
                btnCopy.Enabled = false;
                btnCut.Enabled = false;
                btnPaste.Enabled = false;
                btnRename.Enabled = false;
                btnOpen.Enabled = false;
                tsMru.Enabled = false;

                if (accepted == "true") EditFeatures(true);
                else EditFeatures(false);
            }
        }

        private void mruItems_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem senderitem = sender as ToolStripMenuItem;

            bool exists = false;

            for (int i = 0; i < lvWads.Groups.Count; i++)
            {
                if (lvWads.Groups[i].Tag.ToString() == senderitem.Text)
                {
                    exists = true;
                }
            }

            if (exists == false)
            {
                NewMRU(senderitem.Text);

                if (addsub != "true") { AddWads(senderitem.Text); }
                else { AddWadsSub(senderitem.Text); }
            }
        }

        /// <summary>
        /// Checks and Adds MRU Path to the right place
        /// </summary>
        /// <param name="path">MRU Path</param>
        private void NewMRU(string path)
        {
            int index = 4;
            for (int i = 0; i < 5; i++)
            {
                if (mru[i] == path) { index = i; }
            }

            for (int z = index; z > 0; z--)
            {
                mru[z] = mru[z - 1];
            }
            mru[0] = path;            
        }

        private void lvWads_VisibleChanged(object sender, EventArgs e)
        {
            if (lvWads.Visible == false)
            { lvWads.Enabled = false; }
            else
            {
                lvWads.Enabled = true;
                btnRefresh_Click(null, null);
            }
        
        }

        private void lvNand_VisibleChanged(object sender, EventArgs e)
        {
            if (lvNand.Visible == false)
            { lvNand.Enabled = false; }
            else
            {
                lvNand.Enabled = true;
                btnRefresh_Click(null, null);
            }
        }

        private void btnShowMiiWads_Click(object sender, EventArgs e)
        {
            if (btnShowMiiWads.Checked == true)
            {
                btnShowMiiNand.Checked = false;
                lvWads.Visible = true;
                lvNand.Visible = false;

                lbFiles.Text = Messages[16] + ":";
                lbFolders.Text = Messages[37] + ":";
                lbFolderCount.Text = "0";
                lbFileCount.Text = "0";

                lvQueue.Items.Clear();
                lbQueue.Visible = false;
                lbQueueCount.Visible = false;
                lbQueueCount.Text = "0";
                lbQueueDiscard.Visible = false;
                lbQueueInstall.Visible = false;

                this.Text = "ShowMiiWads " + version + " by Leathl";
            }
            else { btnShowMiiWads.Checked = true; }
        }

        private void btnShowMiiNand_Click(object sender, EventArgs e)
        {
            if (btnShowMiiNand.Checked == true)
            {
                if (nandpath != "")
                {
                    btnShowMiiWads.Checked = false;
                    lvWads.Visible = false;
                    lvNand.Visible = true;

                    lbFiles.Text = Messages[86] + ":";
                    lbFolders.Text = "";
                    lbFolderCount.Text = "";
                    lbFileCount.Text = "0";
                    
                    this.Text = "ShowMiiNand " + version + " by Leathl";
                }
                else
                {
                    btnShowMiiNand.Checked = false;
                    MessageBox.Show(Messages[57], Messages[58], MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else { btnShowMiiNand.Checked = true; }
        }

        private void lvNand_Resize(object sender, EventArgs e)
        {
            if (autosize == "true")
            {
                lvNand.Columns[0].Width = Convert.ToInt32(lvNand.Width * ((double)145 / 914)); //Filename
                lvNand.Columns[1].Width = Convert.ToInt32(lvNand.Width * ((double)56 / 914)); //Title ID
                lvNand.Columns[2].Width = Convert.ToInt32(lvNand.Width * ((double)60 / 914)); //Blocks
                lvNand.Columns[3].Width = Convert.ToInt32(lvNand.Width * ((double)82 / 914)); //Filesize
                lvNand.Columns[4].Width = Convert.ToInt32(lvNand.Width * ((double)53 / 914)); //IOS Flag
                lvNand.Columns[5].Width = Convert.ToInt32(lvNand.Width * ((double)70 / 914)); //Region Flag
                lvNand.Columns[6].Width = Convert.ToInt32(lvNand.Width * ((double)49 / 914)); //Content
                lvNand.Columns[8].Width = Convert.ToInt32(lvNand.Width * ((double)78 / 914)); //Type
                lvNand.Columns[9].Width = Convert.ToInt32(lvNand.Width * ((double)47 / 914)); //Version
                lvNand.Columns[10].Width = Convert.ToInt32(lvNand.Width * ((double)148 / 914)); //Channel Title
                lvNand.Columns[7].Width = Convert.ToInt32(lvNand.Width -                //Path
                    lvNand.Columns[0].Width -
                    lvNand.Columns[1].Width -
                    lvNand.Columns[2].Width -
                    lvNand.Columns[3].Width -
                    lvNand.Columns[4].Width -
                    lvNand.Columns[5].Width -
                    lvNand.Columns[6].Width -
                    lvNand.Columns[8].Width -
                    lvNand.Columns[9].Width -
                    lvNand.Columns[10].Width);
            }
        }

        private void lvNand_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvSorter.SortColumn)
            {
                if (lvSorter.Order == SortOrder.Ascending)
                {
                    lvSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                lvSorter.Order = SortOrder.Ascending;
            }

            lvSorter.Column = e.Column;
            lvSorter.SortColumn = e.Column;
            lvNand.Sort();
        }

        private void lvNand_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (lvNand.SelectedItems.Count != 0)
                {
                    cmNand.Show(lvNand, e.Location);
                }
            }
        }

        private void btnPackWad_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = Messages[74];

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string packpath = fbd.SelectedPath;

                if (Directory.Exists(packpath) && 
                    Directory.GetFiles(packpath, "*.app").Length > 1 &&
                    Directory.GetFiles(packpath, "*.cert").Length >= 1 &&
                    Directory.GetFiles(packpath, "*.tik").Length >= 1 &&
                    Directory.GetFiles(packpath, "*.tmd").Length >= 1)
                {
                    try
                    {
                        byte[] tmd = Wii.Tools.LoadFileToByteArray(Directory.GetFiles(packpath, "*.tmd")[0]);
                        byte[] app = Wii.Tools.LoadFileToByteArray(packpath + "\\00000000.app");

                        string channelname = "";
                        switch (language)
                        {
                            case "Dutch":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[6] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            case "Italian":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[5] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            case "Spanish":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[4] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            case "French":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[3] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            case "German":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[2] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            default:
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[1] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            case "Japanese":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[0] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                        }

                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "Wad Files (*.wad)|*.wad";
                        sfd.Title = Messages[54];
                        sfd.FileName = channelname + ".wad";
                        sfd.InitialDirectory = Application.StartupPath;

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            Cursor.Current = Cursors.WaitCursor;

                            Wii.WadPack.PackWad(packpath, sfd.FileName, true);

                            Cursor.Current = Cursors.Default;
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
                else
                {
                    MessageBox.Show(Messages[82], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void EditFeatures(bool enordisable)
        {
            tsChangeRegion.Enabled = enordisable;
            btnChangeID.Enabled = enordisable;
            cmChangeID.Enabled = enordisable;
            cmChangeRegion.Enabled = enordisable;
            tsExtract.Enabled = enordisable;
            cmExtract.Enabled = enordisable;
            btnChannelName.Enabled = enordisable;
            cmChannelName.Enabled = enordisable;
            tsTools.Enabled = enordisable;
            cmNandPackWad.Enabled = enordisable;
            cmInstall.Enabled = enordisable;
        }

        private void btnPackWadWithoutTrailer_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = Messages[74];

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string packpath = fbd.SelectedPath;

                if (Directory.Exists(packpath) &&
                    Directory.GetFiles(packpath, "*.app").Length > 1 &&
                    Directory.GetFiles(packpath, "*.cert").Length >= 1 &&
                    Directory.GetFiles(packpath, "*.tik").Length >= 1 &&
                    Directory.GetFiles(packpath, "*.tmd").Length >= 1)
                {
                    try
                    {
                        byte[] tmd = Wii.Tools.LoadFileToByteArray(Directory.GetFiles(packpath, "*.tmd")[0]);
                        byte[] app = Wii.Tools.LoadFileToByteArray(packpath + "\\00000000.app");

                        string channelname = "";
                        switch (language)
                        {
                            case "Dutch":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[6] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            case "Italian":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[5] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            case "Spanish":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[4] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            case "French":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[3] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            case "German":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[2] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            default:
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[1] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                            case "Japanese":
                                channelname = Wii.WadInfo.GetChannelTitlesFromApp(app)[0] + " - " + Wii.WadInfo.GetTitleID(tmd, 1);
                                break;
                        }

                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "Wad Files (*.wad)|*.wad";
                        sfd.Title = Messages[54];
                        sfd.FileName = channelname + ".wad";
                        sfd.InitialDirectory = Application.StartupPath;

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            Cursor.Current = Cursors.WaitCursor;

                            Wii.WadPack.PackWad(packpath, sfd.FileName, false);

                            Cursor.Current = Cursors.Default;
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
                else
                {
                    MessageBox.Show(Messages[82], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lvNand_DragDrop(object sender, DragEventArgs e)
        {
            string[] drop = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string thisdrop in drop)
            {
                if (File.Exists(thisdrop))
                {
                    bool exists = false;

                    for (int i = 0; i < lvQueue.Items.Count; i++)
                    {
                        if ((string)lvQueue.Items[i] == thisdrop) exists = true;
                    }

                    if (exists == false) lvQueue.Items.Add(thisdrop);
                }
                else if (Directory.Exists(thisdrop))
                {
                    string[] wadfiles = Directory.GetFiles(thisdrop, "*.wad");

                    for (int j = 0; j < wadfiles.Length; j++)
                    {
                        bool exists = false;

                        for (int z = 0; z < lvQueue.Items.Count; z++)
                        {
                            if ((string)lvQueue.Items[z] == wadfiles[j]) exists = true;
                        }

                        if (exists == false) lvQueue.Items.Add(wadfiles[j]);
                    }
                }
            }

            if (lvQueue.Items.Count > 0)
            {
                if (lbQueue.Visible == false) lbQueue.Visible = true;
                if (lbQueueCount.Visible == false) lbQueueCount.Visible = true;
                if (lbQueueDiscard.Visible == false) lbQueueDiscard.Visible = true;
                if (lbQueueInstall.Visible == false) lbQueueInstall.Visible = true;
            }

            lbQueueCount.Text = lvQueue.Items.Count.ToString();
        }

        private void lvNand_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drop = (string[])e.Data.GetData(DataFormats.FileDrop);
                bool right = true;

                for (int i = 0; i < drop.Length; i++)
                {
                    if (!File.Exists(drop[i]) && !Directory.Exists(drop[i]))
                    {
                        right = false;
                    }
                    else if (File.Exists(drop[i]) && drop[i].Remove(0, drop[i].LastIndexOf('.')) != ".wad")
                    {
                        right = false;
                    }
                }

                if (right == true && accepted == "true")
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            } 
        }

        private void lbQueueDiscard_Click(object sender, EventArgs e)
        {
            lvQueue.Items.Clear();
            lbQueue.Visible = false;
            lbQueueCount.Visible = false;
            lbQueueCount.Text = "0";
            lbQueueDiscard.Visible = false;
            lbQueueInstall.Visible = false;
        }

        private void lbQueueInstall_Click(object sender, EventArgs e)
        {
            pbProgress.Tag = "NoProgress";
            Cursor.Current = Cursors.WaitCursor;

            for (int i = 0; i < lvQueue.Items.Count; i++)
            {
                pbProgress.Value = (i + 1) * 100 / lvQueue.Items.Count;

                if (File.Exists((string)lvQueue.Items[i]))
                {
                    CopyToNand((string)lvQueue.Items[i]);
                }
            }

            Cursor.Current = Cursors.Default;
            pbProgress.Tag = "";
            pbProgress.Value = 100;

            lbQueueCount.Text = "0";
            lbQueueCount.Visible = false;
            lbQueue.Visible = false;
            lbQueueInstall.Visible = false;
            lbQueueDiscard.Visible = false;

            btnRefresh_Click(null, null);
        }

        private void cmInstallWad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Wad (*.wad)|*.wad";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                CopyToNand(ofd.FileName);
                Cursor.Current = Cursors.Default;
                btnRefresh_Click(null, null);
            }
        }

        private void cmInstallFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = Messages[92];

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string[] wads = Directory.GetFiles(fbd.SelectedPath, "*.wad");
                pbProgress.Tag = "NoProgress";
                Cursor.Current = Cursors.WaitCursor;

                for (int i = 0; i < wads.Length; i++)
                {
                    pbProgress.Value = (i + 1) * 100 / wads.Length;

                    CopyToNand(wads[i]);
                }

                pbProgress.Tag = "";
                Cursor.Current = Cursors.Default;
                btnRefresh_Click(null, null);
            }
        }

        private void btnAddSub_Click(object sender, EventArgs e)
        {
            if (btnAddSub.Checked == true)
            {
                addsub = "true";
            }
            else
            {
                addsub = "false";
            }

            SaveSettings();
        }

        private void btnPackToWad_Click(object sender, EventArgs e)
        {
            if (lvNand.SelectedItems.Count == 1)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Wad|*.wad";

                switch (lvNand.SelectedItems[0].SubItems[8].Text)
                {
                    case "System: IOS":
                        sfd.FileName = lvNand.SelectedItems[0].SubItems[1].Text + "-v" + lvNand.SelectedItems[0].SubItems[9].Text;
                        break;
                    case "System: Boot2":
                        sfd.FileName = "Boot2-v" + lvNand.SelectedItems[0].SubItems[9].Text;
                        break;
                    case "System: MIOS":
                        sfd.FileName = "MIOS-v" + lvNand.SelectedItems[0].SubItems[9].Text;
                        break;
                    case "System: BC":
                        sfd.FileName = "BC-v" + lvNand.SelectedItems[0].SubItems[9].Text;
                        break;
                    case "Hidden Channel":
                        sfd.FileName = "Hidden Channel - " + lvNand.SelectedItems[0].SubItems[1].Text;
                        break;
                    default:
                        sfd.FileName = lvNand.SelectedItems[0].SubItems[10].Text + " - " + lvNand.SelectedItems[0].SubItems[1].Text;
                        break;
                }

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    string path = lvNand.SelectedItems[0].SubItems[7].Text;

                    try { Wii.WadPack.PackWadFromNand(nandpath, path, sfd.FileName); }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }

                    pbProgress.Value = 100;
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void cmPreview_Click(object sender, EventArgs e)
        {
            if (lvWads.Visible == true)
            {
                if (lvWads.SelectedItems.Count == 1)
                {
                    if (lvWads.SelectedItems[0].SubItems[8].Text.Contains("Channel"))
                    {
                        if (!lvWads.SelectedItems[0].SubItems[8].Text.Contains("Hidden"))
                        {
                            try
                            {
                                if (Directory.Exists(ImageTempPath)) Directory.Delete(ImageTempPath, true);

                                FileStream fs = new FileStream(lvWads.SelectedItems[0].Group.Tag.ToString() + "\\" + lvWads.SelectedItems[0].Text, FileMode.Open);
                                byte[] wadfile = new byte[fs.Length];
                                fs.Read(wadfile, 0, wadfile.Length);
                                fs.Close();

                                pbProgress.Value = 20;

                                byte[] nullapp = Wii.WadUnpack.UnpackNullApp(wadfile);
                                byte[] bannerbin = Wii.U8Unpack.GetBannerBin(nullapp);
                                byte[] iconbin = Wii.U8Unpack.GetIconBin(nullapp);

                                pbProgress.Value = 40;

                                Wii.U8Unpack.UnpackTpls(bannerbin, ImageTempPath + "banner");
                                Wii.U8Unpack.UnpackTpls(iconbin, ImageTempPath + "icon");

                                pbProgress.Value = 60;

                                string[] bannertpls = Directory.GetFiles(ImageTempPath + "banner", "*.tpl");
                                string[] icontpls = Directory.GetFiles(ImageTempPath + "icon", "*.tpl");

                                foreach (string thistpl in bannertpls)
                                {
                                    byte[] tpl = Wii.Tools.LoadFileToByteArray(thistpl);
                                    Bitmap tplbmp = Wii.TPL.ConvertTPL(tpl);
                                    tplbmp.Save(thistpl.Remove(thistpl.LastIndexOf('.')) + ".png");
                                }

                                pbProgress.Value = 80;

                                foreach (string thistpl in icontpls)
                                {
                                    byte[] tpl = Wii.Tools.LoadFileToByteArray(thistpl);
                                    Bitmap tplbmp = Wii.TPL.ConvertTPL(tpl);
                                    tplbmp.Save(thistpl.Remove(thistpl.LastIndexOf('.')) + ".png");
                                }

                                pbProgress.Value = 100;

                                Preview pvw = new Preview();
                                pvw.lbFormatText.Text = Messages[35] + ":";
                                pvw.lbSizeText.Text = Messages[36] + ":";
                                pvw.btnClose.Text = Messages[102];
                                pvw.btnSave.Text = Messages[59];
                                pvw.Text = Messages[99];
                                pvw.cmSave.Text = Messages[59];
                                pvw.cmSaveAll.Text = Messages[103];
                                pvw.cmBannerImages.Text = Messages[104];
                                pvw.cmIconImages.Text = Messages[105];
                                pvw.cmBothImages.Text = Messages[106];
                                pvw.ShowDialog();
                            }
                            catch (Exception ex) { MessageBox.Show(ex.Message); }
                        }
                        else
                        {
                            MessageBox.Show(Messages[107], Messages[65], MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show(Messages[95], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                if (lvNand.SelectedItems.Count == 1)
                {
                    if (lvNand.SelectedItems[0].SubItems[8].Text.Contains("Channel"))
                    {
                        try
                        {
                            byte[] tmd = Wii.Tools.LoadFileToByteArray(nandpath + "\\title\\" + lvNand.SelectedItems[0].SubItems[7].Text + "\\content\\title.tmd");
                            string nullappname = "";
                            string[,] contents = Wii.WadInfo.GetContentInfo(tmd);

                            for (int i = 0; i < contents.GetLength(0); i++)
                            {
                                if (contents[i, 1] == "00000000")
                                {
                                    nullappname = contents[i, 0];
                                    break;
                                }
                            }

                            pbProgress.Value = 20;

                            byte[] nullapp = Wii.Tools.LoadFileToByteArray(nandpath + "\\title\\" + lvNand.SelectedItems[0].SubItems[7].Text + "\\content\\" + nullappname + ".app");
                            byte[] bannerbin = Wii.U8Unpack.GetBannerBin(nullapp);
                            byte[] iconbin = Wii.U8Unpack.GetIconBin(nullapp);

                            pbProgress.Value = 40;

                            Wii.U8Unpack.UnpackTpls(bannerbin, ImageTempPath + "banner");
                            Wii.U8Unpack.UnpackTpls(iconbin, ImageTempPath + "icon");

                            pbProgress.Value = 60;

                            string[] bannertpls = Directory.GetFiles(ImageTempPath + "banner", "*.tpl");
                            string[] icontpls = Directory.GetFiles(ImageTempPath + "icon", "*.tpl");

                            foreach (string thistpl in bannertpls)
                            {
                                byte[] tpl = Wii.Tools.LoadFileToByteArray(thistpl);
                                Bitmap tplbmp = Wii.TPL.ConvertTPL(tpl);
                                tplbmp.Save(thistpl.Remove(thistpl.LastIndexOf('.')) + ".png");
                            }

                            pbProgress.Value = 80;

                            foreach (string thistpl in icontpls)
                            {
                                byte[] tpl = Wii.Tools.LoadFileToByteArray(thistpl);
                                Bitmap tplbmp = Wii.TPL.ConvertTPL(tpl);
                                tplbmp.Save(thistpl.Remove(thistpl.LastIndexOf('.')) + ".png");
                            }

                            pbProgress.Value = 100;

                            Preview pvw = new Preview();
                            pvw.lbFormatText.Text = Messages[35] + ":";
                            pvw.lbSizeText.Text = Messages[36] + ":";
                            pvw.btnClose.Text = Messages[102];
                            pvw.btnSave.Text = Messages[59];
                            pvw.Text = Messages[99];
                            pvw.cmSave.Text = Messages[59];
                            pvw.cmSaveAll.Text = Messages[103];
                            pvw.cmBannerImages.Text = Messages[104];
                            pvw.cmIconImages.Text = Messages[105];
                            pvw.cmBothImages.Text = Messages[106];
                            pvw.ShowDialog();
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                    }
                    else
                    {
                        MessageBox.Show(Messages[95], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnUnpackU8_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "U8|*.bin; *.app; *.bnr; *.u8; *.arc|*.*|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (Wii.U8Unpack.CheckU8(ofd.FileName) == true)
                {
                    try
                    {
                        string outfolder = ofd.FileName.Remove(0, ofd.FileName.LastIndexOf('\\') + 1).Replace('.', '_') + "_OUT";

                        FolderBrowserDialog fbd = new FolderBrowserDialog();
                        fbd.Description = string.Format(Messages[39], outfolder);
                        fbd.SelectedPath = ofd.FileName.Remove(ofd.FileName.LastIndexOf('\\'));

                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            byte[] file = Wii.Tools.LoadFileToByteArray(ofd.FileName);
                            Wii.U8Unpack.UnpackU8(file, fbd.SelectedPath + "\\" + outfolder);
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
                else
                {
                    MessageBox.Show(Messages[96], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnConvertTpl_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "TPL|*.tpl";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] tpl = Wii.Tools.LoadFileToByteArray(ofd.FileName);

                    if (Wii.TPL.GetTextureCount(tpl) > 1)
                    {
                        MessageBox.Show(Messages[97], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (Wii.TPL.GetTextureFormat(tpl) == -1)
                    {
                        MessageBox.Show(Messages[98], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.InitialDirectory = ofd.FileName.Remove(ofd.FileName.LastIndexOf('\\'));
                    sfd.Filter = "PNG|*.png";
                    sfd.FileName = ofd.FileName.Remove(ofd.FileName.LastIndexOf('.')).Remove(0, ofd.FileName.LastIndexOf('\\') + 1);

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = Wii.TPL.ConvertTPL(tpl);
                        bmp.Save(sfd.FileName);
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void btnCreateKey_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Application.StartupPath + "\\common-key.bin") && !File.Exists(Application.StartupPath + "\\key.bin"))
            {
                InputBoxDialog ipb = new InputBoxDialog();
                ipb.FormPrompt = string.Format(Messages[108], "45e", "common-key.bin");
                ipb.FormCaption = Messages[110];
                ipb.MaxLength = 3;
                ipb.btnCancel.Text = Messages[27];

                if (ipb.ShowDialog() == DialogResult.OK)
                {
                    if (ipb.InputResponse == "45e")
                    {
                        Wii.Tools.CreateCommonKey(ipb.InputResponse, Application.StartupPath);
                    }
                    else
                    {
                        MessageBox.Show(string.Format(Messages[109], "45e"), Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                if (File.Exists(Application.StartupPath + "\\common-key.bin"))
                    MessageBox.Show(string.Format(Messages[111], "common-key.bin"), Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (File.Exists(Application.StartupPath + "\\key.bin"))
                    MessageBox.Show(string.Format(Messages[111], "key.bin"), Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdateCheck_Click(object sender, EventArgs e)
        {
            UpdateCheck(false);
        }

        private void UpdateCheck(bool silentifno)
        {
            if (!version.Contains("Beta"))
            {
                try
                {
                    WebClient GetVersion = new WebClient();
                    string newversion = GetVersion.DownloadString("http://showmiiwads.googlecode.com/svn/version.txt");
                    newversion.Replace(" ", "");

                    if (newversion == version)
                    {
                        if (silentifno == false)
                            MessageBox.Show(Messages[112], Messages[65], MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string mes = string.Format(Messages[113], newversion, "http://showmiiwads.googlecode.com");
                        string part1 = mes.Remove(mes.IndexOf('<'));
                        string part2 = mes.Remove(mes.LastIndexOf('<')).Remove(0, mes.IndexOf('>') + 1);
                        string part3 = mes.Remove(0, mes.LastIndexOf('>') + 1);

                        if (MessageBox.Show(part1 + "\r\n" + part2 + "\r\n" + part3, Messages[65], MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                            System.Diagnostics.Process.Start("http://showmiiwads.googlecode.com/");
                    }
                }
                catch (Exception ex)
                {
                    if (silentifno == false)
                        MessageBox.Show(ex.Message, Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CreateBackup(string FileToBackup)
        {
            if (backup == "true")
            {
                if (!File.Exists(FileToBackup + ".backup"))
                    File.Copy(FileToBackup, FileToBackup + ".backup");
            }
        }

        private void btnCreateBackups_Click(object sender, EventArgs e)
        {
            if (btnCreateBackups.Checked == true)
            {
                backup = "true";
            }
            else
            {
                backup = "false";
            }
        }

        private void cmRestore_Click(object sender, EventArgs e)
        {
            if (lvWads.SelectedItems.Count == 1)
            {
                string wadfile = lvWads.SelectedItems[0].Group.Tag.ToString() + "\\" + lvWads.SelectedItems[0].Text;

                if (File.Exists(wadfile + ".backup"))
                {
                    File.Delete(wadfile);
                    File.Move(wadfile + ".backup", wadfile);
                    btnRefresh_Click(null, null);
                    MessageBox.Show(Messages[115], Messages[65], MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(Messages[116], Messages[19], MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
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
using System.Windows.Forms;
using System.IO;

namespace ShowMiiWads
{
    public partial class Preview : Form
    {
        public Preview()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Preview_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Directory.Exists(Main.ImageTempPath)) Directory.Delete(Main.ImageTempPath, true);
            cbBanner.Items.Clear();
            cbIcon.Items.Clear();
        }

        private void Preview_Load(object sender, EventArgs e)
        {
            this.CenterToParent();
            string[] bannerpics = Directory.GetFiles(Main.ImageTempPath + "banner", "*.png");
            string[] iconpics = Directory.GetFiles(Main.ImageTempPath + "icon", "*.png");

            foreach (string thispic in bannerpics)
            {
                string picname = thispic.Remove(0, thispic.LastIndexOf('\\') + 1);
                picname = picname.Remove(picname.LastIndexOf('.'));
                cbBanner.Items.Add((object)picname);
            }

            foreach (string thispic in iconpics)
            {
                string picname = thispic.Remove(0, thispic.LastIndexOf('\\') + 1);
                picname = picname.Remove(picname.LastIndexOf('.'));
                cbIcon.Items.Add((object)picname);
            }

            try { cbBanner.SelectedIndex = 0; } catch { try { cbIcon.SelectedIndex = 0; } catch { } }
        }

        private void cbBanner_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBanner.SelectedIndex != -1)
            {
                pbPic.ImageLocation = Main.ImageTempPath + "banner\\" + cbBanner.SelectedItem.ToString() + ".png";
                
                byte[] tpl = Wii.Tools.LoadFileToByteArray(Main.ImageTempPath + "banner\\" + cbBanner.SelectedItem.ToString() + ".tpl");
                lbSize.Text = Wii.TPL.GetTextureWidth(tpl).ToString() + " x " + Wii.TPL.GetTextureHeight(tpl).ToString();
                lbFormat.Text = Wii.TPL.GetTextureFormatName(tpl);

                cbIcon.SelectedIndex = -1;
            }
        }

        private void cbIcon_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbIcon.SelectedIndex != -1)
            {
                pbPic.ImageLocation = Main.ImageTempPath + "icon\\" + cbIcon.SelectedItem.ToString() + ".png";

                byte[] tpl = Wii.Tools.LoadFileToByteArray(Main.ImageTempPath + "icon\\" + cbIcon.SelectedItem.ToString() + ".tpl");
                lbSize.Text = Wii.TPL.GetTextureWidth(tpl).ToString() + " x " + Wii.TPL.GetTextureHeight(tpl).ToString();
                lbFormat.Text = Wii.TPL.GetTextureFormatName(tpl);

                cbBanner.SelectedIndex = -1;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            cmPic.Show(MousePosition);
        }

        private void cmSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = pbPic.ImageLocation.Remove(0, pbPic.ImageLocation.LastIndexOf('\\') + 1);
            sfd.Filter = "PNG|*.png";
            if (sfd.ShowDialog() == DialogResult.OK)
                File.Copy(pbPic.ImageLocation, sfd.FileName);
        }

        private void btnBannerImages_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < cbBanner.Items.Count; i++)
                {
                    string filename = cbBanner.Items[i].ToString();
                    File.Copy(Main.ImageTempPath + "\\banner\\" + filename + ".png", fbd.SelectedPath + "\\" + filename + ".png");
                }
            }
        }

        private void btnIconImages_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < cbIcon.Items.Count; i++)
                {
                    string filename = cbIcon.Items[i].ToString();
                    File.Copy(Main.ImageTempPath + "\\icon\\" + filename + ".png", fbd.SelectedPath + "\\" + filename + ".png");
                }
            }
        }

        private void btnBothImages_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < cbBanner.Items.Count; i++)
                {
                    string filename = cbBanner.Items[i].ToString();
                    File.Copy(Main.ImageTempPath + "\\banner\\" + filename + ".png", fbd.SelectedPath + "\\Banner_" + filename + ".png");
                }
                for (int i = 0; i < cbIcon.Items.Count; i++)
                {
                    string filename = cbIcon.Items[i].ToString();
                    File.Copy(Main.ImageTempPath + "\\icon\\" + filename + ".png", fbd.SelectedPath + "\\Icon_" + filename + ".png");
                }
            }
        }
    }
}

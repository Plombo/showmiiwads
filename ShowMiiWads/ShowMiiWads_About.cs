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

using System.Windows.Forms;

namespace ShowMiiWads
{
    public partial class ShowMiiWads_About : Form
    {
        public bool x64 = false;

        public ShowMiiWads_About()
        {
            InitializeComponent();
            this.Icon = global::ShowMiiWads.Properties.Resources.ShowMiiWads_Icon;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://showmiiwads.googlecode.com/");
            linkLabel1.LinkVisited = true;
        }

        private void lbDonate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=8731272");
            lbDonate.LinkVisited = true;
        }

        private void About_Load(object sender, System.EventArgs e)
        {
            if (x64 == true) lbPlatform.Text = "You're running the 64 bit Version";
        }
    }
}

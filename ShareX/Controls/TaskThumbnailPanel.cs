﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2019 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using ShareX.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ShareX
{
    public partial class TaskThumbnailPanel : UserControl
    {
        public new event EventHandler MouseEnter
        {
            add
            {
                base.MouseEnter += value;
                lblTitle.MouseEnter += value;
                pThumbnail.MouseEnter += value;
                pbThumbnail.MouseEnter += value;
                pbProgress.MouseEnter += value;
            }
            remove
            {
                base.MouseEnter -= value;
                lblTitle.MouseEnter -= value;
                pThumbnail.MouseEnter -= value;
                pbThumbnail.MouseEnter -= value;
                pbProgress.MouseEnter -= value;
            }
        }

        public new event MouseEventHandler MouseDown
        {
            add
            {
                base.MouseDown += value;
                lblTitle.MouseDown += value;
                pThumbnail.MouseDown += value;
                pbThumbnail.MouseDown += value;
                pbProgress.MouseDown += value;
            }
            remove
            {
                base.MouseDown -= value;
                lblTitle.MouseDown -= value;
                pThumbnail.MouseDown -= value;
                pbThumbnail.MouseDown -= value;
                pbProgress.MouseDown -= value;
            }
        }

        public new event MouseEventHandler MouseUp
        {
            add
            {
                base.MouseUp += value;
                lblTitle.MouseUp += value;
                pThumbnail.MouseUp += value;
                pbThumbnail.MouseUp += value;
                pbProgress.MouseUp += value;
            }
            remove
            {
                base.MouseUp -= value;
                lblTitle.MouseUp -= value;
                pThumbnail.MouseUp -= value;
                pbThumbnail.MouseUp -= value;
                pbProgress.MouseUp -= value;
            }
        }

        public WorkerTask Task { get; private set; }

        private bool selected;

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                if (selected != value)
                {
                    selected = value;

                    pThumbnail.Selected = selected;
                }
            }
        }

        private string title;

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;

                if (lblTitle.Text != title)
                {
                    lblTitle.Text = title;
                }
            }
        }

        private bool titleVisible = true;

        public bool TitleVisible
        {
            get
            {
                return titleVisible;
            }
            set
            {
                if (titleVisible != value)
                {
                    titleVisible = value;
                    lblTitle.Visible = titleVisible;
                    UpdateLayout();
                }
            }
        }

        private ThumbnailTitleLocation titleLocation;

        public ThumbnailTitleLocation TitleLocation
        {
            get
            {
                return titleLocation;
            }
            set
            {
                if (titleLocation != value)
                {
                    titleLocation = value;
                    pThumbnail.StatusLocation = value;
                    UpdateLayout();
                }
            }
        }

        private int progress;

        public int Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;

                if (pbProgress.Value != progress)
                {
                    pbProgress.Value = progress;
                }
            }
        }

        private bool progressVisible;

        public bool ProgressVisible
        {
            get
            {
                return progressVisible;
            }
            set
            {
                progressVisible = value;
                pbProgress.Visible = progressVisible;
            }
        }

        public bool ThumbnailExists { get; private set; }

        private Size thumbnailSize;

        public Size ThumbnailSize
        {
            get
            {
                return thumbnailSize;
            }
            set
            {
                if (thumbnailSize != value)
                {
                    thumbnailSize = value;

                    UpdateLayout();
                }
            }
        }

        public bool ThumbnailSupportsClick { get; private set; }

        private Rectangle dragBoxFromMouseDown;

        public TaskThumbnailPanel(WorkerTask task)
        {
            Task = task;

            InitializeComponent();
            UpdateTheme();
            UpdateTitle();
        }

        public void UpdateTheme()
        {
            if (ShareXResources.UseDarkTheme)
            {
                lblTitle.ForeColor = ShareXResources.Theme.TextColor;
                lblTitle.TextShadowColor = ShareXResources.Theme.DarkBackgroundColor;
                pThumbnail.PanelColor = ShareXResources.Theme.DarkBackgroundColor;
                ttMain.BackColor = ShareXResources.Theme.BackgroundColor;
                ttMain.ForeColor = ShareXResources.Theme.TextColor;
            }
            else
            {
                lblTitle.ForeColor = SystemColors.ControlText;
                lblTitle.TextShadowColor = Color.Transparent;
                pThumbnail.PanelColor = SystemColors.ControlLight;
                ttMain.BackColor = SystemColors.Window;
                ttMain.ForeColor = SystemColors.ControlText;
            }
        }

        public void UpdateTitle()
        {
            Title = Task.Info?.FileName;

            if (Task.Info != null && !string.IsNullOrEmpty(Task.Info.ToString()))
            {
                lblTitle.Cursor = Cursors.Hand;
                ttMain.SetToolTip(lblTitle, Task.Info.ToString());
            }
            else
            {
                lblTitle.Cursor = Cursors.Default;
                ttMain.SetToolTip(lblTitle, null);
            }
        }

        private void UpdateLayout()
        {
            lblTitle.Width = pThumbnail.Padding.Horizontal + ThumbnailSize.Width;
            pThumbnail.Size = new Size(pThumbnail.Padding.Horizontal + ThumbnailSize.Width, pThumbnail.Padding.Vertical + ThumbnailSize.Height);
            int panelHeight = pThumbnail.Height;
            if (TitleVisible)
            {
                panelHeight += lblTitle.Height + 2;
            }
            Size = new Size(pThumbnail.Width, panelHeight);

            if (TitleLocation == ThumbnailTitleLocation.Top)
            {
                lblTitle.Location = new Point(0, 0);

                if (TitleVisible)
                {
                    pThumbnail.Location = new Point(0, lblTitle.Height + 2);
                }
                else
                {
                    pThumbnail.Location = new Point(0, 0);
                }
            }
            else
            {
                pThumbnail.Location = new Point(0, 0);
                lblTitle.Location = new Point(0, pThumbnail.Height + 2);
            }
        }

        public void UpdateThumbnail(Image image = null)
        {
            ClearThumbnail();

            if (!ThumbnailSize.IsEmpty && Task.Info != null)
            {
                try
                {
                    string filePath = Task.Info.FilePath;

                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        ThumbnailSupportsClick = true;
                        pbThumbnail.Cursor = Cursors.Hand;
                    }

                    Image img = CreateThumbnail(filePath, image);

                    if (img != null)
                    {
                        pbThumbnail.Image = img;

                        ThumbnailExists = true;
                    }
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                }
            }
        }

        private Image CreateThumbnail(string filePath, Image image = null)
        {
            if (image != null)
            {
                return ImageHelpers.ResizeImage(image, ThumbnailSize, false);
            }
            else
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = Task.Info.FileName;
                }
                else if (File.Exists(filePath))
                {
                    using (Image img = ImageHelpers.LoadImage(filePath))
                    {
                        if (img != null)
                        {
                            return ImageHelpers.ResizeImage(img, ThumbnailSize, false);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    using (Icon icon = NativeMethods.GetJumboFileIcon(filePath, false))
                    using (Image img = icon.ToBitmap())
                    {
                        return ImageHelpers.ResizeImage(img, ThumbnailSize, false, true);
                    }
                }
            }

            return null;
        }

        public void UpdateProgress()
        {
            if (Task.Info != null)
            {
                Progress = (int)Task.Info.Progress.Percentage;
            }
        }

        public void UpdateStatus()
        {
            if (Task.Info != null)
            {
                pThumbnail.UpdateStatusColor(Task.Status);
            }

            UpdateTitle();
        }

        public void ClearThumbnail()
        {
            Image temp = pbThumbnail.Image;
            pbThumbnail.Image = null;

            if (temp != null && temp != pbThumbnail.ErrorImage && temp != pbThumbnail.InitialImage)
            {
                temp.Dispose();
            }

            ThumbnailSupportsClick = false;
            pbThumbnail.Cursor = Cursors.Default;

            ThumbnailExists = false;
        }

        private void LblTitle_MouseClick(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.None && e.Button == MouseButtons.Left && Task.Info != null)
            {
                if (Task.Info.Result != null)
                {
                    string url = Task.Info.Result.ToString();

                    if (!string.IsNullOrEmpty(url))
                    {
                        URLHelpers.OpenURL(url);
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(Task.Info.FilePath))
                {
                    Helpers.OpenFile(Task.Info.FilePath);
                }
            }
        }

        private void PbThumbnail_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Size dragSize = new Size(10, 10);
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
            }
        }

        private void PbThumbnail_MouseUp(object sender, MouseEventArgs e)
        {
            dragBoxFromMouseDown = Rectangle.Empty;
        }

        private void PbThumbnail_MouseClick(object sender, MouseEventArgs e)
        {
            if (ThumbnailSupportsClick && ModifierKeys == Keys.None && e.Button == MouseButtons.Left && Task.Info != null)
            {
                string filePath = Task.Info.FilePath;

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    if (Helpers.IsImageFile(filePath))
                    {
                        pbThumbnail.Enabled = false;

                        try
                        {
                            ImageViewer.ShowImage(filePath);
                        }
                        finally
                        {
                            pbThumbnail.Enabled = true;
                        }
                    }
                    else if (Helpers.IsTextFile(filePath) || Helpers.IsVideoFile(filePath) || MessageBox.Show("Would you like to open this file?" + "\r\n\r\n" + filePath,
                        Resources.ShareXConfirmation, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Helpers.OpenFile(filePath);
                    }
                }
            }
        }

        private void PbThumbnail_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
            {
                if (Task.Info != null && !string.IsNullOrEmpty(Task.Info.FilePath) && File.Exists(Task.Info.FilePath))
                {
                    Program.MainForm.AllowDrop = false;
                    IDataObject dataObject = new DataObject(DataFormats.FileDrop, new string[] { Task.Info.FilePath });
                    dragBoxFromMouseDown = Rectangle.Empty;
                    pbThumbnail.DoDragDrop(dataObject, DragDropEffects.Copy | DragDropEffects.Move);
                    Program.MainForm.AllowDrop = true;
                }
                else
                {
                    dragBoxFromMouseDown = Rectangle.Empty;
                }
            }
        }

        private void TtMain_Draw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText();
        }
    }
}
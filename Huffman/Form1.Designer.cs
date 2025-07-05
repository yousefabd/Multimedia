using Huffman.data;
using Huffman.Huffman;
using Huffman.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Huffman {
    public partial class Form1 : Form {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel compressPanel;
        private TableLayoutPanel decompressPanel;
        private Panel fileListPanel;
        private Panel archivedListPanel;

        private Button browseFileButton;
        private Button browseFolderButton;
        private Button compressButton;
        private Button decompressButton;
        private Button browseArchiveButton;

        ProgressWindow progressWindow;


        protected override void Dispose(bool disposing) {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            browseFileButton = new Button();
            browseFolderButton = new Button();
            compressButton = new Button();
            decompressButton = new Button();
            browseArchiveButton = new Button();

            fileListPanel = new Panel();
            archivedListPanel = new Panel();
            compressPanel = new TableLayoutPanel();
            decompressPanel = new TableLayoutPanel();

            progressWindow = new ProgressWindow();

            compressPanel.SuspendLayout();
            decompressPanel.SuspendLayout();
            SuspendLayout();

            // 
            // browseFileButton
            // 
            browseFileButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            browseFileButton.BackColor = Color.LightSteelBlue;
            browseFileButton.FlatStyle = FlatStyle.Flat;
            browseFileButton.ImageAlign = ContentAlignment.MiddleLeft;
            browseFileButton.Location = new Point(13, 13);
            browseFileButton.Name = "browseFileButton";
            browseFileButton.Padding = new Padding(10, 0, 10, 0);
            browseFileButton.Size = new Size(168, 57);
            browseFileButton.TabIndex = 0;
            browseFileButton.Text = " Browse File";
            browseFileButton.TextAlign = ContentAlignment.MiddleRight;
            browseFileButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            browseFileButton.UseVisualStyleBackColor = false;
            browseFileButton.Click += browseButton_Click;
            browseFileButton.Image = Image.FromFile("../../../media/browse.png"); // 🔍 File icon

            // 
            // browseFolderButton
            // 
            browseFolderButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            browseFolderButton.BackColor = Color.LightSteelBlue;
            browseFolderButton.FlatStyle = FlatStyle.Flat;
            browseFolderButton.ImageAlign = ContentAlignment.MiddleLeft;
            browseFolderButton.Location = new Point(187, 13);
            browseFolderButton.Name = "browseFolderButton";
            browseFolderButton.Padding = new Padding(10, 0, 10, 0);
            browseFolderButton.Size = new Size(180, 57);
            browseFolderButton.TabIndex = 4;
            browseFolderButton.Text = " Browse Folder";
            browseFolderButton.TextAlign = ContentAlignment.MiddleRight;
            browseFolderButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            browseFolderButton.UseVisualStyleBackColor = false;
            browseFolderButton.Click += BrowseFolderButton_Click; ;
            browseFolderButton.Image = Image.FromFile("../../../media/folder.png"); // 📁 Folder icon

            // 
            // compressButton
            // 
            compressButton.BackColor = Color.LightGreen;
            compressButton.FlatStyle = FlatStyle.Flat;
            compressButton.ImageAlign = ContentAlignment.MiddleLeft;
            compressButton.Location = new Point(361, 13);
            compressButton.Name = "compressButton";
            compressButton.Padding = new Padding(10, 0, 10, 0);
            compressButton.Size = new Size(188, 57);
            compressButton.TabIndex = 1;
            compressButton.Text = " Add to Archive";
            compressButton.TextAlign = ContentAlignment.MiddleRight;
            compressButton.UseVisualStyleBackColor = false;
            compressButton.Image = Image.FromFile("../../../media/compress.png"); // 📦 Compress icon
            compressButton.Click += compressButton_Click;

            // 
            // browseArchiveButton
            // 
            browseArchiveButton.BackColor = Color.LightSteelBlue;
            browseArchiveButton.FlatStyle = FlatStyle.Flat;
            browseArchiveButton.ImageAlign = ContentAlignment.MiddleLeft;
            browseArchiveButton.Location = new Point(13, 13);
            browseArchiveButton.Name = "browseArchiveButton";
            browseArchiveButton.Padding = new Padding(10, 0, 10, 0);
            browseArchiveButton.Size = new Size(188, 57);
            browseArchiveButton.TabIndex = 5;
            browseArchiveButton.Text = " Browse Archive";
            browseArchiveButton.TextAlign = ContentAlignment.MiddleRight;
            browseArchiveButton.UseVisualStyleBackColor = false;
            browseArchiveButton.Image = Image.FromFile("../../../media/browse.png"); // 📦 Archive icon
            browseArchiveButton.Click += BrowseArchiveButton_Click;

            // 
            // decompressButton
            // 
            decompressButton.BackColor = Color.LightCoral;
            decompressButton.FlatStyle = FlatStyle.Flat;
            decompressButton.ImageAlign = ContentAlignment.MiddleLeft;
            decompressButton.Location = new Point(207, 13);
            decompressButton.Name = "decompressButton";
            decompressButton.Padding = new Padding(10, 0, 10, 0);
            decompressButton.Size = new Size(144, 57);
            decompressButton.TabIndex = 2;
            decompressButton.Text = " Extract";
            decompressButton.TextAlign = ContentAlignment.MiddleRight;
            decompressButton.UseVisualStyleBackColor = false;
            decompressButton.Image = Image.FromFile("../../../media/decompress.png");
            decompressButton.Click += decompressButton_Click;

            // 
            // fileListPanel
            // 
            fileListPanel.AutoScroll = true;
            fileListPanel.BackColor = Color.WhiteSmoke;
            fileListPanel.BorderStyle = BorderStyle.FixedSingle;
            fileListPanel.Dock = DockStyle.Top;
            fileListPanel.Location = new Point(0, 83);
            fileListPanel.Name = "fileListPanel";
            fileListPanel.Size = new Size(1000, 200);
            fileListPanel.TabIndex = 0;

            // 
            // archivedListPanel
            // 
            archivedListPanel.AutoScroll = true;
            archivedListPanel.BackColor = Color.WhiteSmoke;
            archivedListPanel.BorderStyle = BorderStyle.FixedSingle;
            archivedListPanel.Dock = DockStyle.Fill;
            archivedListPanel.Location = new Point(0, 366);
            archivedListPanel.Name = "archivedListPanel";
            archivedListPanel.Size = new Size(1000, 234);
            archivedListPanel.TabIndex = 3;

            // 
            // compressPanel
            // 
            compressPanel.BackColor = Color.WhiteSmoke;
            compressPanel.ColumnCount = 3;
            compressPanel.ColumnStyles.Add(new ColumnStyle());
            compressPanel.ColumnStyles.Add(new ColumnStyle());
            compressPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            compressPanel.Controls.Add(browseFileButton, 0, 0);
            compressPanel.Controls.Add(browseFolderButton, 1, 0);
            compressPanel.Controls.Add(compressButton, 2, 0);
            compressPanel.Dock = DockStyle.Top;
            compressPanel.Location = new Point(0, 0);
            compressPanel.Name = "compressPanel";
            compressPanel.Padding = new Padding(10);
            compressPanel.RowCount = 1;
            compressPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            compressPanel.Size = new Size(1000, 83);
            compressPanel.TabIndex = 1;

            // 
            // decompressPanel
            // 
            decompressPanel.BackColor = Color.WhiteSmoke;
            decompressPanel.ColumnCount = 2;
            decompressPanel.ColumnStyles.Add(new ColumnStyle());
            decompressPanel.ColumnStyles.Add(new ColumnStyle());
            decompressPanel.Controls.Add(browseArchiveButton, 0, 0);
            decompressPanel.Controls.Add(decompressButton, 1, 0);
            decompressPanel.Dock = DockStyle.Top;
            decompressPanel.Location = new Point(0, 283);
            decompressPanel.Name = "decompressPanel";
            decompressPanel.Padding = new Padding(10);
            decompressPanel.RowCount = 1;
            decompressPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            decompressPanel.Size = new Size(1000, 83);
            decompressPanel.TabIndex = 2;

            //
            // ProgressWindow
            //
            progressWindow.CancelButton.Click += ProgressWindowCancelButton_Click;
            progressWindow.PauseButton.Click += ProgressWindowPauseButton_Click;

            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 600);
            Controls.Add(archivedListPanel);
            Controls.Add(decompressPanel);
            Controls.Add(fileListPanel);
            Controls.Add(compressPanel);
            Name = "Form1";
            Text = "Huffman Archiver";

            compressPanel.ResumeLayout(false);
            decompressPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void AddFileToListUI(string filePath, Panel listPanel) {
            var fileName = "📄 " + Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(filePath).ToUpperInvariant();
            var fileInfo = new FileInfo(filePath);
            string fileSizeStr = fileInfo.Length >= 1024 * 1024
                ? $"{fileInfo.Length / (1024 * 1024.0):F2} MB"
                : $"{fileInfo.Length / 1024.0:F2} KB";

            var container = new Panel {
                Height = 50,
                Dock = DockStyle.Top,
                Padding = new Padding(5),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };

            var nameLabel = new Label {
                Text = fileName,
                AutoSize = true,
                Location = new Point(50, 10),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
            };

            var typeLabel = new Label {
                Text = fileExtension + " File",
                AutoSize = true,
                Location = new Point(50, 28),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            var sizeLabel = new Label {
                Text = fileSizeStr,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray,
                Location = new Point(container.Width - 80, 15),
            };

            container.Resize += (s, e) => {
                sizeLabel.Left = container.Width - sizeLabel.Width - 10;
            };

            container.Controls.Add(nameLabel);
            container.Controls.Add(typeLabel);
            container.Controls.Add(sizeLabel);

            listPanel.Controls.Add(container);
            listPanel.Controls.SetChildIndex(container, 0);
        }
        private void AddFileToListUI(FolderFileEntry entry, Panel listPanel) {
            string fileName = "📄 " + Path.GetFileName(entry.RelativePath);
            string fileExtension = Path.GetExtension(entry.RelativePath).ToUpperInvariant();
            double sizeInKB = entry.Data != null ? entry.Data.Length / 1024.0 : 0;
            string fileSizeStr = sizeInKB >= 1024
                ? $"{sizeInKB / 1024.0:F2} MB"
                : $"{sizeInKB:F2} KB";

            var container = new Panel {
                Height = 50,
                Dock = DockStyle.Top,
                Padding = new Padding(5),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };

            var nameLabel = new Label {
                Text = fileName,
                AutoSize = true,
                Location = new Point(50, 10),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
            };

            var typeLabel = new Label {
                Text = fileExtension + " File",
                AutoSize = true,
                Location = new Point(50, 28),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            var sizeLabel = new Label {
                Text = fileSizeStr,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray
            };

            var decompressButton = new PictureBox {
                Image = Image.FromFile("../../../media/decompress_icon.png"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(20, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };

            decompressButton.Click += (s, e) =>
            {
                using FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                folderDialog.Description = "Choose where to extract the file";

                if (folderDialog.ShowDialog() != DialogResult.OK)
                    return;

                string outputDir = folderDialog.SelectedPath;
                entry.FullPath = Path.Combine(outputDir, entry.RelativePath);
                try {
                    DecompressSingleFile(entry);
                    MessageBox.Show("Decompression complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) {
                    MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            container.Resize += (s, e) =>
            {
                int padding = 10;
                decompressButton.Left = container.Width - decompressButton.Width - padding;
                decompressButton.Top = (container.Height - decompressButton.Height) / 2;

                sizeLabel.Left = decompressButton.Left - sizeLabel.Width - 5;
                sizeLabel.Top = (container.Height - sizeLabel.Height) / 2;
            };

            // Trigger initial position setup
            container.PerformLayout();

            container.Controls.Add(nameLabel);
            container.Controls.Add(typeLabel);
            container.Controls.Add(sizeLabel);
            container.Controls.Add(decompressButton);

            listPanel.Controls.Add(container);
            listPanel.Controls.SetChildIndex(container, 0);
        }


        private void ClearFileList(Panel listPanel) {
            listPanel.Controls.Clear();
        }

        private bool IsFileListEmpty() {
            return !fileListPanel.Controls
                .OfType<Panel>()
                .Any(p => p.BorderStyle == BorderStyle.FixedSingle);
        }

    }
}

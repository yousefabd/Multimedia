using Huffman.data;
using Huffman.Huffman;

namespace Huffman;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        inputFolderEntries = new();
        compressor = new HuffmanProcessor();
    }
    private void AddInputFile(string containingFolderPath,string filePath) {
        string relativePath = Path.GetRelativePath(containingFolderPath, filePath);
        inputFolderEntries.Add(new FolderFileEntry(filePath,relativePath));
        AddFileToList(filePath,fileListPanel);
    }

    private void DecompressSingleFile(FolderFileEntry entry) {
        // Ensure parent directories exist
        Directory.CreateDirectory(Path.GetDirectoryName(entry.FullPath)!);

        // Write the decompressed file
        File.WriteAllBytes(entry.FullPath, entry.Data!);
    }

    private void browseButton_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            AddInputFile(openFileDialog.FileName,openFileDialog.FileName);
        }
    }

    private void compressButton_Click(object sender, EventArgs e)
    {
        using var saveFileDialog = new SaveFileDialog {
            Title = "Save Compressed Archive",
            Filter = "Huffman Archive (*.huff)|*.huff",
            DefaultExt = "huff",
            FileName = "Compressed.huff",
            AddExtension = true
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK) {
            archivePath = saveFileDialog.FileName;
            try {
                this.compressor.Compress(inputFolderEntries, archivePath);
                MessageBox.Show("Compression complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex){
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    private void decompressButton_Click(object sender, EventArgs e) {
        using FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        folderDialog.Description = "Choose where to extract the files";

        if (folderDialog.ShowDialog() != DialogResult.OK)
            return;

        string outputDir = folderDialog.SelectedPath;

        try {
            // Call Decompress and get output entries (each with FullPath and decompressed Data)
            this.outputEntries = this.compressor.Decompress(this.archivePath, outputDir);

            // Save each decompressed file to disk
            foreach (var entry in outputEntries) {
                DecompressSingleFile(entry);
            }

            MessageBox.Show("Decompression complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) {
            MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BrowseFolderButton_Click(object sender, EventArgs e) {
        using var folderDialog = new FolderBrowserDialog();

        if (folderDialog.ShowDialog() == DialogResult.OK) {
            string baseFolder = folderDialog.SelectedPath;
            var files = Directory.GetFiles(baseFolder, "*", SearchOption.AllDirectories);

            inputFolderEntries.Clear(); // Clear any previous folder selections
            ClearFileList(fileListPanel);

            foreach (var file in files) {
                string relativePath = Path.GetRelativePath(baseFolder, file);
                inputFolderEntries.Add(new FolderFileEntry(file, relativePath));

                AddFileToList(file,fileListPanel); // optional: visually show each file in the UI
            }
        }
    }
    private void BrowseArchiveButton_Click(object sender, EventArgs e) {
        ClearFileList(archivedListPanel);

        OpenFileDialog openFileDialog = new OpenFileDialog {
            Filter = "Huffman Archive (*.huff)|*.huff",
            Title = "Select Huffman Compressed File"
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
            return;

        archivePath = openFileDialog.FileName;

        try {
            // Decompress archive and store results
            outputEntries = this.compressor.Decompress(archivePath,archivePath);

            // Show decompressed file entries in the UI
            foreach (var entry in outputEntries) {
                AddFileToList(entry, archivedListPanel);
            }
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to load archive:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }



}
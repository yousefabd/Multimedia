using Huffman.data;
using Huffman.Huffman;

namespace Huffman;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        folderEntries = new();
        compressor = new HuffmanProcessor();
    }
    private void AddInputFile(string containingFolderPath,string filePath) {
        string relativePath = Path.GetRelativePath(containingFolderPath, filePath);
        folderEntries.Add(new FolderFileEntry(filePath,relativePath));
        AddFileToList(filePath,fileListPanel);
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
            outputFile = saveFileDialog.FileName;
            try {
                this.compressor.Compress(folderEntries, outputFile);
                MessageBox.Show("Compression complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex){
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    private void decompressButton_Click(object sender, EventArgs e)
    {

        FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        folderDialog.Description = "Choose where to extract the files";

        if (folderDialog.ShowDialog() != DialogResult.OK)
            return;

        string outputDir = folderDialog.SelectedPath;

        try {
            this.compressor.Decompress(outputFile, outputDir);
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

            folderEntries.Clear(); // Clear any previous folder selections
            ClearFileList(fileListPanel);

            foreach (var file in files) {
                string relativePath = Path.GetRelativePath(baseFolder, file);
                folderEntries.Add(new FolderFileEntry(file, relativePath));

                AddFileToList(file,fileListPanel); // optional: visually show each file in the UI
            }
        }
    }
    private void BrowseArchiveButton_Click(object sender, EventArgs e) {
        ClearFileList(archivedListPanel);
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Huffman Archive (*.huff)|*.huff";
        openFileDialog.Title = "Select Huffman Compressed File";

        if (openFileDialog.ShowDialog() != DialogResult.OK)
            return;

        outputFile = openFileDialog.FileName;
        AddFileToList(outputFile, archivedListPanel);
    }

}
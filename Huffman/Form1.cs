using Huffman.data;
using Huffman.Huffman;
using Huffman.ShannonFano;

namespace Huffman;

public partial class Form1 : Form
{
    //the current list of folders/files we want to compress
    public List<FolderFileEntry> inputFolderEntries { get; set; }
    //file entries containing the compressed bytes that will be saved
    public List<FolderFileEntry> outputEntries { get; set; }
    //the current compressed output file path that we want to decompress
    public string archivePath { get; set; }
    //the separate thread class for handling multithreading
    private ArchiveWorker archiveWorker;

    public Form1() {
        InitializeComponent();

        inputFolderEntries = new();
        archiveWorker = new ArchiveWorker(new HuffmanProcessor());

        archiveWorker.CompressionCompleted += () => {
            Invoke(() => {
                progressWindow.Hide();
                MessageBox.Show("Compression complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        };

        archiveWorker.CompressionFailed += (ex) => {
            Invoke(() => {
                MessageBox.Show($"Compression failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressWindow.Hide();
            });
        };

        archiveWorker.DecompressionCompleted += (entries) => {
            Invoke(() => {
                outputEntries = entries;
                foreach (var entry in outputEntries) {
                    DecompressSingleFile(entry);
                }
                progressWindow.Hide();
                MessageBox.Show("Decompression complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);;
            });
        };

        archiveWorker.DecompressionFailed += (ex) => {
            Invoke(() => {
                MessageBox.Show($"Decompression failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressWindow.Hide();
            });
        };
        archiveWorker.ProgressChanged += (report) => {
            Invoke(() => {
                progressWindow.UpdateProgress(report);
            });
        };
        archiveWorker.OperationCancelled += () => {
            Invoke(() => {
                progressWindow.Hide();
                MessageBox.Show("Operation was cancelled.", "Cancelled");
            });
        };
    }

    private void AddInputFile(string containingFolderPath, string filePath) {
        string relativePath = Path.GetRelativePath(containingFolderPath, filePath);
        inputFolderEntries.Add(new FolderFileEntry(filePath, relativePath));
        AddFileToListUI(filePath, fileListPanel);
    }

    private void DecompressSingleFile(FolderFileEntry entry) {
        Directory.CreateDirectory(Path.GetDirectoryName(entry.FullPath)!);
        File.WriteAllBytes(entry.FullPath, entry.Data!);
    }

    private void browseButton_Click(object sender, EventArgs e) {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == DialogResult.OK) {
            AddInputFile(openFileDialog.FileName, openFileDialog.FileName);
        }
    }

    private void compressButton_Click(object sender, EventArgs e) {
        string extension, filter;
        if (archiveWorker.Processor is HuffmanProcessor) {
            extension = "huff";
            filter = "Huffman Archive (*.huff)|*.huff";
        }
        else if (archiveWorker.Processor is ShannonFanoProcessor) {
            extension = "sfarc";
            filter = "Shannon-Fano Archive (*.sfarc)|*.sfarc";
        }
        else {
            extension = "bin";
            filter = "All files (*.*)|*.*";
        }

        // Suggested archive name
        string suggestedName = "";
        if (inputFolderEntries.Count == 1) {
            suggestedName = Path.GetFileNameWithoutExtension(inputFolderEntries[0].FullPath);
        }
        else if(inputFolderEntries.Count > 1){
            // Get the common root folder name if possible
            var firstPath = inputFolderEntries[0].FullPath;
            string? topFolder = Path.GetDirectoryName(firstPath)?.Split(Path.DirectorySeparatorChar).LastOrDefault();
            suggestedName = !string.IsNullOrEmpty(topFolder) ? topFolder : "Compressed";
        }

        using var saveFileDialog = new SaveFileDialog {
            Title = "Save Compressed Archive",
            Filter = filter,
            DefaultExt = extension,
            FileName = suggestedName + "." + extension,
            AddExtension = true
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK) {
            archivePath = saveFileDialog.FileName;
            archiveWorker.CompressAsync(inputFolderEntries, archivePath);
            progressWindow.Show();
            progressWindow.SetAction("adding");
        }
    }



    private void decompressButton_Click(object sender, EventArgs e) {
        using FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        folderDialog.Description = "Choose where to extract the files";

        if (folderDialog.ShowDialog() != DialogResult.OK)
            return;

        string outputDir = folderDialog.SelectedPath;
        if(outputEntries.Count != 0) {
            for(int i = 0; i < outputEntries.Count; i++) {
                var entry = outputEntries[i];
                entry.FullPath = Path.Combine(outputDir, entry.RelativePath!);
                DecompressSingleFile(entry);
            }
            MessageBox.Show("Saved extracted files successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); ;
            return;
        }
        archiveWorker.DecompressAsync(archivePath, outputDir);
        progressWindow.Show();
        progressWindow.SetAction("extracting");
    }

    private void BrowseFolderButton_Click(object sender, EventArgs e) {
        using var folderDialog = new FolderBrowserDialog();

        if (folderDialog.ShowDialog() == DialogResult.OK) {
            string baseFolder = folderDialog.SelectedPath;
            var files = Directory.GetFiles(baseFolder, "*", SearchOption.AllDirectories);

            inputFolderEntries.Clear();
            ClearFileList(fileListPanel);

            foreach (var file in files) {
                string relativePath = Path.GetRelativePath(baseFolder, file);
                inputFolderEntries.Add(new FolderFileEntry(file, relativePath));
                AddFileToListUI(file, fileListPanel);
            }
        }
    }

    private void BrowseArchiveButton_Click(object sender, EventArgs e) {
        ClearFileList(archivedListPanel);

        OpenFileDialog openFileDialog = new OpenFileDialog {
            Filter = "All Supported Archives (*.huff;*.sfarc)|*.huff;*.sfarc|Huffman Archive (*.huff)|*.huff|Shannon-Fano Archive (*.sfarc)|*.sfarc",
            Title = "Select Compressed Archive"
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
            return;

        archivePath = openFileDialog.FileName;

        string ext = Path.GetExtension(archivePath).ToLowerInvariant();
        if (ext == ".sfarc")
            archiveWorker.SetProcessor(new ShannonFanoProcessor());
        else if (ext == ".huff")
            archiveWorker.SetProcessor(new HuffmanProcessor());
        else {
            MessageBox.Show("Unsupported archive format.");
            return;
        }

        // Just decompress to a temp folder and show results (no saving to disk)
        string tempOutput = Path.Combine(Path.GetTempPath(), "huff_extracted_preview_" + Guid.NewGuid());
        Directory.CreateDirectory(tempOutput);

        archiveWorker.DecompressAsync(archivePath, tempOutput);
        progressWindow.Show();
        progressWindow.SetAction("extracting");

        // Entries shown when DecompressionCompleted fires
        archiveWorker.DecompressionCompleted += (entries) => {
            Invoke(() => {
                ClearFileList(archivedListPanel);
                foreach (var entry in entries) {
                    AddFileToListUI(entry, archivedListPanel);
                }
            });
        };
    }



    public void ProgressWindowCancelButton_Click(object sender, EventArgs e) {
        archiveWorker.Cancel();
    }
    public void ProgressWindowPauseButton_Click(object sender, EventArgs e) {
        archiveWorker.TogglePause();
    }

}
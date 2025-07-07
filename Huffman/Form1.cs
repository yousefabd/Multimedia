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
        outputEntries = new();
        archiveWorker = new ArchiveWorker(new HuffmanProcessor());

        archiveWorker.CompressionCompleted += (ratio) => {
            Invoke(() => {
                progressWindow.Hide();
                MessageBox.Show($"Compression complete with {ratio}% compression ratio", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        string suggestedName = FileUtils.GetSuggestedArchiveName(inputFolderEntries);

        using var saveFileDialog = new SaveFileDialog {
            Title = "Save Compressed Archive",
            Filter = "Huffman Archive (*.huff)|*.huff|Shannon-Fano Archive (*.sfarc)|*.sfarc",
            DefaultExt = "huff",
            FileName = suggestedName + ".huff",
            AddExtension = true
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK) {
            return;
        }
        archivePath = saveFileDialog.FileName;
        switch (saveFileDialog.FilterIndex) {
            case 1:
                archiveWorker.SetProcessor(new HuffmanProcessor());
                break;
            case 2:
                archiveWorker.SetProcessor(new ShannonFanoProcessor());
                break;
            default:
                MessageBox.Show("Unknown compression type selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
        }
        archiveWorker.CompressAsync(inputFolderEntries, archivePath);
        progressWindow.Show();
        progressWindow.SetAction("adding");
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


    //Right Hand Integration
    public void ExtractArchiveFromPathRegistry(string archivePath) {
        try {
            if (!File.Exists(archivePath))
                throw new FileNotFoundException("Archive file does not exist.", archivePath);

            string ext = Path.GetExtension(archivePath).ToLowerInvariant();
            if (ext == ".sfarc")
                archiveWorker.SetProcessor(new ShannonFanoProcessor());
            else if (ext == ".huff")
                archiveWorker.SetProcessor(new HuffmanProcessor());
            else
                throw new InvalidOperationException("Unsupported archive format.");

            // Determine the output folder
            string archiveNameWithoutExt = Path.GetFileNameWithoutExtension(archivePath);
            string archiveDirectory = Path.GetDirectoryName(archivePath)!;
            string extractPath = Path.Combine(archiveDirectory, archiveNameWithoutExt);

            // Show progress UI
            progressWindow.Show();
            progressWindow.SetAction("extracting");

            // Hook up events (this instance only)
            archiveWorker.DecompressionCompleted += (entries) => {
                Invoke(() => {
                    outputEntries = entries;
                    foreach (var entry in outputEntries) {
                        DecompressSingleFile(entry);
                    }

                    progressWindow.Hide();
                    Application.Exit();
                });
            };

            archiveWorker.DecompressionFailed += (ex) => {
                Invoke(() => {
                    progressWindow.Hide();
                    Application.Exit();
                });
            };

            archiveWorker.ProgressChanged += (report) => {
                Invoke(() => progressWindow.UpdateProgress(report));
            };

            archiveWorker.OperationCancelled += () => {
                Invoke(() => {
                    progressWindow.Hide();
                    Application.Exit();
                });
            };

            // Start decompression
            archiveWorker.DecompressAsync(archivePath, extractPath);
        }
        catch (Exception ex) {
            MessageBox.Show($"Startup failure:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
    private void CompressFilesArchiveFromPathRegistry(List<string> paths) {
        string baseFolder = Path.GetDirectoryName(paths[0])!;
        inputFolderEntries.Clear();

        foreach (var path in paths) {
            if (Directory.Exists(path)) {
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                foreach (var file in files) {
                    string relativePath = Path.GetRelativePath(baseFolder, file);
                    inputFolderEntries.Add(new FolderFileEntry(file, relativePath));
                }
            }
            else if (File.Exists(path)) {
                string relativePath = Path.GetFileName(path);
                inputFolderEntries.Add(new FolderFileEntry(path, relativePath));
            }
        }

        string archiveName = FileUtils.GetSuggestedArchiveName(inputFolderEntries);
        archivePath = Path.Combine(baseFolder, archiveName + ".huff");

        archiveWorker.SetProcessor(new HuffmanProcessor());
        archiveWorker.CompressAsync(inputFolderEntries, archivePath);
        progressWindow.Show();
        progressWindow.SetAction("adding");
    }
    private void CompressFolderArchiveRegistry(string folderPath) {
        inputFolderEntries.Clear();

        string archiveName = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(archiveName))
            archiveName = "EmptyArchive";

        archivePath = Path.Combine(folderPath, archiveName + ".huff");

        archiveWorker.SetProcessor(new HuffmanProcessor());
        archiveWorker.CompressAsync(inputFolderEntries, archivePath);
        progressWindow.Show();
        progressWindow.SetAction("adding");
    }


    protected override void OnShown(EventArgs e) {
        base.OnShown(e);

        //command line tag check
        if (Tag is CommandLineArgs clt) {
            if (clt.IsExtract && clt.Paths.Count == 1) {
                ExtractArchiveFromPathRegistry(clt.Paths[0]);
                Hide();
            }
            else if (clt.IsCompress && clt.Paths.Count > 0) {
                CompressFilesArchiveFromPathRegistry(clt.Paths);
                Hide();
            }
            else if (clt.IsCreateEmpty && clt.Paths.Count == 1) {
                CompressFolderArchiveRegistry(clt.Paths[0]);
                Hide();
            }
        }
    }

}
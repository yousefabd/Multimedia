using Huffman.Archive;
using System.Windows.Forms;

namespace Huffman.UI; 
public partial class ProgressWindow : Form {
    public Label StatusLabel { get; private set; }
    public Label FileLabel { get; private set; }
    public Label PercentageLabel { get; set; }
    public ProgressBar ProgressBar { get; private set; }
    public new Button CancelButton { get; private set; }
    public Button PauseButton { get; private set; }

    private bool isPaused = false;

    public ProgressWindow() {
        BuildUI();
    }

    private void BuildUI() {
        Text = "Processing Archive";
        Size = new Size(420, 180);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen; // Center on screen

        var panel = new Panel {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        Controls.Add(panel);

        // Status line (e.g. "adding...")
        StatusLabel = new Label {
            Text = "adding...",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            AutoSize = true,
            Location = new Point(10, 10)
        };
        panel.Controls.Add(StatusLabel);

        // File name and percent aligned properly
        var fileRow = new Panel {
            Width = 380,
            Height = 25,
            Location = new Point(10, 35)
        };

        FileLabel = new Label {
            Text = "file.txt",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            AutoSize = true,
            Location = new Point(0, 5)
        };

        PercentageLabel = new Label {
            Text = "0%",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleRight,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(fileRow.Width - 40, 5)
        };

        fileRow.Controls.Add(FileLabel);
        fileRow.Controls.Add(PercentageLabel);
        panel.Controls.Add(fileRow);

        // Progress bar
        ProgressBar = new ProgressBar {
            Width = 380,
            Height = 20,
            Location = new Point(10, 65),
            Style = ProgressBarStyle.Continuous
        };
        panel.Controls.Add(ProgressBar);

        // Cancel button
        CancelButton = new Button {
            Text = "Cancel",
            Width = 80,
            Height = 30,
            Location = new Point(210, 100)
        };
        panel.Controls.Add(CancelButton);

        // Pause button
        PauseButton = new Button {
            Text = "Pause",
            Width = 80,
            Height = 30,
            Location = new Point(300, 100)
        };
        PauseButton.Click += (sender, e) => {
            isPaused = !isPaused;
            PauseButton.Text = isPaused ? "Resume" : "Pause";
        };
        panel.Controls.Add(PauseButton);
    }

    public void UpdateProgress(ProgressReport report) {
        FileLabel.Text = report.FileName;
        ProgressBar.Value = report.Percentage;
        PercentageLabel.Text = $"{report.Percentage}%";
    }
    public void SetAction(string action) {
        StatusLabel.Text = action + "..";
    }
    public bool IsPaused() => isPaused;
}

// DetailsForm.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace dependo_testing_app
{
    public class DetailsForm : Form
    {
        private readonly Game _game;

        public DetailsForm(Game game)
        {
            _game = game;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"{_game.Name} - Details";
            this.Size = new Size(520, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemeManager.Background;
            this.ForeColor = ThemeManager.TextColor;

            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(25), FlowDirection = FlowDirection.TopDown, WrapContents = false };
            this.Controls.Add(panel);
            
            Action<string, Font, Color> addLabel = (text, font, color) => {
                panel.Controls.Add(new Label { Text = text, Font = font, ForeColor = color, AutoSize = true, MaximumSize = new Size(panel.ClientSize.Width, 0) });
            };

            addLabel($"DirectX: {_game.DirectX}", new Font("Segoe UI", 10F), ThemeManager.TextColor);
            addLabel($"VC++: {(_game.VCRedist != null ? string.Join(", ", _game.VCRedist) : "N/A")}", new Font("Segoe UI", 10F), ThemeManager.TextColor);
            addLabel($".NET: {_game.DotNet}", new Font("Segoe UI", 10F), ThemeManager.TextColor);
            if (_game.DLLs?.Any() == true) addLabel($"Missing DLLs: {string.Join(", ", _game.DLLs)}", new Font("Segoe UI", 10F), ThemeManager.TextColor);
            
            addLabel("\nFixes:", new Font("Segoe UI", 12F, FontStyle.Bold), ThemeManager.TextColor);
            if(_game.Fixes != null)
            {
                foreach (var fix in _game.Fixes) addLabel($"• {fix}", new Font("Segoe UI", 10F), ThemeManager.SubtleTextColor);
            }

            if (_game.Downloads != null)
            {
                addLabel("\nDownload Links:", new Font("Segoe UI", 12F, FontStyle.Bold), ThemeManager.TextColor);
                var links = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(_game.Downloads.DirectX)) links.Add("DirectX Runtime", _game.Downloads.DirectX);
                if (_game.Downloads.VCRedist != null)
                {
                    foreach (var vcLink in _game.Downloads.VCRedist.OrderBy(kv => kv.Key)) links.Add($"VC++ {vcLink.Key}", vcLink.Value);
                }
                if (!string.IsNullOrEmpty(_game.Downloads.DotNet)) links.Add($".NET Framework {_game.DotNet}", _game.Downloads.DotNet);

                foreach(var link in links)
                {
                    var linkLabel = new LinkLabel {
                        Text = $"• {link.Key}",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10F),
                        LinkColor = ThemeManager.AccentColor,
                        ActiveLinkColor = Color.Red,
                        Margin = new Padding(0, 5, 0, 0)
                    };
                    linkLabel.LinkClicked += (sender, e) => {
                        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(link.Value) { UseShellExecute = true }); }
                        catch (Exception ex) { MessageBox.Show($"Could not open the link. Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    };
                    panel.Controls.Add(linkLabel);
                }
            }
        }
    }
}
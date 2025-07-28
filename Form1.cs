using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dependo_testing_app
{
    // C# Models that match the JSON structure from the URL
    #region DataModels

    public class Game
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("directx")]
        public string DirectX { get; set; }

        [JsonPropertyName("vcredist")]
        public List<string> VCRedist { get; set; }

        [JsonPropertyName("dotnet")]
        public string DotNet { get; set; }

        [JsonPropertyName("dlls")]
        public List<string> DLLs { get; set; }

        [JsonPropertyName("downloads")]
        public DownloadLinks Downloads { get; set; }

        [JsonPropertyName("fixes")]
        public List<string> Fixes { get; set; }
    }

    public class DownloadLinks
    {
        [JsonPropertyName("directx")]
        public string DirectX { get; set; }

        [JsonPropertyName("vcredist")]
        public Dictionary<string, string> VCRedist { get; set; }

        [JsonPropertyName("dotnet")]
        public string DotNet { get; set; }
    }

    #endregion

    // Class responsible for fetching the game data from the web
    public static class GamesFetcher
    {
        public static async Task<List<Game>> LoadGamesAsync()
        {
            using var client = new HttpClient();
            var url = "https://raw.githubusercontent.com/stuffbymax/game-dependencies-db/main/games.json";
            
            try
            {
                var games = await client.GetFromJsonAsync<List<Game>>(url);
                return games ?? new List<Game>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game data. Please check your internet connection.\n\nError: {ex.Message}", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<Game>();
            }
        }
    }

    // Main application form
    public partial class Form1 : Form
    {
        private List<Game> allGames = new();
        private Panel topPanel;
        private Label titleLabel;
        private TextBox searchBox;
        private Button themeToggle;
        private FlowLayoutPanel gamePanel;
        private Button backToTopButton;
        private bool darkMode = true;

        private readonly Color darkThemeBack = Color.FromArgb(28, 28, 28);
        private readonly Color darkThemeSurface = Color.FromArgb(45, 45, 48);
        private readonly Color darkThemeText = Color.White;
        private readonly Color darkThemeSubtleText = Color.FromArgb(170, 170, 170);
        private readonly Color darkThemeAccent = Color.FromArgb(0, 122, 204);

        private readonly Color lightThemeBack = Color.FromArgb(242, 242, 242);
        private readonly Color lightThemeSurface = Color.White;
        private readonly Color lightThemeText = Color.Black;
        private readonly Color lightThemeSubtleText = Color.FromArgb(100, 100, 100);
        private readonly Color lightThemeAccent = Color.FromArgb(0, 122, 204);
        
        public Form1()
        {
            InitializeComponent();
            SetupUI();
            _ = LoadGamesAsync();
        }

        private async Task LoadGamesAsync()
        {
            allGames = await GamesFetcher.LoadGamesAsync();
            allGames = allGames.OrderBy(g => g.Name).ToList();
            DisplayGames(allGames);
        }

        private void SetupUI()
        {
            this.Text = "Dependo - Game Dependency Viewer";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.Size = new Size(950, 750);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 0, 20, 0)
            };
            this.Controls.Add(topPanel);
            
            titleLabel = new Label
            {
                Text = "Dependo",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            topPanel.Controls.Add(titleLabel);

            themeToggle = new Button
            {
                Font = new Font("Segoe UI", 14F),
                Dock = DockStyle.Right,
                Size = new Size(50, 40),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
            };
            themeToggle.FlatAppearance.BorderSize = 0;
            themeToggle.Click += (s, e) => ToggleTheme();
            topPanel.Controls.Add(themeToggle);

            searchBox = new TextBox
            {
                PlaceholderText = "Search games...",
                Font = new Font("Segoe UI", 11F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };
            topPanel.Controls.Add(searchBox);
            searchBox.BringToFront(); // Ensure it's not hidden
            searchBox.TextChanged += SearchBox_TextChanged;
            
            gamePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20)
            };
            this.Controls.Add(gamePanel);
            gamePanel.BringToFront();

            backToTopButton = new Button
            {
                Text = "⬆",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Size = new Size(45, 45),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Visible = false 
            };
            backToTopButton.FlatAppearance.BorderSize = 0;
            backToTopButton.Click += (s, e) => gamePanel.ScrollControlIntoView(gamePanel.Controls.Count > 0 ? gamePanel.Controls[0] : gamePanel);
            this.Controls.Add(backToTopButton);
            backToTopButton.BringToFront();

            gamePanel.Scroll += (s, e) => backToTopButton.Visible = gamePanel.VerticalScroll.Value > 200;
            this.Resize += OnFormResize;
            
            // Initial call to set everything correctly
            OnFormResize(this, EventArgs.Empty);
            ApplyTheme();
        }

        private void OnFormResize(object sender, EventArgs e)
        {
            // This method provides reliable centering and resizing of the search box
            int searchBoxWidth = (int)(topPanel.ClientSize.Width * 0.4); // 40% of top panel width
            searchBox.Width = searchBoxWidth;
            searchBox.Location = new Point((topPanel.ClientSize.Width - searchBoxWidth) / 2, (topPanel.Height - searchBox.Height) / 2);

            backToTopButton.Location = new Point(this.ClientSize.Width - backToTopButton.Width - 20, this.ClientSize.Height - backToTopButton.Height - 20);

            // Redraw cards to fit new window size
            DisplayGames(allGames.Where(g => string.IsNullOrEmpty(searchBox.Text) || g.Name.Contains(searchBox.Text, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        private void ApplyTheme()
        {
            var backColor = darkMode ? darkThemeBack : lightThemeBack;
            var surfaceColor = darkMode ? darkThemeSurface : lightThemeSurface;
            var textColor = darkMode ? darkThemeText : lightThemeText;

            this.BackColor = backColor;
            topPanel.BackColor = backColor;
            titleLabel.ForeColor = textColor;
            titleLabel.BackColor = backColor;
            searchBox.BackColor = surfaceColor;
            searchBox.ForeColor = textColor;
            themeToggle.Text = darkMode ? "☀️" : "🌙";
            themeToggle.BackColor = backColor;
            themeToggle.ForeColor = textColor;
            gamePanel.BackColor = backColor;
            backToTopButton.BackColor = surfaceColor;
            backToTopButton.ForeColor = textColor;

            DisplayGames(allGames.Where(g => string.IsNullOrEmpty(searchBox.Text) || g.Name.Contains(searchBox.Text, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        private void ToggleTheme()
        {
            darkMode = !darkMode;
            ApplyTheme();
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            var filteredGames = allGames
                .Where(g => g.Name.Contains(searchBox.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();
            DisplayGames(filteredGames);
        }

        private void DisplayGames(List<Game> games)
        {
            if (gamePanel == null || !this.IsHandleCreated) return;
            gamePanel.SuspendLayout();
            gamePanel.Controls.Clear();
            if (games.Any())
            {
                int cardWidth = gamePanel.ClientSize.Width - 5;
                foreach (var game in games)
                {
                    gamePanel.Controls.Add(CreateGameCard(game, cardWidth));
                }
            }
            gamePanel.ResumeLayout();
        }

        private Panel CreateGameCard(Game game, int cardWidth)
        {
            var card = new Panel
            {
                Width = cardWidth,
                Height = 150,
                BackColor = darkMode ? darkThemeSurface : lightThemeSurface,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(20)
            };
            
            var nameLabel = new Label
            {
                Text = game.Name,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = darkMode ? darkThemeText : lightThemeText,
                Dock = DockStyle.Top,
                Height = 35
            };

            var detailsLabel = new Label
            {
                Text = $"DirectX: {game.DirectX}  •  .NET: {game.DotNet}  •  VC++: {string.Join(", ", game.VCRedist)}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = darkMode ? darkThemeSubtleText : lightThemeSubtleText,
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(0, 5, 0, 0)
            };

            var showBtn = new Button
            {
                Text = "View Details",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = darkThemeAccent,
                ForeColor = Color.White,
                Size = new Size(130, 40),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(card.Width - 130 - 20, card.Height - 40 - 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            showBtn.FlatAppearance.BorderSize = 0;
            showBtn.Click += (s, e) => ShowDetailsPopup(game);

            card.Controls.Add(showBtn);
            card.Controls.Add(nameLabel);
            card.Controls.Add(detailsLabel);
            
            return card;
        }
        
        // COMPLETELY REWRITTEN - This is the robust method for creating links.
        private void ShowDetailsPopup(Game game)
        {
            using (var detailsForm = new Form())
            {
                detailsForm.Text = $"{game.Name} - Details";
                detailsForm.Size = new Size(520, 550);
                detailsForm.BackColor = darkMode ? darkThemeBack : lightThemeBack;
                detailsForm.ForeColor = darkMode ? darkThemeText : lightThemeText;
                detailsForm.StartPosition = FormStartPosition.CenterParent;
                detailsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailsForm.MaximizeBox = false;
                detailsForm.MinimizeBox = false;

                var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(25) };
                detailsForm.Controls.Add(panel);
                
                var linkLabel = new LinkLabel
                {
                    AutoSize = true,
                    MaximumSize = new Size(450, 0),
                    Font = new Font("Segoe UI", 10.5F),
                    LinkColor = darkThemeAccent,
                    ActiveLinkColor = Color.Red,
                    VisitedLinkColor = Color.Plum,
                    Location = new Point(0, 0),
                    // Use a fixed line height for consistent spacing
                    Height = 0 
                };
                panel.Controls.Add(linkLabel);

                var content = new System.Text.StringBuilder();

                // Add non-link content first
                content.AppendLine($"DirectX: {game.DirectX}");
                content.AppendLine($"VC++: {string.Join(", ", game.VCRedist)}");
                content.AppendLine($".NET: {game.DotNet}");
                if (game.DLLs?.Any() == true)
                    content.AppendLine($"Missing DLLs: {string.Join(", ", game.DLLs)}");
                
                content.AppendLine("\nFixes:");
                foreach (var fix in game.Fixes)
                    content.AppendLine($"• {fix}");
                
                // Set the initial text
                linkLabel.Text = content.ToString();

                // Now, append link text one by one and add the LinkArea immediately
                if (game.Downloads != null)
                {
                    AppendText(linkLabel, "\nDownload Links:\n");

                    var linksToCreate = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(game.Downloads.DirectX))
                        linksToCreate.Add("DirectX Runtime", game.Downloads.DirectX);
                    if (game.Downloads.VCRedist != null)
                    {
                        foreach (var vcLink in game.Downloads.VCRedist.OrderBy(kv => kv.Key))
                            linksToCreate.Add($"VC++ {vcLink.Key}", vcLink.Value);
                    }
                    if (!string.IsNullOrEmpty(game.Downloads.DotNet))
                        linksToCreate.Add($".NET Framework {game.DotNet}", game.Downloads.DotNet);
                    
                    foreach(var link in linksToCreate)
                    {
                        AppendLink(linkLabel, $"• {link.Key}\n", link.Value);
                    }
                }

                linkLabel.LinkClicked += (sender, e) =>
                {
                    try
                    {
                        e.Link.Visited = true;
                        string url = e.Link.LinkData as string;
                        if (!string.IsNullOrEmpty(url))
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open the link. Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                detailsForm.ShowDialog(this);
            }
        }

        // Helper methods for robustly adding text and links
        private void AppendText(LinkLabel label, string text)
        {
            label.Text += text;
        }

        private void AppendLink(LinkLabel label, string text, string url)
        {
            int start = label.Text.Length;
            // Use Trim() on the display text to get accurate length for the link area, excluding newlines
            int length = text.Trim().Length;
            label.Text += text;
            label.Links.Add(start, length, url);
        }
    }
}
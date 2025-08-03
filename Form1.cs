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
    // C# Models (No changes needed)
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

    // Data Fetcher (No changes needed)
    public static class GamesFetcher
    {
        public static async Task<List<Game>> LoadGamesAsync()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "DependoApp/1.0");
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
    
    // --- DESIGN IMPROVEMENT 1: Centralized Theme Manager ---
    // This static class now holds all theme-related logic, making it easy to manage.
    public static class ThemeManager
    {
        public static bool IsDarkMode { get; private set; } = true;

        // Theme Palettes
        private static readonly Color DarkBack = Color.FromArgb(28, 28, 28);
        private static readonly Color DarkSurface = Color.FromArgb(45, 45, 48);
        private static readonly Color DarkText = Color.White;
        private static readonly Color DarkSubtleText = Color.FromArgb(170, 170, 170);
        
        private static readonly Color LightBack = Color.FromArgb(242, 242, 242);
        private static readonly Color LightSurface = Color.White;
        private static readonly Color LightText = Color.Black;
        private static readonly Color LightSubtleText = Color.FromArgb(100, 100, 100);

        public static readonly Color AccentColor = Color.FromArgb(0, 122, 204);

        // Getters for current theme colors
        public static Color Background => IsDarkMode ? DarkBack : LightBack;
        public static Color Surface => IsDarkMode ? DarkSurface : LightSurface;
        public static Color TextColor => IsDarkMode ? DarkText : LightText;
        public static Color SubtleTextColor => IsDarkMode ? DarkSubtleText : LightSubtleText;

        public static void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }
    }


    // Main application form (Refactored)
    public partial class Form1 : Form
    {
        private List<Game> allGames = new();
        private Panel topPanel;
        private Label titleLabel;
        private TextBox searchBox;
        private Button themeToggle;
        private FlowLayoutPanel gamePanel;
        private Button backToTopButton;
        private ProgressBar loadingIndicator; // --- DESIGN IMPROVEMENT 2: Loading Indicator ---
        
        public Form1()
        {
            InitializeComponent(); // This is a standard method for WinForms designer support.
            SetupUI();
            ApplyTheme(); // Apply the default theme on startup.
            _ = LoadGamesAndDisplayAsync(); // Fire-and-forget async loading.
        }

        // --- DESIGN IMPROVEMENT 3: Combined Loading and Initial Display ---
        private async Task LoadGamesAndDisplayAsync()
        {
            ShowLoading(true);
            allGames = (await GamesFetcher.LoadGamesAsync()).OrderBy(g => g.Name).ToList();
            DisplayFilteredGames();
            ShowLoading(false);
        }

        private void SetupUI()
        {
            this.Text = "Dependo - Game Dependency Viewer";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.Size = new Size(950, 750);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

            // Top Panel for Title, Search, and Theme Toggle
            topPanel = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(20, 0, 20, 0) };
            titleLabel = new Label { Text = "Dependo", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            themeToggle = new Button { Font = new Font("Segoe UI", 14F), Dock = DockStyle.Right, Size = new Size(50, 40), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            themeToggle.FlatAppearance.BorderSize = 0;
            themeToggle.Click += (s, e) => { ThemeManager.ToggleTheme(); ApplyTheme(); };
            searchBox = new TextBox { PlaceholderText = "Search games...", Font = new Font("Segoe UI", 11F), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BorderStyle = BorderStyle.FixedSingle };
            searchBox.TextChanged += (s, e) => DisplayFilteredGames();
            topPanel.Controls.AddRange(new Control[] { titleLabel, themeToggle, searchBox });
            searchBox.BringToFront();

            // Main panel for game cards
            gamePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20) };
            gamePanel.Scroll += (s, e) => backToTopButton.Visible = gamePanel.VerticalScroll.Value > 200;

            // "Back to Top" Button
            backToTopButton = new Button { Text = "⬆", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Size = new Size(45, 45), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Visible = false };
            backToTopButton.FlatAppearance.BorderSize = 0;
            backToTopButton.Click += (s, e) => gamePanel.ScrollControlIntoView(gamePanel.Controls.Count > 0 ? gamePanel.Controls[0] : gamePanel);
            
            // Loading Indicator
            loadingIndicator = new ProgressBar { Style = ProgressBarStyle.Marquee, Size = new Size(300, 20), Visible = false };
            
            this.Controls.AddRange(new Control[] { gamePanel, topPanel, backToTopButton, loadingIndicator });
            loadingIndicator.BringToFront();

            this.Resize += OnFormResize;
            OnFormResize(this, EventArgs.Empty); // Initial positioning
        }

        private void OnFormResize(object sender, EventArgs e)
        {
            // Center the search box
            int searchBoxWidth = (int)(topPanel.ClientSize.Width * 0.4);
            searchBox.Width = searchBoxWidth;
            searchBox.Location = new Point((topPanel.ClientSize.Width - searchBoxWidth) / 2, (topPanel.Height - searchBox.Height) / 2);
            
            // Position the back to top button
            backToTopButton.Location = new Point(this.ClientSize.Width - backToTopButton.Width - 20, this.ClientSize.Height - backToTopButton.Height - 20);

            // Center the loading indicator
            loadingIndicator.Location = new Point((this.ClientSize.Width - loadingIndicator.Width) / 2, (this.ClientSize.Height - loadingIndicator.Height) / 2);

            // Adjust card widths on resize
            if (gamePanel.Controls.Count > 0)
            {
                int cardWidth = gamePanel.ClientSize.Width - 5;
                foreach (Control card in gamePanel.Controls)
                {
                    card.Width = cardWidth;
                }
            }
        }
        
        private void ApplyTheme()
        {
            // Apply theme using the central ThemeManager
            this.BackColor = ThemeManager.Background;
            topPanel.BackColor = ThemeManager.Background;
            gamePanel.BackColor = ThemeManager.Background;

            titleLabel.ForeColor = ThemeManager.TextColor;
            searchBox.BackColor = ThemeManager.Surface;
            searchBox.ForeColor = ThemeManager.TextColor;
            
            themeToggle.Text = ThemeManager.IsDarkMode ? "☀️" : "🌙";
            themeToggle.BackColor = ThemeManager.Background;
            themeToggle.ForeColor = ThemeManager.TextColor;
            
            backToTopButton.BackColor = ThemeManager.Surface;
            backToTopButton.ForeColor = ThemeManager.TextColor;

            // Re-theme all existing game cards
            foreach (Control control in gamePanel.Controls)
            {
                if (control is Panel card)
                {
                    card.BackColor = ThemeManager.Surface;
                    // Find and theme child controls by name or type
                    var nameLabel = card.Controls.OfType<Label>().FirstOrDefault(c => c.Font.Bold);
                    if (nameLabel != null) nameLabel.ForeColor = ThemeManager.TextColor;
                    
                    var detailsLabel = card.Controls.OfType<Label>().FirstOrDefault(c => !c.Font.Bold);
                    if (detailsLabel != null) detailsLabel.ForeColor = ThemeManager.SubtleTextColor;
                    
                    var showBtn = card.Controls.OfType<Button>().FirstOrDefault();
                    if (showBtn != null) showBtn.BackColor = ThemeManager.AccentColor;
                }
            }
        }
        
        // --- DESIGN IMPROVEMENT 4: Single Method for Displaying Filtered Games ---
        private void DisplayFilteredGames()
        {
            if (gamePanel == null || !this.IsHandleCreated) return;
            
            gamePanel.SuspendLayout();
            gamePanel.Controls.Clear();
            
            var filteredGames = allGames
                .Where(g => string.IsNullOrWhiteSpace(searchBox.Text) || g.Name.Contains(searchBox.Text, StringComparison.OrdinalIgnoreCase));

            int cardWidth = gamePanel.ClientSize.Width - 5;
            foreach (var game in filteredGames)
            {
                gamePanel.Controls.Add(CreateGameCard(game, cardWidth));
            }
            
            gamePanel.ResumeLayout();
        }

        private void ShowLoading(bool isLoading)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => loadingIndicator.Visible = isLoading));
            }
            else
            {
                loadingIndicator.Visible = isLoading;
            }
        }

        private Panel CreateGameCard(Game game, int cardWidth)
        {
            var card = new Panel
            {
                Width = cardWidth,
                Height = 150,
                BackColor = ThemeManager.Surface,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(20)
            };
            
            var nameLabel = new Label
            {
                Text = game.Name,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeManager.TextColor,
                Dock = DockStyle.Top,
                Height = 35
            };

            var detailsLabel = new Label
            {
                Text = $"DirectX: {game.DirectX}  •  .NET: {game.DotNet}  •  VC++: {string.Join(", ", game.VCRedist)}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.SubtleTextColor,
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(0, 5, 0, 0)
            };

            var showBtn = new Button
            {
                Text = "View Details",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = ThemeManager.AccentColor,
                ForeColor = Color.White,
                Size = new Size(130, 40),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(card.Width - 130 - 20, card.Height - 40 - 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            showBtn.FlatAppearance.BorderSize = 0;
            showBtn.Click += (s, e) => ShowDetailsPopup(game);

            card.Controls.AddRange(new Control[] { showBtn, nameLabel, detailsLabel });
            
            return card;
        }
        
        private void ShowDetailsPopup(Game game)
        {
            // The details popup logic is self-contained and already functions well as a method.
            using (var detailsForm = new Form())
            {
                detailsForm.Text = $"{game.Name} - Details";
                detailsForm.Size = new Size(520, 550);
                detailsForm.BackColor = ThemeManager.Background;
                detailsForm.ForeColor = ThemeManager.TextColor;
                detailsForm.StartPosition = FormStartPosition.CenterParent;
                detailsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailsForm.MaximizeBox = false;
                detailsForm.MinimizeBox = false;

                var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(25), FlowDirection = FlowDirection.TopDown, WrapContents = false };
                detailsForm.Controls.Add(panel);
                
                // Helper function to add styled labels
                Action<string, Font, Color> addLabel = (text, font, color) => {
                    panel.Controls.Add(new Label { Text = text, Font = font, ForeColor = color, AutoSize = true, MaximumSize = new Size(panel.ClientSize.Width, 0) });
                };

                addLabel($"DirectX: {game.DirectX}", new Font("Segoe UI", 10F), ThemeManager.TextColor);
                addLabel($"VC++: {string.Join(", ", game.VCRedist)}", new Font("Segoe UI", 10F), ThemeManager.TextColor);
                addLabel($".NET: {game.DotNet}", new Font("Segoe UI", 10F), ThemeManager.TextColor);
                if (game.DLLs?.Any() == true) addLabel($"Missing DLLs: {string.Join(", ", game.DLLs)}", new Font("Segoe UI", 10F), ThemeManager.TextColor);
                
                addLabel("\nFixes:", new Font("Segoe UI", 12F, FontStyle.Bold), ThemeManager.TextColor);
                foreach (var fix in game.Fixes) addLabel($"• {fix}", new Font("Segoe UI", 10F), ThemeManager.SubtleTextColor);

                if (game.Downloads != null)
                {
                    addLabel("\nDownload Links:", new Font("Segoe UI", 12F, FontStyle.Bold), ThemeManager.TextColor);
                    var links = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(game.Downloads.DirectX)) links.Add("DirectX Runtime", game.Downloads.DirectX);
                    if (game.Downloads.VCRedist != null)
                    {
                        foreach (var vcLink in game.Downloads.VCRedist.OrderBy(kv => kv.Key)) links.Add($"VC++ {vcLink.Key}", vcLink.Value);
                    }
                    if (!string.IsNullOrEmpty(game.Downloads.DotNet)) links.Add($".NET Framework {game.DotNet}", game.Downloads.DotNet);

                    foreach(var link in links)
                    {
                        var linkLabel = new LinkLabel {
                            Text = $"• {link.Key}",
                            Tag = link.Value,
                            AutoSize = true,
                            Font = new Font("Segoe UI", 10F),
                            LinkColor = ThemeManager.AccentColor,
                            ActiveLinkColor = Color.Red,
                            Margin = new Padding(0, 5, 0, 0)
                        };
                        linkLabel.LinkClicked += (sender, e) => {
                            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Link.LinkData as string) { UseShellExecute = true }); }
                            catch (Exception ex) { MessageBox.Show($"Could not open the link. Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                        };
                        linkLabel.Links.Add(0, linkLabel.Text.Length, link.Value);
                        panel.Controls.Add(linkLabel);
                    }
                }
                
                detailsForm.ShowDialog(this);
            }
        }
    }
}
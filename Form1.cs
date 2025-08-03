// Form1.cs
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
    #region DataModels and Fetcher (Keep these as they are)

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

    public static class GamesFetcher
    {
        public static async Task<List<Game>> LoadGamesAsync()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "DependoApp/1.0");
            var url = "https://raw.githubusercontent.com/stuffbymax/game-dependencies-db/main/games.json";
            try { return await client.GetFromJsonAsync<List<Game>>(url) ?? new List<Game>(); }
            catch (Exception ex) { MessageBox.Show($"Failed to load game data. Please check your internet connection.\n\nError: {ex.Message}", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return new List<Game>(); }
        }
    }

    #endregion

    public partial class Form1 : Form
    {
        private List<Game> allGames = new();
        private Panel topPanel;
        private TextBox searchBox;
        private FlowLayoutPanel gamePanel;
        private ProgressBar loadingIndicator;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            ApplyTheme();
            _ = LoadGamesAndDisplayAsync();
        }

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
            this.Font = new Font("Segoe UI", 9F);

            topPanel = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(20, 0, 20, 0) };
            var titleLabel = new Label { Text = "Dependo", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            var themeToggle = new Button { Font = new Font("Segoe UI", 14F), Dock = DockStyle.Right, Size = new Size(50, 40), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            themeToggle.FlatAppearance.BorderSize = 0;
            themeToggle.Click += (s, e) => { ThemeManager.ToggleTheme(); ApplyTheme(); };
            themeToggle.Text = ThemeManager.IsDarkMode ? "☀️" : "🌙"; // Initial text

            searchBox = new TextBox { PlaceholderText = "Search games...", Font = new Font("Segoe UI", 11F), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BorderStyle = BorderStyle.FixedSingle };
            searchBox.TextChanged += (s, e) => DisplayFilteredGames();
            
            topPanel.Controls.AddRange(new Control[] { titleLabel, themeToggle, searchBox });
            searchBox.BringToFront();

            gamePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20) };
            
            loadingIndicator = new ProgressBar { Style = ProgressBarStyle.Marquee, Size = new Size(300, 20), Visible = false };
            
            this.Controls.AddRange(new Control[] { gamePanel, topPanel, loadingIndicator });
            loadingIndicator.BringToFront();

            this.Resize += (s, e) => {
                int searchBoxWidth = (int)(topPanel.ClientSize.Width * 0.4);
                searchBox.Width = searchBoxWidth;
                searchBox.Location = new Point((topPanel.ClientSize.Width - searchBoxWidth) / 2, (topPanel.Height - searchBox.Height) / 2);
                loadingIndicator.Location = new Point((this.ClientSize.Width - loadingIndicator.Width) / 2, (this.ClientSize.Height - loadingIndicator.Height) / 2);
            };
            this.Resize += (s, e) => { if (gamePanel.Controls.Count > 0) foreach (Control card in gamePanel.Controls) card.Width = gamePanel.ClientSize.Width - 5; };
            
            // Trigger resize once to set initial positions
            this.OnResize(EventArgs.Empty);
        }
        
        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            topPanel.BackColor = ThemeManager.Background;
            gamePanel.BackColor = ThemeManager.Background;

            // Apply theme to children of the top panel
            foreach (Control control in topPanel.Controls)
            {
                if(control is Label) control.ForeColor = ThemeManager.TextColor;
                if(control is Button toggle)
                {
                    toggle.Text = ThemeManager.IsDarkMode ? "☀️" : "🌙";
                    toggle.ForeColor = ThemeManager.TextColor;
                    toggle.BackColor = ThemeManager.Background;
                }
                if(control is TextBox search)
                {
                    search.BackColor = ThemeManager.Surface;
                    search.ForeColor = ThemeManager.TextColor;
                }
            }

            // Apply theme to the GameCard controls
            foreach (GameCard card in gamePanel.Controls.OfType<GameCard>())
            {
                card.ApplyTheme();
            }
        }
        
        private void DisplayFilteredGames()
        {
            if (gamePanel == null || !this.IsHandleCreated) return;
            gamePanel.SuspendLayout();
            gamePanel.Controls.Clear();
            
            var filteredGames = allGames.Where(g => string.IsNullOrWhiteSpace(searchBox.Text) || g.Name.Contains(searchBox.Text, StringComparison.OrdinalIgnoreCase));

            int cardWidth = gamePanel.ClientSize.Width - 5;
            foreach (var game in filteredGames)
            {
                // The main form now simply creates a GameCard and adds it.
                var card = new GameCard(game) { Width = cardWidth };
                gamePanel.Controls.Add(card);
            }
            gamePanel.ResumeLayout();
            
            // Re-apply theme to the newly created cards
            ApplyTheme();
        }

        private void ShowLoading(bool isLoading)
        {
            if (this.InvokeRequired) this.Invoke(new Action(() => loadingIndicator.Visible = isLoading));
            else loadingIndicator.Visible = isLoading;
        }
    }
}
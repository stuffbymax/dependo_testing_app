using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dependo_testing_app
{
    public partial class Form1 : Form
    {
        private List<Game> allGames = new();
        private Panel topPanel;
        private TextBox searchBox;
        private Button themeToggle;
        private FlowLayoutPanel gamePanel;
        private Button backToTopButton;
        private Label titleLabel;
        private bool darkMode = true;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            LoadGames();
        }

        private async void LoadGames()
        {
            allGames = await GamesFetcher.LoadGamesAsync();
            DisplayGames(allGames);
        }

        private void SetupUI()
        {
            this.Text = "Dependo - Game Dependency Viewer";
            this.StartPosition = FormStartPosition.CenterScreen;
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Size = new Size((int)(screen.Width * 0.8), (int)(screen.Height * 0.8));
            this.MinimumSize = new Size(800, 600);

            ApplyTheme();

            // Top Panel setup
            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(15),
                BackColor = this.BackColor,
            };
            this.Controls.Add(topPanel);

            // Title Label (left)
            titleLabel = new Label
            {
                Text = "Dependo",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = this.ForeColor,
                AutoSize = true,
                Location = new Point(20, 15)
            };
            topPanel.Controls.Add(titleLabel);

            // Search Box (center)
            searchBox = new TextBox
            {
                PlaceholderText = "Search games...",
                Font = new Font("Segoe UI", 14),
                Width = 300,
                Height = 38,
                Anchor = AnchorStyles.Top,
                Location = new Point((topPanel.Width - 300) / 2, 16),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            searchBox.TextChanged += SearchBox_TextChanged;
            topPanel.Controls.Add(searchBox);

            // Theme Toggle Button (right)
            themeToggle = new Button
            {
                Text = darkMode ? "☀️ Light" : "🌙 Dark",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(topPanel.Width - 110, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            themeToggle.FlatAppearance.BorderSize = 0;
            themeToggle.Click += (s, e) => ToggleTheme();
            topPanel.Controls.Add(themeToggle);

            // Adjust controls on topPanel resize
            topPanel.Resize += (s, e) =>
            {
                searchBox.Location = new Point((topPanel.Width - searchBox.Width) / 2, 16);
                themeToggle.Location = new Point(topPanel.Width - themeToggle.Width - 20, 20);
            };

            // Game Panel (scrollable, centered cards)
            gamePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = this.BackColor,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20),
            };
            this.Controls.Add(gamePanel);

            // Back to Top button (bottom center)
            backToTopButton = new Button
            {
                Text = "⬆ Back to Top",
                Height = 40,
                Width = 150,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom,
            };
            backToTopButton.FlatAppearance.BorderSize = 0;
            backToTopButton.Click += (s, e) =>
            {
                gamePanel.VerticalScroll.Value = 0;
                gamePanel.PerformLayout();
            };
            backToTopButton.Location = new Point((this.ClientSize.Width - backToTopButton.Width) / 2, this.ClientSize.Height - backToTopButton.Height - 10);
            this.Controls.Add(backToTopButton);

            // Adjust backToTopButton on resize
            this.Resize += (s, e) =>
            {
                backToTopButton.Location = new Point((this.ClientSize.Width - backToTopButton.Width) / 2, this.ClientSize.Height - backToTopButton.Height - 10);
            };
        }

        private void ApplyTheme()
        {
            if (darkMode)
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                this.ForeColor = Color.White;
            }
            else
            {
                this.BackColor = Color.White;
                this.ForeColor = Color.Black;
            }
        }

        private void ToggleTheme()
        {
            darkMode = !darkMode;
            ApplyTheme();

            // Update controls colors
            topPanel.BackColor = this.BackColor;
            titleLabel.ForeColor = this.ForeColor;

            searchBox.BackColor = darkMode ? Color.FromArgb(50, 50, 50) : Color.White;
            searchBox.ForeColor = darkMode ? Color.White : Color.Black;

            themeToggle.BackColor = darkMode ? Color.FromArgb(50, 50, 50) : Color.LightGray;
            themeToggle.ForeColor = darkMode ? Color.White : Color.Black;
            themeToggle.Text = darkMode ? "☀️ Light" : "🌙 Dark";

            gamePanel.BackColor = this.BackColor;

            backToTopButton.BackColor = darkMode ? Color.FromArgb(50, 50, 50) : Color.LightGray;
            backToTopButton.ForeColor = darkMode ? Color.White : Color.Black;

            // Refresh displayed games to apply theme colors
            DisplayGames(allGames);
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            var filtered = allGames
                .Where(g => g.Name.Contains(searchBox.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();
            DisplayGames(filtered);
        }

        private void DisplayGames(List<Game> games)
        {
            gamePanel.Controls.Clear();

            int cardWidth = Math.Min(800, gamePanel.ClientSize.Width - 40);
            if (cardWidth < 400) cardWidth = gamePanel.ClientSize.Width - 40;

            foreach (var game in games)
            {
                var card = CreateGameCard(game, cardWidth);
                gamePanel.Controls.Add(card);
            }
        }

        private Panel CreateGameCard(Game game, int cardWidth)
        {
            var card = new Panel
            {
                Width = cardWidth,
                Height = 180,
                BackColor = darkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240),
                Margin = new Padding(10),
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Rounded corners: Optional if you want to do custom drawing (WinForms default Panel doesn't support it)

            var nameLabel = new Label
            {
                Text = game.Name,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = darkMode ? Color.White : Color.Black,
                AutoSize = false,
                Height = 40,
                Dock = DockStyle.Top
            };
            card.Controls.Add(nameLabel);

            var detailsLabel = new Label
            {
                Text = $"DirectX: {game.DirectX} | .NET: {game.DotNet}",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = darkMode ? Color.LightGray : Color.DarkGray,
                AutoSize = false,
                Height = 30,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 5, 0, 0)
            };
            card.Controls.Add(detailsLabel);

            var showBtn = new Button
            {
                Text = "View Details",
                Width = 120,
                Height = 30,
                BackColor = darkMode ? Color.FromArgb(90, 90, 90) : Color.LightGray,
                ForeColor = darkMode ? Color.White : Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(15, card.Height - 45)
            };
            showBtn.FlatAppearance.BorderSize = 0;
            showBtn.Click += (s, e) => ShowDetailsPopup(game);
            card.Controls.Add(showBtn);

            return card;
        }

        private void ShowDetailsPopup(Game game)
        {
            var details = new Form
            {
                Text = game.Name + " - Details",
                Size = new Size(500, 600),
                BackColor = darkMode ? Color.FromArgb(30, 30, 30) : Color.White,
                ForeColor = darkMode ? Color.White : Color.Black,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(460, 0),
                Font = new Font("Segoe UI", 11),
                Text = $"DirectX: {game.DirectX}\n" +
                       $".NET: {game.DotNet}\n" +
                       $"VC++: {string.Join(", ", game.VCRedist)}\n" +
                       $"DLLs: {string.Join(", ", game.DLLs)}\n\nFixes:\n- {string.Join("\n- ", game.Fixes)}",
                ForeColor = darkMode ? Color.White : Color.Black,
                Location = new Point(10, 10)
            };

            details.Controls.Add(label);

            details.ShowDialog();
        }
    }
}

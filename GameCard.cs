// GameCard.cs
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace dependo_testing_app
{
    public class GameCard : UserControl
    {
        private readonly Game _game;
        private Label nameLabel;
        private Label detailsLabel;
        private Button showBtn;
        private TableLayoutPanel tableLayout;

        public GameCard(Game game)
        {
            _game = game;
            InitializeComponent();
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Height = 120;
            this.Margin = new Padding(0, 0, 0, 10);
            this.Padding = new Padding(15);
            
            tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

            nameLabel = new Label
            {
                Text = _game.Name,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            tableLayout.SetColumnSpan(nameLabel, 2);

            detailsLabel = new Label
            {
                Text = $"DirectX: {_game.DirectX}  •  .NET: {_game.DotNet}  •  VC++: {(_game.VCRedist != null ? string.Join(", ", _game.VCRedist) : "N/A")}",
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };

            showBtn = new Button
            {
                Text = "View Details",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(130, 40),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            showBtn.FlatAppearance.BorderSize = 0;
            showBtn.Click += OnShowDetailsClick;

            tableLayout.Controls.Add(nameLabel, 0, 0);
            tableLayout.Controls.Add(detailsLabel, 0, 1);
            tableLayout.Controls.Add(showBtn, 1, 1);
            
            this.Controls.Add(tableLayout);
        }

        public void ApplyTheme()
        {
            this.BackColor = ThemeManager.Surface;
            nameLabel.ForeColor = ThemeManager.TextColor;
            detailsLabel.ForeColor = ThemeManager.SubtleTextColor;
            showBtn.BackColor = ThemeManager.AccentColor;
        }

        private void OnShowDetailsClick(object sender, EventArgs e)
        {
            // The card is now responsible for showing its own details.
            using (var detailsForm = new DetailsForm(_game))
            {
                detailsForm.ShowDialog(this.FindForm());
            }
        }
    }
}
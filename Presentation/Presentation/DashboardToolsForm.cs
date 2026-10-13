using System;
using System.Drawing;
using System.Windows.Forms;

namespace Presentation
{
    public sealed class DashboardToolsForm : Form
    {
        public DashboardToolsForm(Action seedAction, Action exportAction, Action viewLogsAction)
        {
            Text = "Dashboard Tools";
            Size = new Size(420, 300);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);

            Panel card = new Panel
            {
                Location = new Point(18, 18),
                Size = new Size(368, 226),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label header = new Label
            {
                Text = "Additional Tools",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 144, 255),
                Location = new Point(16, 14)
            };

            Label subtitle = new Label
            {
                Text = "Use these support actions when needed.",
                AutoSize = true,
                ForeColor = Color.FromArgb(95, 95, 95),
                Font = new Font("Segoe UI", 9),
                Location = new Point(16, 38)
            };

            Button btnSeed = CreateButton("Seed Sample Data", new Point(16, 74), Color.FromArgb(30, 144, 255));
            btnSeed.Click += (s, e) => seedAction?.Invoke();

            Button btnExport = CreateButton("Export Requests to CSV", new Point(16, 120), Color.FromArgb(30, 144, 255));
            btnExport.Click += (s, e) => exportAction?.Invoke();

            Button btnLogs = CreateButton("View Logs", new Point(16, 166), Color.FromArgb(255, 140, 0));
            btnLogs.Click += (s, e) => viewLogsAction?.Invoke();

            card.Controls.AddRange(new Control[] { header, subtitle, btnSeed, btnExport, btnLogs });
            Controls.Add(card);
        }

        private static Button CreateButton(string text, Point location, Color backColor)
        {
            Button button = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(336, 36),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Presentation
{
    public class LogViewerForm : Form
    {
        private ListView lv;
        private TextBox txtDetails;
        private Button btnRefresh;
        private Button btnSaveAs;
        private Button btnClose;
        private CheckBox chkAutoRefresh;
        private System.Windows.Forms.Timer _autoRefreshTimer;
        private TextBox txtSearch;
        private ComboBox cmbLevelFilter;

        private readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? ".", "application.log");

        private static readonly Regex LineRegex = new Regex("^\\[(?<ts>\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2})\\] (?<level>INFO|WARN|ERROR): (?<msg>.*)$", RegexOptions.Compiled);

        public LogViewerForm()
        {
            this.Text = "Application Log Viewer";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeComponents();
            // Apply current theme
            Theme.ApplyTheme(this, Theme.IsDark);

            _autoRefreshTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            _autoRefreshTimer.Tick += (s, e) => { if (chkAutoRefresh.Checked) LoadLog(); };
        }

        private void InitializeComponents()
        {
            // Top panel with controls
            var top = new Panel { Dock = DockStyle.Top, Height = 38 }; 

            btnRefresh = new Button { Text = "Refresh", Width = 80, Location = new Point(8, 6) };
            btnRefresh.Click += (s, e) => LoadLog();

            btnSaveAs = new Button { Text = "Save As...", Width = 80, Location = new Point(96, 6) };
            btnSaveAs.Click += (s, e) => SaveAs();

            chkAutoRefresh = new CheckBox { Text = "Auto-refresh", Location = new Point(188, 9), AutoSize = true };

            txtSearch = new TextBox { PlaceholderText = "Search text...", Location = new Point(300, 6), Width = 240 };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) ApplyFilters(); };

            cmbLevelFilter = new ComboBox { Location = new Point(548, 6), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLevelFilter.Items.AddRange(new string[] { "All", "INFO", "WARN", "ERROR" });
            cmbLevelFilter.SelectedIndex = 0;
            cmbLevelFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            var btnApply = new Button { Text = "Apply", Width = 60, Location = new Point(678, 6) };
            btnApply.Click += (s, e) => ApplyFilters();

            btnClose = new Button { Text = "Close", Width = 80, Location = new Point(760, 6) };
            btnClose.Click += (s, e) => this.Close();

            top.Controls.AddRange(new Control[] { btnRefresh, btnSaveAs, chkAutoRefresh, txtSearch, cmbLevelFilter, btnApply, btnClose });

            // Main ListView
            lv = new ListView { Dock = DockStyle.Top, Height = 420, View = View.Details, FullRowSelect = true }; 
            lv.Columns.Add("Timestamp", 160);
            lv.Columns.Add("Level", 80);
            lv.Columns.Add("Message", 700);
            lv.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lv.SelectedIndexChanged += Lv_SelectedIndexChanged;

            // Details box
            txtDetails = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 10F) };

            this.Controls.Add(txtDetails);
            this.Controls.Add(lv);
            this.Controls.Add(top);

            // initial load
            LoadLog();
        }

        private void ApplyFilters()
        {
            LoadLog();
        }

        private void LoadLog()
        {
            try
            {
                lv.BeginUpdate();
                lv.Items.Clear();

                if (!File.Exists(_logPath))
                {
                    txtDetails.Text = "Log file not found: " + _logPath;
                    return;
                }

                var lines = File.ReadAllLines(_logPath);
                string search = txtSearch.Text?.Trim();
                string levelFilter = cmbLevelFilter.SelectedItem?.ToString();

                var items = new List<ListViewItem>();
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var m = LineRegex.Match(line);
                    string ts = "";
                    string lvl = "";
                    string msg = line;
                    if (m.Success)
                    {
                        ts = m.Groups["ts"].Value;
                        lvl = m.Groups["level"].Value;
                        msg = m.Groups["msg"].Value;
                    }

                    if (!string.IsNullOrEmpty(levelFilter) && levelFilter != "All" && !string.Equals(lvl, levelFilter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!string.IsNullOrEmpty(search) && !line.Contains(search, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var lvi = new ListViewItem(new[] { ts, lvl, msg });
                    // colorize level
                    if (string.Equals(lvl, "ERROR", StringComparison.OrdinalIgnoreCase))
                        lvi.BackColor = Color.FromArgb(255, 230, 230);
                    else if (string.Equals(lvl, "WARN", StringComparison.OrdinalIgnoreCase))
                        lvi.BackColor = Color.FromArgb(255, 245, 204);
                    else
                        lvi.BackColor = Color.White;

                    items.Add(lvi);
                }

                lv.Items.AddRange(items.ToArray());
                if (lv.Items.Count > 0) lv.Items[lv.Items.Count - 1].EnsureVisible();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load log: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                lv.EndUpdate();
            }
        }

        private void Lv_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv.SelectedItems.Count == 0)
            {
                txtDetails.Text = string.Empty;
                return;
            }

            var it = lv.SelectedItems[0];
            txtDetails.Text = $"[{it.SubItems[0].Text}] {it.SubItems[1].Text}: {it.SubItems[2].Text}";
        }

        private void SaveAs()
        {
            try
            {
                using (var sfd = new SaveFileDialog { Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*", FileName = Path.GetFileName(_logPath) })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;
                    File.Copy(_logPath, sfd.FileName, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save log: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}

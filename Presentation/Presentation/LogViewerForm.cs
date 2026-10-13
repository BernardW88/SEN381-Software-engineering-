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

            BackColor = Color.FromArgb(245, 247, 250);
            InitializeComponents();

            _autoRefreshTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            _autoRefreshTimer.Tick += (s, e) => { if (chkAutoRefresh.Checked) LoadLog(); };
        }

        private void InitializeComponents()
        {
            // Top panel with controls
            var top = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.White };

            btnRefresh = new Button { Text = "Refresh", Width = 90, Height = 30, Location = new Point(8, 6), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30,144,255), ForeColor = Color.White };
            btnRefresh.Click += (s, e) => LoadLog();

            btnSaveAs = new Button { Text = "Save As...", Width = 90, Height = 30, Location = new Point(104, 6), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30,144,255), ForeColor = Color.White };
            btnSaveAs.Click += (s, e) => SaveAs();

            chkAutoRefresh = new CheckBox { Text = "Auto-refresh", Location = new Point(206, 10), AutoSize = true, Font = new Font("Segoe UI", 9) };

            txtSearch = new TextBox { PlaceholderText = "Search logs...", Location = new Point(320, 8), Width = 210, Font = new Font("Segoe UI", 9) };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) ApplyFilters(); };

            cmbLevelFilter = new ComboBox { Location = new Point(536, 8), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbLevelFilter.Items.AddRange(new string[] { "All", "INFO", "WARN", "ERROR" });
            cmbLevelFilter.SelectedIndex = 0;
            cmbLevelFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            var btnApply = new Button { Text = "Apply", Width = 70, Height = 30, Location = new Point(652, 6), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255,140,0), ForeColor = Color.White };
            btnApply.Click += (s, e) => ApplyFilters();

            btnClose = new Button { Text = "Close", Width = 90, Height = 30, Location = new Point(728, 6), FlatStyle = FlatStyle.Flat, BackColor = Color.Gainsboro, ForeColor = Color.Black };
            btnClose.Click += (s, e) => this.Close();

            top.Controls.AddRange(new Control[] { btnRefresh, btnSaveAs, chkAutoRefresh, txtSearch, cmbLevelFilter, btnApply, btnClose });

            // Main ListView
            lv = new ListView { Dock = DockStyle.Top, Height = 420, View = View.Details, FullRowSelect = true, Font = new Font("Segoe UI", 9) };
            lv.Columns.Add("Timestamp", 160);
            lv.Columns.Add("Level", 80);
            lv.Columns.Add("Message", 700);
            lv.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lv.SelectedIndexChanged += Lv_SelectedIndexChanged;

            // Details box
            txtDetails = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, BackColor = Color.White, Font = new Font("Consolas", 10F) };

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
                Text = $"Application Log Viewer - {lv.Items.Count} item(s)";
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

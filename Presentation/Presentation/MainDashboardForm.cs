using BusinessLogic;
using Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Presentation
{
    public partial class MainDashboardForm : Form
    {
        private readonly ServiceRequestService _requestService = new ServiceRequestService();
        private readonly User _currentUser;
        private readonly BusinessLogic.Notifications.NotificationService _notificationService;
        private readonly BusinessLogic.Escalation.EscalationService _escalationService;

        private Label lblUserInfo;
        private Label lblStats;
        private TextBox txtSearch;
        private ComboBox cmbFilterCategory;
        private ComboBox cmbFilterStatus;
        private DataGridView dgvRequests;
        private ContextMenuStrip _gridContextMenu;
        private ToolTip _toolTip;
        private Panel filterPanel;
        private GroupBox gbNew;
        private GroupBox gbActions;
        private Panel pnlMetricCards;
        private Button btnExportCsv;

        private TextBox txtTitle;
        private TextBox txtLocation;
        private ComboBox cmbCategory;
        private ComboBox cmbPriority;
        private Button btnSubmitRequest;
        private Button btnAdvanceStatus;

        public MainDashboardForm(User user)
        {
            InitializeComponent();
            _currentUser = user ?? new User { Username = "Guest", Role = "Citizen" };
            // Initialize supporting services used by the UI.
            _notificationService = new BusinessLogic.Notifications.NotificationService(new BusinessLogic.Notifications.INotificationStrategy[] {
                new BusinessLogic.Notifications.InAppNotificationStrategy(),
                new BusinessLogic.Notifications.EmailNotificationStrategy()
            });

            _escalationService = new BusinessLogic.Escalation.EscalationService(new BusinessLogic.Escalation.IEscalationStrategy[] {
                new BusinessLogic.Escalation.ImmediateEscalationStrategy(),
                new BusinessLogic.Escalation.TimeBasedEscalationStrategy()
            });

            BuildDashboardUI();
            this.Load += MainDashboardForm_Load;
        }

        private void BuildDashboardUI()
        {
            this.Text = "CivicConnect - Community Service Requests";
            this.Size = new Size(1000, 660);
            this.StartPosition = FormStartPosition.CenterScreen;

            _toolTip = new ToolTip();

            lblUserInfo = new Label { Location = new Point(18, 12), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lblStats = new Label { Location = new Point(18, 34), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.FromArgb(30, 144, 255) };

            // Metric cards
            pnlMetricCards = new Panel { Location = new Point(12, 12), Size = new Size(960, 40), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            // create 4 simple cards
            var cardWidth = 230;
            var spacing = 10;
            var p1 = CreateMetricCard("Total", new Point(0, 0));
            var p2 = CreateMetricCard("Pending", new Point(cardWidth + spacing, 0));
            var p3 = CreateMetricCard("In Progress", new Point((cardWidth + spacing) * 2, 0));
            var p4 = CreateMetricCard("Resolved", new Point((cardWidth + spacing) * 3, 0));
            pnlMetricCards.Controls.AddRange(new Control[] { p1, p2, p3, p4 });
            this.Controls.Add(pnlMetricCards);

            // Top filter panel
            filterPanel = new Panel { Location = new Point(12, 60), Size = new Size(960, 46) };

            Label lblSearch = new Label { Text = "Search:", Location = new Point(6, 12), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(60, 8), Width = 220 };
            txtSearch.TextChanged += (s, e) => RefreshDashboard();
            txtSearch.GotFocus += (s, e) => RemoveSearchPlaceholder();
            txtSearch.LostFocus += (s, e) => EnsureSearchPlaceholder();

            Label lblFilterCat = new Label { Text = "Category:", Location = new Point(300, 12), AutoSize = true };
            cmbFilterCategory = new ComboBox { Location = new Point(360, 8), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList }; 
            cmbFilterCategory.SelectedIndexChanged += (s, e) => RefreshDashboard();

            Label lblFilterStat = new Label { Text = "Status:", Location = new Point(540, 12), AutoSize = true };
            cmbFilterStatus = new ComboBox { Location = new Point(590, 8), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterStatus.SelectedIndexChanged += (s, e) => RefreshDashboard();

            Button btnClearFilters = new Button { Text = "Clear", Location = new Point(750, 6), Width = 70, Height = 28, FlatStyle = FlatStyle.System };
            btnClearFilters.Click += (s, e) => { txtSearch.Clear(); cmbFilterCategory.SelectedIndex = 0; cmbFilterStatus.SelectedIndex = 0; };

            // smaller top-right action buttons
            // top-right action buttons removed per request

            filterPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblFilterCat, cmbFilterCategory, lblFilterStat, cmbFilterStatus, btnClearFilters });

            // Grid
            dgvRequests = new DataGridView
            {
                Location = new Point(12, 110),
                Size = new Size(960, 340),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoGenerateColumns = true,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvRequests.CellDoubleClick += DgvRequests_CellDoubleClick;
            dgvRequests.CellMouseEnter += DgvRequests_CellMouseEnter;
            dgvRequests.CellMouseLeave += DgvRequests_CellMouseLeave;
            dgvRequests.MouseLeave += (s, e) => HideHoverPreview();
            // Improve readability for demo
            dgvRequests.Font = new Font("Segoe UI", 10F);
            dgvRequests.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            // Form Entry Group
            gbNew = new GroupBox { Text = "Submit New Request", Location = new Point(12, 462), Size = new Size(640, 140) };

            Label lblAddTitle = new Label { Text = "Title:", Location = new Point(12, 28), AutoSize = true };
            txtTitle = new TextBox { Location = new Point(60, 24), Width = 260 };

            Label lblAddCat = new Label { Text = "Category:", Location = new Point(340, 28), AutoSize = true };
            cmbCategory = new ComboBox { Location = new Point(405, 24), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblAddLoc = new Label { Text = "Location:", Location = new Point(12, 62), AutoSize = true };
            txtLocation = new TextBox { Location = new Point(70, 58), Width = 250 };

            Label lblAddPrio = new Label { Text = "Priority:", Location = new Point(340, 62), AutoSize = true };
            cmbPriority = new ComboBox { Location = new Point(405, 58), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };

            btnSubmitRequest = new Button { Text = "Submit Request", Location = new Point(12, 95), Width = 150, Height = 30, BackColor = Color.FromArgb(30, 144, 255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSubmitRequest.Click += BtnSubmitRequest_Click;

            gbNew.Controls.AddRange(new Control[] { lblAddTitle, txtTitle, lblAddCat, cmbCategory, lblAddLoc, txtLocation, lblAddPrio, cmbPriority, btnSubmitRequest });

            // Actions Group
            gbActions = new GroupBox { Text = "Actions", Location = new Point(664, 462), Size = new Size(308, 140) };
            btnAdvanceStatus = new Button { Text = "Update Status", Location = new Point(12, 24), Width = 140, Height = 32, BackColor = Color.FromArgb(255,140,0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdvanceStatus.Click += BtnAdvanceStatus_Click;
            Button btnNotifyNow = new Button { Text = "Notify Selected", Location = new Point(12, 64), Width = 140, Height = 32, BackColor = Color.FromArgb(30,144,255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnNotifyNow.Click += (s, e) => NotifySelected();
            Button btnEscalateNow = new Button { Text = "Escalate Selected", Location = new Point(158, 24), Width = 140, Height = 32, BackColor = Color.FromArgb(255,140,0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnEscalateNow.Click += (s, e) => EscalateSelected();
            Button btnSeed = new Button { Text = "Seed Sample Data", Location = new Point(158, 64), Width = 140, Height = 32, FlatStyle = FlatStyle.System };
            btnSeed.Click += (s, e) => SeedSampleData();
            Button btnViewLogs = new Button { Text = "View Logs", Location = new Point(12, 100), Width = 286, Height = 28, FlatStyle = FlatStyle.System };
            btnViewLogs.Click += (s, e) => ShowLogViewer();

            gbActions.Controls.AddRange(new Control[] { btnAdvanceStatus, btnNotifyNow, btnEscalateNow, btnSeed, btnViewLogs, btnExportCsv });

            // Add to form
            this.Controls.AddRange(new Control[] { lblUserInfo, lblStats, filterPanel, dgvRequests, gbNew, gbActions });

            // Context menu and tooltips
            InitializeGridContextMenu();
            _toolTip.SetToolTip(txtSearch, "Type to filter requests by title, location or description");
            _toolTip.SetToolTip(btnSubmitRequest, "Validate and submit a new service request");
            _toolTip.SetToolTip(btnAdvanceStatus, "Advance the selected request to the next logical status");

            // Hover preview init
            InitializeHoverPreview();

            // Apply modern styling (theme, button styles, grid polish)
            ApplyModernStyling();
            // Ensure hover panel is above other controls
            _hoverPanel.BringToFront();
        }

        private void InitializeGridContextMenu()
        {
            _gridContextMenu = new ContextMenuStrip();
            var miView = new ToolStripMenuItem("View Details");
            miView.Click += (s, e) => ShowSelectedDetails();
            var miNotify = new ToolStripMenuItem("Notify");
            miNotify.Click += (s, e) => NotifySelected();
            var miEscalate = new ToolStripMenuItem("Escalate");
            miEscalate.Click += (s, e) => EscalateSelected();
            _gridContextMenu.Items.AddRange(new[] { miView, miNotify, miEscalate });
            dgvRequests.ContextMenuStrip = _gridContextMenu;
        }

        private void RemoveSearchPlaceholder()
        {
            if (txtSearch.ForeColor == Color.Gray)
            {
                txtSearch.Text = string.Empty;
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void EnsureSearchPlaceholder()
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.ForeColor = Color.Gray;
                txtSearch.Text = "Search...";
            }
        }

        private void MainDashboardForm_Load(object sender, EventArgs e)
        {
            lblUserInfo.Text = $"Active User: {_currentUser.Username} | Role: {_currentUser.Role}";

            cmbFilterCategory.Items.AddRange(new string[] { "All Categories", "Pothole", "Water Outage", "Electricity", "Waste" });
            cmbFilterCategory.SelectedIndex = 0;

            cmbFilterStatus.Items.AddRange(new string[] { "All Statuses", "Submitted", "In Progress", "Resolved", "Closed" });
            cmbFilterStatus.SelectedIndex = 0;

            cmbCategory.Items.AddRange(new string[] { "Select Category", "Pothole", "Water Outage", "Electricity", "Waste" });
            cmbCategory.SelectedIndex = 0;

            cmbPriority.Items.AddRange(new string[] { "Low", "Medium", "High", "Critical" });
            cmbPriority.SelectedIndex = 1;

            // Set initial search placeholder
            EnsureSearchPlaceholder();

            RefreshDashboard();

            // Run a quick escalation pass on load to ensure stale or critical requests are marked.
            try
            {
                var all = _requestService.GetFilteredRequests("", "All Categories", "All Statuses");
                bool anyEscalated = false;
                foreach (var r in all)
                {
                    anyEscalated |= _escalationService.RunEscalationFor(r);
                }

                if (anyEscalated)
                    RefreshDashboard();
            }
            catch { /* Keep UI resilient if escalation/init fails */ }

            // Position top-right controls so they remain visible regardless of overlapping panels
            try
            {
                int margin = 12;
                pnlMetricCards.Width = Math.Max(300, this.ClientSize.Width - (margin * 2));
                _hoverPanel?.BringToFront();
            }
            catch { }
        }

        private void RefreshDashboard()
        {
            try
            {
                var requests = _requestService.GetFilteredRequests(txtSearch.Text, cmbFilterCategory.SelectedItem?.ToString(), cmbFilterStatus.SelectedItem?.ToString());
                dgvRequests.DataSource = null;
                dgvRequests.DataSource = requests;

                FormatGrid();

                var (total, pending, inProgress, resolved) = _requestService.GetDashboardMetrics();
                lblStats.Text = $"CivicConnect Metrics: {total} Total Requests | {pending} Pending Triage | {inProgress} In Progress | {resolved} Resolved/Closed";
                UpdateMetricCards(total, pending, inProgress, resolved);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading service requests: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FormatGrid()
        {
            if (dgvRequests.Columns.Count == 0) return;

            // Hide internal identifier and long description by default
            if (dgvRequests.Columns["RequestID"] != null) dgvRequests.Columns["RequestID"].Visible = false;
            if (dgvRequests.Columns["Description"] != null) dgvRequests.Columns["Description"].Visible = false;

            dgvRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRequests.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 250);
            dgvRequests.EnableHeadersVisualStyles = false;
            dgvRequests.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 144, 255);
            dgvRequests.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRequests.ColumnHeadersDefaultCellStyle.Font = new Font(dgvRequests.Font, FontStyle.Bold);

            // Format date column if present
            if (dgvRequests.Columns["DateCreated"] != null)
            {
                dgvRequests.Columns["DateCreated"].DefaultCellStyle.Format = "g";
                dgvRequests.Columns["DateCreated"].HeaderText = "Created";
                dgvRequests.Columns["DateCreated"].Width = 140;
            }

            // Adjust priority column visuals
            if (dgvRequests.Columns["Priority"] != null)
                dgvRequests.Columns["Priority"].Width = 90;

            // Status Visual Indicator Highlight
            foreach (DataGridViewRow row in dgvRequests.Rows)
            {
                if (row.DataBoundItem is ServiceRequest req)
                {
                    row.DefaultCellStyle.BackColor = req.Status switch
                    {
                        "Submitted" => Color.FromArgb(255, 249, 196),   // Light amber
                        "In Progress" => Color.FromArgb(255, 250, 205), // Pale yellow
                        "Resolved" => Color.FromArgb(212, 237, 218),    // Soft Green
                        "Closed" => Color.FromArgb(226, 227, 229),      // Soft Gray
                        _ => Color.White
                    };

                    if (!string.IsNullOrWhiteSpace(req.Description))
                    {
                        row.Cells[0].ToolTipText = req.Description.Length > 200
                            ? req.Description.Substring(0, 200) + "..."
                            : req.Description;
                    }
                }
            }
        }

        private void BtnSubmitRequest_Click(object sender, EventArgs e)
        {
            try
            {
                var created = _requestService.CreateNewRequest(txtTitle.Text, cmbCategory.SelectedItem?.ToString(), txtLocation.Text, "", cmbPriority.SelectedItem?.ToString(), _currentUser.Username);
                if (created != null)
                {
                    MessageBox.Show("Service request logged successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtTitle.Clear();
                    txtLocation.Clear();
                    cmbCategory.SelectedIndex = 0;
                    RefreshDashboard();

                    // Notify relevant parties about the new request (UI and email by default)
                    try
                    {
                        _notificationService.NotifyAll(created, $"New service request submitted: {created.Title}", _currentUser.Username ?? "system");
                    }
                    catch { }
                }
                else
                {
                    MessageBox.Show("Failed to create service request.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnAdvanceStatus_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a service request from the table.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ServiceRequest selected = (ServiceRequest)dgvRequests.SelectedRows[0].DataBoundItem;

            // Ask the service for valid next canonical statuses
            var options = _requestService.GetValidNextStatuses(selected.Status);
            if (options == null || options.Count == 0)
            {
                MessageBox.Show("No further status transitions available for the selected request.", "No Actions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var picker = new StatusPickerForm(selected.Status, options))
            {
                var dr = picker.ShowDialog(this);
                if (dr != DialogResult.OK || string.IsNullOrWhiteSpace(picker.SelectedStatus)) return;

                string targetStatus = picker.SelectedStatus;
                try
                {
                    if (_requestService.AdvanceRequestStatus(selected.RequestID, selected.Status, targetStatus, _currentUser.Username))
                    {
                        // Get the updated record and notify
                        var updated = _requestService.GetRequestById(selected.RequestID);
                        MessageBox.Show($"Request '{selected.Title}' status advanced to '{targetStatus}'!", "Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshDashboard();

                        try
                        {
                            if (updated != null)
                                _notificationService.NotifyAll(updated, $"Request '{updated.Title}' status changed to {updated.Status}", _currentUser.Username ?? "system");
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "State Machine Violation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void DgvRequests_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ShowSelectedDetails();
        }

        private void DgvRequests_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                var row = dgvRequests.Rows[e.RowIndex];
                if (row?.DataBoundItem is ServiceRequest r)
                {
                    // compute location just below the cell
                    var cellRect = dgvRequests.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                    var screenPoint = dgvRequests.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
                    var clientPoint = this.PointToClient(screenPoint);
                    ShowHoverPreview(r, clientPoint);
                }
            }
            catch { }
        }

        private void DgvRequests_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                HideHoverPreview();
            }
            catch { }
        }

        // Hover preview panel and animation
        private Panel _hoverPanel;
        private Label _hoverTitleLabel;
        private Label _hoverBodyLabel;
        private System.Windows.Forms.Timer _hoverTimer;
        private int _hoverTargetHeight = 0;
        private bool _hoverExpanding = false;
        private ServiceRequest _hoveredRequest = null;

        private void InitializeHoverPreview()
        {
            _hoverPanel = new Panel
            {
                Size = new Size(360, 0),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            _hoverTitleLabel = new Label { Location = new Point(8, 6), AutoSize = false, Size = new Size(340, 20), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            _hoverBodyLabel = new Label { Location = new Point(8, 28), AutoSize = false, Size = new Size(340, 80), Font = new Font("Segoe UI", 8F), ForeColor = Color.DimGray };

            _hoverPanel.Controls.Add(_hoverTitleLabel);
            _hoverPanel.Controls.Add(_hoverBodyLabel);
            this.Controls.Add(_hoverPanel);

            _hoverTimer = new System.Windows.Forms.Timer { Interval = 12 };
            _hoverTimer.Tick += HoverTimer_Tick;
        }

        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                int step = 12;
                if (_hoverExpanding)
                {
                    if (_hoverPanel.Height < _hoverTargetHeight)
                    {
                        _hoverPanel.Height = Math.Min(_hoverPanel.Height + step, _hoverTargetHeight);
                    }
                    else
                    {
                        _hoverTimer.Stop();
                    }
                }
                else
                {
                    if (_hoverPanel.Height > 0)
                    {
                        _hoverPanel.Height = Math.Max(0, _hoverPanel.Height - step);
                    }
                    else
                    {
                        _hoverTimer.Stop();
                        _hoverPanel.Visible = false;
                        _hoveredRequest = null;
                    }
                }
            }
            catch { _hoverTimer.Stop(); }
        }

        private void ShowHoverPreview(ServiceRequest r, Point clientLocation)
        {
            if (r == null) return;
            _hoveredRequest = r;
            _hoverTitleLabel.Text = r.Title;
            var body = $"Category: {r.Category}  |  Priority: {r.Priority}\nLocation: {r.Location}\nStatus: {r.Status}";
            _hoverBodyLabel.Text = body;

            // position panel near the mouse but keep inside form bounds
            int x = clientLocation.X + 16;
            int y = clientLocation.Y + 16;
            if (x + _hoverPanel.Width > this.ClientSize.Width) x = this.ClientSize.Width - _hoverPanel.Width - 8;
            if (y + 160 > this.ClientSize.Height) y = clientLocation.Y - 160;
            if (y < 0) y = 8;

            _hoverPanel.Location = new Point(x, y);
            _hoverPanel.Height = 0;
            _hoverTargetHeight = 120;
            _hoverPanel.Visible = true;
            _hoverExpanding = true;
            _hoverTimer.Start();
        }

        private void HideHoverPreview()
        {
            _hoverExpanding = false;
            _hoverTimer.Start();
        }

        private Panel CreateMetricCard(string title, Point location)
        {
            var p = new Panel { Size = new Size(230, 36), Location = location, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var lblTitle = new Label { Text = title, Location = new Point(8, 4), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Regular) };
            var lblValue = new Label { Name = "_val", Text = "0", Location = new Point(8, 16), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(30,144,255) };
            p.Controls.Add(lblTitle);
            p.Controls.Add(lblValue);
            return p;
        }

        private void UpdateMetricCards(int total, int pending, int inProgress, int resolved)
        {
            try
            {
                if (pnlMetricCards == null) return;
                if (pnlMetricCards.Controls.Count >= 4)
                {
                    pnlMetricCards.Controls[0].Controls["_val"].Text = total.ToString();
                    pnlMetricCards.Controls[1].Controls["_val"].Text = pending.ToString();
                    pnlMetricCards.Controls[2].Controls["_val"].Text = inProgress.ToString();
                    pnlMetricCards.Controls[3].Controls["_val"].Text = resolved.ToString();
                }
            }
            catch { }
        }

        private void ApplyModernStyling()
        {
            try
            {
                var primary = Color.FromArgb(30, 144, 255);
                var accent = Color.FromArgb(255, 140, 0);
                if (Theme.IsDark)
                {
                    this.BackColor = Color.FromArgb(30, 34, 40);
                }
                else
                {
                    this.BackColor = Color.FromArgb(245, 247, 250);
                }

                // DataGridView modern look
                dgvRequests.EnableHeadersVisualStyles = false;
                dgvRequests.ColumnHeadersDefaultCellStyle.BackColor = primary;
                dgvRequests.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvRequests.ColumnHeadersHeight = 36;
                dgvRequests.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dgvRequests.GridColor = Color.FromArgb(220, 220, 220);
                dgvRequests.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvRequests.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 255);
                dgvRequests.DefaultCellStyle.SelectionForeColor = Color.Black;
                dgvRequests.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 252);
                dgvRequests.BackgroundColor = Color.White;
                dgvRequests.RowTemplate.Height = 28;

                // Style all buttons consistently
                StyleButtons(this);

                // Ensure labels and text controls are themed correctly
                try { Theme.ApplyTheme(this, Theme.IsDark); } catch { }

                // Apply rounded corners to the main form and panels
                SetRoundedRegion(this, 12);

                // Style metric cards
                if (pnlMetricCards != null && pnlMetricCards.Controls.Count >= 4)
                {
                    var p0 = pnlMetricCards.Controls[0];
                    var p1 = pnlMetricCards.Controls[1];
                    var p2 = pnlMetricCards.Controls[2];
                    var p3 = pnlMetricCards.Controls[3];

                    p0.BackColor = primary;
                    p1.BackColor = Color.FromArgb(255, 200, 130);
                    p2.BackColor = Color.FromArgb(70, 160, 255);
                    p3.BackColor = Color.FromArgb(120, 200, 170);

                    foreach (Control p in new Control[] { p0, p1, p2, p3 })
                    {
                        foreach (Control c in p.Controls)
                        {
                            c.ForeColor = Color.White;
                        }
                        SetRoundedRegion(p, 8);
                    }
                }
            }
            catch { }
        }

        private void SetRoundedRegion(Control ctrl, int radius)
        {
            try
            {
                var rect = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
                using (GraphicsPath gp = new GraphicsPath())
                {
                    int d = radius * 2;
                    gp.AddArc(rect.X, rect.Y, d, d, 180, 90);
                    gp.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                    gp.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                    gp.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                    gp.CloseFigure();
                    ctrl.Region = new Region(gp);
                }
            }
            catch { }
        }

        private void StyleButtons(Control parent)
        {
            var primary = Color.FromArgb(30, 144, 255);
            var accent = Color.FromArgb(255, 140, 0);
            foreach (Control c in parent.Controls)
            {
                if (c is Button b)
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 0;
                    b.ForeColor = Color.White;
                    if (b.Text.IndexOf("Escalate", StringComparison.OrdinalIgnoreCase) >= 0 || b.Text.IndexOf("Update", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        b.BackColor = accent;
                    }
                    else if (b.Text.IndexOf("Submit", StringComparison.OrdinalIgnoreCase) >= 0 || b.Text.IndexOf("Notify", StringComparison.OrdinalIgnoreCase) >= 0 || b.Text.IndexOf("Export", StringComparison.OrdinalIgnoreCase) >= 0 || b.Text.IndexOf("Presentation", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        b.BackColor = primary;
                    }
                    else
                    {
                        b.BackColor = Color.White;
                        b.ForeColor = primary;
                        b.FlatAppearance.BorderSize = 1;
                        b.FlatAppearance.BorderColor = primary;
                    }
                }

                // Recurse
                if (c.HasChildren) StyleButtons(c);
            }
        }


        private void ExportGridToCsv()
        {
            try
            {
                if (dgvRequests.DataSource == null || dgvRequests.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var sfd = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", FileName = "requests.csv" })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;

                    var sb = new System.Text.StringBuilder();
                    // headers
                    var cols = new List<string>();
                    foreach (DataGridViewColumn c in dgvRequests.Columns)
                    {
                        if (c.Visible)
                            cols.Add(c.HeaderText);
                    }
                    sb.AppendLine(string.Join(",", cols));

                    foreach (DataGridViewRow row in dgvRequests.Rows)
                    {
                        var cells = new List<string>();
                        foreach (DataGridViewColumn c in dgvRequests.Columns)
                        {
                            if (!c.Visible) continue;
                            var val = row.Cells[c.Index].Value;
                            var s = val == null ? "" : val.ToString().Replace("\"", "\"\"");
                            if (s.Contains(",") || s.Contains("\n")) s = "\"" + s + "\"";
                            cells.Add(s);
                        }
                        sb.AppendLine(string.Join(",", cells));
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString());
                    MessageBox.Show("Export complete.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowSelectedDetails()
        {
            if (dgvRequests.SelectedRows.Count == 0) return;

            var req = (ServiceRequest)dgvRequests.SelectedRows[0].DataBoundItem;
            string details =
                $"Title: {req.Title}\n" +
                $"Category: {req.Category}\n" +
                $"Location: {req.Location}\n" +
                $"Priority: {req.Priority}\n" +
                $"Status: {req.Status}\n" +
                $"Created: {req.DateCreated:g}\n\n" +
                $"Description:\n{req.Description}";

            MessageBox.Show(details, "Request Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void NotifySelected()
        {
            if (dgvRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a request to notify about.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var req = (ServiceRequest)dgvRequests.SelectedRows[0].DataBoundItem;
            try
            {
                _notificationService.NotifyAll(req, $"Update on request: {req.Title}", _currentUser.Username ?? "system");
                MessageBox.Show("Notification(s) sent.", "Notified", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Notification failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void EscalateSelected()
        {
            if (dgvRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a request to escalate.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var req = (ServiceRequest)dgvRequests.SelectedRows[0].DataBoundItem;
            try
            {
                bool changed = _escalationService.RunEscalationFor(req);
                MessageBox.Show(changed ? "Escalation applied." : "No escalation required.", "Escalation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (changed) RefreshDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Escalation failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SeedSampleData()
        {
            try
            {
                string actor = _currentUser.Username ?? "system";
                _requestService.CreateNewRequest("Pothole on Main St", "Pothole", "Main St", "Large pothole near the traffic light.", "High", actor);
                _requestService.CreateNewRequest("Water outage in Block C", "Water Outage", "Block C", "No water supply since morning.", "Critical", actor);
                _requestService.CreateNewRequest("Streetlight flickering", "Electricity", "Oak Avenue", "Intermittent streetlight at the corner.", "Medium", actor);
                _requestService.CreateNewRequest("Missed garbage pickup", "Waste", "Pine Road", "Garbage was not collected this week.", "Low", actor);
                RefreshDashboard();
                MessageBox.Show("Sample data seeded successfully.", "Seed Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Seeding failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowLogViewer()
        {
            try
            {
                using (var viewer = new LogViewerForm())
                {
                    viewer.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open log viewer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
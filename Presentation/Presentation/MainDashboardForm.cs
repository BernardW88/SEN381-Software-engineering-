using BusinessLogic;
using Models;
using System;
using System.Drawing;
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

            // Top filter panel
            Panel filterPanel = new Panel { Location = new Point(12, 60), Size = new Size(960, 46) };

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

            // Form Entry Group
            GroupBox gbNew = new GroupBox { Text = "Submit New Request", Location = new Point(12, 462), Size = new Size(640, 140) };

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
            GroupBox gbActions = new GroupBox { Text = "Actions", Location = new Point(664, 462), Size = new Size(308, 140) };
            btnAdvanceStatus = new Button { Text = "Update Status", Location = new Point(12, 24), Width = 140, Height = 32, BackColor = Color.FromArgb(255,140,0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdvanceStatus.Click += BtnAdvanceStatus_Click;
            Button btnNotifyNow = new Button { Text = "Notify Selected", Location = new Point(12, 64), Width = 140, Height = 32, BackColor = Color.FromArgb(30,144,255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnNotifyNow.Click += (s, e) => NotifySelected();
            Button btnEscalateNow = new Button { Text = "Escalate Selected", Location = new Point(158, 24), Width = 140, Height = 32, BackColor = Color.FromArgb(255,140,0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnEscalateNow.Click += (s, e) => EscalateSelected();

            gbActions.Controls.AddRange(new Control[] { btnAdvanceStatus, btnNotifyNow, btnEscalateNow });

            // Add to form
            this.Controls.AddRange(new Control[] { lblUserInfo, lblStats, filterPanel, dgvRequests, gbNew, gbActions });

            // Context menu and tooltips
            InitializeGridContextMenu();
            _toolTip.SetToolTip(txtSearch, "Type to filter requests by title, location or description");
            _toolTip.SetToolTip(btnSubmitRequest, "Validate and submit a new service request");
            _toolTip.SetToolTip(btnAdvanceStatus, "Advance the selected request to the next logical status");
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

            // Simple status selector dialog prompt
            string targetStatus = selected.Status switch
            {
                "Submitted" => "In Progress",
                "In Progress" => "Resolved",
                "Resolved" => "Closed",
                _ => "Closed"
            };

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

        private void DgvRequests_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ShowSelectedDetails();
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
    }
}
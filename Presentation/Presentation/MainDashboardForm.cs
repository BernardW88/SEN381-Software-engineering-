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

        private Label lblUserInfo;
        private Label lblStats;
        private TextBox txtSearch;
        private ComboBox cmbFilterCategory;
        private ComboBox cmbFilterStatus;
        private DataGridView dgvRequests;

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
            BuildDashboardUI();
            this.Load += MainDashboardForm_Load;
        }

        private void BuildDashboardUI()
        {
            this.Text = "CivicConnect - Community Service Request Management Platform";
            this.Size = new Size(950, 620);
            this.StartPosition = FormStartPosition.CenterScreen;

            lblUserInfo = new Label { Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lblStats = new Label { Location = new Point(20, 38), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.DarkBlue };

            // Filters
            Label lblSearch = new Label { Text = "Search:", Location = new Point(20, 70), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(70, 67), Width = 150 };
            txtSearch.TextChanged += (s, e) => RefreshDashboard();

            Label lblFilterCat = new Label { Text = "Category:", Location = new Point(235, 70), AutoSize = true };
            cmbFilterCategory = new ComboBox { Location = new Point(300, 67), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterCategory.SelectedIndexChanged += (s, e) => RefreshDashboard();

            Label lblFilterStat = new Label { Text = "Status:", Location = new Point(455, 70), AutoSize = true };
            cmbFilterStatus = new ComboBox { Location = new Point(505, 67), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterStatus.SelectedIndexChanged += (s, e) => RefreshDashboard();

            // Grid
            dgvRequests = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(890, 310),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            // Form Entry Controls
            Label lblAddTitle = new Label { Text = "Title:", Location = new Point(20, 430), AutoSize = true };
            txtTitle = new TextBox { Location = new Point(60, 427), Width = 160 };

            Label lblAddCat = new Label { Text = "Category:", Location = new Point(230, 430), AutoSize = true };
            cmbCategory = new ComboBox { Location = new Point(295, 427), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblAddLoc = new Label { Text = "Location:", Location = new Point(435, 430), AutoSize = true };
            txtLocation = new TextBox { Location = new Point(495, 427), Width = 160 };

            Label lblAddPrio = new Label { Text = "Priority:", Location = new Point(665, 430), AutoSize = true };
            cmbPriority = new ComboBox { Location = new Point(715, 427), Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };

            btnSubmitRequest = new Button { Text = "Submit Request", Location = new Point(20, 470), Width = 140, Height = 32, BackColor = Color.ForestGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSubmitRequest.Click += BtnSubmitRequest_Click;

            btnAdvanceStatus = new Button { Text = "Update Request Status", Location = new Point(170, 470), Width = 160, Height = 32, BackColor = Color.DarkOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdvanceStatus.Click += BtnAdvanceStatus_Click;

            this.Controls.AddRange(new Control[] {
                lblUserInfo, lblStats, lblSearch, txtSearch, lblFilterCat, cmbFilterCategory, lblFilterStat, cmbFilterStatus,
                dgvRequests, lblAddTitle, txtTitle, lblAddCat, cmbCategory, lblAddLoc, txtLocation, lblAddPrio, cmbPriority,
                btnSubmitRequest, btnAdvanceStatus
            });
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

            RefreshDashboard();
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

            if (dgvRequests.Columns["RequestID"] != null) dgvRequests.Columns["RequestID"].Visible = false;
            dgvRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Status Visual Indicator Highlight
            foreach (DataGridViewRow row in dgvRequests.Rows)
            {
                ServiceRequest req = (ServiceRequest)row.DataBoundItem;
                if (req != null)
                {
                    row.DefaultCellStyle.BackColor = req.Status switch
                    {
                        "Submitted" => Color.FromArgb(255, 243, 205),   // Soft Amber/Yellow
                        "In Progress" => Color.FromArgb(209, 236, 241), // Soft Blue
                        "Resolved" => Color.FromArgb(212, 237, 218),    // Soft Green
                        "Closed" => Color.FromArgb(226, 227, 229),      // Soft Gray
                        _ => Color.White
                    };
                }
            }
        }

        private void BtnSubmitRequest_Click(object sender, EventArgs e)
        {
            try
            {
                if (_requestService.CreateNewRequest(txtTitle.Text, cmbCategory.SelectedItem?.ToString(), txtLocation.Text, "", cmbPriority.SelectedItem?.ToString(), _currentUser.Username))
                {
                    MessageBox.Show("Service request logged successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtTitle.Clear();
                    txtLocation.Clear();
                    cmbCategory.SelectedIndex = 0;
                    RefreshDashboard();
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
                    MessageBox.Show($"Request '{selected.Title}' status advanced to '{targetStatus}'!", "Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshDashboard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "State Machine Violation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
using BusinessLogic;
using Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Presentation
{
    public partial class MainDashboardForm : Form
    {
        private readonly ToolService _toolService = new ToolService();
        private readonly User _currentUser;

        private Label lblUserInfo;
        private Label lblStats;
        private TextBox txtSearch;
        private ComboBox cmbFilterCategory;
        private DataGridView dgvTools;
        private TextBox txtToolName;
        private ComboBox cmbAddCategory;
        private Button btnAddTool;
        private Button btnToggleStatus;

        public MainDashboardForm(User user)
        {
            InitializeComponent();
            _currentUser = user ?? new User { Username = "Guest", Role = "Member" };
            BuildDashboardUI();
            this.Load += MainDashboardForm_Load;
        }

        private void BuildDashboardUI()
        {
            this.Text = "City Makerspace Management System";
            this.Size = new Size(850, 580);
            this.StartPosition = FormStartPosition.CenterScreen;

            lblUserInfo = new Label { Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lblStats = new Label { Location = new Point(20, 40), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.DarkBlue };

            Label lblSearch = new Label { Text = "Search:", Location = new Point(20, 75), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(75, 72), Width = 180 };
            txtSearch.TextChanged += (s, e) => RefreshDashboard();

            Label lblFilter = new Label { Text = "Category:", Location = new Point(275, 75), AutoSize = true };
            cmbFilterCategory = new ComboBox { Location = new Point(345, 72), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterCategory.SelectedIndexChanged += (s, e) => RefreshDashboard();

            dgvTools = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(790, 300),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            Label lblAddName = new Label { Text = "Tool Name:", Location = new Point(20, 430), AutoSize = true };
            txtToolName = new TextBox { Location = new Point(95, 427), Width = 180 };

            Label lblAddCat = new Label { Text = "Category:", Location = new Point(295, 430), AutoSize = true };
            cmbAddCategory = new ComboBox { Location = new Point(365, 427), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };

            btnAddTool = new Button { Text = "Add New Tool", Location = new Point(540, 425), Width = 120, Height = 28, BackColor = Color.ForestGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddTool.Click += BtnAddTool_Click;

            btnToggleStatus = new Button { Text = "Toggle Borrow / Return", Location = new Point(670, 425), Width = 140, Height = 28, BackColor = Color.DarkOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnToggleStatus.Click += BtnToggleStatus_Click;

            this.Controls.AddRange(new Control[] {
                lblUserInfo, lblStats, lblSearch, txtSearch, lblFilter, cmbFilterCategory,
                dgvTools, lblAddName, txtToolName, lblAddCat, cmbAddCategory, btnAddTool, btnToggleStatus
            });
        }

        private void MainDashboardForm_Load(object sender, EventArgs e)
        {
            lblUserInfo.Text = $"Active User: {_currentUser.Username} | Role: {_currentUser.Role}";

            cmbFilterCategory.Items.AddRange(new string[] { "All Categories", "Power Tools", "Hand Tools", "Digital Fabrication" });
            cmbFilterCategory.SelectedIndex = 0;

            cmbAddCategory.Items.AddRange(new string[] { "Select Category", "Power Tools", "Hand Tools", "Digital Fabrication" });
            cmbAddCategory.SelectedIndex = 0;

            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            try
            {
                string search = txtSearch.Text;
                string category = cmbFilterCategory.SelectedItem?.ToString();

                var tools = _toolService.GetFilteredInventory(search, category);
                dgvTools.DataSource = null;
                dgvTools.DataSource = tools;

                FormatGrid();

                var (total, available, checkedOut) = _toolService.GetInventoryStats();
                lblStats.Text = $"Inventory Metrics: {total} Total Tools | {available} Available | {checkedOut} Checked Out";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading inventory data: " + ex.Message, "Database Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FormatGrid()
        {
            if (dgvTools.Columns.Count == 0) return;

            if (dgvTools.Columns["ToolID"] != null) dgvTools.Columns["ToolID"].Visible = false;
            if (dgvTools.Columns["IsAvailable"] != null) dgvTools.Columns["IsAvailable"].Visible = false;

            dgvTools.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            foreach (DataGridViewRow row in dgvTools.Rows)
            {
                Tool tool = (Tool)row.DataBoundItem;
                if (tool != null)
                {
                    row.DefaultCellStyle.BackColor = tool.IsAvailable
                        ? Color.FromArgb(235, 247, 238)
                        : Color.FromArgb(253, 237, 237);
                }
            }
        }

        private void BtnAddTool_Click(object sender, EventArgs e)
        {
            try
            {
                if (_toolService.AddNewTool(txtToolName.Text, cmbAddCategory.SelectedItem?.ToString(), _currentUser.Username))
                {
                    MessageBox.Show("New tool added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtToolName.Clear();
                    cmbAddCategory.SelectedIndex = 0;
                    RefreshDashboard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnToggleStatus_Click(object sender, EventArgs e)
        {
            if (dgvTools.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a tool from the table.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Tool selectedTool = (Tool)dgvTools.SelectedRows[0].DataBoundItem;
            string action = selectedTool.IsAvailable ? "check out" : "return";

            DialogResult confirm = MessageBox.Show($"Are you sure you want to {action} '{selectedTool.ToolName}'?",
                "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                if (_toolService.ToggleBorrowReturn(selectedTool.ToolID, selectedTool.IsAvailable, _currentUser.Username))
                {
                    RefreshDashboard();
                }
            }
        }
    }
}
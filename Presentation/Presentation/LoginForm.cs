using BusinessLogic;
using Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Presentation
{
    public partial class LoginForm : Form
    {
        private readonly UserService _userService = new UserService();

        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblStatus;

        public LoginForm()
        {
            InitializeComponent();
            BuildLoginFormUI();
        }

        private void BuildLoginFormUI()
        {
            this.Text = "City Makerspace - Login";
            this.Size = new Size(380, 260);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label { Text = "Makerspace Portal Login", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(30, 20), AutoSize = true };
            Label lblUser = new Label { Text = "Username:", Location = new Point(30, 60), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(120, 57), Width = 200, Text = "admin" };

            Label lblPass = new Label { Text = "Password:", Location = new Point(30, 100), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(120, 97), Width = 200, PasswordChar = '*', Text = "admin123" };

            btnLogin = new Button { Text = "Login", Location = new Point(120, 135), Width = 200, Height = 32, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogin.Click += BtnLogin_Click;

            lblStatus = new Label { Location = new Point(30, 180), AutoSize = true, ForeColor = Color.Red };

            this.Controls.AddRange(new Control[] { lblTitle, lblUser, txtUsername, lblPass, txtPassword, btnLogin, lblStatus });
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "";
                User loggedInUser = _userService.AuthenticateUser(txtUsername.Text, txtPassword.Text);

                MainDashboardForm dashboard = new MainDashboardForm(loggedInUser);
                this.Hide();
                dashboard.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
            }
        }
    }
}
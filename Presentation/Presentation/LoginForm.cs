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
            Text = "CivicConnect - Sign In";
            Size = new Size(460, 360);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);

            Panel card = new Panel
            {
                Location = new Point(40, 30),
                Size = new Size(360, 270),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "Welcome to CivicConnect",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 144, 255),
                Location = new Point(24, 20),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = "Sign in to manage community service requests",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(90, 90, 90),
                Location = new Point(24, 50),
                AutoSize = true
            };

            Label lblUser = new Label { Text = "Username", Location = new Point(24, 88), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtUsername = new TextBox { Location = new Point(24, 108), Width = 308, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };

            Label lblPass = new Label { Text = "Password", Location = new Point(24, 144), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtPassword = new TextBox { Location = new Point(24, 164), Width = 308, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, PasswordChar = '*' };

            btnLogin = new Button
            {
                Text = "Sign In",
                Location = new Point(24, 204),
                Width = 308,
                Height = 36,
                BackColor = Color.FromArgb(30, 144, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            lblStatus = new Label
            {
                Location = new Point(24, 246),
                AutoSize = true,
                ForeColor = Color.FromArgb(192, 57, 43),
                Font = new Font("Segoe UI", 9)
            };

            AcceptButton = btnLogin;

            card.Controls.AddRange(new Control[]
            {
                lblTitle, lblSubtitle, lblUser, txtUsername, lblPass, txtPassword, btnLogin, lblStatus
            });

            Controls.Add(card);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "";

                string username = txtUsername.Text?.Trim();
                string password = txtPassword.Text ?? string.Empty;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    lblStatus.Text = "Please enter username and password.";
                    return;
                }

                btnLogin.Enabled = false;
                User loggedInUser = _userService.AuthenticateUser(username, password);

                MainDashboardForm dashboard = new MainDashboardForm(loggedInUser);
                Hide();
                dashboard.ShowDialog();
                Close();
            }
            catch
            {
                lblStatus.Text = "Invalid credentials. Please try again.";
                txtPassword.Clear();
                txtPassword.Focus();
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }
    }
}

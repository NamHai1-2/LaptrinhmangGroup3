using _1_SharedLibrary.Models;
using _3_ChatClient.Network;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3_ChatClient.UI
{
    public partial class LoginForm : Form
    {
        private readonly TcpClientHelper _clientHelper;
        private MainChatForm _mainChatForm;
        private bool _isRegistrationMode = false;

        private TextBox _usernameTextBox;
        private TextBox _passwordTextBox;
        private TextBox _emailTextBox;
        private Label _emailLabel;
        private Label _statusLabel;
        private Button _loginButton;
        private Button _registerButton;
        private Button _toggleModeButton;

        public LoginForm()
        {
            _clientHelper = new TcpClientHelper();

            _clientHelper.OnMessageReceived += OnServerMessageReceived;
            // _clientHelper.OnError += OnError; 
             //_clientHelper.OnDisconnected += OnDisconnected;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Chat Application - Login";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.FromArgb(240, 240, 240);


            _usernameTextBox = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(50, 145), Size = new Size(300, 30) };
            _passwordTextBox = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(50, 215), Size = new Size(300, 30), UseSystemPasswordChar = true };
            _emailTextBox = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(50, 285), Size = new Size(300, 30), Visible = false };
            _emailLabel = new Label { Text = "Email:", Location = new Point(50, 260), Visible = false };
            _statusLabel = new Label { Text = "Ready to connect", Location = new Point(50, 430), Size = new Size(300, 20), TextAlign = ContentAlignment.MiddleCenter };

            _loginButton = new Button { Text = "Login", Location = new Point(50, 340), Size = new Size(140, 40), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _registerButton = new Button { Text = "Register", Location = new Point(210, 340), Size = new Size(140, 40), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _toggleModeButton = new Button { Text = "New to Chat? Register", Location = new Point(50, 390), Size = new Size(300, 30), FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(52, 152, 219) };

            _loginButton.Click += (sender, e) => HandleLoginAsync();
            _toggleModeButton.Click += (sender, e) => ToggleMode();

            this.Controls.Add(new Label { Text = "Chat Application", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(50, 30), Size = new Size(300, 40), TextAlign = ContentAlignment.MiddleCenter });
            this.Controls.Add(new Label { Text = "Username:", Location = new Point(50, 120) });
            this.Controls.Add(new Label { Text = "Password:", Location = new Point(50, 190) });
            this.Controls.Add(_usernameTextBox);
            this.Controls.Add(_passwordTextBox);
            this.Controls.Add(_emailLabel);
            this.Controls.Add(_emailTextBox);
            this.Controls.Add(_loginButton);
            this.Controls.Add(_registerButton);
            this.Controls.Add(_toggleModeButton);
            this.Controls.Add(_statusLabel);

            this.ResumeLayout(false);
        }

        private void ToggleMode()
        {
            _isRegistrationMode = !_isRegistrationMode;
            _emailTextBox.Visible = _isRegistrationMode;
            _emailLabel.Visible = _isRegistrationMode;

            if (_isRegistrationMode)
            {
                _loginButton.Text = "Register";
                _registerButton.Text = "Back to Login";
                _toggleModeButton.Text = "Already have an account? Login";
            }
            else
            {
                _loginButton.Text = "Login";
                _registerButton.Text = "Register";
                _toggleModeButton.Text = "New to Chat? Register";
            }
        }

        private void HandleLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(_usernameTextBox.Text) || string.IsNullOrWhiteSpace(_passwordTextBox.Text))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _statusLabel.Text = "Connecting and Authenticating...";

            _clientHelper.Connect("127.0.0.1", 8080);
            _clientHelper.CurrentUsername = _usernameTextBox.Text;

            var packet = new MessagePacket
            {
                Command = CommandType.Login,
                Sender = _usernameTextBox.Text,
                Content = _passwordTextBox.Text 
            };

            _clientHelper.SendPacket(packet);
        }

        
        private void OnServerMessageReceived(MessagePacket packet)
        {
            this.Invoke(new Action(() =>
            {
                if (packet.Command == CommandType.LoginSuccess)
                {
                    _statusLabel.Text = "Success!";
                    OpenMainChatForm();
                }
                else if (packet.Command == CommandType.LoginFail)
                {
                    _statusLabel.Text = "Login Failed!";
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        private void OpenMainChatForm()
        {
            _mainChatForm = new MainChatForm(_clientHelper);
            _mainChatForm.FormClosed += (sender, e) => this.Close();
            _mainChatForm.Show();
            this.Hide();
        }
    }
}
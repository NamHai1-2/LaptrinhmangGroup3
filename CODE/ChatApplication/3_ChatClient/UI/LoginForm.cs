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
        private TextBox _usernameTextBox, _passwordTextBox, _ipTextBox, _portTextBox;
        private Label _statusLabel;
        private Button _loginButton, _toggleModeButton;
        private bool _isRegistrationMode = false;

        public LoginForm()
        {
            _clientHelper = new TcpClientHelper();
            _clientHelper.OnMessageReceived += OnServerMessageReceived;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Chat Application - Login";
            this.Size = new Size(400, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            AddLabel("IP Server:", 100);
            _ipTextBox = new TextBox { Text = "127.0.0.1", Location = new Point(120, 97), Size = new Size(230, 25) };

            AddLabel("Port:", 140);
            _portTextBox = new TextBox { Text = "8080", Location = new Point(120, 137), Size = new Size(230, 25) };

            AddLabel("Username:", 180);
            _usernameTextBox = new TextBox { Location = new Point(120, 177), Size = new Size(230, 25) };

            AddLabel("Password:", 220);
            _passwordTextBox = new TextBox { Location = new Point(120, 217), Size = new Size(230, 25), UseSystemPasswordChar = true };

            _loginButton = new Button { Text = "LOGIN", Location = new Point(50, 300), Size = new Size(300, 45), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            _toggleModeButton = new Button { Text = "New to Chat? Register", Location = new Point(50, 360), Size = new Size(300, 30), FlatStyle = FlatStyle.Flat, ForeColor = Color.DimGray };
            _statusLabel = new Label { Text = "Ready", Location = new Point(50, 410), Size = new Size(300, 20), TextAlign = ContentAlignment.MiddleCenter };

            _loginButton.Click += async (s, e) => await HandleSubmitAsync();
            _toggleModeButton.Click += (s, e) => ToggleMode();

            this.Controls.AddRange(new Control[] { _ipTextBox, _portTextBox, _usernameTextBox, _passwordTextBox, _loginButton, _toggleModeButton, _statusLabel });
            this.ResumeLayout(false);
        }

        private void AddLabel(string text, int y) => this.Controls.Add(new Label { Text = text, Location = new Point(30, y), AutoSize = true });

        private void ToggleMode()
        {
            _isRegistrationMode = !_isRegistrationMode;
            _loginButton.Text = _isRegistrationMode ? "REGISTER" : "LOGIN";
            _loginButton.BackColor = _isRegistrationMode ? Color.FromArgb(46, 204, 113) : Color.FromArgb(52, 152, 219);
            _toggleModeButton.Text = _isRegistrationMode ? "Already have account? Login" : "New to Chat? Register";
        }

        private async Task HandleSubmitAsync()
        {
            string username = _usernameTextBox.Text.Trim();
            string password = _passwordTextBox.Text.Trim();

            if (username.Length < 1)
            {
                MessageBox.Show("Tên tài khoản phải có ít nhất 1 ký tự!", "Lỗi nhập liệu");
                return;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 1)
            {
                MessageBox.Show("Mật khẩu không được để trống và phải có ít nhất 1 ký tự!", "Lỗi bảo mật");
                return;
            }

            if (string.IsNullOrWhiteSpace(_ipTextBox.Text) || !int.TryParse(_portTextBox.Text, out int port))
            {
                MessageBox.Show("IP hoặc Port không hợp lệ!");
                return;
            }

            _statusLabel.Text = "Đang tìm kiếm Server...";
            _clientHelper.CurrentUsername = username;

            bool isConnected = await _clientHelper.ConnectAsync(_ipTextBox.Text, port);

            if (!isConnected)
            {
                _statusLabel.Text = "Kết nối thất bại!";
                MessageBox.Show($"Không thể tìm thấy Server tại địa chỉ {_ipTextBox.Text} : {port}.\n\nVui lòng kiểm tra lại xem Server đã được Start chưa, hoặc thông tin IP/Port có bị sai lệch không!",
                                "Lỗi Mạng (Connection Refused)",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            var packet = new MessagePacket
            {
                Command = _isRegistrationMode ? CommandType.Register : CommandType.Login,
                Sender = username,
                Content = password
            };
            await _clientHelper.SendMessageAsync(packet);
        }
        
        private void OnServerMessageReceived(MessagePacket packet)
        {
            if (this.IsDisposed) return;

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
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (packet.Command == CommandType.RegisterSuccess)
                {
                    
                    MessageBox.Show("Đăng ký thành công! Bạn có thể đăng nhập ngay.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ToggleMode(); 
                }
                
                else if (packet.Command == CommandType.RegisterFail)
                {
                    
                    _statusLabel.Text = "Registration Failed!";
                    MessageBox.Show("Tên tài khoản này đã có người sử dụng. Vui lòng chọn tên khác!", "Lỗi Đăng Ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }
        private void OpenMainChatForm()
        {
            MainChatForm mainChatForm = new MainChatForm(_clientHelper);

            mainChatForm.FormClosed += (sender, e) => this.Close();
            mainChatForm.Show();
            this.Hide();
        }
    }
}
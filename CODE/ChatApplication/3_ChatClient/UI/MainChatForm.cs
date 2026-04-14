using _1_SharedLibrary.Models;
using _3_ChatClient.Network;
using _3_ChatClient.UI.CustomControls;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _3_ChatClient.UI
{
    public partial class MainChatForm : Form
    {
        private readonly TcpClientHelper _clientHelper;

        private ListBox _usersListBox;
        private FlowLayoutPanel _pnlChatBoard;
        private TextBox _messageTextBox;
        private Button _sendButton;
        private Label _statusLabel;
        private Label _chatModeLabel;
        private ContextMenuStrip _userContextMenu;
        private string _privateReceiver = ""; 

        public MainChatForm(TcpClientHelper clientHelper)
        {
            _clientHelper = clientHelper;
            InitializeComponent();
            SetupEventHandlers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Chat Application - Main";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(41, 128, 185)
            };

            var lblTitle = new Label
            {
                Text = "💬 Nhóm Chat ",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 10),
                AutoSize = true
            };

            var btnLogout = new Button
            {
                Text = "Đăng xuất",
                Dock = DockStyle.Right,
                Width = 100,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => {
                _clientHelper.Disconnect();
                Application.Restart(); 
            };

            topBar.Controls.Add(lblTitle);
            topBar.Controls.Add(btnLogout);
            this.Controls.Add(topBar);

            var mainSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 240,
                FixedPanel = FixedPanel.Panel1
            };
            this.Controls.Add(mainSplitContainer);
            mainSplitContainer.BringToFront(); 

            var lblOnline = new Label
            {
                Text = "Online Users",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 50
            };

            _statusLabel = new Label
            {
                Text = $"👤 : {_clientHelper.CurrentUsername}",
                Dock = DockStyle.Bottom,
                Height = 45,
                Padding = new Padding(15, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(236, 240, 241)
            };

            _usersListBox = new ListBox
            {
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };

            _userContextMenu = new ContextMenuStrip();
            var menuPrivate = new ToolStripMenuItem("Chat Riêng (Private)");
            menuPrivate.Click += (s, e) => StartPrivateChat();
            var menuGlobal = new ToolStripMenuItem("Quay lại Chat Tổng");
            menuGlobal.Click += (s, e) => EndPrivateChat();
            _userContextMenu.Items.AddRange(new ToolStripItem[] { menuPrivate, menuGlobal });
            _usersListBox.ContextMenuStrip = _userContextMenu;

            mainSplitContainer.Panel1.Controls.Add(_usersListBox);
            mainSplitContainer.Panel1.Controls.Add(lblOnline);
            mainSplitContainer.Panel1.Controls.Add(_statusLabel);


            _chatModeLabel = new Label
            {
                Text = "🌍 Chế độ: Chat Tổng (Gửi cho tất cả)",
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(245, 245, 245),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };

            var bottomInputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 75,
                Padding = new Padding(10, 5, 10, 10)
            };

            _sendButton = new Button
            {
                Text = "Send",
                Width = 100,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            _messageTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Dock = DockStyle.Fill,
                Multiline = true,
                WordWrap = true,
                ScrollBars = ScrollBars.Vertical
            };

            bottomInputPanel.Controls.Add(_messageTextBox);
            bottomInputPanel.Controls.Add(_sendButton);

            _pnlChatBoard = new FlowLayoutPanel
            {
                BackColor = Color.White,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 30, 20, 20)
            };

            mainSplitContainer.Panel2.Controls.Add(_pnlChatBoard);
            mainSplitContainer.Panel2.Controls.Add(bottomInputPanel);
            mainSplitContainer.Panel2.Controls.Add(_chatModeLabel);

            this.ResumeLayout(false);
        }

        private void SetupEventHandlers()
        {
            _clientHelper.OnMessageReceived += OnServerMessageReceived;
            _sendButton.Click += (sender, e) => SendMessage();
            _messageTextBox.KeyDown += (sender, e) => {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    SendMessage();
                }
            };

            _usersListBox.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Right)
                {
                    int index = _usersListBox.IndexFromPoint(e.Location);
                    if (index != -1) _usersListBox.SelectedIndex = index;
                }
            };
        }

        private void StartPrivateChat()
        {
            if (_usersListBox.SelectedIndex != -1)
            {
                string selected = _usersListBox.SelectedItem.ToString().Replace(" (Online)", "");
                if (selected == _clientHelper.CurrentUsername) return;

                _privateReceiver = selected;
                _chatModeLabel.Text = $"🔒 Đang CHAT RIÊNG với: {_privateReceiver}";
                _chatModeLabel.ForeColor = Color.DarkRed;
                _chatModeLabel.BackColor = Color.MistyRose;
            }
        }

        private void EndPrivateChat()
        {
            _privateReceiver = "";
            _usersListBox.ClearSelected();
            _chatModeLabel.Text = "🌍 Chế độ: Chat Tổng (Gửi cho tất cả)";
            _chatModeLabel.ForeColor = Color.DarkGreen;
            _chatModeLabel.BackColor = Color.FromArgb(245, 245, 245);
        }

        private async void SendMessage()
        {
            var message = _messageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            MessagePacket packet;
            if (string.IsNullOrEmpty(_privateReceiver))
            {
                packet = new MessagePacket
                {
                    Command = CommandType.BroadcastMessage,
                    Content = message,
                    Sender = _clientHelper.CurrentUsername
                };
                AddMessageToDisplay("You: " + message, DateTime.Now.ToString("HH:mm"), true);
            }
            else
            {
                packet = new MessagePacket
                {
                    Command = CommandType.PrivateMessage,
                    Content = message,
                    Sender = _clientHelper.CurrentUsername,
                    Receiver = _privateReceiver
                };
                AddMessageToDisplay($"Bạn -> {_privateReceiver}: {message}", DateTime.Now.ToString("HH:mm"), true);
            }

            await _clientHelper.SendMessageAsync(packet);
            _messageTextBox.Clear();
        }

        private void OnServerMessageReceived(MessagePacket packet)
        {
            if (this.IsDisposed) return;
            this.Invoke(new Action(() => {
                if (packet.Command == CommandType.BroadcastMessage && packet.Sender != _clientHelper.CurrentUsername)
                {
                    AddMessageToDisplay(packet.Sender + ": " + packet.Content, packet.Timestamp.ToString("HH:mm"), false);
                }
                else if (packet.Command == CommandType.PrivateMessage)
                {
                    AddMessageToDisplay($"[Chat riêng] {packet.Sender}: {packet.Content}", packet.Timestamp.ToString("HH:mm"), false);
                }
                else if (packet.Command == CommandType.UserListUpdate)
                {
                    UpdateUsersList(packet.Content);
                }
            }));
        }
        private void UpdateUsersList(string userString)
        {
            _usersListBox.Items.Clear();
            if (string.IsNullOrEmpty(userString)) return;

            string[] users = userString.Split(',');
            foreach (var u in users)
            {
                _usersListBox.Items.Add(u + " (Online)");
            }
        }

        private void AddMessageToDisplay(string message, string time, bool isMe)
        {
            ChatBubble bubble = new ChatBubble();
            bubble.SetMessage(message, time, isMe);
            bubble.AutoSize = true;
            bubble.MaximumSize = new Size(_pnlChatBoard.Width - 100, 0);

            int bubbleWidth = bubble.PreferredSize.Width;
            int bubbleHeight = bubble.PreferredSize.Height;
            bubble.Size = new Size(bubbleWidth, bubbleHeight);

            Panel wrapper = new Panel();
            wrapper.Width = _pnlChatBoard.Width - 30; 
            wrapper.Height = bubbleHeight; 
            wrapper.Margin = new Padding(0, 5, 0, 5); 

            if (isMe)
            {
                bubble.Left = wrapper.Width - bubbleWidth;
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            else
            {
                bubble.Left = 0;
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }

            wrapper.Controls.Add(bubble);
            _pnlChatBoard.Controls.Add(wrapper);
            _pnlChatBoard.ScrollControlIntoView(wrapper);
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                await _clientHelper.SendMessageAsync(new MessagePacket { Command = CommandType.Disconnect });
                _clientHelper.Disconnect();
            }
            catch { }
            base.OnFormClosing(e);
            Environment.Exit(0);
        }
    }
}
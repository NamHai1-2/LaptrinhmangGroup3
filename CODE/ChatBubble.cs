using System;
using System.Drawing;
using System.Windows.Forms;

namespace _3_ChatClient.UI.CustomControls
{
    public class ChatBubble : UserControl
    {
        private Label _lblMessage;
        private Label _lblTime;

        public ChatBubble()
        {
            _lblMessage = new Label();
            _lblTime = new Label();

            this.SuspendLayout();

            this.AutoSize = true; 
            this.MinimumSize = new Size(150, 50);
            this.MaximumSize = new Size(400, 0); 
            this.Padding = new Padding(10);
            this.Margin = new Padding(5, 5, 5, 10); 

            _lblMessage.AutoSize = true;
            _lblMessage.MaximumSize = new Size(380, 0); 
            _lblMessage.Font = new Font("Segoe UI", 10);
            _lblMessage.Location = new Point(10, 10);

            _lblTime.AutoSize = true;
            _lblTime.Font = new Font("Segoe UI", 8);
            _lblTime.ForeColor = Color.Gray;

            this.Controls.Add(_lblMessage);
            this.Controls.Add(_lblTime);

            this.ResumeLayout(false);
        }

        public void SetMessage(string message, string time, bool isMe)
        {
            _lblMessage.Text = message;
            _lblTime.Text = time;

            _lblTime.Location = new Point(
                _lblMessage.Location.X,
                _lblMessage.Bottom + 5
            );

            if (isMe)
            {
                this.BackColor = Color.FromArgb(220, 248, 198);
            }
            else
            {
                this.BackColor = Color.WhiteSmoke;
            }
        }
    }
}
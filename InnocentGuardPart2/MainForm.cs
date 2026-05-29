using System;
using System.Drawing;
using System.Media;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InnocentGuardPart2
{
    public class MainForm : Form
    {
       
        private RichTextBox chatBox;
        private TextBox inputBox;
        private Button sendButton;
        private Label headerLabel;
        private Label asciiLabel;
        private Panel headerPanel;
        private Panel inputPanel;

        
        private ResponseEngine engine = new ResponseEngine();
        private string userName = string.Empty;
        private bool askedForName = false;

    
        [DllImport("winmm.dll")]
        private static extern bool PlaySound(string szSound, IntPtr hMod, uint fdwSound);

        public MainForm()
        {
            BuildUI();
            PlayVoiceGreeting();
            ShowWelcome();
        }

        
        private void BuildUI()
        {
            // Form settings
            this.Text = "InnocentGuard – Cybersecurity Assistant";
            this.Size = new Size(800, 620);
            this.MinimumSize = new Size(700, 550);
            this.BackColor = Color.FromArgb(13, 17, 23);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Consolas", 10f);

            // ── Header panel ──────────────────────────────────────────────
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.FromArgb(22, 27, 34),
                Padding = new Padding(10)
            };

            // ASCII art logo (translated from Part 1)
            asciiLabel = new Label
            {
                Text =
                    "  ██╗███╗   ██╗███╗   ██╗ ██████╗  ██████╗███████╗███╗   ██╗████████╗ ██████╗ ██╗   ██╗ █████╗ ██████╗ ██████╗ \n" +
                    "  ██║████╗  ██║████╗  ██║██╔═══██╗██╔════╝██╔════╝████╗  ██║╚══██╔══╝██╔════╝ ██║   ██║██╔══██╗██╔══██╗██╔══██╗\n" +
                    "  ██║██╔██╗ ██║██╔██╗ ██║██║   ██║██║     █████╗  ██╔██╗ ██║   ██║   ██║  ███╗██║   ██║███████║██████╔╝██║  ██║\n" +
                    "  ██║██║╚██╗██║██║╚██╗██║██║   ██║██║     ██╔══╝  ██║╚██╗██║   ██║   ██║   ██║██║   ██║██╔══██║██╔══██╗██║  ██║\n" +
                    "  ██║██║ ╚████║██║ ╚████║╚██████╔╝╚██████╗███████╗██║ ╚████║   ██║   ╚██████╔╝╚██████╔╝██║  ██║██║  ██║██████╔╝",
                Font = new Font("Consolas", 4.5f),
                ForeColor = Color.FromArgb(0, 200, 150),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerLabel = new Label
            {
                Text = "🔒  Protecting South Africa Online  🔒",
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 220, 180),
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 25,
                BackColor = Color.Transparent
            };

            headerPanel.Controls.Add(asciiLabel);
            headerPanel.Controls.Add(headerLabel);

            // ── Chat display ──────────────────────────────────────────────
            chatBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(13, 17, 23),
                ForeColor = Color.FromArgb(200, 210, 220),
                Font = new Font("Consolas", 10f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10),
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // ── Bottom input panel ────────────────────────────────────────
            inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(22, 27, 34),
                Padding = new Padding(8)
            };

            inputBox = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 37, 46),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Type your message here..."
            };

            sendButton = new Button
            {
                Text = "Send",
                Dock = DockStyle.Right,
                Width = 80,
                BackColor = Color.FromArgb(0, 160, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            sendButton.FlatAppearance.BorderSize = 0;

            inputPanel.Controls.Add(inputBox);
            inputPanel.Controls.Add(sendButton);

            // ── Wire up events ────────────────────────────────────────────
            sendButton.Click += OnSend;
            inputBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    OnSend(s, e);
                }
            };

            // ── Add everything to form ────────────────────────────────────
            this.Controls.Add(chatBox);
            this.Controls.Add(inputPanel);
            this.Controls.Add(headerPanel);
        }

        // ══════════════════════════════════════════════════════════════════
        // VOICE GREETING
        // ══════════════════════════════════════════════════════════════════
        private void PlayVoiceGreeting()
        {
            try
            {
                string audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
                if (File.Exists(audioPath))
                    PlaySound(audioPath, IntPtr.Zero, 0x00020000 | 0x00000001); // async so UI loads first
            }
            catch { /* silently skip if audio fails */ }
        }

        // ══════════════════════════════════════════════════════════════════
        // INITIAL WELCOME MESSAGE
        // ══════════════════════════════════════════════════════════════════
        private void ShowWelcome()
        {
            AppendDivider();
            AppendBot("Welcome to InnocentGuard — your Cybersecurity Awareness Assistant.");
            AppendBot("I'm here to help you stay safe online.");
            AppendDivider();
            AppendBot("Before we start, what's your name?");
            askedForName = true;
        }

        // ══════════════════════════════════════════════════════════════════
        // SEND MESSAGE
        // ══════════════════════════════════════════════════════════════════
        private void OnSend(object sender, EventArgs e)
        {
            string input = inputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            AppendUser(input);
            inputBox.Clear();

            // First message = user's name
            if (askedForName)
            {
                userName = input;
                engine.SetUserName(userName);
                askedForName = false;
                AppendDivider();
                AppendBot($"Nice to meet you, {userName}! I'm ready to help you stay cyber-safe.");
                AppendBot("You can ask me about: passwords, phishing, malware, VPN, 2FA, scams, privacy, and more.");
                AppendBot("Type 'help' to see all topics, or just ask me anything!");
                AppendDivider();
                return;
            }

            // Normal conversation
            string response = engine.GetResponse(input);
            AppendBot(response);
        }

        // ══════════════════════════════════════════════════════════════════
        // CHAT DISPLAY HELPERS
        // ══════════════════════════════════════════════════════════════════
        private void AppendUser(string message)
        {
            chatBox.SelectionStart = chatBox.TextLength;
            chatBox.SelectionColor = Color.FromArgb(255, 210, 80);
            chatBox.AppendText($"\n  👤 {userName}: {message}\n");
            chatBox.SelectionColor = chatBox.ForeColor;
            chatBox.ScrollToCaret();
        }

        private void AppendBot(string message)
        {
            chatBox.SelectionStart = chatBox.TextLength;
            chatBox.SelectionColor = Color.FromArgb(0, 200, 150);
            chatBox.AppendText($"\n  🤖 InnocentGuard: ");
            chatBox.SelectionColor = Color.FromArgb(200, 210, 220);
            chatBox.AppendText($"{message}\n");
            chatBox.SelectionColor = chatBox.ForeColor;
            chatBox.ScrollToCaret();
        }

        private void AppendDivider()
        {
            chatBox.SelectionStart = chatBox.TextLength;
            chatBox.SelectionColor = Color.FromArgb(50, 80, 70);
            chatBox.AppendText("\n  ────────────────────────────────────────────\n");
            chatBox.SelectionColor = chatBox.ForeColor;
        }
    }
}
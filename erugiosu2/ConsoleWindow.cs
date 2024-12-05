using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace erugiosu2
{
    internal class ConsoleWindow : Form
    {
        private TextBox _consoleOutput;
        private bool show = true;
        
        public ConsoleWindow()
        {
            // ウィンドウの基本設定
            this.Text = "C++ Console Output";
            this.Size = new Size(1400, 600); // 初期サイズ
            this.BackColor = Color.Black;  // 背景色を黒
            this.FormBorderStyle = FormBorderStyle.Sizable; // リサイズ可能
            this.StartPosition = FormStartPosition.CenterScreen;

            // TextBox の作成
            _consoleOutput = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Consolas", 11), // コマンドプロンプト風のフォント
                Dock = DockStyle.Fill // 親フォームにフィット
            };

            // TextBox をフォームに追加
            this.Controls.Add(_consoleOutput);

            // リサイズ時の処理を追加（オプション）
            this.Resize += (s, e) => _consoleOutput.Refresh();

            this.FormClosing += ConsoleWindow_FormClosing;
        }

        // 外部からログを追加するためのメソッド
        public void AppendText(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AppendText(text)));
            }
            else
            {
                _consoleOutput.AppendText(text + Environment.NewLine);
                _consoleOutput.SelectionStart = _consoleOutput.Text.Length;
                _consoleOutput.ScrollToCaret();
            }
        }

        // 外部からフォームを再表示するメソッド
        public bool ShowConsole()
        {
            if (!show)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => this.Show()));
                }
                else
                {
                    this.Show();
                }
                show = true;
                return true;
            }
            return false;


        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ConsoleWindow
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "ConsoleWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConsoleWindow_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ConsoleWindow_FormClosed);
            this.ResumeLayout(false);

        }

        private void ConsoleWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            // フォームを非表示にする
            e.Cancel = true;
            this.Hide();
            show = false;
        }

        private void ConsoleWindow_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}

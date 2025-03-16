using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace erugiosu2
{
    internal class ConsoleWindow : Form
    {
        private RichTextBox _consoleOutput;
        private List<string> _allLinesBackup = new List<string>();
        private Dictionary<int, int> _turnIndexMap = new Dictionary<int, int>(); // ターン番号 → 行インデックス
        private bool show = true;
        private bool disposing1 = false;
        private int _lastline1 = 0;
        private int _lastEdited = -1;

        public ConsoleWindow()
        {
            // OSバージョンチェック
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) // Windows 7 (6.1)以降かをチェック
            {
                MessageBox.Show("このアプリケーションはWindows 7以降でのみ動作します。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }

            // ウィンドウの基本設定
            this.Text = "C++ Console Output";
            this.Size = new Size(1400, 600);
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;

            // TextBox の作成
            // RichTextBox の作成
            _consoleOutput = new RichTextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BackColor = Color.FromArgb(12, 12, 12),
                ForeColor = Color.FromArgb(204, 204, 204),
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 11),
                WordWrap = false,
            };

            // TextBox をフォームに追加
            this.Controls.Add(_consoleOutput);

            // リサイズ時の処理
            this.Resize += (s, e) => _consoleOutput.Refresh();
            this.FormClosing += ConsoleWindow_FormClosing;
        }

        /// <summary>
        /// 外部からログ行（1行ずつ）を追加する
        /// </summary>
        public void AppendText(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AppendText(text)));
            }
            else
            {

                if (text.StartsWith("turn"))
                {
                    _allLinesBackup.Add(Environment.NewLine + "  " + text);
                    _consoleOutput.AppendText(Environment.NewLine + "  " + text);
                    _consoleOutput.SelectionStart = 0; // 左端にリセット
                    _consoleOutput.ScrollToCaret();
                    return;
                }

                Match m = Regex.Match(text, @"^(\d+)(\s+)(.*)$");
                if (m.Success)
                {
                    _allLinesBackup.Add("  " + text);
                    int lineIndex = _allLinesBackup.Count - 1;
                    int turnNumber = int.Parse(m.Groups[1].Value);
                    _turnIndexMap[turnNumber] = lineIndex; // ターン番号に対するインデックスを保存
                    _consoleOutput.AppendText(Environment.NewLine + "  " + text);
                }
                else
                {
                    _allLinesBackup.Add(text);
                    if (String.IsNullOrEmpty(_consoleOutput.Text))
                        _consoleOutput.AppendText(text);
                    else
                        _consoleOutput.AppendText(Environment.NewLine + text);
                }
                _lastline1 = 0;
            }
        }

        /// <summary>
        /// 最新のターン番号を更新する
        /// </summary>
        public void UpdateCurrentTurn(int turn)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateCurrentTurn(turn)));
            }
            else
            {
                Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(6, 1));
                // ウィンドウが最小化状態なら、選択やスクロール更新を行わない
                if (this.WindowState == FormWindowState.Minimized)
                    return;

                turn = turn + 1;
                int index = _turnIndexMap.GetOrDefault(turn, -1);
                if (index == -1)
                {

                    return;
                }
                if (_allLinesBackup.Count <= index)
                {
                    return;
                }
                string line = _allLinesBackup[index];
                Match m = Regex.Match(line, @"^\s+(\d+)(\s+)(.*)$");
                if (m.Success)
                {
                    string newLine = $"> {turn}" + m.Groups[2].Value + m.Groups[3].Value;

                    int originalSelectionStart = _consoleOutput.SelectionStart;

                    // テキスト全体を取得して行ごとに分割
                    var lines = _consoleOutput.Lines.ToList();

                    if(_lastEdited == index)
                    {
                        return;
                    }

                    if (_lastEdited != -1 && _lastEdited < lines.Count)
                    {
                        lines[_lastEdited] = _allLinesBackup[_lastEdited];
                    }
                    _lastEdited = index;
                    if (index < lines.Count)
                    {
                        lines[index] = newLine;
                    }

                    // 更新されたテキストを再設定
                    _consoleOutput.Lines = lines.ToArray();


                    _consoleOutput.SelectionStart = originalSelectionStart;

                    // 初回は0行目固定、10ターン目以降のみ自動スクロールする
                    if (turn >= 5)
                    {
                       int firstVisibleLine = _lastline1;
                        

                        // 表示可能な行数を算出
                        int visibleLines = _consoleOutput.ClientSize.Height / _consoleOutput.Font.Height;
                        int lastVisibleLine = firstVisibleLine + visibleLines - 1;
                        // 更新した行が下端近くなら1行分下へスクロール
                        int updatedLine = index;
                        //インデックス上の最後の値を取得
                        int last = _turnIndexMap.Values.Last();
                        int first = _turnIndexMap.Values.First();

                        if(firstVisibleLine + visibleLines >= last + 1)
                        {
                            _consoleOutput.ScrollToCaret();
                            return;
                        }

                        if (last > updatedLine)
                        {
                            int nextLine = firstVisibleLine + 1;
                            int nextLineCharIndex = _consoleOutput.GetFirstCharIndexFromLine(nextLine);
                            if (nextLineCharIndex > 0)
                            {
                                _consoleOutput.SelectionStart = nextLineCharIndex;
                                _lastline1 = nextLine;
                                _consoleOutput.ScrollToCaret();
                            }
                        }
                        else
                        {
                            _consoleOutput.SelectionStart = _consoleOutput.Text.Length;
                            _consoleOutput.ScrollToCaret();
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 内部状態のリセット（＋リセットメッセージを表示）
        /// </summary>
        public void ResetState()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ResetState()));
            }
            else
            {
                _allLinesBackup.Clear();
                _turnIndexMap.Clear();
                _allLinesBackup.Add("Console has been reset");
                _consoleOutput.Text = "Console has been reset";
                _lastEdited = -1;
                _lastline1 = 0;
            }
        }

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

        private void ConsoleWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            show = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing1) // すでにDisposeされているかチェック
            {
                disposing1 = true; // ここでフラグを立てる

                if (disposing)
                {
                    // マネージドリソースの解放
                    _consoleOutput?.Dispose();
                    _consoleOutput = null;
                }

                // コレクションをクリア（nullにはしない）
                _allLinesBackup.Clear();
                _turnIndexMap.Clear();
            }

            base.Dispose(disposing);
        }
    }
}

public static class DictionaryExtensions {
    /// <summary>
    /// 値を取得、keyがなければデフォルト値を設定し、デフォルト値を取得
    /// </summary>
    public static TV GetOrDefault<TK, TV>(this Dictionary<TK, TV> dic, TK key,TV defaultValue = default(TV))
    {
        TV result;
        return dic.TryGetValue(key, out result) ? result : defaultValue;
    }
}
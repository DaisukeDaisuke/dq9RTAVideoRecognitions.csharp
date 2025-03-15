using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace erugiosu2
{
    internal class ConsoleWindow : Form
    {
        private TextBox _consoleOutput;
        private List<string> _allLines = new List<string>();
        private List<string> _allLinesBackup = new List<string>();
        private Dictionary<int, int> _turnIndexMap = new Dictionary<int, int>(); // ターン番号 → 行インデックス
        private bool show = true;
        private bool disposing1 = false;

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
            _consoleOutput = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(230, 230, 230),
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
                    _allLines.Add("  " + text);
                    _allLinesBackup.Add("  " + text);
                    return;
                }

                _consoleOutput.SelectionStart = 0; // 左端にリセット
                _consoleOutput.ScrollToCaret();

                Match m = Regex.Match(text, @"^(\d+)(\s+)(.*)$");
                if (m.Success)
                {
                    _allLines.Add("  " + text);
                    _allLinesBackup.Add("  " + text);
                    int lineIndex = _allLines.Count - 1;
                    int turnNumber = int.Parse(m.Groups[1].Value);
                    _turnIndexMap[turnNumber] = lineIndex; // ターン番号に対するインデックスを保存
                }
                else
                {
                    _allLines.Add(text);
                    _allLinesBackup.Add(text);
                }
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
                // ウィンドウが最小化状態なら、選択やスクロール更新を行わない
                if (this.WindowState == FormWindowState.Minimized)
                    return;

                turn = turn + 1;
                int index = _turnIndexMap.GetOrDefault(turn, -1);
                if (index == -1)
                {
                    _allLines.Clear();   // 中身を空にする
                    _allLines = null;    // 参照を解除（GCの対象にする）
                    _allLines = new List<string>(_allLinesBackup);  // バックアップを新しいリストにコピー
                    _consoleOutput.Text = string.Join(Environment.NewLine, _allLines);
                    return;
                }

                _allLines.Clear();   // 中身を空にする
                _allLines = null;    // 参照を解除（GCの対象にする）
                _allLines = new List<string>(_allLinesBackup);  // バックアップを新しいリストにコピー
                if (_allLines.Count <= index)
                {
                    _allLines = new List<string>(_allLinesBackup);  // バックアップを新しいリストにコピー
                    _consoleOutput.Text = string.Join(Environment.NewLine, _allLines);
                    return;
                }
                string line = _allLines[index];
                Match m = Regex.Match(line, @"^\s+(\d+)(\s+)(.*)$");
                if (m.Success)
                {

                    // 更新前の選択状態を保存
                    int savedSelectionStart = _consoleOutput.SelectionStart;
                    int savedSelectionLength = _consoleOutput.SelectionLength;

                    string newLine = $"> {turn}" + m.Groups[2].Value + m.Groups[3].Value;
                    _allLines[index] = newLine;
                    _consoleOutput.Text = string.Join(Environment.NewLine, _allLines);
                    // 初回は0行目固定、10ターン目以降のみ自動スクロールする
                    if (turn >= 7)
                    {
                        // 現在の表示領域の先頭行を取得
                        int firstVisibleLine = _consoleOutput.GetLineFromCharIndex(
                            _consoleOutput.GetCharIndexFromPosition(new Point(0, 0)));

                        if (firstVisibleLine <= 0)// 一番下にスクロールしてるとなぜか0になる。なぜか勝手に一番上にスクロールするおまけつき
                        {
                            _consoleOutput.SelectionStart = _consoleOutput.Text.Length;
                            _consoleOutput.ScrollToCaret();
                            return;
                        }

                        // 表示可能な行数を算出
                        int visibleLines = _consoleOutput.ClientSize.Height / _consoleOutput.Font.Height;
                        int lastVisibleLine = firstVisibleLine + visibleLines - 1;
                        // 更新した行が下端近くなら1行分下へスクロール
                        int updatedLine = _consoleOutput.GetLineFromCharIndex(
                            _consoleOutput.GetFirstCharIndexFromLine(index));

                        if (updatedLine <= 7)
                        {
                            return;
                        }

                        // TextBox の全行数を取得
                        int totalLines = _consoleOutput.Lines.Length;
                        // 更新対象の行 index が全体の下から3行以内なら、末尾へスクロール
                        if (updatedLine >= totalLines - 3)
                        {
                            _consoleOutput.SelectionStart = _consoleOutput.Text.Length;
                            _consoleOutput.ScrollToCaret();
                            return;
                        }

                        if (updatedLine > (lastVisibleLine + 1))
                        {
                            int nextLine = updatedLine + 3;
                            int nextLineCharIndex = _consoleOutput.GetFirstCharIndexFromLine(nextLine);
                            if (nextLineCharIndex > 0)
                            {
                                _consoleOutput.SelectionStart = nextLineCharIndex;
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

                    // 更新後、ユーザーが選択していた状態を復元
                    // ※ただし、ユーザーが既にカーソルを移動させていた場合はそのままにするか、条件で制御する
                    _consoleOutput.SelectionStart = savedSelectionStart;
                    _consoleOutput.SelectionLength = savedSelectionLength;
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
                _allLines.Clear();
                _allLinesBackup.Clear();
                _turnIndexMap.Clear();
                _allLines.Add("Console has been reset");
                _allLinesBackup.Add("Console has been reset");
                _consoleOutput.Text = "Console has been reset";
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
                _allLines.Clear();
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
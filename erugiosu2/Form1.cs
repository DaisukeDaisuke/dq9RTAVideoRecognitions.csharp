using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Hompus.VideoInputDevices;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using erugiosu2;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Text;
using System.Threading;

namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private System.Windows.Forms.Timer _timer;
        private List<BossTemplate> bossTemplates = new List<BossTemplate>(); // BossTemplateのリスト
        private IRecognitionBoss ActiveBoss = null;

        Dictionary<int, List<BattleAction>> battleLog = new Dictionary<int, List<BattleAction>>();

        private int marginX = 25;
        private int marginY = 205;

        public bool updateText1()
        {
            //UpdateOutputText("b " + FormatParseInput() + " " + TurnIndex + " " + BattleAction.updateText1(battleLog));
            return true;
        }

        public void runSearch()
        {
            //string test = ("b " + FormatParseInput() + " " + TurnIndex + " " + BattleAction.updateText1(battleLog));
            //_consoleManager.SendInput(test);
            //flag = true;
        }

        private void clearActions()
        {
            foreach (var log in battleLog.Values)
            {
                log.Clear(); // 各List内の参照を解除
            }
            battleLog.Clear(); // Dictionary内の参照も解除
            dataGridView1.Rows.Clear();
            UpdateOutputText("");
        }

        // 行動を記録するメソッド
        void RecordAction(int participantId, int ActionIndex, int action)
        {
            if (!battleLog.ContainsKey(participantId))
            {
                battleLog[participantId] = new List<BattleAction>();
            }

            if(dataGridView1.RowCount < participantId+1)
            {
                dataGridView1.Rows.Add();
                // 一番下の行にスクロール
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
                dataGridView1.Rows[participantId].Cells[0].Value = participantId.ToString();
            }

            // ダメージが不明の状態で行動を記録
            battleLog[participantId].Add(new BattleAction(action));
            dataGridView1.Rows[participantId].Cells[ActionIndex * 2 + 1].Value = BattleAction.GetActionName(action);

            updateText1();
        }

        // 行動を修正するための関数
        void UpdateAction(int participantId, int actionIndex, int newAction)
        {
            if (battleLog.ContainsKey(participantId) &&
                actionIndex >= 0 &&
                actionIndex < battleLog[participantId].Count)
            {
                // 古いアクションを新しいアクションに置き換える
                battleLog[participantId][actionIndex] = new BattleAction(newAction);

                // DataGridViewの更新
                dataGridView1.Rows[participantId].Cells[actionIndex * 2 + 1].Value = BattleAction.GetActionName(newAction);
            }
            else
            {
                // participantIdが存在しないか、actionIndexが無効な場合の処理
                MessageBox.Show("指定された行動が見つかりません。");
            }

            updateText1();
        }

        bool flag = false;

        void UpdateDamage(int participantId, int actionIndex, int damage)
        {
            if (battleLog.ContainsKey(participantId) &&
                actionIndex < battleLog[participantId].Count)
            {
                var action = battleLog[participantId][actionIndex];

                // ダメージが未確定の場合のみ更新
                if (action.IsDamagePending)
                {
                    action.Damage = damage;
                }

                dataGridView1.Rows[participantId].Cells[actionIndex * 2 + 2].Value = damage;

                if (participantId >= 4 && actionIndex == 2 && !flag)
                {
                    runSearch();
                }

                updateText1();

                var action1 = battleLog[participantId][actionIndex].Action;
                //if (action1 == BattleAction.ATTACK_ENEMY || action1 == BattleAction.CRITICAL_ATTACK || action1 == BattleAction.LIGHTNING_STORM || action1 == BattleAction.ULTRA_HIGH_SPEED_COMBO) {
                //    if (Sleeping && (ActionTaken || actionIndex == 2))
                //    {
                //        Sleeping = false;
                //    }
                //}
            }
        }

        // DataGridViewの初期設定
        private void InitializeDataGridView()
        {
            // スクロールバーの幅を取得
            int scrollBarWidth = SystemInformation.VerticalScrollBarWidth;

            // カラムの追加
            for (int i = 0; i < 7; i++)
            {
                dataGridView1.Columns.Add($"Column{i + 1}", $"Column {i + 1}");
            }

            // 各カラムのヘッダーテキスト設定
            dataGridView1.Columns[0].HeaderText = "ind";
            dataGridView1.Columns[1].HeaderText = "AcT1";
            dataGridView1.Columns[2].HeaderText = "D1";
            dataGridView1.Columns[3].HeaderText = "AcT2";
            dataGridView1.Columns[4].HeaderText = "D2";
            dataGridView1.Columns[5].HeaderText = "AcT3";
            dataGridView1.Columns[6].HeaderText = "D3";

            // 最初のカラム幅の設定
            AdjustColumnWidths();
        }

        private bool isMinimized = false;

        // フォームのサイズ変更イベント
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                isMinimized = true;
            }
            else if (isMinimized && this.WindowState == FormWindowState.Normal)
            {
                isMinimized = false;
                AdjustColumnWidths();
                SaveSettings(); // ウィンドウサイズと設定を保存
            }
            else
            {
                AdjustColumnWidths();
                SaveSettings(); // 通常のサイズ変更時も保存
            }
        }

        // DataGridViewとカラム幅を調整するメソッド
        private void AdjustColumnWidths()
        {
            int scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            int totalWidth = dataGridView1.Width - scrollBarWidth - 25;
            if (this.ClientSize.Width < 400)
            {
                totalWidth -= 5;
            }

            // デフォルトカラム幅
            int columnCount = 4;
            int halfWidthColumnCount = 3;
            int defaultWidth = totalWidth / (columnCount + (halfWidthColumnCount / 4));
            int numberColumnWidth = defaultWidth / 4;

            // カラム幅の設定
            dataGridView1.Columns[0].Width = numberColumnWidth;
            dataGridView1.Columns[1].Width = defaultWidth;
            dataGridView1.Columns[2].Width = numberColumnWidth;
            dataGridView1.Columns[3].Width = defaultWidth;
            dataGridView1.Columns[4].Width = numberColumnWidth;
            dataGridView1.Columns[5].Width = defaultWidth;
            dataGridView1.Columns[6].Width = numberColumnWidth;

            // DataGridViewのサイズをフォームに合わせて調整（マージン分を差し引く）
            dataGridView1.Width = this.ClientSize.Width - marginX;
            dataGridView1.Height = this.ClientSize.Height - marginY;
        }

        // コピー ボタンのクリック イベント ハンドラー
        private void CopyButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(outputTextBox.Text))
            {
                Clipboard.SetText(outputTextBox.Text);
                //MessageBox.Show("テキストがコピーされました!", "コピー完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("コピーするテキストがありません", "コピーエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private CppConsoleManager _consoleManager;
        private const string CppProgramPath = "resource\\cbe.exe"; // C++プログラムのパス
        private ConsoleWindow _ConsoleWindow;
        private TemplateMatcher _matcher1;

        // 出力エリアに動的にデータを追加するメソッド例
        private void UpdateOutputText(string data)
        {
            outputTextBox.Text = data;
        }

        public Form1()
        {
            Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(6, 1));
            InitializeComponent();
            InitializeDataGridView();

            DebugTextBox.Visible = false;  // 初期状態で非表示
            OutputLabel.Visible = false;  // 初期状態で非表示

            outputTextBox.Visible = false;  // 初期状態で非表示
            DebugLabel.Visible = false;  // 初期状態で非表示

            TimeVisible.Checked = true;

            // outputTextBox を設定
            outputTextBox.ReadOnly = true;
            outputTextBox.Multiline = true;
            outputTextBox.ScrollBars = ScrollBars.Vertical;

            copyButton.Click += CopyButton_Click;

            // 必要に応じて初期出力データを設定
            outputTextBox.Text = "";
            this.Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resource", "Icon.ico"));

            // Resizeイベントでフォームサイズ変更時にDataGridViewの幅を追従
            this.Resize += new EventHandler(Form1_Resize);

            using (var sde = new SystemDeviceEnumerator())
            {
                var devices = sde.ListVideoInputDevice(); // 例: Dictionary<int, string>

                // "OBS Virtual Camera" に一致するインデックスを取得
                try
                {
                    int obsCameraIndex = devices
                        .FirstOrDefault(d => d.Value == "OBS Virtual Camera").Key;
                    if (obsCameraIndex == default(int) && !devices.ContainsKey(obsCameraIndex))
                    {
                        // カメラが見つからない場合に警告ポップアップを表示

                        MessageBox.Show("OBS Virtual Camera が見つかりません。プログラムを終了します。",
                                      "警告",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Warning);


                        // プログラムを終了
                        Environment.Exit(0);
                        return;
                    }

                    // カメラ初期化
                    _capture = new VideoCapture(obsCameraIndex);
                    _capture.Set(CapProp.FrameWidth, 1920);
                    _capture.Set(CapProp.FrameHeight, 1080);

                    Console.WriteLine($"OBS Virtual Camera Index: {obsCameraIndex}");
                }
                catch
                {
                    // カメラが見つからない場合に警告ポップアップを表示

                    MessageBox.Show("OBS Virtual Camera が見つかりません。プログラムを終了します(コンピーターにカメラが1つも接続されていません)",
                                  "警告",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);


                    // プログラムを終了
                    Environment.Exit(0);
                    return;
                }
            }

            // タイマー設定（5fps）
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 300;
            _timer.Tick += new EventHandler(CaptureFrame);
            _timer.Start();


            DebugTextBox.Font = new Font(DebugTextBox.Font.FontFamily, 10); // サイズを24に設定

            _consoleManager = new CppConsoleManager(CppProgramPath);
            _consoleManager.OnOutputReceived += OnCppOutputReceived;

            _ConsoleWindow = new ConsoleWindow();
            _ConsoleWindow.Show();

            this.Icon = null;
            _ConsoleWindow.setIcon(null);

            


            UIManager actionManager = new UIManager(RecordAction, UpdateAction, UpdateDamage, clearActions, UpdateDebug);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string resourceDir = Path.Combine(baseDir, "resource");

            // BossTemplate を作成
            BossTemplate neko2BossTemplate = new BossTemplate(
                "neko2",
                () => new neko2(resourceDir, actionManager), // パスを渡すファクトリ関数
                "neko2_template",
                 Path.Combine(resourceDir, "neko", "Icon.ico"),
                 2
            );

            // リストに追加
            bossTemplates.Add(neko2BossTemplate);


            _matcher1 = new TemplateMatcher(Path.Combine(resourceDir, "BossTemplate"), 150, new Size(130, 45), false, null, 200, 0.7);
        }

        private void OnCppOutputReceived(string obj)
        {
            Debug.WriteLine(obj);

            if (_ConsoleWindow != null)
            {
                _ConsoleWindow.AppendText(obj);
            }
        }

        // ParseInputの出力をフォーマットして文字列として返す関数
        public string FormatParseInput()
        {
            int[] numbers = ParseInputTimer();
            StringBuilder sb = new StringBuilder();

            // 文字列として"1 0 0"のように結合
            for (int i = 0; i < numbers.Length; i++)
            {
                sb.Append(numbers[i]);
                if (i < numbers.Length - 1)
                    sb.Append(" ");
            }

            return sb.ToString();
        }

        public int[] ParseInputTimer()
        {
            var input = InputTimer.Text;
            // 空白や改行をトリム
            input = input.Trim();

            // 入力が空の場合
            if (string.IsNullOrEmpty(input)) return new int[] { 0, 0, 0 };

            // スペースで区切り、整数に変換
            string[] parts = input.Split(' ');

            // 部分が3つに分かれていない場合
            if (parts.Length != 3) return new int[] { 0, 0, 0 };

            // 変換結果を格納する配列
            int[] numbers = new int[3];

            // 各部分を数値化
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out numbers[i]))
                {
                    return new int[] { 0, 0, 0 }; // 変換失敗時
                }
            }

            return numbers;
        }

        private async Task onTimeReadComplete(string timer)
        {
            var text = NormalizeTime(timer);
            if (text != "") {
                InputTimer.Text = text;
            }
            await Task.CompletedTask;
            return;
        }


        static string NormalizeTime(string input)
        {
            // 正規表現パターンを定義（hh:mm:ss.ms または mm:ss.ms の形式を考慮）
            var pattern = @"^(?:(\d+):)?(?:(\d+):)?(\d+)\.(\d+)$";
            var match = Regex.Match(input, pattern);

            if (match.Success)
            {
                int hours = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
                int minutes = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                int seconds = int.Parse(match.Groups[3].Value);
                int milliseconds = int.Parse(match.Groups[4].Value);

                // 秒数を正規化
                seconds += milliseconds / 100; // ミリ秒を秒に変換
                if (seconds >= 60)
                {
                    minutes += seconds / 60;
                    seconds %= 60;
                }

                // 分数を正規化
                if (minutes >= 60)
                {
                    hours += minutes / 60;
                    minutes %= 60;
                }

                // 正規化された形式で出力
                return $"{hours} {minutes} {seconds}";
            }

            return "";
        }

        private async void CaptureFrame(object sender, EventArgs e)
        {
            using (Mat frame = new Mat())
            {
                _capture.Read(frame);

                if (!frame.IsEmpty)
                {
                    if (ActiveBoss == null)
                    {
                        using (Mat tmp = new Mat(frame, new Rectangle(78, 645, 160, 70)))
                        {
                            _matcher1.ProcessImage(tmp);
                            if (_matcher1.MatchResults[0] != TemplateMatcher.NO_MATCH)
                            {
                                foreach (var entry in bossTemplates)
                                {
                                    if (entry.Match(_matcher1.MatchResults[0], tmp))
                                    {
                                        using (Mat trimed2 = new Mat(frame, new Rectangle(420, 645, 200, 70)))
                                        {
                                            _matcher1.ProcessImage(trimed2);
                                            if (_matcher1.MatchResults[0] != TemplateMatcher.NO_MATCH)
                                            {
                                                await LiveSplitPipeClient.GetCurrentTimeAsync(onTimeReadComplete);
                                                ActiveBoss = entry.Activate();
                                                this.Icon = new Icon(entry.IcopPath);
                                                _ConsoleWindow.setIcon(entry.IcopPath);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ActiveBoss.ExecuteRecognition(frame))
                        {
                            ActiveBoss.Dispose();
                            ActiveBoss = null;

                            this.Icon = null;
                            _ConsoleWindow.setIcon(null);
                        }
                    }

                    // 複数領域を条件に基づいて処理する
                    //ProcessCaptureAreas(frame);
                    using (Mat resizedFrame1 = new Mat(frame, new Rectangle(0, 0, 958, 718)))
                    {
                        using (Mat resizedFrame2 = new Mat())
                        {
                            CvInvoke.Resize(resizedFrame1, resizedFrame2, new Size(958 / 4, 718 / 4)); // フレームをリサイズ

                            if (pictureBox1.Image != null)
                            {
                                pictureBox1.Image.Dispose();
                            }
                            // フレームの更新（ミラー用に再描画）
                            pictureBox1.Image = resizedFrame2.ToBitmap();
                        }
                    }
                }
            }
        }

        private void UpdateDebug(String text)
        {
            DebugTextBox.Text = text;
        }
 
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void copyButton_Click_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void ReloadState()
        {
            if (!TableOnlyVisible.Checked)
            {
                // チェックボックスの状態に応じてDebugTextBoxの表示を切り替える
                DebugTextBox.Visible = showDebugCheckBox.Checked;
                DebugLabel.Visible = showDebugCheckBox.Checked;
            }

            if (!TableOnlyVisible.Checked)
            {
                outputTextBox.Visible = OutputVisible.Checked;
                OutputLabel.Visible = OutputVisible.Checked;
            }

            InputTimer.Visible = TimeVisible.Checked;
            label2.Visible = TimeVisible.Checked;
            if (!TableOnlyVisible.Checked)
            {
                copyButton.Visible = TimeVisible.Checked;
                ConsoleButton.Visible = TimeVisible.Checked;
            }

            if (TableOnlyVisible.Checked)
            {
                //InputTimer.Visible = false;
                InputTimer.Location = new Point(85, 10);
                label2.Location = new Point(10, 14);
                TimeVisible.Location = new Point(330, 12);
                ConsoleButton.Location = new Point(230, 8);
                ConsoleButton.Size = new Size(75, 27);
                ConsoleButton.Visible = TimeVisible.Checked;
                InputTimer.Visible = TimeVisible.Checked;
                label2.Visible = TimeVisible.Checked;
                copyButton.Visible = false;
                outputTextBox.Visible = false;
                OutputLabel.Visible = false;
                DebugTextBox.Visible = false;
                DebugLabel.Visible = false;
                pictureBox1.Visible = false;

                OutputVisible.Visible = false;
                showDebugCheckBox.Visible = false;
                marginX = 25;
                marginY = 45;
                dataGridView1.Location = new Point(12, 40);
            }
            else
            {
                pictureBox1.Visible = true;
                //InputTimer.Visible = true;
                InputTimer.Location = new Point(299, 30);
                label2.Location = new Point(299, 9);
                TimeVisible.Location = new Point(301, 193);
                ConsoleButton.Location = new Point(344, 110);
                ConsoleButton.Size = new Size(88, 48);
                ConsoleButton.Visible = TimeVisible.Checked;
                label2.Visible = TimeVisible.Checked;
                copyButton.Visible = TimeVisible.Checked;
                OutputVisible.Visible = true;
                showDebugCheckBox.Visible = true;
                outputTextBox.Visible = OutputVisible.Checked;
                OutputLabel.Visible = OutputVisible.Checked;
                DebugTextBox.Visible = showDebugCheckBox.Checked;
                DebugLabel.Visible = showDebugCheckBox.Checked;
                marginX = 25;
                marginY = 255;
                dataGridView1.Location = new Point(14, 248);
            }


            AdjustColumnWidths();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ReloadState();
        }

        private void OutputVisible_CheckedChanged(object sender, EventArgs e)
        {
            ReloadState();
        }

        private void TimerVisible_CheckedChanged(object sender, EventArgs e)
        {
            ReloadState();
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            ReloadState();
        }


        private const string ConfigFileName = "settings.config";
        private const int TimeVisibleBit = 1 << 0;    // 1
        private const int TableOnlyVisibleBit = 1 << 1; // 2
        private const int ShowDebugBit = 1 << 2;      // 4
        private const int OutputVisibleBit = 1 << 3;   // 8
                                                       // 設定ファイルからウィンドウサイズと設定を読み込み
        private const int MinWidth = 300;
        private const int MinHeight = 200;
        private const int MaxSizeBits = 0x3FFF; // サイズ情報のビット幅

        // 設定ファイルにウィンドウサイズと設定を保存
        private void SaveSettings()
        {
            int settings = 0;

            // チェックボックスの設定をビットで保存
            if (TimeVisible.Checked) settings |= TimeVisibleBit;
            if (TableOnlyVisible.Checked) settings |= TableOnlyVisibleBit;
            if (showDebugCheckBox.Checked) settings |= ShowDebugBit;
            if (OutputVisible.Checked) settings |= OutputVisibleBit;
            // ウィンドウサイズをビットにエンコード

            int width = this.Width < MinWidth ? MinWidth : this.Width;
            int height = this.Height < MinHeight ? MinHeight : this.Height;
            if (isMinimized)
            {
                width = 623;
                height = 432;
            }
           
            int encodedSize = ((width & MaxSizeBits) << 14) | (height & MaxSizeBits);

            // 設定とサイズを1つの整数にまとめて保存
            int combinedSettings = (settings & 0xF) | (encodedSize << 4); // 上位にサイズ情報

            File.WriteAllText(ConfigFileName, combinedSettings.ToString());
        }

        private void LoadSettings()
        {
            if (File.Exists(ConfigFileName))
            {
                string fileContent = File.ReadAllText(ConfigFileName);
                if (int.TryParse(fileContent, out int combinedSettings))
                {
                    // チェックボックスの設定
                    int settings = combinedSettings & 0xF;
                    TimeVisible.Checked = (settings & TimeVisibleBit) != 0;
                    TableOnlyVisible.Checked = (settings & TableOnlyVisibleBit) != 0;
                    showDebugCheckBox.Checked = (settings & ShowDebugBit) != 0;
                    OutputVisible.Checked = (settings & OutputVisibleBit) != 0;

                    // ウィンドウサイズの読み込み
                    int encodedSize = (combinedSettings >> 4);
                    int width = (encodedSize >> 14) & MaxSizeBits;
                    int height = encodedSize & MaxSizeBits;
                    this.Width = width < MinWidth ? MinWidth : width;
                    this.Height = height < MinHeight ? MinHeight : height;
                }
            }
            else
            {
                // デフォルト値を設定
                TimeVisible.Checked = true;
                TableOnlyVisible.Checked = false;
                showDebugCheckBox.Checked = false;
                OutputVisible.Checked = false;
            }
        }

        // フォームロード時に呼び出してサイズと設定を反映
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();
            ReloadState();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
            _timer.Stop();
            _capture.Dispose();
            _consoleManager.Dispose();
            _ConsoleWindow.Close();
            _ConsoleWindow.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!_ConsoleWindow.ShowConsole())
            {
               runSearch();
            }
        }

        internal void OnExit()
        {
            if (_timer != null) _timer.Stop();
            if (_capture != null) _capture.Dispose();
            if (_consoleManager != null) _consoleManager.Dispose();
            if (_ConsoleWindow != null)
            {
                _ConsoleWindow.Close();
                _ConsoleWindow.Dispose();
            }
            if(ActiveBoss != null)
            {
                ActiveBoss.Dispose();
            }
            bossTemplates.Clear();
            if(_matcher1 != null)
            {
                _matcher1.Dispose();
            }
        }
    }
}

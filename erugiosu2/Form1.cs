using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Emgu.CV.Reg;
using Emgu.CV.ML;
using Emgu.CV.ML.MlEnum;
using Hompus.VideoInputDevices;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using erugiosu2;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Text;
using System.Reflection.Emit;
using System.Drawing.Printing;

namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private Timer _timer;
        private Bitmap bitmap;
        private int frameCounter = 0; // フレームカウンタ
        private Dictionary<string, Mat> templateCache = new Dictionary<string, Mat>(); // テンプレートキャッシュ
        private Dictionary<string, Mat> NumberCache = new Dictionary<string, Mat>(); // テンプレートキャッシュ
        private Dictionary<string, Mat> templateCache1 = new Dictionary<string, Mat>(); // テンプレートキャッシュ
        private Dictionary<string, Mat> templateCache2 = new Dictionary<string, Mat>(); // テンプレートキャッシュ
        int[] matchResults1 = new int[3] { -1, -1, -1 };
        int[] matchResults2= new int[3] { -1, -1, -1 };

        Dictionary<int, List<BattleAction>> battleLog = new Dictionary<int, List<BattleAction>>();

        private string lastHit1 = ""; 
        private string lastHit2 = ""; 
        private string lastHit3 = "";

        private int preAction = -1;
        private bool Initialized = false;
        private int NeedDamage1 = -1;
        private bool NeedDamage1Enabled = false;
        private int NeedDamage2 = -1;
        private bool NeedDamage2Enabled = false;
        private int lastdamage1 = -1;
        private int lastdamage2 = -1;
        private int ActionIndex = 0;
        private int TurnIndex = 0;
        private int maybeCritical = -1;
        private DateTime LastDetection = DateTime.Now;
        // グローバルスコープで一致率の最も高い画像名と一致率を格納する変数を定義
        double[] highestMatchPercentage = new double[3] { 0, 0, 0 };
        string[] highestMatchImageNames = new string[3] { "", "", "" };
        private int marginX = 25;
        private int marginY = 205;



        public bool updateText1()
        {
            if (ActionIndex == 0)
            {
                UpdateOutputText(FormatParseInput() + " " + TurnIndex + " " + BattleAction.updateText1(battleLog));
            }
            else
            {
                UpdateOutputText(FormatParseInput() + " " + (TurnIndex + 1) + " " + BattleAction.updateText1(battleLog));
            }

            return true;
        }

        public int ConvertMatchResults(int[] matchResults)
        {
            // 配列がnullまたは長さが3以外の場合は例外をスロー
            if (matchResults == null || matchResults.Length != 3)
                throw new ArgumentException("matchResults must be an array with 3 elements.");

            // その他の入力に対して動的に変換処理
            int result = 0;
            bool hasValidNumber = false;

            if (matchResults[0] == -1)
            {
                return -1;
            }
            if (matchResults[1] == -1&& matchResults[2] != -1)
            {
                return -1;
            }

            for (int i = 0; i < matchResults.Length; i++)
            {
                if (matchResults[i] != -1)
                {
                    result = result * 10 + matchResults[i];
                    hasValidNumber = true;
                }
            }

            // 有効な数字がない場合、無効として-1を返す
            return hasValidNumber ? result : -1;
        }

        // 行動を記録するメソッド
        void RecordAction(int participantId, int action)
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

                updateText1();
            }
        }

        private bool ActionTaken = false;

        private async void ProcessState()
        {
            if (lastHit1 == "erugio.png"&&lastHit2 == "reset.png")
            {
                if (!Initialized)
                {
                    foreach (var log in battleLog.Values)
                    {
                        log.Clear(); // 各List内の参照を解除
                    }
                    battleLog.Clear(); // Dictionary内の参照も解除
                    dataGridView1.Rows.Clear();
                    Initialized = true;
                    ActionIndex = 0;
                    TurnIndex = 0;
                    LastDetection = DateTime.Now;
                    NeedDamage2Enabled = false;
                    NeedDamage2 = -1;
                    lastdamage2 = -1;
                    NeedDamage1Enabled = false;
                    NeedDamage1 = -1;
                    lastdamage1 = -1;
                    ActionTaken = false;
                    UpdateOutputText("");
                    await LiveSplitPipeClient.GetCurrentTimeAsync(onTimeReadComplete);
                    

                }
                return;
            }
            Initialized = false;

            int action = -1;
            int damage = -1;

            if (maybeCritical != -1)
            {
                int turnind = maybeCritical & 0xfff;
                int actionid = (maybeCritical >> 12) & 0xf;
                if(lastHit1 == "critical.png")
                {
                    UpdateAction(turnind, actionid, BattleAction.CRITICAL_ATTACK);
                    maybeCritical = -1;
                }
            }

            int damageTest1 = ConvertMatchResults(matchResults1);
            int damageTest2 = ConvertMatchResults(matchResults2);
           if (NeedDamage1 != -1&& NeedDamage1Enabled || lastdamage1 < damageTest1)
            {
                if (lastHit1 == "guard.png" || lastHit1 == "miss.png" || lastHit1 == "miss2.png")
                {
                    int turnind = NeedDamage1 & 0xfff;
                    int actionid = (NeedDamage1 >> 12) & 0xf;
                    NeedDamage1Enabled = false;
                    //NeedDamage1 = -1;
                    maybeCritical = -1;
                    UpdateDamage(turnind, actionid, 0);
                }
                
                if (damageTest1 != -1)
                {
                    lastdamage1 = damageTest1;
                    int turnind = NeedDamage1 & 0xfff;
                    int actionid = (NeedDamage1 >> 12) & 0xf;
                    UpdateDamage(turnind, actionid, damageTest1);
                    NeedDamage1Enabled = false;
                    //NeedDamage1 = -1;
                }
                else
                {
                    
                    if (damageTest2 != -1&& damageTest1 < damageTest2)
                    {
                        lastdamage1 = damageTest2;
                        int turnind = NeedDamage1 & 0xfff;
                        int actionind = (NeedDamage1 >> 12) & 0xf;
                        NeedDamage1Enabled = false;
                        //NeedDamage1 = -1;
                        UpdateDamage(turnind, actionind, damageTest2);
                    }
                }
                return;
            }
/*            else if (!NeedDamage1Enabled && NeedDamage1 != -1)
            {
                if (lastdamage1 > 0 && lastdamage1 < damageTest1 && damageTest1 != -1)
                {
                    int turnind = NeedDamage1 & 0xfff;
                    int actionid = (NeedDamage1 >> 12) & 0xf;
                    UpdateDamage(turnind, actionid, damageTest2);
                }
            }*/else if ((NeedDamage2 != -1&& NeedDamage2Enabled) || lastdamage2 < damageTest2)
            {
                if (lastHit1 == "guard.png"||lastHit1 == "miss.png"||lastHit1 == "miss2.png")
                {
                    int turnind = NeedDamage2 & 0xfff;
                    int actionind = (NeedDamage2 >> 12) & 0xf;
                    //NeedDamage2 = -1;
                    NeedDamage2Enabled = false;
                    maybeCritical = -1;
                    lastdamage2 = -1;
                    UpdateDamage(turnind, actionind, 0);
                }
                if (damageTest2 != -1)
                {
                    int turnind = NeedDamage2 & 0xfff;
                    int actionind = (NeedDamage2 >> 12) & 0xf;
                    NeedDamage2Enabled = false;
                    //NeedDamage2 = -1;
                    lastdamage2 = damageTest2;
                    UpdateDamage(turnind, actionind, damageTest2);
                }
                else
                {
                    if (damageTest1 != -1 && damageTest2 < damageTest1)
                    {
                        int turnind = NeedDamage2 & 0xfff;
                        int actionid = (NeedDamage2 >> 12) & 0xf;
                        NeedDamage2Enabled = false;
                        lastdamage2 = damageTest1;
                        //NeedDamage2 = -1;
                        UpdateDamage(turnind, actionid, damageTest1);
                    }
                }
                return;
            }
/*            else if (!NeedDamage2Enabled && NeedDamage2 != -1)
            {
                if (lastdamage1 > 0 && lastdamage2 < damageTest2 && damageTest2 != -1)
                {
                    int turnind = NeedDamage2 & 0xfff;
                    int actionid = (NeedDamage2 >> 12) & 0xf;
                    UpdateDamage(turnind, actionid, damageTest2);
                }
            }*/

            if (lastHit1 == "sukara.png")
            {
                action = BattleAction.BUFF;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }
            if(lastHit1 == "hadou.png")
            {
                action = BattleAction.DISRUPTIVE_WAVE;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
            }
            if (lastHit1 == "yaketuku.png")
            {
                action = BattleAction.BURNING_BREATH;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
            }
            if (lastHit1 == "zilyoukuu.png")
            {
                action = BattleAction.SKY_ATTACK;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }
            if (lastHit1 == "merazoma.png")
            {
                action = BattleAction.MERA_ZOMA;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }
            if (lastHit1 == "mira-.png")
            {
                action = BattleAction.MAGIC_MIRROR;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }

            if (lastHit1 == "erugio.png"&&lastHit2 == "attack.png")
            {
                maybeCritical = (ActionIndex << 12) | TurnIndex;
                action = BattleAction.ATTACK_ENEMY;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }

            if(lastHit1 == "samidare.png"||lastHit1 == "samidare2.png")
            {
                action = BattleAction.MULTITHRUST;
                NeedDamage2 = (ActionIndex << 12) | TurnIndex;
                NeedDamage1 = -1;
                ActionTaken = true;
            }

            if (lastHit1 == "no_hadou.png")
            {
                action = BattleAction.LAUGH;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
            }

            if(lastHit1 == "tameru.png")
            {
                action = BattleAction.PSYCHE_UP;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
            }

            if(lastHit1 == "zigosupa.png")
            {
                action = BattleAction.LIGHTNING_STORM;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }

            if(lastHit1 == "kuroi.png")
            {
                action = BattleAction.DARK_BREATH;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }

            if(lastHit1 == "erugio.png" && lastHit2 == "uhsc.png")
            {
                action = BattleAction.ULTRA_HIGH_SPEED_COMBO;
                NeedDamage2 = (ActionIndex << 12) | TurnIndex;
                NeedDamage1 = -1;
            }

            if(lastHit1 == "sutemi.png")
            {
                action = BattleAction.DOUBLE_UP;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }
            if(lastHit1 == "meisou.png")
            {
                action = BattleAction.MEDITATION;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
            }
            if(lastHit1 == "madannte.png"){
                action = BattleAction.MAGIC_BURST;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }
            if (lastHit1 == "ice.png")
            {
                action = BattleAction.FREEZING_BLIZZARD;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }
            if(lastHit1 == "fullheal.png")
            {
                action = BattleAction.FULLHEAL;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }
            if(lastHit1 == "more_heal.png")
            {
                action = BattleAction.MORE_HEAL;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }

            if (lastHit1 == "defense_champion.png" &&lastHit2 == "defense_champion2.png")
            {
                action = BattleAction.DEFENDING_CHAMPION;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }

            if(lastHit1 == "ayasii.png")
            {
                action = BattleAction.LULLAB_EYE;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
            }


            if (lastHit1 == "mp2.png")
            {
                action = BattleAction.RESTORE_MP;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
            }

            if(lastHit1 == "WakeUp.png"&&lastHit2 != "inori.png"&&ActionIndex != 0&&!ActionTaken)
            {
                action = BattleAction.TURN_SKIPPED;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }

            if (lastHit1 == "Paralysis.png") 
            {
                action = BattleAction.PARALYSIS;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }

            if(lastHit1 == "sleeping2.png"&&lastHit2 == "" && lastHit3 == "")
            {
                action = BattleAction.SLEEPING;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }

            if (lastHit1 == "sleeping2.png" && ( lastHit3 == "dead.png" || lastHit3 == "dead2.png"))
            {
                action = BattleAction.DEAD;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
            }

            if(lastHit1 == "song.png")
            {
                action = BattleAction.SONG;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }


            if (action != -1&&action != preAction && (lastHit1 != "" || lastHit2 != ""))
            {
                NeedDamage1Enabled = NeedDamage1 != -1;
                NeedDamage2Enabled = NeedDamage2 != -1;
                lastdamage1 = -1;
                lastdamage2 = -1;

                if (lastHit2 != "attack.png")
                {
                    maybeCritical = -1;
                }
                preAction = action;
                RecordAction(TurnIndex, action);
                if (NeedDamage1 == -1&&NeedDamage2 == -1)
                {
                    UpdateDamage(TurnIndex, ActionIndex, 0);
                }
                ActionIndex++;
                if (ActionIndex == 3)
                {
                    ActionIndex = 0;
                    TurnIndex++;
                    ActionTaken = false;
                }
            }
            else if(action != -1 && action == preAction)
            {
                LastDetection = DateTime.Now;
            }

            DateTime currentTime = DateTime.Now;

            //Debug.WriteLine((currentTime - LastDetection).TotalSeconds);

            if ((currentTime - LastDetection).TotalSeconds > 3)
            {
                // LastDetectionは1秒以上前です
                if (lastHit1 == "")
                {
                    preAction = 0;
                }
            }
            
           

            return;
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

        // 出力エリアに動的にデータを追加するメソッド例
        private void UpdateOutputText(string data)
        {
            outputTextBox.Text = data;
        }

        public Form1()
        {
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

            // Resizeイベントでフォームサイズ変更時にDataGridViewの幅を追従
            this.Resize += new EventHandler(Form1_Resize);

            using (var sde = new SystemDeviceEnumerator())
            {
                var devices = sde.ListVideoInputDevice(); // 例: Dictionary<int, string>

                // "OBS Virtual Camera" に一致するインデックスを取得
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

            // タイマー設定（5fps）
            _timer = new Timer();
            _timer.Interval = 300;
            _timer.Tick += new EventHandler(CaptureFrame);
            _timer.Start();


            // テンプレート画像をキャッシュ
            LoadTemplatesToCache();
        }

        private void LoadTemplatesToCache()
        {
            // resourceフォルダ内のテンプレート画像をキャッシュに読み込む
            string resourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resource", "message_v2");
            string[] templateFiles = Directory.GetFiles(resourceDir, "*.png");

            foreach (string templateFile in templateFiles)
            {
                if (!templateCache.ContainsKey(templateFile))
                {
                    Mat template = CvInvoke.Imread(templateFile, Emgu.CV.CvEnum.ImreadModes.Grayscale);
                    templateCache.Add(templateFile, template);
                }
            }

            // resourceフォルダ内のテンプレート画像をキャッシュに読み込む
            string resourceDir1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resource", "numbers");
            string[] templateFiles1 = Directory.GetFiles(resourceDir1, "*.png");

            foreach (string templateFile in templateFiles1)
            {
                if (!NumberCache.ContainsKey(templateFile))
                {
                    Mat template = CvInvoke.Imread(templateFile, Emgu.CV.CvEnum.ImreadModes.Grayscale);
                    NumberCache.Add(templateFile, template);
                }
            }

            // resourceフォルダ内のテンプレート画像をキャッシュに読み込む
            string resourceDir2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resource", "submessage_v2");
            string[] templateFiles2 = Directory.GetFiles(resourceDir2, "*.png");

            foreach (string templateFile in templateFiles2)
            {
                if (!templateCache1.ContainsKey(templateFile))
                {
                    Mat template = CvInvoke.Imread(templateFile, Emgu.CV.CvEnum.ImreadModes.Grayscale);
                    templateCache1.Add(templateFile, template);
                }
            }

            // resourceフォルダ内のテンプレート画像をキャッシュに読み込む
            string resourceDir3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resource", "sub2message_v2");
            string[] templateFiles3 = Directory.GetFiles(resourceDir3, "*.png");

            foreach (string templateFile in templateFiles3)
            {
                if (!templateCache2.ContainsKey(templateFile))
                {
                    Mat template = CvInvoke.Imread(templateFile, Emgu.CV.CvEnum.ImreadModes.Grayscale);
                    templateCache2.Add(templateFile, template);
                }
            }



            DebugTextBox.Font = new Font(DebugTextBox.Font.FontFamily, 10); // サイズを24に設定

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
            lastHit1 = "";
            lastHit2 = "";

            highestMatchImageNames[0] = "";
            highestMatchImageNames[1] = "";
            highestMatchImageNames[2] = "";

            highestMatchPercentage[0] = 0;
            highestMatchPercentage[1] = 0;
            highestMatchPercentage[2] = 0;


            using (Mat frame = new Mat())
            {
                _capture.Read(frame);

                if (!frame.IsEmpty)
                {
                    // 複数領域を条件に基づいて処理する
                    ProcessCaptureAreas(frame);
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

                StringBuilder sb = new StringBuilder();
                sb.Append("int1: ").Append(string.Join(", ", matchResults1)).Append(" \r\n");
                sb.Append("int2: ").Append(string.Join(", ", matchResults2)).Append(" \r\n");

                // tem1, tem2, tem3の部分を追加
                for (int j = 0; j < 3; j++)
                {
                    string templateName = string.IsNullOrEmpty(highestMatchImageNames[j]) ? "null" : highestMatchImageNames[j];
                    double matchPercentage = highestMatchPercentage[j];
                    sb.Append($"tem{j + 1}: {templateName}: {matchPercentage:F1}% \r\n");  // 一致率を1桁にフォーマット
                }

                DebugTextBox.Text = sb.ToString();


                ProcessState();

                frame.Dispose();
            }
        }

        // 領域の指定と処理を行う関数
        private void ProcessCaptureAreas(Mat frame)
        {
            // 複数のキャプチャ領域の座標を指定
            Rectangle[] areas = {
            new Rectangle(78, 645, 160, 70),  // 1つ目の領域
            // 他の条件に基づく領域もここに追加
        };

            Rectangle[] ocr = {
            new Rectangle(179, 645, 160, 60),  // 1つ目の領域
            // 他の条件に基づく領域もここに追加
        };

            Rectangle[] ocr2 = {
            new Rectangle(518, 619, 100, 90),  // 1つ目の領域
            // 他の条件に基づく領域もここに追加
        };


            foreach (var area in areas)
            {
                //// この領域に対する画像処理
                using (Mat cropped = new Mat(frame, area))
                {
                    ProcessCroppedImage(cropped);
                }
            }

            foreach (var area in ocr)
            {
                //// この領域に対する画像処理
                using (Mat cropped = new Mat(frame, area))
                {
                    ProcessCroppedImage1(cropped);
                }
            }

            foreach (var area in ocr2)
            {
                //// この領域に対する画像処理
                using (Mat cropped = new Mat(frame, area))
                {
                    ProcessCroppedImage2(cropped);
                }
            }

            foreach (var area in areas)
            {
                DrawCaptureArea(frame, area);
            }

            foreach (var area in ocr)
            {
                DrawCaptureArea(frame, area);
            }

            foreach (var area in ocr2)
            {
               DrawCaptureArea(frame, area);
            }
        }

        // キャプチャ領域の枠を描画
        private void DrawCaptureArea(Mat frame, Rectangle area)
        {
            CvInvoke.Rectangle(frame, area, new MCvScalar(150, 150, 150), 5); // 緑色の枠を描画
        }

        private void ProcessCroppedImage2(Mat cropped)
        {
            Image<Bgr, Byte> img = cropped.ToImage<Bgr, Byte>();

            // RGBの下限と上限を設定
            var lowerBound = new Bgr(140, 140, 140); // RGBそれぞれ200以上
            var upperBound = new Bgr(255, 255, 255); // 白の範囲

            // マスクを作成 (範囲内のピクセルが白、それ以外が黒)
            Image<Gray, Byte> mask = img.InRange(lowerBound, upperBound);

            // マスクを適用して白黒画像を作成
            Image<Bgr, Byte> result = img.CopyBlank();
            result.SetValue(new Bgr(255, 255, 255), mask);

            using (Mat Tmp = result.Mat)
            {
               
                using (Mat trimmed = TrimFirstPixel(Tmp, 40, 60))
                {
                    if (trimmed.Width == 40 && trimmed.Height == 60)
                    {
                        foreach (var entry in templateCache1)
                        {
                            string templateFile = entry.Key;
                            Mat template = entry.Value;

                            using (Mat resultMat = new Mat())
                            {
                                // テンプレートマッチングを実行
                                CvInvoke.MatchTemplate(trimmed, template, resultMat, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

                                // 一致率を計算
                                double minVal = 0, maxVal = 0;
                                Point minLoc = new Point(), maxLoc = new Point();
                                CvInvoke.MinMaxLoc(resultMat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                                // 一致率をパーセンテージに変換
                                double matchPercentage = maxVal * 100.0;


                                // 現在の一致率がこれまでの最高値を超える場合のみ更新
                                if (matchPercentage > highestMatchPercentage[2])
                                {
                                    highestMatchPercentage[2] = matchPercentage;
                                    highestMatchImageNames[2] = Path.GetFileName(templateFile); // 画像名を保存
                                }

                                if (matchPercentage >= 80)
                                {
                                    Console.WriteLine($"2Matched with {Path.GetFileName(templateFile)}: {matchPercentage}%");
                                    lastHit2 = Path.GetFileName(templateFile);
                                }
                            }
                        }

                        if (frameCounter % 2 == 0)
                        {
                            //SaveMatAsImage(trimmed, 1);
                            //pictureBox4.Image.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}.png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
        }

        private void ProcessCroppedImage(Mat cropped)
        {
            Image<Bgr, Byte> img = cropped.ToImage<Bgr, Byte>();

            // RGBの下限と上限を設定
            var lowerBound = new Bgr(140, 140, 140); // RGBそれぞれ200以上
            var upperBound = new Bgr(255, 255, 255); // 白の範囲

            // マスクを作成 (範囲内のピクセルが白、それ以外が黒)
            Image<Gray, Byte> mask = img.InRange(lowerBound, upperBound);

            // マスクを適用して白黒画像を作成
            Image<Bgr, Byte> result = img.CopyBlank();
            result.SetValue(new Bgr(255, 255, 255), mask);

            using (Mat Tmp = result.Mat) {
                using (Mat trimmed = TrimFirstPixel(Tmp, 130, 45))
                {
                    if (trimmed.Width == 130 && trimmed.Height == 45)
                    {
                        //if (pictureBox2.Image != null)
                        //{
                        //    pictureBox2.Image.Dispose();
                        //}
                        //pictureBox2.Image = trimmed.ToBitmap();

                        foreach (var entry in templateCache)
                        {
                            string templateFile = entry.Key;
                            Mat template = entry.Value;

                            using (Mat resultMat = new Mat())
                            {
                                // テンプレートマッチングを実行
                                CvInvoke.MatchTemplate(trimmed, template, resultMat, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

                                // 一致率を計算
                                double minVal = 0, maxVal = 0;
                                Point minLoc = new Point(), maxLoc = new Point();
                                CvInvoke.MinMaxLoc(resultMat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                                // 一致率をパーセンテージに変換
                                double matchPercentage = maxVal * 100.0;

                                // 現在の一致率がこれまでの最高値を超える場合のみ更新
                                if (matchPercentage > highestMatchPercentage[0])
                                {
                                    highestMatchPercentage[0] = matchPercentage;
                                    highestMatchImageNames[0] = Path.GetFileName(templateFile); // 画像名を保存
                                }

                                if (matchPercentage >= 80)
                                {
                                    lastHit1 = Path.GetFileName(templateFile);
                                    Console.WriteLine($"Matched with {Path.GetFileName(templateFile)}: {matchPercentage}%");
                                }
                                //SaveMatAsImage(trimmed, 1);
                            }
                        }

                        if (frameCounter % 2 == 0)
                        {
                            
                            //pictureBox2.Image.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}.png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }

            // トリミングする領域の配列
            Rectangle[] areas = {
                new Rectangle(0, 0, 60, 60),
                new Rectangle(55, 0, 60, 60),
                new Rectangle(105, 0, 60, 60),
            };

            // 各領域に対してトリミングを行い、異なるPictureBoxに表示
            for (int i = 0; i < areas.Length; i++)
            {
                var area = areas[i];

                result.ROI = area;

                // Matに変換
                using (Mat resultMat1 = result.Mat)
                {
                    using (Mat trimmed = TrimFirstPixel(resultMat1, 26, 40))
                    {
                        if (trimmed.Width == 26 && trimmed.Height == 40)
                        {
                            bool matched = false;
                            foreach (var entry in NumberCache)
                            {
                                string templateFile = entry.Key;
                                Mat template = entry.Value;

                                using (Mat resultMat = new Mat())
                                {
                                    // テンプレートマッチングを実行
                                    CvInvoke.MatchTemplate(trimmed, template, resultMat, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

                                    // 一致率を計算
                                    double minVal = 0, maxVal = 0;
                                    Point minLoc = new Point(), maxLoc = new Point();
                                    CvInvoke.MinMaxLoc(resultMat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                                    // 一致率をパーセンテージに変換
                                    double matchPercentage = maxVal * 100.0;


                                    if (matchPercentage >= 93.5)
                                    {
                                        string templateFileName = Path.GetFileNameWithoutExtension(templateFile);
                                        string normalizedTemplate = templateFileName.Split('_')[0]; // "_"以降を除去してベース番号を取得
                                        matchResults1[i] = int.Parse(normalizedTemplate);
                                        matched = true;
                                        Console.WriteLine($"Number20 {i} with {Path.GetFileName(templateFile)}: {matchPercentage}%");
                                    }
                                }
                            }

                            if (!matched)
                            {
                                matchResults1[i] = -1;
                            }

                            if (frameCounter % 10 == 0)
                            {
                                //SaveMatAsImage(trimmed, i);
                            }
                        }
                    }
                }
            }
        }


        public void SaveMatAsImage(Mat trimmed, int i)
        {
#if DEBUG
            // MatからBitmapへ変換
            using (Bitmap bmp = trimmed.ToBitmap())
            {
                // 画像をPNG形式で保存
                bmp.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}_{i}.png", ImageFormat.Png);
            }
#endif
        }


        //呼びだす際はusing使う事。使わないとメモリリークする
        private Mat TrimFirstPixel(Mat resultMat1, int xSize, int ySize) {
            // 最初の白ピクセルを探索
            Rectangle firstWhitePixel = new Rectangle(0, 0, 0, 0); // 初期値として無効な位置を設定
            bool found = false, found1 = false;
            int cropWidth = 0, cropHeight = 0;
            int foundX = 0, foundY = 0;
            Mat monoImage = new Mat();
            CvInvoke.CvtColor(resultMat1, monoImage, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
            Image<Bgr, byte> tmp = resultMat1.ToImage<Bgr, byte>();

            for (int y = 0; y < resultMat1.Height; y++)
            {
                 for (int x = 0; x < resultMat1.Width; x++)
                {
                    var color = tmp[y, x];
                    // 白色のピクセルを判定
                    if (color.Blue >= 200 && color.Green >= 200 && color.Red >= 200)
                    {
                        //cropWidth = Math.Min(xSize, resultMat1.Width - x);
                        found1 = true;
                        foundX = x;
                        foundY = y;
                        break;
                    }
                }
                if (found1)
                {
                    break; // 外側のループも抜ける
                }
            }

            if (found1)
            {
                for (int x = 0; x < resultMat1.Width; x++)
                {
                    for (int y = 0; y < resultMat1.Height; y++)
                    {

                        // ピクセルの色を取得
                        var color = tmp[y, x];

                        // 白色のピクセルを判定
                        if (color.Blue >= 200 && color.Green >= 200 && color.Red >= 200)
                        {

                            foundX = Math.Min(foundX, x);
                            foundY = Math.Min(foundY, y);

                            cropHeight = Math.Min(ySize, resultMat1.Height - foundY); // y座標から画像の下端までの距離を超えない
                            cropWidth = Math.Min(xSize, resultMat1.Width - foundX); // y座標から画像の下端までの距離を超えない

                            // 白ピクセルの位置を基準に、切り取り領域を設定
                            found = true;
                            break; // 白ピクセルを見つけたらループを抜ける
                        }
                    }
                    if (found)
                    {
                        break; // 外側のループも抜ける
                    }
                }
            }

            if (found)
            {
                firstWhitePixel = new Rectangle(foundX, foundY, cropWidth, cropHeight);
                return new Mat(monoImage, firstWhitePixel);
            }
            else
            {
                return monoImage;
            }
        }

        private void ProcessCroppedImage1(Mat cropped)
        {
            lastHit3 = "";
            Image<Bgr, Byte> img = cropped.ToImage<Bgr, Byte>();

            // RGBの下限と上限を設定
            var lowerBound = new Bgr(140, 140, 140); // RGBそれぞれ200以上
            var upperBound = new Bgr(255, 255, 255); // 白の範囲

            // マスクを作成 (範囲内のピクセルが白、それ以外が黒)
            Image<Gray, Byte> mask = img.InRange(lowerBound, upperBound);

            // マスクを適用して白黒画像を作成
            Image<Bgr, Byte> result = img.CopyBlank();
            result.SetValue(new Bgr(255, 255, 255), mask);

            using (Mat resultMat1 = result.Mat)
            {
                using (Mat trimmed = TrimFirstPixel(resultMat1, 100, 45))
                {
                     SaveMatAsImage(trimmed, 2);

                    if (trimmed.Width == 100 && trimmed.Height == 45)
                    {
                        
                        foreach (var entry in templateCache2)
                        {
                            string templateFile = entry.Key;
                            Mat template = entry.Value;

                            using (Mat resultMat = new Mat())
                            {
                                // テンプレートマッチングを実行
                                CvInvoke.MatchTemplate(trimmed, template, resultMat, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

                                // 一致率を計算
                                double minVal = 0, maxVal = 0;
                                Point minLoc = new Point(), maxLoc = new Point();
                                CvInvoke.MinMaxLoc(resultMat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                                // 一致率をパーセンテージに変換
                                double matchPercentage = maxVal * 100.0;

                                // 現在の一致率がこれまでの最高値を超える場合のみ更新
                                if (matchPercentage > highestMatchPercentage[1])
                                {
                                    highestMatchPercentage[1] = matchPercentage;
                                    highestMatchImageNames[1] = Path.GetFileName(templateFile); // 画像名を保存
                                }

                                if (matchPercentage >= 80)
                                {
                                    Console.WriteLine($"2Matched with {Path.GetFileName(templateFile)}: {matchPercentage}%");
                                    lastHit3 = Path.GetFileName(templateFile);
                                }
                            }
                        }

                    }
                }
            }

            Rectangle[] areas = {
                new Rectangle(0, 0, 60, 60),
                new Rectangle(43, 0, 60, 60),
                new Rectangle(87, 0, 60, 60),
            };

            // 各領域に対してトリミングを行い、異なるPictureBoxに表示
            for (int i = 0; i < areas.Length; i++)
            {
                var area = areas[i];

                result.ROI = area;

                // Matに変換
                using (Mat resultMat1 = result.Mat)
                {
                    
                    using (Mat trimmed = TrimFirstPixel(resultMat1, 26, 40))
                    {
                        if (trimmed.Width == 26 && trimmed.Height == 40)
                        {
                            bool matched = false;
                            foreach (var entry in NumberCache)
                            {
                                string templateFile = entry.Key;
                                Mat template = entry.Value;

                                using (Mat resultMat = new Mat())
                                {
                                    // テンプレートマッチングを実行
                                    CvInvoke.MatchTemplate(trimmed, template, resultMat, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

                                    // 一致率を計算
                                    double minVal = 0, maxVal = 0;
                                    Point minLoc = new Point(), maxLoc = new Point();
                                    CvInvoke.MinMaxLoc(resultMat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                                    // 一致率をパーセンテージに変換
                                    double matchPercentage = maxVal * 100.0;


                                    if (matchPercentage >= 93.5)
                                    {
                                        Console.WriteLine($"Number10 {i} with {Path.GetFileName(templateFile)}: {matchPercentage}%");
                                        string templateFileName = Path.GetFileNameWithoutExtension(templateFile);
                                        string normalizedTemplate = templateFileName.Split('_')[0]; // "_"以降を除去してベース番号を取得
                                        matchResults2[i] = int.Parse(normalizedTemplate);
                                        matched = true;
                                    }
                                }
                            }

                            if (!matched)
                            {
                                matchResults2[i] = -1;
                            }

                            if (frameCounter % 2 == 0)
                            {
                                //pictureBox.Image.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}_{i}.png", System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }
                    }
                }
            }

            //result.ROI = Rectangle.Empty;

            // 5回に1回画像を保存
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
            }

            if (TableOnlyVisible.Checked)
            {
                //InputTimer.Visible = false;
                InputTimer.Location = new Point(85, 10);
                label2.Location = new Point(10, 14);
                TimeVisible.Location = new Point(330, 12);
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
                InputTimer.Location = new Point(258, 24);
                label2.Location = new Point(258, 9);
                TimeVisible.Location = new Point(258, 138);
                label2.Visible = TimeVisible.Checked;
                copyButton.Visible = TimeVisible.Checked;
                OutputVisible.Visible = true;
                showDebugCheckBox.Visible = true;
                outputTextBox.Visible = OutputVisible.Checked;
                OutputLabel.Visible = OutputVisible.Checked;
                DebugTextBox.Visible = showDebugCheckBox.Checked;
                DebugLabel.Visible = showDebugCheckBox.Checked;
                marginX = 25;
                marginY = 205;
                dataGridView1.Location = new Point(12, 198);
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
        }
    }
}

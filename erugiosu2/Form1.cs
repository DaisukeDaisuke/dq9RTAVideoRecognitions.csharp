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
        int[] matchResults1 = new int[3] { -1, -1, -1 };
        int[] matchResults2= new int[3] { -1, -1, -1 };

        Dictionary<int, List<BattleAction>> battleLog = new Dictionary<int, List<BattleAction>>();

        private string lastHit1 = ""; 
        private string lastHit2 = ""; 
        private string lastHit3 = "";

        private int preAction = -1;
        private bool Initialized = false;
        private int NeedDamage1 = -1;
        private int NeedDamage2 = -1;
        private int ActionIndex = 0;
        private int TurnIndex = 0;
        private int maybeCritical = -1;

        public int ConvertMatchResults(int[] matchResults)
        {
            // 配列がnullまたは長さが3以外の場合は例外をスロー
            if (matchResults == null || matchResults.Length != 3)
                throw new ArgumentException("matchResults must be an array with 3 elements.");

            // その他の入力に対して動的に変換処理
            int result = 0;
            bool hasValidNumber = false;

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
            }

            // ダメージが不明の状態で行動を記録
            battleLog[participantId].Add(new BattleAction(action));
            dataGridView1.Rows[participantId].Cells[ActionIndex * 2].Value = BattleAction.GetActionName(action);


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
                dataGridView1.Rows[participantId].Cells[actionIndex * 2].Value = BattleAction.GetActionName(newAction);
            }
            else
            {
                // participantIdが存在しないか、actionIndexが無効な場合の処理
                MessageBox.Show("指定された行動が見つかりません。");
            }
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

                dataGridView1.Rows[participantId].Cells[actionIndex * 2 + 1].Value = damage;
            }
        }

        private bool ProcessState()
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
                    for (int i = 0; i < 6; i++)
                    {
                        dataGridView1.Columns.Add($"Column{i + 1}", $"Column {i + 1}");
                    }
                }
                return true;
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

            if (NeedDamage1 != -1)
            {
                if (lastHit1 == "guard.png" || lastHit1 == "miss.png")
                {
                    int turnind = NeedDamage1 & 0xfff;
                    int actionid = (NeedDamage1 >> 12) & 0xf;
                    NeedDamage1 = -1;
                    maybeCritical = -1;
                    UpdateDamage(turnind, actionid, 0);
                }
                int damageTest = ConvertMatchResults(matchResults1);
                if (damageTest != -1)
                {
                    int turnind = NeedDamage1 & 0xfff;
                    int actionid = (NeedDamage1 >> 12) & 0xf;
                    NeedDamage1 = -1;
                    UpdateDamage(turnind, actionid, damageTest);
                }
                else
                {
                    damageTest = ConvertMatchResults(matchResults2);
                    if (damageTest != -1)
                    {
                        int turnind = NeedDamage1 & 0xfff;
                        int actionind = (NeedDamage1 >> 12) & 0xf;
                        NeedDamage1 = -1;
                        UpdateDamage(turnind, actionind, damageTest);
                    }
                }
                return true;
            }

            if (NeedDamage2 != -1)
            {
                if (lastHit1 == "guard.png"||lastHit1 == "miss.png")
                {
                    int turnind = NeedDamage2 & 0xfff;
                    int actionind = (NeedDamage2 >> 12) & 0xf;
                    NeedDamage2 = -1;
                    maybeCritical = -1;
                    UpdateDamage(turnind, actionind, 0);
                }
                int damageTest = ConvertMatchResults(matchResults2);
                if (damageTest != -1)
                {
                    int turnind = NeedDamage2 & 0xfff;
                    int actionind = (NeedDamage2 >> 12) & 0xf;
                    NeedDamage2 = -1;
                    UpdateDamage(turnind, actionind, damageTest);
                }
                else
                {
                    damageTest = ConvertMatchResults(matchResults1);
                    if (damageTest != -1)
                    {
                        int turnind = NeedDamage2 & 0xfff;
                        int actionid = (NeedDamage2 >> 12) & 0xf;
                        NeedDamage2 = -1;
                        UpdateDamage(turnind, actionid, damageTest);
                    }
                }
                return true;
            }

            if (lastHit1 == "sukara.png")
            {
                action = BattleAction.BUFF;
            }
            if(lastHit1 == "hadou.png")
            {
                action = BattleAction.DISRUPTIVE_WAVE;
            }
            if (lastHit1 == "yaketuku.png")
            {
                action = BattleAction.BURNING_BREATH;
            }
            if (lastHit1 == "zilyoukuu.png")
            {
                action = BattleAction.SKY_ATTACK;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
            }
            if (lastHit1 == "merazoma.png")
            {
                action = BattleAction.MERA_ZOMA;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
            }
            if (lastHit1 == "mira-.png")
            {
                action = BattleAction.MAGIC_MIRROR;
            }

            if (lastHit1 == "erugio.png"&&lastHit2 == "attack.png")
            {
                maybeCritical = (ActionIndex << 12) | TurnIndex;
                action = BattleAction.ATTACK_ALLY;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
            }

            if(lastHit1 == "samidare.png")
            {
                action = BattleAction.MULTITHRUST;
                NeedDamage2 = (ActionIndex << 12) | TurnIndex;
            }

            if (lastHit1 == "no_hadou.png")
            {
                action = BattleAction.LAUGH;
            }

            if(lastHit1 == "tameru.png")
            {
                action = BattleAction.PSYCHE_UP;
            }

            if(lastHit1 == "zigosupa.png")
            {
                action = BattleAction.LIGHTNING_STORM;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
            }

            if(lastHit1 == "kuroi.png")
            {
                action = BattleAction.DARK_BREATH;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
            }

            if(lastHit1 == "erugio.png" && lastHit2 == "uhsc.png")
            {
                action = BattleAction.ULTRA_HIGH_SPEED_COMBO;
                NeedDamage2 = (ActionIndex << 12) | TurnIndex;
            }

            if(lastHit1 == "sutemi.png")
            {
                action = BattleAction.DOUBLE_UP;
            }
            if(lastHit1 == "meisou.png")
            {
                action = BattleAction.MEDITATION;
            }
            if(lastHit1 == "madannte.png"){
                action = BattleAction.MAGIC_BURST;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
            }




            if (action != -1&&action != preAction && (lastHit1 != "" || lastHit2 != ""))
            {
                if(lastHit2 != "attack.png")
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
                }
            }

            if (lastHit1 == "")
            {
                preAction = 0;
            }
           

            return true;
        }

        public Form1()
        {
            InitializeComponent();

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
            _timer.Interval = 200;
            _timer.Tick += new EventHandler(CaptureFrame);
            _timer.Start();


            // テンプレート画像をキャッシュ
            LoadTemplatesToCache();

            for (int i = 0; i < 6; i++)
            {
                dataGridView1.Columns.Add($"Column{i + 1}", $"Column {i + 1}");
            }

            //https://github.com/rlabrecque/RLSolitaireBot/blob/f4ca851d26d348f79823e0750a1792a403cf4a45/SolitaireAI/Solitaire.cs#L120
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
                if (!templateCache.ContainsKey(templateFile))
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
                if (!templateCache.ContainsKey(templateFile))
                {
                    Mat template = CvInvoke.Imread(templateFile, Emgu.CV.CvEnum.ImreadModes.Grayscale);
                    templateCache1.Add(templateFile, template);
                }
            }

            

            textBox1.Font = new Font(textBox1.Font.FontFamily, 24); // サイズを24に設定
            textBox2.Font = new Font(textBox1.Font.FontFamily, 24); // サイズを24に設定

        }

        private void CaptureFrame(object sender, EventArgs e)
        {

            lastHit1 = "";
            lastHit2 = "";
            using (Mat frame = new Mat())
            {
                _capture.Read(frame);

                if (!frame.IsEmpty)
                {
                    using (Mat resizedFrame = new Mat())
                    {
                        CvInvoke.Resize(frame, resizedFrame, new Size(480, 270)); // フレームをリサイズ

                        if (pictureBox1.Image != null)
                        {
                            pictureBox1.Image.Dispose();
                        }
                        // フレームの更新（ミラー用に再描画）
                        pictureBox1.Image = resizedFrame.ToBitmap();

                        // 複数領域を条件に基づいて処理する
                        ProcessCaptureAreas(frame);

                        //pictureBox1.Image = resizedFrame.ToBitmap();

                        // フレームカウンタを増加
                        frameCounter++;

                        // 5回に1回画像を保存
                        if (frameCounter % 10 == 0)
                        {
                            //pictureBox1.Image.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}.png", System.Drawing.Imaging.ImageFormat.Png);

                        }


                    }
                }

                textBox1.Text = string.Join(", ", matchResults1);
                textBox2.Text = string.Join(", ", matchResults2);

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
            new Rectangle(179, 645, 130, 50),  // 1つ目の領域
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

        }

        // キャプチャ領域の枠を描画
        private void DrawCaptureArea(Mat frame, Rectangle area)
        {
            CvInvoke.Rectangle(frame, area, new MCvScalar(0, 255, 0), 1); // 緑色の枠を描画
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
                        // pictureBoxに表示
                        if (pictureBox4.Image != null)
                        {
                            pictureBox4.Image.Dispose();
                        }
                        pictureBox4.Image = trimmed.ToBitmap();

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


                                if (matchPercentage >= 80)
                                {
                                    Console.WriteLine($"2Matched with {Path.GetFileName(templateFile)}: {matchPercentage}%");
                                    lastHit2 = Path.GetFileName(templateFile);
                                }
                            }
                        }

                        if (frameCounter % 2 == 0)
                        {
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
                        if (pictureBox2.Image != null)
                        {
                            pictureBox2.Image.Dispose();
                        }
                        pictureBox2.Image = trimmed.ToBitmap();

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

                                
                                if (matchPercentage >= 80)
                                {
                                    lastHit1 = Path.GetFileName(templateFile);
                                    Console.WriteLine($"Matched with {Path.GetFileName(templateFile)}: {matchPercentage}%");
                                }
                            }
                        }

                        if (frameCounter % 2 == 0)
                        {
                            //pictureBox2.Image.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}.png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }


            // PictureBoxを配列またはリストで管理
            PictureBox[] pictureBoxes = { pictureBox5, pictureBox6, pictureBox7 };

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
                var pictureBox = pictureBoxes[i];

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

                            // PictureBoxに画像を表示
                            if (pictureBox.Image != null)
                            {
                                pictureBox.Image.Dispose();
                            }
                            pictureBox.Image = trimmed.ToBitmap();
                            if (frameCounter % 10 == 0)
                            {
                                //pictureBox.Image.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}_{i}.png", System.Drawing.Imaging.ImageFormat.Png);

                            }
                        }
                    }
                }
            }
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
            Image<Bgr, Byte> img = cropped.ToImage<Bgr, Byte>();

            // RGBの下限と上限を設定
            var lowerBound = new Bgr(140, 140, 140); // RGBそれぞれ200以上
            var upperBound = new Bgr(255, 255, 255); // 白の範囲

            // マスクを作成 (範囲内のピクセルが白、それ以外が黒)
            Image<Gray, Byte> mask = img.InRange(lowerBound, upperBound);

            // マスクを適用して白黒画像を作成
            Image<Bgr, Byte> result = img.CopyBlank();
            result.SetValue(new Bgr(255, 255, 255), mask);

            Rectangle[] areas = {
                new Rectangle(0, 0, 60, 60),
                new Rectangle(43, 0, 60, 60),
                new Rectangle(87, 0, 60, 60),
            };

            PictureBox[] pictureBoxes = { pictureBox8, pictureBox9, pictureBox10 };


            // 各領域に対してトリミングを行い、異なるPictureBoxに表示
            for (int i = 0; i < areas.Length; i++)
            {
                var area = areas[i];
                var pictureBox = pictureBoxes[i];

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
                                // PictureBoxに画像を表示
                                if (pictureBox.Image != null)
                                {
                                    pictureBox.Image.Dispose();
                                }
                                pictureBox.Image = trimmed.ToBitmap();
                               // pictureBox.Image.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}_{i}.png", System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }
                    }
                }
            }

            result.ROI = Rectangle.Empty;

            // pictureBoxに表示
            if (pictureBox3.Image != null)
            {
                pictureBox3.Image.Dispose();
            }
            pictureBox3.Image = result.ToBitmap();

            // 5回に1回画像を保存
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer.Stop();
            _capture.Dispose();
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
    }
}

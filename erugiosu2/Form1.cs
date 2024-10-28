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
        int[] matchResults1 = new int[3];
        int[] matchResults2= new int[3];

        public Form1()
        {
            InitializeComponent();

            // カメラ初期化
            _capture = new VideoCapture(0);
            _capture.Set(CapProp.FrameWidth, 1920);
            _capture.Set(CapProp.FrameHeight, 1080);

            // タイマー設定（5fps）
            _timer = new Timer();
            _timer.Interval = 200;
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
                    Mat template = CvInvoke.Imread(templateFile, Emgu.CV.CvEnum.ImreadModes.Color);
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
                    Mat template = CvInvoke.Imread(templateFile, Emgu.CV.CvEnum.ImreadModes.Color);
                    NumberCache.Add(templateFile, template);
                }
            }
            
        }

        private void CaptureFrame(object sender, EventArgs e)
        {
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


               
                frame.Dispose();
            }
        }

        // 領域の指定と処理を行う関数
        private void ProcessCaptureAreas(Mat frame)
        {
            // 複数のキャプチャ領域の座標を指定
            Rectangle[] areas = {
            new Rectangle(78, 645, 140, 60),  // 1つ目の領域
            // 他の条件に基づく領域もここに追加
        };

        Rectangle[] ocr = {
            new Rectangle(179, 645, 130, 50),  // 1つ目の領域
            // 他の条件に基づく領域もここに追加
        };

            foreach (var area in areas)
            {
                //// この領域に対する画像処理
                using (Mat cropped = new Mat(frame, area))
                {
                    ProcessCroppedImage(cropped);
                }


                // 枠を描画する
                //DrawCaptureArea(frame, area);
            }

            foreach (var area in ocr)
            {
                //// この領域に対する画像処理
                using (Mat cropped = new Mat(frame, area))
                {
                    ProcessCroppedImage1(cropped);
                }


                // 枠を描画する
                //DrawCaptureArea(frame, area);
            }

        }

        // キャプチャ領域の枠を描画
        private void DrawCaptureArea(Mat frame, Rectangle area)
        {
            CvInvoke.Rectangle(frame, area, new MCvScalar(0, 255, 0), 1); // 緑色の枠を描画
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
                    if (trimmed.Width <= 130 && trimmed.Height <= 45)
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
                                CvInvoke.MatchTemplate(img, template, resultMat, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

                                // 一致率を計算
                                double minVal = 0, maxVal = 0;
                                Point minLoc = new Point(), maxLoc = new Point();
                                CvInvoke.MinMaxLoc(resultMat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                                // 一致率をパーセンテージに変換
                                double matchPercentage = maxVal * 100.0;

                                
                                if (matchPercentage >= 80)
                                {
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


            //using (Mat resultMat2 = result.Mat)
            //{
            //    using (Mat trimed = TrimFirstPixel(resultMat2, 130, 50))
            //    {
            //        if (trimed.Width <= 120 && trimed.Height <= 60)
            //        {
            //            // pictureBoxに表示
            //            if (pictureBox2.Image != null)
            //            {
            //                pictureBox2.Image.Dispose();
            //            }
            //            pictureBox2.Image = trimed.ToBitmap();
            //        }
            //    }
            //}
            //return;

            // テンプレートマッチング
           
            // 縦方向の半分の範囲を設定して切り取る
            //Rectangle roi = new Rectangle(0, 0, result.Width, result.Height / 2);
            //result.ROI = roi;


            // PictureBoxを配列またはリストで管理
            PictureBox[] pictureBoxes = { pictureBox5, pictureBox6, pictureBox7 };

            // トリミングする領域の配列
            Rectangle[] areas = {
                new Rectangle(0, 0, 60, 60),
                new Rectangle(55, 0, 60, 60),
                new Rectangle(110, 0, 60, 60),
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
                                    CvInvoke.MatchTemplate(img, template, resultMat, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

                                    // 一致率を計算
                                    double minVal = 0, maxVal = 0;
                                    Point minLoc = new Point(), maxLoc = new Point();
                                    CvInvoke.MinMaxLoc(resultMat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                                    // 一致率をパーセンテージに変換
                                    double matchPercentage = maxVal * 100.0;


                                    if (matchPercentage >= 86)
                                    {
                                        matchResults1[i] = int.Parse(Path.GetFileNameWithoutExtension(templateFile));
                                        matched = true;
                                        Console.WriteLine($"Number with {Path.GetFileName(templateFile)}: {matchPercentage}%");
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
                            //pictureBox.Image.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}_{i}.png", System.Drawing.Imaging.ImageFormat.Png);
                           
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
                return new Mat(resultMat1, firstWhitePixel);
            }
            else
            {
                return resultMat1;
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
                                    CvInvoke.MatchTemplate(img, template, resultMat, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

                                    // 一致率を計算
                                    double minVal = 0, maxVal = 0;
                                    Point minLoc = new Point(), maxLoc = new Point();
                                    CvInvoke.MinMaxLoc(resultMat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                                    // 一致率をパーセンテージに変換
                                    double matchPercentage = maxVal * 100.0;


                                    if (matchPercentage >= 80)
                                    {
                                        Console.WriteLine($"Number1 {i} with {Path.GetFileName(templateFile)}: {matchPercentage}%");
                                        matchResults1[i] = int.Parse(Path.GetFileNameWithoutExtension(templateFile));
                                        matched = true;
                                    }
                                }
                            }

                            if (!matched)
                            {
                                matchResults1[i] = -1;
                            }

                            if (frameCounter % 2 == 0)
                            {
                                // PictureBoxに画像を表示
                                if (pictureBox.Image != null)
                                {
                                    pictureBox.Image.Dispose();
                                }
                                pictureBox.Image = trimmed.ToBitmap();
                                pictureBox.Image.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}_{i}.png", System.Drawing.Imaging.ImageFormat.Png);
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
            if (frameCounter % 5 == 0)
            {
                //pictureBox2.Image.Save($"C:\\Users\\Owner\\Downloads\\imp\\{frameCounter}_1.png", System.Drawing.Imaging.ImageFormat.Png);
            }
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
    }
}

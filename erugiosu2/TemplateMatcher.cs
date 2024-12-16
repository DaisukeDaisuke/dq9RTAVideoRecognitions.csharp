using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Cuda;
using System.Diagnostics.Metrics;

namespace erugiosu2
{
    internal class TemplateMatcher : IDisposable
    {
        private Dictionary<string, Mat> templateCache = new Dictionary<string, Mat>();
        private int brightnessThreshold;
        private bool disposed = false;
        private Size trimSize;
        private bool splitMode;
        private List<Rectangle>? area;
        private int _debug;
        private double Threshold;
        public const string NO_MATCH = "No Match";

        public List<string> MatchResults { get; private set; } = new List<string>();
        public List<double> MatchPercent { get; private set; } = new List<double>();

        // コンストラクタで初期化
        public TemplateMatcher(string templateDirectory, int brightnessThreshold, Size trimSize, bool splitMode, List<Rectangle>? area, int debug, double Threshold)
        {
            this.brightnessThreshold = brightnessThreshold;
            this.trimSize = trimSize; // デフォルトで130x45
            this.splitMode = splitMode;
            this.area = area;
            this._debug = debug;
            this.Threshold = Threshold;
            // テンプレートキャッシュの読み込み
            foreach (var file in Directory.GetFiles(templateDirectory, "*.png"))
            {
                if (!templateCache.ContainsKey(file))
                {
                    Mat template = CvInvoke.Imread(file, ImreadModes.Grayscale);
                    templateCache.Add(file, template);
                }
            }
        }

        /// <summary>
        /// 画像を指定領域で処理し、最良一致を取得します。
        /// </summary>
        /// <param name="source">入力画像(Mat形式)</param>
        /// <param name="areas">検出する領域のリスト（splitMode=falseのときは無視される）</param>
        public void ProcessImage(Mat source)
        {
            MatchResults.Clear();
            MatchPercent.Clear();

            using (Image<Bgr, Byte> img = source.ToImage<Bgr, Byte>())
            {
                var lowerBound = new Bgr(brightnessThreshold, brightnessThreshold, brightnessThreshold);
                var upperBound = new Bgr(255, 255, 255);
                using (Image<Gray, Byte> mask = img.InRange(lowerBound, upperBound))
                using (Image<Bgr, Byte> result = img.CopyBlank())
                {
                    result.SetValue(new Bgr(255, 255, 255), mask);

                    // 領域分割モード
                    if (splitMode)
                    {
                        if (this.area == null || this.area.Count == 0)
                            throw new ArgumentException("分割モードでは少なくとも1つの領域を指定してください。");

                        int counter = 0;
                        foreach (var area in this.area)
                        {
                            result.ROI = area;

                            using (Mat cropped = result.Mat)
                            using (Mat trimmed = Utilities.TrimFirstPixel(cropped, trimSize.Width, trimSize.Height)) // サイズ調整
                            {
                                if (trimmed.Width == trimSize.Width && trimmed.Height == trimSize.Height)
                                {
                                    Utilities.SaveMatAsImage(trimmed, this._debug + counter);
                                    counter++;
                                    var (bestMatch, bestScore) = FindBestTemplateMatch(trimmed);
                                    MatchResults.Add(bestMatch ?? "No Match");
                                    MatchPercent.Add(bestScore);
                                }
                                else
                                {
                                    MatchResults.Add("No Match");
                                    MatchPercent.Add(0.0);
                                }
                            }
                        }
                    }
                    // 全体処理モード
                    else
                    {
                        int counter = 0;
                        using (Mat trimmed = Utilities.TrimFirstPixel(result.Mat, trimSize.Width, trimSize.Height))
                        {
                            if (trimmed.Width == trimSize.Width && trimmed.Height == trimSize.Height)
                            {
                                Utilities.SaveMatAsImage(trimmed, this._debug + counter);
                                counter++;
                                var (bestMatch, bestScore) = FindBestTemplateMatch(trimmed);
                                MatchResults.Add(bestMatch ?? "No Match");
                                MatchPercent.Add(bestScore);
                            }
                            else
                            {
                                MatchResults.Add("No Match");
                                MatchPercent.Add(0.0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// テンプレートマッチングで最良一致を見つける
        /// </summary>
        /// <param name="image">入力画像(Mat形式)</param>
        /// <returns>一致したテンプレート名、またはnull</returns>
        private (string bestMatch, double bestScore) FindBestTemplateMatch(Mat image)
        {
            {
                string bestMatch = null;
                double bestScore = 0;

                foreach (var entry in templateCache)
                {
                    string templateFile = entry.Key;
                    Mat template = entry.Value;

                    using (Mat resultMat = new Mat())
                    {
                        CvInvoke.MatchTemplate(image, template, resultMat, TemplateMatchingType.CcorrNormed);

                        double minVal = 0, maxVal = 0;
                        Point minLoc = new Point(), maxLoc = new Point();
                        CvInvoke.MinMaxLoc(resultMat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                        if (maxVal > bestScore)
                        {
                            bestScore = maxVal;
                            bestMatch = Path.GetFileNameWithoutExtension(templateFile);
                        }
                    }
                }

                return bestScore >= this.Threshold ? (bestMatch, bestScore) : (null, 0); // 一致率が閾値以下ならnullと0を返す
            }
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    foreach (var template in templateCache.Values)
                    {
                        template.Dispose();
                    }
                    if(area != null)
                    {
                        area.Clear();
                        area = null;
                    }

                    // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                    // TODO: 大きなフィールドを null に設定します
                }

                // TODO: Free unmanaged resources here.

                // Note disposing has been done.
                _disposed = true;
            }
        }

        /// <summary>
        /// リソースの解放
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
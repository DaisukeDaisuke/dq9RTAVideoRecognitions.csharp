using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace erugiosu2
{
    internal class Utilities
    {

        //呼びだす際はusing使う事。使わないとメモリリークする
        public static Mat TrimFirstPixel(Mat resultMat1, int xSize, int ySize)
        {
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
                    if (color.Blue >= 50 && color.Green >= 50 && color.Red >= 50)
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
                        if (color.Blue >= 50 && color.Green >= 50 && color.Red >= 50)
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

        public static void SaveMatAsImage(Mat trimmed, int i)
        {
            Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(6, 1));
#if DEBUG
            // MatからBitmapへ変換
            using (Bitmap bmp = trimmed.ToBitmap())
            {
                // 画像をPNG形式で保存
                //bmp.Save($"C:\\Users\\Owner\\Downloads\\imp\\{i}.png", ImageFormat.Png);
            }
#endif
        }


        public static int ConvertMatchResults(List<string> input)
        {
            // 配列がnullまたは長さが3以外の場合は例外をスロー
            if (input == null)
                throw new ArgumentException("matchResults must be an array with 3 elements.");

            // その他の入力に対して動的に変換処理
            int result = 0;
            bool hasValidNumber = false;
            int[] matchResults = {0, 0, 0};

            int counter = 0;
            foreach (var matchResult in input) {
                if (matchResult != "No Match")
                {
                    string normalizedTemplate = matchResult.Split('_')[0]; // "_"以降を除去してベース番号を取得
                    matchResults[counter++] = int.Parse(normalizedTemplate);
                }
                else
                {
                    matchResults[counter++] = -1;
                }
            }

            if (matchResults[0] == -1)
            {
                return -1;
            }
            if (matchResults[1] == -1 && matchResults[2] != -1)
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
    }
}

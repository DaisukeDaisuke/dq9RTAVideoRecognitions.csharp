using Emgu.CV.CvEnum;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace erugiosu2
{
    internal class BackgroundAnalyzer
    {
        /// <summary>
        /// Mat全体の色相と彩度を分析して背景を判定する
        /// </summary>
        /// <param name="frame">背景画像のMat</param>
        /// <returns>背景の種類を示す文字列 ("Boss1" or "Boss2")</returns>
        public static int AnalyzeBackground(Mat frame)
        {
            if (frame == null || frame.IsEmpty)
            {
                throw new ArgumentException("フレームが無効です");
            }

            // BGRからHSVに変換
            Mat hsvFrame = new Mat();
            CvInvoke.CvtColor(frame, hsvFrame, ColorConversion.Bgr2Hsv);

            // HSVの各チャンネルを分割
            Mat[] hsvChannels = hsvFrame.Split(); // [0]: Hue, [1]: Saturation, [2]: Value

            // 各チャンネルの平均値を計算
            var hueMean = CvInvoke.Mean(hsvChannels[0]).V0; // 色相(Hue)の平均
            var saturationMean = CvInvoke.Mean(hsvChannels[1]).V0; // 彩度(Saturation)の平均

            // メモリ解放
            foreach (var channel in hsvChannels)
            {
                channel.Dispose();
            }
            hsvFrame.Dispose();
            // 背景の判定
            if (hueMean >= 150 && hueMean <= 190 && saturationMean > 50)
            {
                return 2;
            }
            else if (hueMean >= 130 && hueMean <= 150 && saturationMean > 50)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}

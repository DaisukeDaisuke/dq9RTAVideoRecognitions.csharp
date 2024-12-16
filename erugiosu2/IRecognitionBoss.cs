using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erugiosu2
{
    internal interface IRecognitionBoss : IDisposable
    {
        string BossName { get; } // ボスの名前
        bool ExecuteRecognition(Mat frame); // 本格的な処理
    }


}

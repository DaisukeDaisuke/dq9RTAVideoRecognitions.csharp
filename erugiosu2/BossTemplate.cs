using Emgu.CV;
using System;
using System.Diagnostics;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace erugiosu2
{
    internal class BossTemplate
    {
        public string Name { get; set; }
        public string Template_name { get; set; }
        private bool active;
        public Func<IRecognitionBoss> BossFactory { get; set; }
        public string IcopPath { get; set; } // リソースディレクトリ
        int BackgroundID { get; } // ボスの名前
        public Rectangle AwararePos { get; private set; }

        public BossTemplate(string name, Func<IRecognitionBoss> bossFactory, string template_name, string icopPath, int backgroundID, Rectangle awarePos)
        {
            Name = name;
            BossFactory = bossFactory;
            Template_name = template_name;
            IcopPath = icopPath;
            BackgroundID = backgroundID;
            AwararePos = awarePos;
        }

        public bool IsActive() => active;

        public IRecognitionBoss Activate()
        {
            active = true;
            Debug.WriteLine($"{Name} is now active.");
            return BossFactory();
        }

        public void Deactivate()
        {
            active = false;
        }

        public bool Match(string matchedImageName, Mat frame)
        {
            if (BackgroundAnalyzer.AnalyzeBackground(frame) != BackgroundID)
            {
                return false;
            }
            return matchedImageName == Template_name;
        }
    }
}

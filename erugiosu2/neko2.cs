using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.Design.AxImporter;

namespace erugiosu2
{
    internal class neko2 : IRecognitionBoss
    {
        public string BossName => "neko2";

        private readonly string resourceDir;
        private readonly UIManager actions;
        private bool disposedValue;
        private TemplateMatcher _NumberMatcher;
        private TemplateMatcher cropped1Matcher;

        private TemplateMatcher _NumberMatcher2;
        private TemplateMatcher cropped2Matcher;
        private TemplateMatcher cropped3Matcher;

        private bool ActionTaken = false;

        private int preAction = -1;
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

        public neko2(string resourceDir, UIManager actions)
        {
            this.resourceDir = resourceDir;
            this.actions = actions;
            _NumberMatcher = new TemplateMatcher(Path.Combine(resourceDir, "numbers"), 140, new Size(26, 40), true, new List<Rectangle>
            {
                new Rectangle(0, 0, 60, 60),
                new Rectangle(55, 0, 60, 60),
                new Rectangle(105, 0, 60, 60),

            }, 10, 0.8);

            cropped1Matcher = new TemplateMatcher(Path.Combine(resourceDir, "neko", "message1"), 140, new Size(130, 45), false, null, 100, 0.7);


            _NumberMatcher2 = new TemplateMatcher(Path.Combine(resourceDir, "numbers"), 150, new Size(26, 40), true, new List<Rectangle>
            {
                new Rectangle(0, 0, 60, 60),
                new Rectangle(43, 0, 60, 60),
                new Rectangle(87, 0, 60, 60),

            }, 1000, 0.8);

            cropped2Matcher = new TemplateMatcher(Path.Combine(resourceDir, "neko", "message2"), 140, new Size(130, 45), false, null, 10000, 0.7);

            cropped3Matcher = new TemplateMatcher(Path.Combine(resourceDir, "neko", "message3"), 140, new Size(130, 45), false, null, 100000, 0.68);
        }


        public bool ExecuteRecognition(Mat frame)
        {
            return ProcessCaptureAreas(frame);
        }

        // 領域の指定と処理を行う関数
        private bool ProcessCaptureAreas(Mat frame)
        {
            // 複数のキャプチャ領域の座標を指定
            Rectangle[] areas = {
                new Rectangle(78, 645, 160, 70),
            };

            Rectangle[] ocr = {
                new Rectangle(179, 645, 200, 70),
            };

            Rectangle[] ocr2 = {
                new Rectangle(420, 645, 200, 70),
            };


            foreach (var area in areas)
            {
                //// この領域に対する画像処理
                using (Mat cropped = new Mat(frame, area))
                {
                    _NumberMatcher.ProcessImage(cropped);
                    cropped1Matcher.ProcessImage(cropped);
                }
            }

            foreach (var area in ocr)
            {
                //// この領域に対する画像処理
                using (Mat cropped = new Mat(frame, area))
                {
                   _NumberMatcher2.ProcessImage(cropped);
                    cropped2Matcher.ProcessImage(cropped);
                }
            }

            foreach (var area in ocr2)
            {
                //// この領域に対する画像処理
                using (Mat cropped = new Mat(frame, area))
                {
                    cropped3Matcher.ProcessImage(cropped);
                }
            }


            StringBuilder ss = new StringBuilder();
            DebugNumber(ss, _NumberMatcher.MatchResults[0], _NumberMatcher.MatchPercent[0], false);
            DebugNumber(ss, _NumberMatcher.MatchResults[1], _NumberMatcher.MatchPercent[1], false);
            DebugNumber(ss, _NumberMatcher.MatchResults[2], _NumberMatcher.MatchPercent[2], true);
            DebugNumber(ss, _NumberMatcher2.MatchResults[0], _NumberMatcher2.MatchPercent[0], false);
            DebugNumber(ss, _NumberMatcher2.MatchResults[1], _NumberMatcher2.MatchPercent[1], false);
            DebugNumber(ss, _NumberMatcher2.MatchResults[2], _NumberMatcher2.MatchPercent[2], true);
            DebugNumber(ss, cropped1Matcher.MatchResults[0], cropped1Matcher.MatchPercent[0], true);
            DebugNumber(ss, cropped2Matcher.MatchResults[0], cropped2Matcher.MatchPercent[0], true);
            DebugNumber(ss, cropped3Matcher.MatchResults[0], cropped3Matcher.MatchPercent[0], true);
            actions.ExecuteUpdateDebugActions(ss.ToString());

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

            return ProcessState();
        }

        

        private void DebugNumber(StringBuilder ss, string mm, double mp, bool trim)
        {
            if (mm == "No Match")
            {
                ss.Append("n/a, 0%");
                if (trim)
                {
                    ss.Append("\r\n");
                }
                else
                {
                    ss.Append(": ");
                }
            }
            else
            {
                mp = Math.Round(mp * 100, 1);
                ss.Append(mm).Append(", ").Append(mp.ToString());
                if (trim)
                {
                    ss.Append("%\r\n");
                }
                else
                {
                    ss.Append("%: ");
                }
            }
        }

        // キャプチャ領域の枠を描画
        private void DrawCaptureArea(Mat frame, Rectangle area)
        {
            CvInvoke.Rectangle(frame, area, new MCvScalar(150, 150, 150), 5); // 緑色の枠を描画
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト
                    _NumberMatcher.Dispose();
                    cropped1Matcher.Dispose();
                    cropped2Matcher.Dispose();
                    _NumberMatcher2.Dispose();
                    _NumberMatcher = null;
                    cropped1Matcher = null;
                    cropped2Matcher = null;
                    _NumberMatcher2 = null;

                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }


        private bool ProcessState()
        {
            var lastHit1 = cropped1Matcher.MatchResults[0];
            var lastHit2 = cropped2Matcher.MatchResults[0];
            var lastHit3 = cropped3Matcher.MatchResults[0];

            int action = -1;

            Debug.WriteLine(lastHit2);

            int damageTest1 = Utilities.ConvertMatchResults(_NumberMatcher.MatchResults);
            int damageTest2 = Utilities.ConvertMatchResults(_NumberMatcher2.MatchResults);
            if (NeedDamage1 != -1 && NeedDamage1Enabled || lastdamage1 < damageTest1)
            {
                if (lastHit1 == "guard" || lastHit1 == "miss" || lastHit1 == "mikawasi")
                {
                    int turnind = NeedDamage1 & 0xfff;
                    int actionid = (NeedDamage1 >> 12) & 0xf;
                    NeedDamage1Enabled = false;
                    //NeedDamage1 = -1;
                    maybeCritical = -1;
                    actions.ExecuteUpdateDamage(turnind, actionid, 0);
                }

                if (damageTest1 != -1)
                {
                    lastdamage1 = damageTest1;
                    int turnind = NeedDamage1 & 0xfff;
                    int actionid = (NeedDamage1 >> 12) & 0xf;
                    actions.ExecuteUpdateDamage(turnind, actionid, damageTest1);
                    NeedDamage1Enabled = false;
                    //NeedDamage1 = -1;
                }
                else
                {

                    if (damageTest2 != -1 && damageTest1 < damageTest2)
                    {
                        lastdamage1 = damageTest2;
                        int turnind = NeedDamage1 & 0xfff;
                        int actionind = (NeedDamage1 >> 12) & 0xf;
                        NeedDamage1Enabled = false;
                        //NeedDamage1 = -1;
                        actions.ExecuteUpdateDamage(turnind, actionind, damageTest2);
                    }
                }
                return false;
            }
            else if ((NeedDamage2 != -1 && NeedDamage2Enabled) || lastdamage2 < damageTest2)
            {
                if (lastHit1 == "guard" || lastHit1 == "miss" || lastHit1 == "mikawasi")
                {
                    int turnind = NeedDamage2 & 0xfff;
                    int actionind = (NeedDamage2 >> 12) & 0xf;
                    //NeedDamage2 = -1;
                    NeedDamage2Enabled = false;
                    maybeCritical = -1;
                    lastdamage2 = -1;
                    actions.ExecuteUpdateDamage(turnind, actionind, 0);
                }
                if (damageTest2 != -1)
                {
                    int turnind = NeedDamage2 & 0xfff;
                    int actionind = (NeedDamage2 >> 12) & 0xf;
                    NeedDamage2Enabled = false;
                    //NeedDamage2 = -1;
                    lastdamage2 = damageTest2;
                    actions.ExecuteUpdateDamage(turnind, actionind, damageTest2);
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
                        actions.ExecuteUpdateDamage(turnind, actionid, damageTest1);
                    }
                }
                return false;
            }

            if (lastHit1 == "sukara")
            {
                action = BattleAction.BUFF;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }
            if (lastHit1 == "kaenn")
            {
                action = BattleAction.FLAME_SLASH;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }
            if (lastHit1 == "mahilyado")
            {
                action = BattleAction.KACRACKLE_SLASH;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
            }
            if (lastHit1 == "mazinn")
            {
                action = BattleAction.HATCHET_MAN;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }
            if (lastHit2 == "kiriage")
            {
                action = BattleAction.UPWARD_SLICE;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }
            if (lastHit2 == "tatia")
            {
                action = BattleAction.TURN_SKIPPED;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }

            if (lastHit1 == "gilyumei" && lastHit3 == "attack")
            {
                maybeCritical = (ActionIndex << 12) | TurnIndex;
                action = BattleAction.ATTACK_ENEMY;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
            }

            if (lastHit1 == "samidare" || lastHit1 == "samidare2")
            {
                action = BattleAction.MULTITHRUST;
                NeedDamage2 = (ActionIndex << 12) | TurnIndex;
                NeedDamage1 = -1;
                ActionTaken = true;
            }
            if (lastHit1 == "fullheal")
            {
                action = BattleAction.FULLHEAL;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }
            if (lastHit1 == "more_heal")
            {
                action = BattleAction.MORE_HEAL;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }

            if (lastHit1 == "defense_champion" && lastHit2 == "defense_champion2")
            {
                action = BattleAction.DEFENDING_CHAMPION;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }

            if (lastHit1 == "sleeping2" && (lastHit2 == "dead"))
            {
                action = BattleAction.DEAD;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
            }

            if (lastHit1 == "song")
            {
                action = BattleAction.SONG;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }

            if (lastHit1 == "sippuu")
            {
                action = BattleAction.MERCURIAL_THRUST;
                NeedDamage1 = (ActionIndex << 12) | TurnIndex;
                NeedDamage2 = -1;
                ActionTaken = true;
            }
            if (lastHit1 == "sutemi")
            {
                action = BattleAction.DOUBLE_UP;
                NeedDamage1 = -1;
                NeedDamage2 = -1;
                ActionTaken = true;
            }
            if(lastHit1 == "gilyumei" && lastHit3 == "def")
            {
                return true;
            }
            

            if (action != -1 && action != preAction && (lastHit1 != "" || lastHit2 != ""))
            {
                NeedDamage1Enabled = NeedDamage1 != -1;
                NeedDamage2Enabled = NeedDamage2 != -1;
                lastdamage1 = -1;
                lastdamage2 = -1;
                preAction = action;
                actions.ExecuteRecordAction(TurnIndex, ActionIndex, action);
                if (NeedDamage1 == -1 && NeedDamage2 == -1)
                {
                    actions.ExecuteUpdateDamage(TurnIndex, ActionIndex, 0);
                }
                ActionIndex++;
                if (ActionIndex == 3)
                {
                    ActionIndex = 0;
                    TurnIndex++;
                    ActionTaken = false;
                }
            }
            else if (action != -1 && action == preAction)
            {
                LastDetection = DateTime.Now;
                if (ActionIndex == 0)
                {
                    ActionTaken = false;
                }
            }
            else
            {
                if (ActionIndex == 0)
                {
                    ActionTaken = false;
                }
            }

            DateTime currentTime = DateTime.Now;

            //Debug.WriteLine((currentTime - LastDetection).TotalSeconds);

            if ((currentTime - LastDetection).TotalSeconds > 5)
            {
                // LastDetectionは1秒以上前です
                if (lastHit1 == "")
                {
                    preAction = 0;
                }
            }



            return false;
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erugiosu2
{
    internal class BattleAction
    {
        public int Action { get; set; }     // 行動内容（例：「攻撃」「防御」）
        public int Damage { get; set; } = -1;  // デフォルトで-1（未確定の状態を示す）

        public bool IsDamagePending => Damage == -1; // ダメージが未確定かを判定

        public BattleAction(int action)
        {
            Action = action;
        }

        public static string updateText1(Dictionary<int, List<BattleAction>> battleLog)
        {
            // 各カテゴリのデータを格納する StringBuilder
            System.Text.StringBuilder damagesSb = new System.Text.StringBuilder();
            System.Text.StringBuilder actionSb = new System.Text.StringBuilder();
            System.Text.StringBuilder aActionsSb = new System.Text.StringBuilder();

            // battleLog内のすべてのエントリをループ
            foreach (var entry in battleLog)
            {
                foreach (var action in entry.Value)
                {
                    // ダメージが未確定の場合はスキップ
                    if (action.IsDamagePending)
                    {
                        continue;
                    }

                    // 味方アクションの集計
                    if (IsAllyAction(action)) // 味方かを判定する関数
                    {
                        aActionsSb.Append(action.Action).Append(" ");
                    }
                    else
                    {
                        // アクションを集計（例: actionSb に追加）
                        actionSb.Append(action.Action).Append(" ");
                    }

                    // 確定したダメージ値を集計（例: damagesSb に追加）
                    damagesSb.Append(action.Damage).Append(" ");
                }
            }

            // 集計結果を文字列に変換
            string damages = damagesSb.ToString().Trim();
            string actions = actionSb.ToString().Trim();
            string aActions = aActionsSb.ToString().Trim();

            return $"\"{actions}\" \"{aActions}\" \"{damages}\"";
        }




        public const int ATTACK_ENEMY = 1;
        public const int ULTRA_HIGH_SPEED_COMBO = 2;
        public const int SWITCH_2B = 3;
        public const int SWITCH_2C = 4;
        public const int LIGHTNING_STORM = 5;
        public const int CRITICAL_ATTACK = 6;
        public const int SKY_ATTACK = 8;
        public const int MERA_ZOMA = 9;
        public const int FREEZING_BLIZZARD = 10;
        public const int SWITCH_2A = 11;
        public const int LULLAB_EYE = 12;
        public const int SWITCH_2E = 13;
        public const int LAUGH = 15;
        public const int DISRUPTIVE_WAVE = 16;
        public const int BURNING_BREATH = 17;
        public const int DARK_BREATH = 18;
        public const int SWITCH_2D = 19;
        public const int INACTIVE_ENEMY = 21;
        public const int INACTIVE_ALLY = 22;
        public const int MEDICINAL_HERBS = 23;
        public const int PARALYSIS = 24;
        public const int ATTACK_ALLY = 25;
        public const int HEAL = 26;
        public const int DEFENCE = 27;
        public const int CURE_PARALYSIS = 28;
        public const int BUFF = 30;
        public const int MAGIC_MIRROR = 31;
        public const int MORE_HEAL = 32;
        public const int DOUBLE_UP = 33;
        public const int MULTITHRUST = 34;
        public const int SLEEPING = 35;
        public const int MIDHEAL = 36;
        public const int FULLHEAL = 37;
        public const int DEFENDING_CHAMPION = 38;
        public const int PSYCHE_UP = 39;
        public const int CURE_SLEEPING = 40;
        public const int MEDITATION = 41;
        public const int MAGIC_BURST = 42;
        public const int RESTORE_MP = 43;
        public const int MERCURIAL_THRUST = 44;
        public const int THUNDER_THRUST = 45;
        public const int TURN_SKIPPED = 46;
        public const int SAGE_ELIXIR = 47;
        public const int ELFIN_ELIXIR = 48;
        public const int MAGIC_WATER = 49;
        public const int SPECIAL_MEDICINE = 50;
        public const int DEAD = 51;
        public const int SONG = 52;


        // 味方アクションかどうかを判定するダミー関数（実装は任意）
        private static bool IsDamageActions(BattleAction action)
        {
            // AllyAction に action.Action がキーとして存在すれば、味方アクションと判定
            return hasDamageActions.ContainsKey(action.Action);
        }


        private static readonly Dictionary<int, string> hasDamageActions = new()
    {
        { ATTACK_ENEMY, "攻撃" },
        { ULTRA_HIGH_SPEED_COMBO, "超高速連打" },
        { SKY_ATTACK, "上空から攻撃" },
        { CRITICAL_ATTACK, "痛恨" },
        { DARK_BREATH, "黒輝く息" },
        { FREEZING_BLIZZARD, "凍える吹雪" },
        { MERA_ZOMA, "メラゾーマ" },
        { MULTITHRUST, "さみだれ" },
        { ATTACK_ALLY, "攻撃" },
        { LIGHTNING_STORM, "ジゴスパ" },
        { MAGIC_BURST, "マダンテ" },
        { MERCURIAL_THRUST, "しっぷう突き" },
    };

        // 味方アクションかどうかを判定するダミー関数（実装は任意）
        private static bool IsAllyAction(BattleAction action)
        {
            // AllyAction に action.Action がキーとして存在すれば、味方アクションと判定
            return AllyAction.ContainsKey(action.Action);
        }

        private static readonly Dictionary<int, string> AllyAction = new()
    {
        { BUFF, "スカラ" },
        { PARALYSIS, "麻痺で動けない" },
        { CURE_PARALYSIS, "麻痺回復" },
        { MORE_HEAL, "ベホイム" },
        { MIDHEAL, "ベホイミ" },
        { DOUBLE_UP, "すてみ" },
        { MULTITHRUST, "さみだれ" },
        { ATTACK_ALLY, "攻撃" },
        { HEAL, "ホイミ" },
        { DEFENCE, "防御" },
        { MAGIC_MIRROR, "ミラーシールド" },
        { SLEEPING, "眠っている！" },
        { CURE_SLEEPING, "起きた" },
        { FULLHEAL, "ベホマ" },
        { DEFENDING_CHAMPION, "大防御" },
        { MERCURIAL_THRUST, "しっぷう突き" },
        { TURN_SKIPPED, "**ターンスキップ**" },
        { SAGE_ELIXIR, "賢者聖水" },
        { ELFIN_ELIXIR, "エルフののみぐすり" },
        { MAGIC_WATER, "まほうのせいすい" },
        { DEAD, "しんでしまった！" },
        { SONG, "ゴスペルソング" }
    };

        private static readonly Dictionary<int, string> actionNames = new()
    {
        { BUFF, "スカラ" },
        { ATTACK_ENEMY, "攻撃" },
        { PARALYSIS, "麻痺で動けない" },
        { CURE_PARALYSIS, "麻痺回復" },
        { ULTRA_HIGH_SPEED_COMBO, "超高速連打" },
        { SKY_ATTACK, "上空から攻撃" },
        { CRITICAL_ATTACK, "痛恨" },
        { LAUGH, "笑い" },
        { DISRUPTIVE_WAVE, "凍てつく波動" },
        { BURNING_BREATH, "やけつくいき" },
        { DARK_BREATH, "黒輝く息" },
        { MORE_HEAL, "ベホイム" },
        { MIDHEAL, "ベホイミ" },
        { FREEZING_BLIZZARD, "凍える吹雪" },
        { MERA_ZOMA, "メラゾーマ" },
        { DOUBLE_UP, "すてみ" },
        { MULTITHRUST, "さみだれ" },
        { ATTACK_ALLY, "攻撃" },
        { HEAL, "ホイミ" },
        { DEFENCE, "防御" },
        { MAGIC_MIRROR, "ミラーシールド" },
        { LIGHTNING_STORM, "ジゴスパ" },
        { LULLAB_EYE, "あやしいひとみ" },
        { SLEEPING, "眠っている！" },
        { CURE_SLEEPING, "起きた" },
        { FULLHEAL, "ベホマ" },
        { DEFENDING_CHAMPION, "大防御" },
        { PSYCHE_UP, "ためる(敵)" },
        { MEDITATION, "瞑想" },
        { MAGIC_BURST, "マダンテ" },
        { RESTORE_MP, "祈り" },
        { MERCURIAL_THRUST, "しっぷう突き" },
        { TURN_SKIPPED, "**ターンスキップ**" },
        { SAGE_ELIXIR, "賢者聖水" },
        { ELFIN_ELIXIR, "エルフののみぐすり" },
        { MAGIC_WATER, "まほうのせいすい" },
        { DEAD, "しんでしまった！" },
        { SONG, "ゴスペルソング" }
    };

        public static string GetActionName(int actionId) =>
            actionNames.TryGetValue(actionId, out var name) ? name : "Unknown Action";
    }


}

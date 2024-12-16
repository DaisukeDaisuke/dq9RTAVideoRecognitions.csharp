using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erugiosu2
{
    internal class UIManager
    {
        public UIManager(Action<int, int, int> recordAction, Action<int, int, int> updateAction, Action<int, int, int> updateDamage, Action clearActions, Action<string> updateDebug)
        {
            RecordAction = recordAction;
            UpdateAction = updateAction;
            UpdateDamage = updateDamage;
            ClearActions = clearActions;
            UpdateDebug = updateDebug;
        }

        // 行動を記録するメソッド
        public Action<int, int, int> RecordAction { get; set; }

        // 行動を修正するメソッド
        public Action<int, int, int> UpdateAction { get; set; }

        // ダメージを更新するメソッド
        public Action<int, int, int> UpdateDamage { get; set; }

        // すべての行動をクリアするメソッド
        public Action ClearActions { get; set; }

        public Action<string> UpdateDebug { get; set; }


        // 必要なら直接呼び出せるユーティリティメソッド
        public void ExecuteRecordAction(int participantId, int aind, int action)
        {
            RecordAction?.Invoke(participantId, aind, action);
        }

        public void ExecuteUpdateAction(int participantId, int actionIndex, int newAction)
        {
            UpdateAction?.Invoke(participantId, actionIndex, newAction);
        }

        public void ExecuteUpdateDamage(int participantId, int actionIndex, int damage)
        {
            UpdateDamage?.Invoke(participantId, actionIndex, damage);
        }

        public void ExecuteClearActions()
        {
            ClearActions?.Invoke();
        }

        public void ExecuteUpdateDebugActions(string Text)
        {
            UpdateDebug?.Invoke(Text);
        }
    }
}

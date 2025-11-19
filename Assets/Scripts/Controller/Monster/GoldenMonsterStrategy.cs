using Module.Data;
using Utils;

namespace Controller.Monster
{
    public class GoldenMonsterStrategy : IMonsterBehaviorStrategy
    {
        public void OnInit(MonsterController monster)
        {
      
        }
        
        public void OnTakeDamage(MonsterController monster, int damage)
        {
            // 金怪受击立刻逃跑
            // 切换到逃跑状态（若尚未逃跑）
            if ( monster.state != MonsterState.Flee)
                monster.ChangeState(MonsterState.Flee);
            // 广播逃跑
            EventCenter.Instance.TriggerEvent(EventMessages.NotifyToFlee,  monster.monsterType);
        }

        public bool ShouldFlee(MonsterController monster)
        {
            return true;
        }

        public void OnDie(MonsterController monster)
        {
            // 金怪掉落金箱子
            // 或额外掉落金币
        }
    }
}
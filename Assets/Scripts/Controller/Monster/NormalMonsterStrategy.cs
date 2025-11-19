using Module.Data;
using Utils;

namespace Controller.Monster
{
    public class NormalMonsterStrategy : IMonsterBehaviorStrategy
    {
        public void OnInit(MonsterController monster)
        {
        
        }

        public void OnTakeDamage(MonsterController monster, int damage)
        {
            if ( monster.state != MonsterState.Flee)
                monster.ChangeState(MonsterState.Flee);
            // 广播逃跑
            EventCenter.Instance.TriggerEvent(EventMessages.NotifyToFlee,  monster.monsterType);
        }

        public bool ShouldFlee(MonsterController monster)
        {
            return true; // 普通怪会逃跑
        }

        public void OnDie(MonsterController monster)
        {
        
        }
    }
}
namespace Controller.Monster
{
    public class GiantMonsterStrategy : IMonsterBehaviorStrategy
    {
        public void OnInit(MonsterController monster)
        {
            // 巨型怪物初始化：血量更高、速度更慢
   
        }

        public void OnTakeDamage(MonsterController monster, int damage)
        {
            // 不逃跑，原地战斗
            // 可以加入受击硬直
        }

        public bool ShouldFlee(MonsterController monster)
        {
            return false; // 巨型怪永远不逃跑
        }

        public void OnDie(MonsterController monster)
        {
            // 巨型怪掉落更多资源
        }
    }
}
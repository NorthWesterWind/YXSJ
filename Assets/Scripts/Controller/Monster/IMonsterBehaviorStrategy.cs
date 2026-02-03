namespace Controller.Monster
{
    public interface IMonsterBehaviorStrategy
    {
        void OnInit(MonsterController monster);
        void OnTakeDamage(MonsterController monster, float damage);
        void OnDie(MonsterController monster);
        bool ShouldFlee(MonsterController monster);
    }
}
using UnityEngine;

namespace Controller
{
    public class WeaponController : MonoBehaviour
    {
        public int atkValue;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // 不受物理力影响
            rb.simulated = true;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true; // 攻击检测必须是Trigger
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Monster"))
            {
                Debug.Log($"[Weapon] 攻击命中怪物: {other.name}");
                AttackMonster(other.gameObject);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Monster"))
            {
                AttackMonster(other.gameObject);
            }
        }

        private void AttackMonster(GameObject monster)
        {
            var monsterCtrl = monster.GetComponent<MonsterController>();
            if (monsterCtrl != null && monsterCtrl.currentHp > 0)
            {
                monsterCtrl.TakeDamage(atkValue ,transform);
            }
           
        }
    }
}

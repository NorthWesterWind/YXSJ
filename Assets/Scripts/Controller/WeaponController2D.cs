using System;
using Module;
using UnityEngine;
using World.Controller;

public class WeaponController2D : MonoBehaviour
{
    private int atkValue;
    void Awake()
    {
        atkValue = Convert.ToInt32(PlayerDataModule.Instance.data.atk + PlayerDataModule.Instance.data.addAtk);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            AttackMonster(other.gameObject);
        }
    }

    private void AttackMonster(GameObject monster)
    {
        var monsterCtr2 = monster.GetComponent<MonsterController2D>();
        if (monsterCtr2 != null && !monsterCtr2.isDead)
        {
            if (monsterCtr2.TakeDamage(atkValue))
            {
                AudioSourceController.Instance?.PlayMonsterHitSfx(monsterCtr2.monsterType);
            }
        }
    }
}

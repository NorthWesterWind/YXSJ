using System;
using System.Xml.Schema;
using Module;
using Module.Data;
using UnityEngine;
using Utils;

namespace Controller
{
    public class WeaponController : MonoBehaviour
    {

        void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerValueInfo, UpdatePlayerValueInfo);
        }
        void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerValueInfo, UpdatePlayerValueInfo);
        }

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true; // 攻击检测必须是Trigger
        }


        public float atkValue;
        private float slowDownValue;
        public void UpdatePlayerValueInfo(params object[] args)
        {
            atkValue = Convert.ToInt32(PlayerDataModule.Instance.data.atk + PlayerDataModule.Instance.data.addAtk);
            var card = PlayerDataModule.Instance.data.cardUpProgressesList.Find(x => x.developType == CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk);
            if (card != null)
            {
                atkValue *= (1f + card.level * 0.3f);
            }
            if (Mathf.Approximately(PlayerDataModule.Instance.data.addweaponSize, 0.25f))
            {
                transform.localScale = new Vector3(1.25f, 1.25f, 1f);
                transform.localPosition = new Vector3(2.2f, 0.1f, 0f);
            }
            else if (Mathf.Approximately(PlayerDataModule.Instance.data.addweaponSize, 0.5f))
            {
                transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                transform.localPosition = new Vector3(2.4f, 0.1f, 0f);
            }

            slowDownValue = PlayerDataModule.Instance.data.slowDownValue * (1 + PlayerDataModule.Instance.data.addSlowDownValue);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Monster"))
            {
                Debug.Log($"[Weapon] 攻击命中怪物: {other.name}");
                AttackMonster(other.gameObject);
            }
        }

        // private void OnTriggerStay2D(Collider2D other)
        // {
        //     if (other.CompareTag("Monster"))
        //     {
        //         AttackMonster(other.gameObject);
        //     }
        // }

        private void AttackMonster(GameObject monster)
        {
            var monsterCtrl = monster.GetComponent<MonsterController>();
            if (monsterCtrl != null && monsterCtrl.currentHp > 0)
            {
                monsterCtrl.TakeDamage(atkValue, transform, slowDownValue);
            }
            var monsterCtr2 = monster.GetComponent<MonsterController2D>();
            if (monsterCtr2 != null && !monsterCtr2.isDead)
            {
                Debug.Log($"[Weapon] 攻击命中怪物2D: atkvalue: {atkValue}");
                monsterCtr2.TakeDamage(atkValue);
            }

        }
    }
}

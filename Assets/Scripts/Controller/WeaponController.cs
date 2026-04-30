using System;
using System.Collections.Generic;
using Module;
using Module.Data;
using UnityEngine;
using Utils;
using World.Controller;

namespace Controller
{
    public class WeaponController : MonoBehaviour
    {
        public bool isPlayer;
        public bool playMonsterHitSfx = true;
        public float hitInterval = 0.25f;
        private readonly Dictionary<int, float> nextHitTime = new();
        public WarehouseCategoryType warehouseCategoryType;

        void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdatePlayerValueInfo, UpdatePlayerValueInfo);
            UpdatePlayerValueInfo();
        }
        void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdatePlayerValueInfo, UpdatePlayerValueInfo);
            nextHitTime.Clear();
        }

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogError($"[WeaponController] Missing Collider2D on {name}", this);
                enabled = false;
                return;
            }
            col.isTrigger = true;
        }


        public float atkValue;
        private float slowDownValue;
        public void UpdatePlayerValueInfo(params object[] args)
        {
            var playerData = PlayerDataModule.Instance?.data;
            if (playerData == null)
            {
                atkValue = 0f;
                slowDownValue = 0f;
                return;
            }

            if (isPlayer)
            {
                atkValue = Convert.ToInt32(playerData.atk + playerData.addAtk);
                var card = playerData.cardUpProgressesList?.Find(x => x.developType == CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk);
                if (card != null)
                {
                    atkValue *= (1f + card.level * 0.3f);
                }
                if (Mathf.Approximately(playerData.addweaponSize, 0.25f))
                {
                    transform.localScale = new Vector3(1.25f, 1.25f, 1f);
                    transform.localPosition = new Vector3(2.2f, 0.1f, 0f);
                }
                else if (Mathf.Approximately(playerData.addweaponSize, 0.5f))
                {
                    transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                    transform.localPosition = new Vector3(2.4f, 0.1f, 0f);
                }

                slowDownValue = playerData.slowDownValue * (1 + playerData.addSlowDownValue);
            }
            else
            {
                var warehouse = playerData.warehouselist?.Find(x => x.warehouseCategoryType == warehouseCategoryType);
                if (warehouse == null)
                {
                    atkValue = 0f;
                    return;
                }
                atkValue = Convert.ToInt32(warehouse.atk);
                var card = playerData.cardUpProgressesList?.Find(x => x.developType == CardDevelopType.UpgradeCharacterWithXuanCaiTuAtk);
                if (card != null)
                {
                    atkValue *= (1f + card.level * 0.3f);
                }
            }

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryAttack(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryAttack(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other == null) return;
            nextHitTime.Remove(other.GetInstanceID());
        }

        private void TryAttack(Collider2D other)
        {
            if (other == null || !other.CompareTag("Monster"))
            {
                return;
            }

            int id = other.GetInstanceID();
            if (nextHitTime.TryGetValue(id, out var nextTime) && Time.time < nextTime)
            {
                return;
            }

            nextHitTime[id] = Time.time + hitInterval;
            AttackMonster(other.gameObject);
        }

        private void AttackMonster(GameObject monster)
        {
            var monsterCtrl = monster.GetComponent<MonsterController>();
            if (monsterCtrl != null && monsterCtrl.currentHp > 0)
            {
                if (monsterCtrl.TakeDamage(atkValue, transform, slowDownValue, isPlayer))
                {
                    if (playMonsterHitSfx)
                    {
                        AudioSourceController.Instance?.PlayMonsterHitSfx(monsterCtrl.monsterType);
                    }
                }
            }

            var monsterCtr2 = monster.GetComponent<MonsterController2D>();
            if (monsterCtr2 != null && !monsterCtr2.isDead)
            {
                if (monsterCtr2.TakeDamage(atkValue))
                {
                    if (playMonsterHitSfx)
                    {
                        AudioSourceController.Instance?.PlayMonsterHitSfx(monsterCtr2.monsterType);
                    }
                }
            }
        }
    }
}

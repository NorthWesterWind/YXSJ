using System.Collections.Generic;
using Controller.Pickups;
using Module;
using Module.Data;
using UnityEngine;
using Utils;


namespace Controller.Structure
{
    public class SalesStall : StructureBase
    {
        public Transform receiveTransform;
        public GoodsType currentGoodsType;
        public int currentGoodsCount;
        public Transform baseTransform;
        public Transform parchaseTransform;
        public PlacementGrid grid;
        public List<Production> productList = new();
        public Transform transferPoint;
        public BuildingType buildingType;
        public SpriteRenderer productIcon;
        public SpriteRenderer productIconbg;

        public List<CustomerController> customerList = new();
        private bool productLayoutDirty;

        public override void Start()
        {
            base.Start();
        }

        private void LateUpdate()
        {
            ApplyProductLayoutIfDirty();
        }
        void OnEnable()
        {
            EventCenter.Instance.AddListener(EventMessages.UpdateSturctureLockInfo, Init);
            EventCenter.Instance.AddListener(EventMessages.CustomerArrivedSell, HandleCustomerArrived);
            EventCenter.Instance.AddListener(EventMessages.CustomerLeave, HandleCustomerLeft);

        }
        void OnDisable()
        {
            EventCenter.Instance.RemoveListener(EventMessages.UpdateSturctureLockInfo, Init);
            EventCenter.Instance.RemoveListener(EventMessages.CustomerArrivedSell, HandleCustomerArrived);
            EventCenter.Instance.RemoveListener(EventMessages.CustomerLeave, HandleCustomerLeft);
        }

        public void HandleCustomerLeft(params object[] args)
        {
            if (args[0] as SalesStall != this) return;
            CustomerController c = args[1] as CustomerController;
            customerList.RemoveAll(c => c == null);
            if (customerList.Contains(c))
            {
                customerList.Remove(c);
            }
        }
        private void HandleCustomerArrived(params object[] args)
        {
            if (args.Length < 1) return;

            CustomerController c = args[0] as CustomerController;
            if (args[1] as SalesStall != this) return;
            customerList.RemoveAll(c => c == null);
            customerList.Add(c);
            TryServeNextCustomer();
        }

        public void Init(params object[] args)
        {
            var playerData = PlayerDataModule.Instance.data;
            if (GameController.Instance.unlockedBuildingTypes.Contains(buildingType))
            {
                var unlocked = playerData.structUnLockDataDic[playerData.currentMapID];
                if (!unlocked.Contains(buildingType))
                {
                    unlocked.Add(buildingType);
                }
                playerData.structLockDataDic[playerData.currentMapID].Remove(buildingType);
                playerData.structCanUnLockDataDic[playerData.currentMapID].Remove(buildingType);
            }
            var lockData = GetLockData(playerData.currentMapID);
            var state = GetStructureState(playerData, lockData);
            RefreshView(state, lockData);
        }

        public Vector2 GetPurchasePosition()
        {
            Vector2 pos = new Vector2(parchaseTransform.position.x + Random.Range(-1f, 1f), parchaseTransform.position.y + Random.Range(-1f, 0f));
            return pos;
        }

        private void RefreshView(StructureState state, StructureLockData lockData)
        {
            isLock = state == StructureState.Locked;
            isCanUnlockState = state == StructureState.CanUnlock;
            switch (state)
            {
                case StructureState.Locked:
                case StructureState.CanUnlock:
                    ShowLock(lockData);
                    break;

                case StructureState.Unlocked:
                    ShowContent();
                    break;
            }
        }

        private void ShowContent()
        {
            content.SetActive(true);
            structureLock.gameObject.SetActive(false);
            grid.basePosition = baseTransform.position;
            productIcon.sprite = _assetHandle.Get<Sprite>(Extensions.GetGoodsResNameByType(currentGoodsType));
            int newOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            productIcon.sortingOrder = newOrder + 2;
            productIconbg.sortingOrder = newOrder + 1;
            GameController.Instance.unlockedBuildingTypes.Add(buildingType);
            var unlocked = PlayerDataModule.Instance.data.structUnLockDataDic[PlayerDataModule.Instance.data.currentMapID];
            if (!unlocked.Contains(buildingType))
            {
                unlocked.Add(buildingType);
            }
        }

        public StructureLockData GetLockData(int mapId)
        {
            var list = DataController.Instance.GetStructureLockList(mapId);
            return list?.Find(s => s.buildingType == buildingType);
        }
        private StructureState GetStructureState(PlayerData playerData, StructureLockData lockData)
        {
            if (lockData == null)
                return StructureState.Unlocked;

            var locked = playerData.structLockDataDic[playerData.currentMapID];
            var unlocked = playerData.structUnLockDataDic[playerData.currentMapID];
            var canUnlock = playerData.structCanUnLockDataDic[playerData.currentMapID];

            if (unlocked.Contains(buildingType))
                return StructureState.Unlocked;

            if (locked.Contains(buildingType))
                return StructureState.Locked;

            return StructureState.CanUnlock;
        }

        public void AddGoods(Production p)
        {
            FreightClerkController.UnmarkProductReservedByFreight(p);
            p.SetState(ItemState.OnShelf);
            p.canPickup = true;
            productList.Add(p);
            currentGoodsCount++;
            MarkProductLayoutDirty();
            TryServeNextCustomer();
            if (PlayerDataModule.Instance.data.guideStep == GuideStep.SellTea)
            {
                PlayerDataModule.Instance.data.guideStep = GuideStep.ToLingZhangTai;
                UIController.Instance.Show<PlayerGuide>();
            }
        }

        private void TryServeNextCustomer()
        {
            // û�˿�
            customerList.RemoveAll(c => c == null ||
            (c.state != NpcState.WaitGouMaiWanCheng && !c.severing));
            if (customerList.Count == 0)
                return;

            // û��Ʒ
            if (productList.Count == 0)
                return;

            EnsureProductLayoutUpToDate();

            CustomerController customer = customerList[0];

            // �������ڷ����еĹ˿ͣ������ظ�����TryPurchase����purchaseList�����
            if (customer.severing)
                return;

            if (customer.data.carryNum > productList.Count)
                return;

            if (TryPurchase(customer, customer.data.carryNum, customer.purchaseList))
            {

                customerList.RemoveAt(0);
                // ����������һ��
                TryServeNextCustomer();
            }
        }


        /// <summary>
        /// ���Թ���ָ��������Ʒ���ɹ�����ʵ����Ʒ�б���ʧ�ܷ��ؿ��б�
        /// </summary>
        public bool TryPurchase(CustomerController customer, int count, List<Production> outList)
        {
            if (productList.Count < count)
                return false;
            if (customer.state != NpcState.WaitGouMaiWanCheng)
            {
                return false;
            }
            EnsureProductLayoutUpToDate();
            customer.severing = true;
            outList.Clear();
            int resolvedCount = 0;
            for (int i = 0; i < count; i++)
            {
                int lastIndex = productList.Count - 1;
                Production p = productList[lastIndex];
                productList.RemoveAt(lastIndex);
                p.canPickup = false;
                p.isTaken = true;
                p.FlyTo_1(customer.receiveTransform.position, 0.15f, customer.transform, () =>
                {
                    if (p == null)
                    {
                        resolvedCount++;
                        if (resolvedCount >= count && customer != null && customer.state == NpcState.WaitGouMaiWanCheng &&
                            customer.purchaseList.Count < customer.data.carryNum)
                        {
                            customer.severing = false;
                        }

                        return;
                    }

                    if (customer != null && customer.CanReceivePurchasedProduct())
                    {
                        customer.ReceivePurchasedProduct(p);
                    }
                    else
                    {
                        PlaceProduct(p);
                    }

                    resolvedCount++;
                    if (resolvedCount >= count && customer != null && customer.state == NpcState.WaitGouMaiWanCheng &&
                        customer.purchaseList.Count < customer.data.carryNum)
                    {
                        customer.severing = false;
                    }
                });
                grid.ReleaseOne();
                currentGoodsCount--;
            }
            MarkProductLayoutDirty();
            return true;
        }
        public void PlaceProduct(Production p)
        {
            if (p == null) return;
            p.transform.SetParent(null, true);
            p.canPickup = false;
            p.isTaken = false;
            var targetPos = grid.GetNextPosition();
            p.SetState(ItemState.HeldByAssistant);
            if (p.spriteRenderer != null)
            {
                p.spriteRenderer.sortingOrder = grid.GetLastSortingOrder(sprite.sortingOrder, 2);
            }
            p.FlyTo(targetPos, () =>
            {
                if (p == null) return;
                AddGoods(p);
            });
        }

        public void ReceiveProduct(FreightClerkController controller)
        {
            List<Production> list = controller.productList;
            for (int i = 0; i < list.Count; i++)
            {
                PlaceProduct(list[i]);
            }
            controller.productList.Clear();
        }

        public void RefreshProductLayout()
        {
            productLayoutDirty = true;
            ApplyProductLayout();
        }

        private void MarkProductLayoutDirty()
        {
            productLayoutDirty = true;
        }

        private void EnsureProductLayoutUpToDate()
        {
            ApplyProductLayoutIfDirty();
        }

        private void ApplyProductLayoutIfDirty()
        {
            if (!productLayoutDirty)
            {
                return;
            }

            ApplyProductLayout();
        }

        private void ApplyProductLayout()
        {
            productLayoutDirty = false;
            if (baseTransform != null)
            {
                grid.basePosition = baseTransform.position;
            }
            else
            {
                grid.basePosition = transform.position;
            }

            productList.RemoveAll(p => p == null);
            productList.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                int yCompare = a.transform.position.y.CompareTo(b.transform.position.y);
                if (yCompare != 0) return yCompare;
                return a.transform.position.x.CompareTo(b.transform.position.x);
            });

            int baseOrder = 30000 - Mathf.RoundToInt(transform.position.y * 100);
            if (sprite != null && sprite.sortingOrder > 0)
            {
                baseOrder = sprite.sortingOrder;
            }

            for (int i = 0; i < productList.Count; i++)
            {
                var product = productList[i];
                if (product == null) continue;

                Vector2 pos = grid.GetPositionByIndex(i);
                product.transform.position = new Vector3(pos.x, pos.y, product.transform.position.z);
                if (product.spriteRenderer != null)
                {
                    product.spriteRenderer.sortingOrder = grid.GetSortingOrderByIndex(baseOrder, 2, i);
                }
            }

            currentGoodsCount = productList.Count;
            grid.currentIndex = productList.Count;
        }
    }
}



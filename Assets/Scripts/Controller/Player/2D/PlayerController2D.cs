using System.Collections;
using System.Collections.Generic;
using Controller;
using Module;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class PlayerController2D : MonoBehaviour
{

    public SkeletonGraphic skeletonGraphic;
    public Vector2 _dirValue;
    public GameObject weapon;
    public Transform weaponRoot;
    public Image weaponRenderer;
    public float speed;
    public Rigidbody2D _rigidbody;
    public bool isMoving = false;
    private AssetHandle _assetHandle;

    void Awake()
    {
        if (_assetHandle == null) _assetHandle = GetComponent<AssetHandle>();
        if (DataController.Instance.weaponDataDic.ContainsKey(PlayerDataModule.Instance.data.currentWeapon))
        {
            WeaponData weaponData = DataController.Instance.weaponDataDic[PlayerDataModule.Instance.data.currentWeapon];
            weaponRenderer.sprite = _assetHandle.Get<Sprite>(weaponData.name);
        }
        skeletonGraphic.initialSkinName = PlayerDataModule.Instance.data.currentClothing.ToString();
        skeletonGraphic.Initialize(true);
    }
    void Start()
    {
        ApplyCurrentClothing();
        ApplyCurrentEquipment();

        var state = skeletonGraphic.AnimationState;
        var current = state.GetCurrent(0);
        if (current == null || current.Animation.Name != "攻击腿不动")
        {
            state.SetAnimation(0, "攻击腿不动", true);
        }
    }
    public float moveSpeed;
    bool newIsMoving = false;
    void Update()
    {
        newIsMoving = _dirValue != Vector2.zero;
        if (newIsMoving)
        {
            if (_dirValue.x < 0)
            {
                skeletonGraphic.transform.localScale = new Vector3(-0.6f, 0.6f, 0.6f);
                skeletonGraphic.Skeleton.SetAttachment("衣服", "衣服");
            }
            else
            {
                skeletonGraphic.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                skeletonGraphic.Skeleton.SetAttachment("衣服", "8_2");
            }
        }
        if (isMoving != newIsMoving)
        {
            isMoving = newIsMoving;
            UpdateAnimation();
        }


        weaponRoot.Rotate(0f, 0f, -speed * Time.deltaTime);
        float z = weaponRoot.localEulerAngles.z;
        if (z > 180f) z -= 360f;
        float t = Mathf.Abs(Mathf.Cos(z * Mathf.Deg2Rad));
        float scale = Mathf.Lerp(0.85f, 1.1f, t);
        weaponRoot.localScale = Vector3.one * scale;
    }
    void UpdateAnimation()
    {
        var state = skeletonGraphic.AnimationState;
        var current = state.GetCurrent(0);

        if (isMoving)
        {
            if (current == null || current.Animation.Name != "攻击")
            {
                state.SetAnimation(0, "攻击", true);
                Debug.Log("切换到：攻击");
            }
        }
        else
        {
            if (current == null || current.Animation.Name != "攻击腿不动")
            {
                state.SetAnimation(0, "攻击腿不动", true);
                Debug.Log("切换到：攻击腿不动");
            }
        }
    }


    private void FixedUpdate()
    {
        if (isMoving)
        {
            _rigidbody.MovePosition(_rigidbody.position +
                                    new Vector2(_dirValue.x, _dirValue.y) * ((PlayerDataModule.Instance.data.moveSpeed + PlayerDataModule.Instance.data.addMoveSpeed + moveSpeed) * Time.fixedDeltaTime));
        }
    }

    public void SetDir(Vector2 direction)
    {
        _dirValue = direction;
    }

    private void ApplyCurrentClothing()
    {
        if (skeletonGraphic == null)
        {
            return;
        }

        skeletonGraphic.Initialize(false);

        string skinName = PlayerDataModule.Instance.data.currentClothing.ToString();
        if (skeletonGraphic.Skeleton.Data.FindSkin(skinName) == null)
        {
            Debug.LogWarning($"PlayerController2D missing skin: {skinName}");
            return;
        }

        skeletonGraphic.Skeleton.SetSkin(skinName);
        skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        skeletonGraphic.AnimationState.Apply(skeletonGraphic.Skeleton);
    }

    private void ApplyCurrentEquipment()
    {
        if (skeletonGraphic == null)
        {
            return;
        }

        if (DataController.Instance.weaponDataDic.ContainsKey(PlayerDataModule.Instance.data.currentWeapon))
        {
            WeaponData weaponData = DataController.Instance.weaponDataDic[PlayerDataModule.Instance.data.currentWeapon];
            weaponRenderer.sprite = _assetHandle.Get<Sprite>(weaponData.name);
            skeletonGraphic.Skeleton.SetAttachment(weaponData.slotName, weaponData.attachmentName);
        }

        if (DataController.Instance.storageBagDataDic.ContainsKey(PlayerDataModule.Instance.data.currentBag))
        {
            StotageBagData stotageBagData = DataController.Instance.storageBagDataDic[PlayerDataModule.Instance.data.currentBag];
            skeletonGraphic.Skeleton.SetAttachment(stotageBagData.slotName, stotageBagData.attachmentName);
        }
    }
}

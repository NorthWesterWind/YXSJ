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
    public SkeletonGraphic weaponEffect;
    public Rigidbody2D _rigidbody;
    public bool isMoving = false;
    private AssetHandle _assetHandle;

    void Awake()
    {
        if (_assetHandle == null) _assetHandle = GetComponent<AssetHandle>();
       // skeletonGraphic = transform.Find("Character").GetComponent<SkeletonGraphic>();
        // WeaponData weaponData = DataController.Instance.weaponDataDic[PlayerDataModule.Instance.data.currentWeapon];
        // weaponRenderer.sprite = _assetHandle.Get<Sprite>(weaponData.name);
        // skeletonGraphic.Skeleton.SetAttachment(weaponData.slotName, weaponData.attachmentName);
        // SkeletonDataAsset skeletonDataAsset = _assetHandle.Get<SkeletonDataAsset>(weaponData.name + "data");
        // weaponEffect.skeletonDataAsset = skeletonDataAsset;
        // weaponEffect.Initialize(true);
        // StotageBagData stotageBagData = DataController.Instance.storageBagDataDic[PlayerDataModule.Instance.data.currentBag];
        // skeletonGraphic.Skeleton.SetAttachment(stotageBagData.slotName, stotageBagData.attachmentName);

    }

    void Update()
    {
        if (_dirValue != Vector2.zero)
        {
            isMoving = true;
            if (_dirValue.x < 0)
            {
                skeletonGraphic.transform.localScale = new Vector3(-0.6f, 0.6f, 0.6f);
            }
            else
            {
                skeletonGraphic.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            }

            var state = skeletonGraphic.AnimationState;
            var current = state.GetCurrent(0);

            if (current == null || current.Animation.Name != "攻击")
            {
                state.SetAnimation(0, "攻击", true);
            }
        }
        else
        {
            isMoving = false;
            var state = skeletonGraphic.AnimationState;
            var current = state.GetCurrent(0);

            if (current == null || current.Animation.Name != "攻击腿不动")
            {
                state.SetAnimation(0, "攻击腿不动", true);
            }



        }


        weaponRoot.Rotate(0f, 0f, -speed * Time.deltaTime);
        float z = weaponRoot.localEulerAngles.z;
        if (z > 180f) z -= 360f;
        float t = Mathf.Abs(Mathf.Cos(z * Mathf.Deg2Rad));
        float scale = Mathf.Lerp(0.85f, 1.1f, t);
        weaponRoot.localScale = Vector3.one * scale;
        weaponEffect.gameObject.SetActive(true);
        var state1 = weaponEffect.AnimationState;
        var current1 = state1.GetCurrent(0);
        if (current1 == null || current1.Animation.Name != "animation")
        {
            state1.SetAnimation(0, "animation", true);
        }
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            _rigidbody.MovePosition(_rigidbody.position +
                                    new Vector2(_dirValue.x, _dirValue.y) * ((PlayerDataModule.Instance.data.moveSpeed + PlayerDataModule.Instance.data.addMoveSpeed) * Time.fixedDeltaTime));
        }
    }

    public void SetDir(Vector2 direction)
    {
        _dirValue = direction;
    }
}

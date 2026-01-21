using DG.Tweening;
using Spine.Unity;
using UnityEngine;

namespace Controller
{
    public class InteractionTrigger : MonoBehaviour
    {
        public SpriteRenderer sprite;
        public SkeletonAnimation skeletonAnimation;
        public bool  isShowEffect;

        void Awake()
        {
            if (isShowEffect)
            {
                sprite.DOFade(0, 0.1f);
                skeletonAnimation.AnimationState.SetAnimation(0, "animation", true);
            }
            else
            {
                skeletonAnimation.gameObject.SetActive(false);
            }
            sprite.transform.DOScale(new Vector3(0.6f, 0.3f, 1), 0.3f);
        }

        public void TriggerEnter()
        {
            sprite.transform.DOScale(new Vector3(1f, 0.5f, 1), 0.3f);
        }

        public void TriggerExit()
        {
            sprite.transform.DOScale(new Vector3(0.6f, 0.3f, 1), 0.3f);
        }
    }
}
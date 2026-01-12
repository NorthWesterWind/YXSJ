using DG.Tweening;
using UnityEngine;

namespace Controller
{
    public class InteractionTrigger : MonoBehaviour
    {
        public SpriteRenderer sprite;
        void Awake()
        {
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
using DG.Tweening;
using UnityEngine;

namespace Controller
{
    public class InteractionTrigger : MonoBehaviour
    {
        public SpriteRenderer sprite;

        public void TriggerEnter()
        {
            sprite.transform.DOScale(new Vector3(1f, 0.5f, 1), 0.3f);
        }

        public void TriggerExit()
        {
            sprite.transform.DOScale(new Vector3(0.5f, 0.25f, 1), 0.3f);
        }
    }
}
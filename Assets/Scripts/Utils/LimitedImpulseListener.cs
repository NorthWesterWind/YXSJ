using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineImpulseListener))]
public class LimitedImpulseListener : MonoBehaviour
{
    public float maxDistance = 2f; // 最大偏移量（世界单位）

    private CinemachineImpulseListener listener;
    private Vector3 originalPos;

    void Awake()
    {
        listener = GetComponent<CinemachineImpulseListener>();
        originalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        Vector3 offset = transform.localPosition - originalPos;
        if (offset.magnitude > maxDistance)
        {
            transform.localPosition = originalPos + offset.normalized * maxDistance;
        }
    }
}
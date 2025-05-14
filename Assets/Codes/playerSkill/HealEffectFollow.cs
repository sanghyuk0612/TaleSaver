using UnityEngine;

public class HealEffectFollow : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;

    public void Initialize(Transform followTarget, float yOffset = 0f)
    {
        target = followTarget;
        offset = new Vector3(0f, yOffset, 0f);
    }

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}

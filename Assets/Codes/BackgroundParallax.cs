using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{

    public Vector2 parallaxFactor = new Vector2(0.1f, 0.05f);
    private Vector3 lastCameraPosition;

    void Start()
    {
        lastCameraPosition = Camera.main.transform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = Camera.main.transform.position - lastCameraPosition;
        transform.position += new Vector3(delta.x * parallaxFactor.x, delta.y * parallaxFactor.y, 0);
        lastCameraPosition = Camera.main.transform.position;
    }

    /*public Transform target; // 플레이어
    public Vector2 parallaxFactor = new Vector2(0.1f, 0.05f); // X, Y 움직임 민감도
    public Vector3 offset = new Vector3(0, 0, 10); // 카메라보다 뒤로

    private Vector3 previousTargetPos;

    private void Start()
    {
        if (target == null)
        {
            StartCoroutine(WaitForPlayer());
        }
        else
        {
            previousTargetPos = target.position;
        }
    }


    IEnumerator WaitForPlayer()
    {
        while (target == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
                previousTargetPos = target.position;
                Debug.Log("Player found and target set.");
                yield break;
            }

            yield return null;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 delta = target.position - previousTargetPos;
        Vector3 move = new Vector3(delta.x * 0.1f, delta.y * 0.05f, 0);


        transform.position += move;
        transform.position = new Vector3(transform.position.x, transform.position.y, offset.z); // z 고정

        previousTargetPos = target.position;
    }*/
}

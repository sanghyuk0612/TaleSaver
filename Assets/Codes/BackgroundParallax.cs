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
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}



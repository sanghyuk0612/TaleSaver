using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private CharacterData characterData;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>(); //

        if (characterData != null)
        {
            animator.runtimeAnimatorController = characterData.animatorController;
        }
    }
}

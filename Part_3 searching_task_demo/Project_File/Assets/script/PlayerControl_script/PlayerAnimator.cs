using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking";
    //通过这样的设置来让IsWalking 变成一个常量，否则在animator.SetBool("IsWalking", player.IsWalking());中会使用"IsWalking"，这种字符串无法不会报错

    [SerializeField] private Player player;

    private Animator animator;

    private void Awake()
    {
        animator =GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetBool(IS_WALKING, player.IsWalking());
    }
}

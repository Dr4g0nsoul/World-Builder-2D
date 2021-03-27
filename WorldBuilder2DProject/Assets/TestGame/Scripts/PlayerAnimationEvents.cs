using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public PlayerController playerController;

    public void OnAttackFinished()
    {
        if(playerController != null)
        {
            playerController.AttackFinished();
        }
    }
}

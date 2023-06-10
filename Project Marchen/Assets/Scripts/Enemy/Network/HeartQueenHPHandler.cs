using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class HeartQueenHPHandler : SkinnedEnemyHPHandler
{
    const int BossStartHP = 1000;

    protected override void Start()
    {
        if(!skipSettingStartValues)
        {
            if(Object.HasStateAuthority)
            {
                HP = BossStartHP;
                isDead = false;
            }
            
            if(heartBar != null)
            {
                heartBar.SetMaxHP(BossStartHP);
                heartBar.SetSlider(HP);
            }
        }
        else
        {
            if(heartBar != null)
            {
                heartBar.SetMaxHP(BossStartHP);
                heartBar.SetSlider(HP);
            }
        }

        isInitialized = true;
    }

    protected override IEnumerator OnDeadCO()
    {
        anim.SetTrigger("doDie");

        yield return new WaitForSeconds(10.0f);

        if(Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
    private void OnDestroy() {
        //spawn the portal
    }
}

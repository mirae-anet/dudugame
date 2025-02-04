using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

/// @brief 스폰너
public class SpawnHandler : NetworkBehaviour
{
    [Header("설정")]
    /// @brief 생성 딜레이
    public float delayTime;
    /// @brief 생성할 프리팹
    public NetworkBehaviour prefab; //EnemyHPHandler, HeartHandler
    public Transform anchorPoint;
    protected bool spawnAble = true;
    public bool skipSettingStartValues = false;
    protected TickTimer respawnDelay = TickTimer.None;

    protected virtual void Start(){}

    protected virtual void OnTriggerStay (Collider other)
    {
        if(!spawnAble)
            return;

        if (other.tag != "Player")
            return;

        if(!Object.HasStateAuthority)
            return;

        if(respawnDelay.ExpiredOrNotRunning(Runner))
            Spawn();
    }

    /// @brief 스폰한다.
    protected virtual void Spawn()
    {
        NetworkBehaviour spawned = Runner.Spawn(prefab, anchorPoint.position, Quaternion.LookRotation(transform.forward),null, initSpawnPoint);

        if(spawned.TryGetComponent<EnemyHPHandler>(out EnemyHPHandler enemyHPHandler))
            enemyHPHandler.Spawner = Object;
        else if(spawned.TryGetComponent<HeartHandler>(out HeartHandler heartHandler))
            heartHandler.Spawner = Object;
        else if(spawned.TryGetComponent<PickUpAction>(out PickUpAction pickUpAction))
            pickUpAction.Spawner = Object;

        Debug.Log($"Spawner Spawned Something");
        spawnAble = false;
        gameObject.SetActive(false);
    }

    /// @brief navMeshAgent를 가진 프리팹의 경우 원하는 위치에 생성하기 위해서 Warp시켜야함.
    private void initSpawnPoint(NetworkRunner networkRunner, NetworkObject networkObject)
    {
        if(networkObject.TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent))
            navMeshAgent.Warp(anchorPoint.position);
    }

    /// @brief 일정한 딜레이를 두고 생성하도록 타이머 설정
    public virtual void SetTimer()
    {
        if(Runner != null && Object.HasStateAuthority)
        {
            respawnDelay = TickTimer.CreateFromSeconds(Runner, delayTime );
            spawnAble = true;
        }
    }

}

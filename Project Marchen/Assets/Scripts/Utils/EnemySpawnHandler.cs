using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EnemySpawnHandler : NetworkBehaviour
{
    [Header("설정")]
    [SerializeField]
    private float delayTime;
    [SerializeField]
    private EnemyHPHandler enemyPrefab;
    [SerializeField]
    private Transform anchorPoint;
    private bool spawnAble = true;
    public bool skipSettingStartValues = false;
    TickTimer respawnDelay = TickTimer.None;

    void Start()
    {
        if(!Object.HasStateAuthority)
            return;
        
        if(!skipSettingStartValues)
        {
            SpawnEnemy();
            Debug.Log("first spawning");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!spawnAble)
            return;

        if (other.tag != "Player")
            return;

        if(!Object.HasStateAuthority)
            return;

        if(respawnDelay.ExpiredOrNotRunning(Runner))
            SpawnEnemy();
    }
    private void SpawnEnemy()
    {
        EnemyHPHandler spawnedEnemy = Runner.Spawn(enemyPrefab, anchorPoint.position, Quaternion.identity);
        spawnedEnemy.Spawner = Object;
        Debug.Log($"spawn enemy");
        spawnAble = false;
        gameObject.SetActive(false);
    }
    public void SetTimer()
    {
        if(Runner != null && Object.HasStateAuthority)
            respawnDelay = TickTimer.CreateFromSeconds(Runner, delayTime);
        spawnAble = true;
    }

    /*
    [Rpc (RpcSources.All, RpcTargets.All)]
    private void RPC_Despawn()
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if(networkRunner != null)
            networkRunner.Despawn(Object);
    }
    */
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public TextMeshProUGUI playerNickNameTM;
    public static NetworkPlayer Local {get; set;}
    public Transform playerBody;
    public Transform WorldSpaceCanvas;

    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> nickName{get; set;} //최대 16자



    // Remote Client Token Hash
   [Networked] public int token {get; set;} //need for Host migration
    bool isPublicJoinMessageSent = false;
    
    public LocalCameraHandler localCameraHandler;
    public GameObject localUI;

    //other components
    NetworkInGameMessages networkInGameMessages;

    private void Awake() 
    {
        networkInGameMessages = GetComponent<NetworkInGameMessages>();    
    }
    void Start()
    {
    }

    public override void Spawned()
    {
        bool Library = SceneManager.GetActiveScene().name == "TestScene(network)orginal";

        //본인 
        if (Object.HasInputAuthority) //플레이어 본인
        {
            Local = this;

            //Sets the layer of the local players model
            //자신의 닉네임은 안 보이도록 레이어를 변경
            Utils.SetRenderLayerInChildren(playerBody, LayerMask.NameToLayer("LocalPlayerModel"));
            Utils.SetRenderLayerInChildren(WorldSpaceCanvas, LayerMask.NameToLayer("IgnoreCamera"));

            //Disable main camera
            if(Camera.main != null)
                Camera.main.gameObject.SetActive(false);

            //Only 1 audio listener is allowed in the scene so enable loacl players audio listener
            AudioListener audioListener = GetComponentInChildren<AudioListener>(true); // true : inactive object도 대상에 포함.
            audioListener.enabled = true;
            
            //Enable the local camera
            // localCameraHandler.localCamera.enabled = true;
            localCameraHandler.localCameraEnable(true);

            //Enable UI for local player
            localUI.SetActive(true);

            //Detach camera if enabled
            localCameraHandler.transform.parent = null;

            RPC_SetNickName(GameManager.instance.playerNickName);
            
            Debug.Log("Spawned local player");
        }
        else //다른플레이어
        {
            //Disable the camera if we are not the local player
            // localCameraHandler.localCamera.enabled = false;
            localCameraHandler.localCameraEnable(false);

            //Disable UI in the PlayerUICanvas
            localUI.SetActive(false);

            //Only 1 audio listener is allowed in the scene so disable remote players audio listener
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;

            Debug.Log("Spawned remote player");
        }

        Runner.SetPlayerObject(Object.InputAuthority, Object);

        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {

        if(Object.HasStateAuthority)
        {
            //서버로 하여금 떠나간 플레이어에 해당하는 아바타만 "left" 메시지 발송
            if (Runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject))
            {
                if(playerLeftNetworkObject == Object)
                    //RPC message를 보내기 전에 아바타가 despawn되는 경우 메시지가 누락될 수 있어서.
                    Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(playerLeftNetworkObject.GetComponent<NetworkPlayer>().nickName.ToString(), "left");
            }
            if (player == Object.InputAuthority)
            {
                Spawner spawner = FindObjectOfType<Spawner>();
                if(spawner != null)
                {
                    foreach (KeyValuePair<int, NetworkPlayer> pair in spawner.mapTokenIDWithNetworkPlayer)
                    {
                        if (pair.Value == this)
                        {
                            spawner.mapTokenIDWithNetworkPlayer.Remove(pair.Key);
                            Runner.Despawn(Object);

                        }
                    }
                }
            }
        }
    }

    //playerNickNameTM은 static으로 만들 수 없어서 나눴다.
    static void OnNickNameChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnNickNameChanged value {changed.Behaviour.nickName}");
        changed.Behaviour.OnNickNameChanged();
    }

    private void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {nickName} for player {gameObject.name}");
        playerNickNameTM.text = nickName.ToString();
    }

    //from client to server
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this.nickName = nickName;

        if(!isPublicJoinMessageSent)
        {
            networkInGameMessages.SendInGameRPCMessage(nickName, "joined");
            isPublicJoinMessageSent = true;
        }

    }


    private void OnDestroy()
    {
        //Get rid of the local camera if we get destroyed as a new one will be spawned with the new Network player   
        if(localCameraHandler != null)
            Destroy(localCameraHandler.gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        /* // 고치면 재활성화
        if(scene.name != "TestScene(network)")
        {
            //Tell the host that we need to perform the spawned code manually
            if (Object.HasStateAuthority && Object.HasInputAuthority)
                Spawned();

            if (Object.HasStateAuthority)
                GetComponent<CharacterRespawnHandler>().RequestRespawn();
        }
        */
    }
}
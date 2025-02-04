using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// @brief 낙사 체크.
public class FallCheckAction : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;
        
        if(other.transform.root.TryGetComponent<HPHandler>(out var hpHandler))
        {
            string nickName = other.transform.root.GetComponent<NetworkPlayer>().nickName.ToString();
            hpHandler.OnTakeDamage(nickName,(int)999,transform.position);
        }
    }
}

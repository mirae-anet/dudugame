using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMain : MonoBehaviour
{
    private EnemyController enemyController;
    //private Rigidbody rigid;
    private BoxCollider boxCollider;
    private MeshRenderer[] meshs;
    private Animator anim;

    public enum Type { Melee, Range };

    [Header("설정")]
    public Type enemyType;
    [Range(1f, 1000f)]
    public int maxHealth = 100;
    [Range(1f, 1000f)]
    public int curHealth = 100;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        //rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "PlayerBullet")  // 원거리 공격
        {
            gameObject.layer = 10;  // 슈퍼 아머

            BulletMain bulletMain = collision.gameObject.GetComponent<BulletMain>();
            curHealth -= bulletMain.damage;
            Vector3 reactDir = transform.position - collision.transform.position;

            enemyController.SetTarget(collision.gameObject.GetComponent<BulletMain>().GetParent()); // 발사한 객체로 타겟 변경(PlayerMain이 담겨있는 오브젝트로)
            Destroy(collision.gameObject); // 피격된 불릿 파괴

            StartCoroutine(OnDamage(reactDir));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerAttack")  // 근접 공격
        {
            gameObject.layer = 10;  // 슈퍼 아머

            WeaponMain weaponMain = other.GetComponent<WeaponMain>();
            curHealth -= weaponMain.damage;
            Vector3 reactDir = transform.position - other.transform.position;

            enemyController.SetTarget(other.GetComponentInParent<Transform>().root); // 타겟 변경(PlayerMain이 담겨있는 오브젝트로)
            //Debug.Log(other.GetComponentInParent<Transform>().root.ToString());
            StartCoroutine(OnDamage(reactDir));
        }
    }

    IEnumerator OnDamage(Vector3 reactDir)
    {
        Debug.Log(gameObject.name + " Hit!");
        enemyController.setIsHit(true);
        anim.SetBool("isWalk", false);

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;

        if (curHealth <= 0)
            OnDie();

        yield return new WaitForSeconds(0.3f);

        enemyController.setIsHit(false);

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white; // 몬스터의 원래 색깔로 변경

        gameObject.layer = 8;  // 슈퍼 아머 해제
    }

    void OnDie()
    {
        gameObject.layer = 10;  // 슈퍼 아머
        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.gray; // 몬스터가 죽으면 회색으로 변경

        //rigid.velocity = Vector3.zero;
        enemyController.SetIsNavEnabled(false);

        anim.SetTrigger("doDie");

        Destroy(gameObject, 3); // 3초 뒤에 삭제
    }

    public Type GetEnemyType()
    {
        return enemyType;
    }
}

using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeAction : InteractionHandler
{
    // 인스펙터
    [Header("설정")]
    [SerializeField]
    private float rotationSpeed = 60.0f; // 회전 속도
    [SerializeField]
    private bool rotatePositive = true; // true: 시계 방향, false: 반시계 방향

    private float rotationAmount;

    private bool move = false;


    [Networked]
    private Vector3 startPosition { get; set; }

    public bool skipSettingStartValues = false;
    private void Start()
    {
        if (!skipSettingStartValues)
        {
            if (Object.HasStateAuthority)
                startPosition = transform.position;
        }
        else
        {
            if (Object.HasStateAuthority)
                transform.position = startPosition;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (!move)
            return;

        // Z축 방향으로 회전할 각도 계산
        rotationAmount = rotationSpeed * Time.deltaTime;

        // 시계 방향 또는 반시계 방향으로 회전
        if (rotatePositive)
        {
            transform.Rotate(0, 0, rotationAmount);
        }
        else
        {
            transform.Rotate(0, 0, -rotationAmount);
        }

        // 회전 방향 전환 확인
        if (transform.localRotation.eulerAngles.z >= 240.0f)
        {
            // 회전 방향 전환
            rotatePositive = !rotatePositive;
        }
        else if (transform.localRotation.eulerAngles.z <= 120.0f)
        {
            // 회전 방향 전환
            rotatePositive = !rotatePositive;
        }

    }

    void FixedUpdate()
    {
        // Z축 방향으로 회전할 각도 계산
        rotationAmount = rotationSpeed * Time.deltaTime;

        // 시계 방향 또는 반시계 방향으로 회전
        if (rotatePositive)
        {
            transform.Rotate(0, 0, rotationAmount);
        }
        else
        {
            transform.Rotate(0, 0, -rotationAmount);
        }

        // 회전 방향 전환 확인
        if (transform.localRotation.eulerAngles.z >= 240.0f)
        {
            // 회전 방향 전환
            rotatePositive = !rotatePositive;
        }
        else if (transform.localRotation.eulerAngles.z <= 120.0f)
        {
            // 회전 방향 전환
            rotatePositive = !rotatePositive;
        }
    }



    /// @brief 일정한 범위(collider) 안에 위치한 경우에만 동작.
    private void OnTriggerStay(Collider other)
    {
        if (!Object.HasStateAuthority)
            return;

        move = true;
    }

}
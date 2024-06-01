using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static BulletBase;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class WeaponBase : ItemBase
{
    [Tooltip("최대 총알 수, 장탄수")]
    public int maxAmmo = 10;
    [Tooltip("연사력")]
    public float fireRate = 0.1f;
    [Tooltip("무게")]
    public float weight = 0f;
    [Tooltip("가격")]
    public uint price = 0;
    [Tooltip("내구도")]
    public float durability = 0f;
    [Tooltip("반동. 기본적 수치.")]
    public float recoil = 0f;
    [Tooltip("조준 거리")]
    public uint sightingRange = 0;
    [Tooltip("탄종")]
    public BulletType ammunitionType;
    [Tooltip("데미지")]
    public float damage = 5.0f;
    [Tooltip("최대 공격력, 치뎀")]
    public float headDamage = 10.0f;
    [Tooltip("명중률")]
    public float accuracy = 0f;
    [Tooltip("치확")]
    public float critRate = 0f;
    [Tooltip("탄속")]
    public uint muzzleVelocity = 0;
    [Tooltip("소음")]
    public float noiseVelocity = 7.0f;

    public GameObject bulletEffect;
    private ParticleSystem ps;

    public int CurrentAmmo
    {
        get => currentAmmo;
        set
        {
            if (currentAmmo != value)
            {
                currentAmmo = value;
                onAmmoChange?.Invoke(currentAmmo, maxAmmo);
            }
        }
    }
    int currentAmmo = 0;

    float coolTime = 0f;

    public bool canFire => coolTime < fireRate && currentAmmo > 0;
    public Action<ItemCode, int> onReload;    //장비창에 장착될때 인벤토리의 리로딩 함수와 연결 
    public Action<int, int> onAmmoChange;
    ItemCode needType = ItemCode.PistolBullet;
    private PlayerUI playerUI;

    public ItemCode NeedType => needType;

    private void Update()
    {
        coolTime -= Time.deltaTime;
    }

    // Player_UI관련 -----------------------------------------------------

    private void OnEnable()
    {
        Inventory_UI inventoryUI = GameManager.Instance.InventoryUI;
        playerUI = GameManager.Instance.PlayerUI;
        if (playerUI != null)
        {
            onAmmoChange += playerUI.Remain.Refresh;
            playerUI.Remain.Refresh(currentAmmo, maxAmmo);
        }
        if (inventoryUI != null)
        {
            onReload += inventoryUI.Inventory.Reload;
            inventoryUI.Inventory.onReload += Reload;
        }
    }

    private void OnDisable()
    {
        if (playerUI != null)
        {
            playerUI.Remain.Refresh(0, 0);
        }
        onAmmoChange = null;
    }

    // ------------------------------------------------------------------------

    public override void Use() //리로딩
    {
        Debug.Log("웨폰베이스 Use메소드 실행");
        int needAmmo = maxAmmo - CurrentAmmo;
        switch (ammunitionType)
        {
            case BulletType.Pistolbullet:
                needType = ItemCode.PistolBullet;
                break;
            case BulletType.Riflebullet:
                needType = ItemCode.RifleBullet;
                break;
            case BulletType.Sniperbullet:
                needType = ItemCode.SniperBullet;
                break;
            case BulletType.Shotgunbullet:
                needType = ItemCode.ShotgunBullet;
                break;
        }
        Debug.Log($"{needType}, {needAmmo}");
        onReload?.Invoke(needType, needAmmo);
    }

    public void Reload(int ammo)
    {
        CurrentAmmo += ammo;
    }

    public virtual void Fire()
    {
        if (canFire)
        {
            CurrentAmmo--;
            coolTime = fireRate;

            // Raycast 및 이펙트 처리
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hitInfo;

            // Raycast 실행, 첫 번째로 맞은 오브젝트 정보 저장
            while (Physics.Raycast(ray, out hitInfo))
            {
                GameObject hitObject = hitInfo.transform.gameObject;
                Debug.Log($"Raycast hit: {hitObject.name} (Layer: {LayerMask.LayerToName(hitObject.layer)}) at position {hitInfo.point}");

                if (hitObject.CompareTag("Item"))
                {
                    // 아이템 태그를 가진 오브젝트를 무시하고, Ray를 그 뒤로 계속 쏘기 위해 Ray의 시작점을 충돌 지점으로 이동
                    ray.origin = hitInfo.point + ray.direction * 0.01f; // 충돌 지점에서 조금 더 이동한 지점을 새로운 시작점으로 설정
                    continue;
                }

                if (hitObject.CompareTag("Enemy"))
                {
                    // Capsule Collider만 감지
                    CapsuleCollider capsuleCollider = hitInfo.collider as CapsuleCollider;
                    if (capsuleCollider != null)
                    {
                        // Enemy 태그를 가진 오브젝트에 데미지를 주고 이펙트를 생성
                        EnemyBace eFSM = hitObject.GetComponent<EnemyBace>();
                        if (eFSM != null)
                        {
                            Debug.Log("Attacking enemy.");
                            eFSM.Demage(damage);
                        }
                        if (bulletEffect != null && ps != null)
                        {
                            bulletEffect.transform.position = hitInfo.point;
                            ps.Play();
                        }
                        // 적과 충돌한 경우 함수 종료
                        return;
                    }
                    else
                    {
                        // Capsule Collider가 아닌 다른 콜라이더는 무시하고, Ray의 시작점을 충돌 지점으로 이동
                        ray.origin = hitInfo.point + ray.direction * 0.01f;
                        continue;
                    }
                }
                else
                {
                    // 첫 번째 적이 아닌 충돌 지점에 이펙트를 생성
                    if (bulletEffect != null && ps != null)
                    {
                        bulletEffect.transform.position = hitInfo.point;
                        ps.Play();
                    }
                    // 적이 아닌 오브젝트와 충돌한 경우 함수 종료
                    return;
                }
            }
        }
        else
        {
            if (CurrentAmmo <= 0)
            {
                Debug.Log("탄약이 부족합니다.");
            }
            else if (coolTime >= fireRate)
            {
                Debug.Log("쿨다운 중입니다.");
            }
        }
    }








    // bulletEffect와 ps를 초기화하는 메서드 추가
    public void InitializeEffects(GameObject bulletEffect, ParticleSystem ps)
    {
        this.bulletEffect = bulletEffect;
        this.ps = ps;
    }
}

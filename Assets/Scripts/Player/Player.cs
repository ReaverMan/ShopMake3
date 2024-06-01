using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;



public class Player : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpPower = 5f;
    Inventory inven;

    public Inventory Inventory => inven;
    float maxHp = 100;
    public float MaxHp => maxHp;
    float hp = 100;
    public float Hp
    {
        get => hp;
        set
        {
            if (hp != value)
            {
                hp = value;
                hp = Mathf.Clamp(hp, 0, maxHp);
                onHealthChange?.Invoke(hp);
                if (hp < 0.1)
                {
                    Die();
                }
            }
        }
    }

    float weight = 0;
    public float Weight
    {
        get => weight;
        set
        {
            if (weight != value)
            {
                weight = value;
                onWeightChange?.Invoke(weight);
            }
        }
    }

    PlayerUI playerUI;
    public PlayerUI PlayerUI => playerUI;
    SlotNumber slotNum;
    public SlotNumber SlotNumber => slotNum;
    public float runningSpeed = 2.0f;
    private CharacterController cc;
    private PlayerMove inputActions;
    private Vector2 movementInput;
    private float gravity = -20f;
    private float yVelocity = 0;
    private bool isRunning = false;
    private Vector3 moveDirection = Vector3.zero;
    public float limitWeight = 20f;
    public float MaxWeight = 40f;
    public float currentWeight = 10f;
    public Action onDie;
    public Action<float> onHealthChange { get; set; }
    public Action<float> onWeightChange { get; set; }
    public float defense = 1.0f;
    public Animator animator;
    bool isMove = false;
    PlayerNoiseSystem noise;
    WaitForSeconds LangingSoundInterval = new WaitForSeconds(0.1f);
    const float runSoundRange = 5.0f;
    const float landingSoundRange = 6.0f;

    private void Awake()
    {
        inputActions = new PlayerMove();
        noise = transform.GetComponentInChildren<PlayerNoiseSystem>(true);
        noise.gameObject.SetActive(false);
        firePosition = transform.GetChild(1);
        slotNum = GetComponentInChildren<SlotNumber>();
    }

    private void OnEnable()
    {
        noise.gameObject.SetActive(false);
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Run.performed += OnRunPerformed;
        inputActions.Player.Run.canceled += OnRunCanceled;
        inputActions.Player.InventoryAction.performed += OnInventoryAction;
        inputActions.Player.InventoryAction.canceled += OnInventoryAction;
        inputActions.Player.Exit.performed += OnExit;
        inputActions.Player.HotbarKey.performed += OnHotbarKey;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Run.performed -= OnRunPerformed;
        inputActions.Player.Run.canceled -= OnRunCanceled;
        inputActions.Player.InventoryAction.performed -= OnInventoryAction;
        inputActions.Player.InventoryAction.canceled -= OnInventoryAction;
        inputActions.Player.Exit.performed -= OnExit;
        inputActions.Player.HotbarKey.performed -= OnHotbarKey;
        inputActions.Player.Disable();
    }

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        playerUI = GameManager.Instance.PlayerUI;

        // 씬 이름이 MainMenuScene이면 입력 시스템 비활성화
        if (SceneManager.GetActiveScene().name == "MainMenuScene")
        {
            inputActions.Player.Disable();
            Debug.Log("플레이어스크립트 비활성화");
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
        isMove = true;
        UpdateNoise();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        movementInput = Vector2.zero;
        isMove = false;
        UpdateNoise();
    }

    private void UpdateNoise()
    {
        if (isMove && isRunning)
        {
            noise.Radius = runSoundRange;
            noise.gameObject.SetActive(true);
        }
        else
        {
            noise.gameObject.SetActive(false);
        }
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (IsGrounded())
        {
            yVelocity = jumpPower;
        }
    }

    private bool IsGrounded()
    {
        float distanceToGround = cc.bounds.extents.y;
        bool isGrounded = Physics.Raycast(transform.position, -Vector3.up, distanceToGround + 0.5f);

        Debug.DrawLine(transform.position, transform.position + (-Vector3.up * (distanceToGround + 0.5f)), isGrounded ? Color.green : Color.red);
        return isGrounded;
    }

    private void OnRunPerformed(InputAction.CallbackContext ctx)
    {
        isRunning = true;
        if (isMove)
        {
            noise.Radius = runSoundRange;
            noise.gameObject.SetActive(true);
        }
    }

    private void OnRunCanceled(InputAction.CallbackContext ctx)
    {
        isRunning = false;
        noise.gameObject.SetActive(false);
    }

    private void OnInventoryAction(InputAction.CallbackContext ctx)
    {
    }

    private void OnExit(InputAction.CallbackContext ctx)
    {
    }

    private void OnHotbarKey(InputAction.CallbackContext ctx)
    {
    }

    private void FixedUpdate()
    {
        CalculateMovement();
        ApplyGravity();
        cc.Move(moveDirection * Time.deltaTime);
    }

    private void CalculateMovement()
    {
        if (Camera.main != null)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0;
            Vector3 cameraRight = Camera.main.transform.right;
            cameraRight.y = 0;
            moveDirection = movementInput.x * cameraRight + movementInput.y * cameraForward;
        }
        AdjustSpeedBasedOnWeight();
    }

    private void AdjustSpeedBasedOnWeight()
    {
        float currentSpeed = moveSpeed;

        if (currentWeight > limitWeight && currentWeight <= MaxWeight)
        {
            currentSpeed -= (currentWeight - limitWeight) / (MaxWeight - limitWeight) * moveSpeed;
        }
        else if (currentWeight > MaxWeight)
        {
            currentSpeed = 0f;
        }

        currentSpeed = isRunning ? currentSpeed + runningSpeed : currentSpeed;
        moveDirection *= currentSpeed;
    }

    private void ApplyGravity()
    {
        if (!IsGrounded())
        {
            yVelocity += gravity * Time.deltaTime;
        }
        else
        {
            yVelocity = Mathf.Max(0, yVelocity);
        }
        moveDirection.y = yVelocity;
    }

    public void Damege(float damege)
    {
        Hp -= (damege / defense);
        Debug.Log($"{damege}받음 / {Hp}");
    }

    public void Heal(float rate)
    {
        Hp += rate;
    }

    public void Die()
    {
        inputActions.Player.Disable();
        onDie?.Invoke();
    }

    Transform firePosition;
    Equipment equipment = Equipment.None;

    public Equipment Equipment
    {
        get => equipment;
        set
        {
            equipment = value;
        }
    }

    public bool IsEquip => firePosition.childCount == 0;

    public void Equipped(Equipment type)
    {
        if (IsEquip)
        {
            slotNum.SwapItem(type, firePosition, false);
        }
        else
        {
            slotNum.SwapItem(type, firePosition, true);
        }
    }

    public void UseItem()
    {
        Transform child = transform.GetChild(1);
        ItemBase item = child.GetComponentInChildren<ItemBase>();
        if (item != null)
            item.Use();
    }

    Equip_UI equip_UI;

    public bool UnEquipped(ItemType type)
    {
        bool result = false;
        ItemBase item = GetComponentInChildren<ItemBase>();
        Inventory_UI inven = GameManager.Instance.InventoryUI;

        switch (type)
        {
            case ItemType.Gun:
                WeaponBase weapon = item as WeaponBase;
                if (weapon != null)
                {
                    if (weapon.CurrentAmmo != 0)
                    {
                        inven.Inventory.AddItem(weapon.NeedType, weapon.CurrentAmmo);
                    }
                    Destroy(weapon.gameObject);
                    result = true;
                }
                break;
            case ItemType.Grenade:
                GrenadeBase grenade = item as GrenadeBase;
                if (grenade != null)
                {
                    Destroy(grenade.gameObject);
                    result = true;
                }
                break;
            case ItemType.Helmet:
                Helmet helmet = item as Helmet;
                if (helmet != null)
                {
                    Destroy(helmet.gameObject);
                    result = true;
                }
                break;
            case ItemType.Armor:
                Vest vest = item as Vest;
                if (vest != null)
                {
                    Destroy(vest.gameObject);
                    result = true;
                }
                break;
            case ItemType.BackPack:
                Backpack backpack = item as Backpack;
                if (backpack != null)
                {
                    Destroy(backpack.gameObject);
                    result = true;
                }
                break;
            case ItemType.Buff:
                BuffBase buff = item as BuffBase;
                if (buff != null)
                {
                    Destroy(buff.gameObject);
                    result = true;
                }
                break;
        }
        return result;
    }

    private Dictionary<string, WeaponState> weaponStates = new Dictionary<string, WeaponState>();
    private List<string> weaponTypes = new List<string> { "Weapon", "Armor", "Helmet", "Throw", "BackPack", "ETC" };

    // 무기 상태 저장 메서드
    public void SaveWeaponState()
    {
        foreach (var weaponType in weaponTypes)
        {
            Transform weaponTransform = GetWeaponTransform(weaponType);
            if (weaponTransform != null && weaponTransform.childCount > 0)
            {
                Transform weapon = weaponTransform.GetChild(0);
                weaponStates[weaponType] = new WeaponState
                {
                    weaponName = weapon.name.Replace("(Clone)", "").Trim(),
                    position = weapon.localPosition,
                    rotation = weapon.localRotation,
                    parentPath = GetFullPath(weaponTransform)
                };
            }
        }
    }

    // 무기 상태 불러오기 메서드
    public void LoadWeaponState()
    {
        foreach (var weaponType in weaponTypes)
        {
            if (weaponStates.ContainsKey(weaponType))
            {
                var weaponState = weaponStates[weaponType];
                GameObject weaponObject = Resources.Load<GameObject>(weaponState.weaponName);
                if (weaponObject != null)
                {
                    GameObject instantiatedWeapon = Instantiate(weaponObject);
                    instantiatedWeapon.transform.localPosition = weaponState.position;
                    instantiatedWeapon.transform.localRotation = weaponState.rotation;

                    // AddItem 메서드를 사용하여 무기 추가
                    SlotNumber slotNumber = GetComponentInChildren<SlotNumber>();
                    slotNumber.AddItem(instantiatedWeapon, (Equipment)Enum.Parse(typeof(Equipment), weaponType));
                }
            }
        }
    }

    // 무기 오브젝트를 찾는 헬퍼 메서드
    private Transform GetWeaponTransform(string weaponType)
    {
        return transform.Find("WeaponTransform/" + weaponType);
    }

    // Transform 경로를 문자열로 반환하는 헬퍼 메서드
    private string GetFullPath(Transform transform)
    {
        string path = "/" + transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = "/" + transform.name + path;
        }
        return path;
    }

    // 경로를 통해 Transform을 찾는 헬퍼 메서드
    private Transform FindTransformByPath(Transform root, string path)
    {
        return root.Find(path.Substring(root.name.Length + 1));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Selection.Contains(gameObject))
        {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(transform.position, transform.up, 5.0f);
            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, transform.up, 5.0f * 0.6f);
        }
    }
#endif
}

[System.Serializable]
public class WeaponState
{
    public string weaponName;
    public Vector3 position;
    public Quaternion rotation;
    public string parentPath;
}
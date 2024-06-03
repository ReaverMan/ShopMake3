using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    private Player player;
    private ItemDataManager itemDataManager;
    private Inventory_UI inventoryUI;
    private WorldInventory_UI worldInventoryUI;
    private InventoryManager inventoryManager;
    private TimeSystem timeSys;
    private WeaponBase weaponBase;
    private ShopInventoryUI shopInventoryUI;
    private Equip_UI equipUI;
    private PlayerUI playerUI;
    private ShopInventory shopInventory;
    private SaveLoadManager saveLoadManager;
    private Equip equip;  // Equip 인스턴스를 추가

    public Player Player => player;
    public ItemDataManager ItemData => itemDataManager;
    public Inventory_UI InventoryUI => inventoryUI;
    public WorldInventory_UI WorldInventory_UI => worldInventoryUI;
    public InventoryManager InventoryManager => inventoryManager;
    public TimeSystem TimeSystem => timeSys;
    public WeaponBase WeaponBase => weaponBase;
    public ShopInventoryUI ShopInventoryUI => shopInventoryUI;
    public Equip_UI EquipUI => equipUI;
    public PlayerUI PlayerUI => playerUI;

    public delegate void SceneAction();
    public event SceneAction OnGameStartCompleted;
    public event SceneAction OnGameEnding;

    private string equipmentDataPath;

    private void Awake()
    {
        equipmentDataPath = Path.Combine(Application.persistentDataPath, "equipmentData.json");
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        saveLoadManager = FindObjectOfType<SaveLoadManager>();
        LoadComponentReferences();
        CheckCurrentScene();
        InitializeEquip();  // Equip 초기화

        Inventory inven = new Inventory(this);
        if (inventoryUI != null)
        {
            inventoryUI.InitializeInventory(inven);
        }

        WorldInventory worldInven = new WorldInventory(this);
        if (worldInventoryUI != null)
        {
            worldInventoryUI.InitializeWorldInventory(worldInven);
        }

        shopInventory = new ShopInventory();
    }

    private void InitializeEquip()
    {
        equip = new Equip(this);  // Equip 인스턴스 생성
        if (equipUI != null)
        {
            equipUI.InitializeInventory(equip);
        }
        else
        {
            Debug.LogError("equipUI가 할당되지 않았습니다!");
        }
    }

    protected override void OnPreInitialize()
    {
        base.OnPreInitialize();
        itemDataManager = GetComponent<ItemDataManager>();
    }

    private void LoadComponentReferences()
    {
        player = FindAnyObjectByType<Player>();
        inventoryUI = FindAnyObjectByType<Inventory_UI>();
        worldInventoryUI = FindAnyObjectByType<WorldInventory_UI>();
        inventoryManager = FindAnyObjectByType<InventoryManager>();
        timeSys = FindAnyObjectByType<TimeSystem>();
        weaponBase = FindAnyObjectByType<WeaponBase>();
        shopInventoryUI = FindAnyObjectByType<ShopInventoryUI>();
        equipUI = FindObjectOfType<Equip_UI>();
        playerUI = FindAnyObjectByType<PlayerUI>();
        if (equipUI == null)
        {
            Debug.LogError("Equip_UI를 찾을 수 없습니다.");
        }
    }

    private void CheckCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("현재 씬 이름: " + currentSceneName);

        if (currentSceneName == "MainMenuScene")
        {
            Debug.Log("메인 메뉴 씬에서 초기화 중...");
        }
        else if (currentSceneName == "InGameScene")
        {
            Debug.Log("인게임 씬에서 초기화 중...");
        }
        else
        {
            Debug.Log("알 수 없는 씬에서 초기화 중..." + currentSceneName);
        }
    }

    public void StartGame(string sceneName)
    {
        if (sceneName == "InGameScene")
        {
            SaveAllData(); // 모든 데이터를 저장
            player.UnequipAllItems(); // 모든 아이템을 해제
        }
        StartCoroutine(LoadScene(sceneName, OnGameStartCompleted));
    }

    public void EndGame(string sceneName)
    {
        if (sceneName == "MainMenuScene")
        {
            SaveInventory(); // 인벤토리 저장
            SavePlayerWeight(); // 플레이어 무게 저장
            player.UnequipAllItems(); // 모든 아이템을 해제
            SaveWorldInventory(); // 월드 인벤토리 저장
            SaveWorldInventoryMoney(); // 월드 인벤토리 돈 저장
            OnGameEnding?.Invoke(); // 게임 종료 이벤트 호출
        }
        SaveAllData(); // 모든 데이터를 저장
        StartCoroutine(LoadScene(sceneName, OnGameEnding));
    }

    private void SaveEquipmentData()
    {
        equip.SaveEquipmentData(equipmentDataPath);
    }

    public void LoadEquipmentData()
    {
        equip.LoadEquipmentData(equipmentDataPath);
    }

    private IEnumerator LoadScene(string sceneName, SceneAction onLoaded)
    {
        Debug.Log($"이 씬을 로딩 중: {sceneName}");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            Debug.Log($"로드 진행 상황 {sceneName}: {asyncLoad.progress * 100}%");
            yield return null;
        }

        if (sceneName == "MainMenuScene")
        {
            LoadAllData(); // 모든 데이터를 로드
        }
        else if (sceneName == "InGameScene")
        {
            LoadInventory(); // 인벤토리 로드
            LoadPlayerWeight(); // 플레이어 무게 로드
            LoadWorldInventory(); // 월드 인벤토리 로드
            LoadWorldInventoryMoney(); // 월드 인벤토리 돈 로드
            OnGameStartCompleted?.Invoke(); // 게임 시작 이벤트 호출
        }

        Player playerScript = FindObjectOfType<Player>();
        if (playerScript != null)
        {
            playerScript.InitializeEquipments(); // Player 클래스에서 장비 초기화
        }

        onLoaded?.Invoke();
    }

    public void SaveAllData()
    {
        SaveInventory();
        SavePlayerWeight();
        SaveWorldInventory();
        SaveWorldInventoryMoney();
    }

    public void LoadAllData()
    {
        LoadInventory();
        LoadPlayerWeight();
        LoadWorldInventoryMoney();
        LoadWorldInventory();
    }

    public void SaveWorldInventory()
    {
        if (worldInventoryUI != null)
            worldInventoryUI.WorldInven.SaveInventoryToJson();
    }

    public void LoadWorldInventory()
    {
        if (worldInventoryUI != null)
            worldInventoryUI.WorldInven.LoadInventoryFromJson();
    }

    public void SaveInventory()
    {
        if (inventoryUI != null)
            inventoryUI.Inventory.SaveInventoryToJson();
    }

    public void LoadInventory()
    {
        if (inventoryUI != null)
            inventoryUI.Inventory.LoadInventoryFromJson();
    }

    public void SavePlayerWeight()
    {
        if (player != null)
        {
            player.SavePlayerData();
        }
    }

    public void LoadPlayerWeight()
    {
        if (player != null)
        {
            player.LoadPlayerData();
        }
    }

    public void SaveWorldInventoryMoney()
    {
        if (worldInventoryUI != null)
        {
            worldInventoryUI.SaveWorldInventoryData();
        }
    }

    public void LoadWorldInventoryMoney()
    {
        if (worldInventoryUI != null)
        {
            worldInventoryUI.LoadWorldInventoryData();
        }
    }

#if UNITY_EDITOR

    public void Test_GameLoad()
    {
        OnGameStartCompleted?.Invoke();
    }

    public void Test_GameEnd()
    {
        OnGameEnding?.Invoke();
    }

#endif
}

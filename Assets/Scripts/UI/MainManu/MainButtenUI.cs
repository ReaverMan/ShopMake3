using UnityEngine;
using UnityEngine.UI;

public class MainButtonUI : MonoBehaviour
{
    /// <summary>
    /// 게임시작 버튼이지만 장비창으로 감
    /// </summary>
    public Button equipmentButton;
    /// <summary>
    /// 상점가는 버튼
    /// </summary>
    public Button shopButton;
    /// <summary>
    /// 인벤,원드인벤이 있는 창고로 가는 버튼
    /// </summary>
    public Button inventoryButton;
    /// <summary>
    /// 게임 종료 버튼 
    /// </summary>
    public Button gameEndButton;

    public Button mainManuButton;
    public Button gamestartButton;

    public Equip_UI equop;
    public Inventory_UI inventory;
    public ShopInventoryUI shopInventory;
    public WorldInventory_UI worldInventory;

    public GameObject mainButtonPanel;
    public GameObject invenButtonPanel;


    void Start()
    {
        // 각 버튼에 클릭 이벤트 리스너 추가
        equipmentButton.onClick.AddListener(() => ToggleUI(Equipment));
        inventoryButton.onClick.AddListener(() => ToggleUI(ToggleInventory));
        shopButton.onClick.AddListener(() => ToggleUI(ToggleShop));
        gameEndButton.onClick.AddListener(() => EndGame());
        mainManuButton.onClick.AddListener(() => ShowMainButtons());
        gamestartButton.onClick.AddListener(() => Gamestart());

        // 초기 UI 설정: 모든 UI 비활성화 및 버튼 패널 활성화
        if (AreAllUIElementsAssigned())
        {
            SetAllUIElementsActive(false);
            mainButtonPanel.SetActive(true);
            invenButtonPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("인스펙터에서 하나 이상의 UI 요소가 할당되지 않았습니다.");
        }

        shopInventory.AddBasicItem();
    }

    bool AreAllUIElementsAssigned()
    {
        bool allAssigned = true;

        if (equop == null)
        {
            Debug.LogError("equop이 할당되지 않았습니다!");
            allAssigned = false;
        }

        if (inventory == null)
        {
            Debug.LogError("inventory가 할당되지 않았습니다!");
            allAssigned = false;
        }

        if (shopInventory == null)
        {
            Debug.LogError("shopInventory가 할당되지 않았습니다!");
            allAssigned = false;
        }

        if (worldInventory == null)
        {
            Debug.LogError("worldInventory가 할당되지 않았습니다!");
            allAssigned = false;
        }

        if (mainButtonPanel == null)
        {
            Debug.LogError("mainButtonPanel이 할당되지 않았습니다!");
            allAssigned = false;
        }

        if (invenButtonPanel == null)
        {
            Debug.LogError("invenButtonPanel이 할당되지 않았습니다!");
            allAssigned = false;
        }

        return allAssigned;
    }

    void Gamestart()
    {
        GameManager.Instance.StartGame("InGameScene");
    }

    void Equipment()
    {
        equop.Open();
        inventory.Open();
        shopInventory.Close();
        worldInventory.Close();
    }

    void ToggleInventory()
    {
        equop.Close();
        inventory.Open();
        shopInventory.Close();
        worldInventory.Open();
    }

    void ToggleShop()
    {
        equop.Close();
        inventory.Close();
        shopInventory.Open();
        worldInventory.Open();
    }

    void EndGame()
    {
        Application.Quit();
    }

    void ToggleUI(System.Action action)
    {
        action.Invoke();
        mainButtonPanel.SetActive(false);
        invenButtonPanel.SetActive(true);
    }

    public void ShowMainButtons()
    {
        SetAllUIElementsActive(false);
        mainButtonPanel.SetActive(true);
        invenButtonPanel.SetActive(false);
    }

    void SetAllUIElementsActive(bool state)
    {
        if (state)
        {
            equop.Open();
            inventory.Open();
            shopInventory.Open();
            worldInventory.Open();
        }
        else
        {
            equop.Close();
            inventory.Close();
            shopInventory.Close();
            worldInventory.Close();
        }
    }
}

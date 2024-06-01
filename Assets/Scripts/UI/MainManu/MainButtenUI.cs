using UnityEngine;
using UnityEngine.UI;

public class MainButtonUI : MonoBehaviour
{
    public Button equipmentButton;
    public Button shopButton;
    public Button inventoryButton;
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
            gamestartButton.gameObject.SetActive(false); // 초기에는 비활성화
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
        gamestartButton.gameObject.SetActive(true); // Equipment 메서드가 호출될 때 활성화
    }

    void ToggleInventory()
    {
        equop.Close();
        inventory.Open();
        shopInventory.Close();
        worldInventory.Open();
        gamestartButton.gameObject.SetActive(false); // 다른 UI를 열 때 비활성화
    }

    void ToggleShop()
    {
        equop.Close();
        inventory.Close();
        shopInventory.Open();
        worldInventory.Open();
        gamestartButton.gameObject.SetActive(false); // 다른 UI를 열 때 비활성화
    }

    void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
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
        gamestartButton.gameObject.SetActive(false); // 메인 버튼을 보여줄 때 비활성화
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

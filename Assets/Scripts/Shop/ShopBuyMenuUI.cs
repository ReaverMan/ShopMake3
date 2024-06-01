using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ShopBuyMenuUI : MonoBehaviour
{
    // UI 컴포넌트 참조
    public TextMeshProUGUI itemNameText; // 아이템 이름을 표시할 UI 텍스트
    public Image itemIcon; // 아이템 아이콘을 표시할 이미지
    public TextMeshProUGUI itemPriceText; // 아이템 가격을 표시할 UI 텍스트
    public TMP_InputField quantityInput; // 수량 입력을 위한 인풋 필드
    public Button purchaseButton; // 구매 버튼
    public Button cancelButton; // 취소 버튼
    public Button plusButton; // 수량 증가 버튼
    public Button minusButton; // 수량 감소 버튼

    // 아이템 슬롯 정보
    private ItemSlot currentItemSlot; // 현재 선택된 아이템 슬롯
    private PlayerInput inputActions; // 플레이어의 입력을 처리하는 시스템

    // 최소 및 최대 구매 수량
    public uint MinItemCount = 1; // 최소 구매 수량
    public uint MaxItemCount = 100; // 최대 구매 수량

    // 구매 이벤트
    public event Action<ItemSlot, uint> OnBuyItem; // 아이템 구매 시 호출될 이벤트

    private void Awake()
    {
        inputActions = new PlayerInput(); // 입력 시스템 초기화

        // 버튼 이벤트 리스너 설정
        purchaseButton.onClick.AddListener(() => AttemptPurchase());
        cancelButton.onClick.AddListener(Close);
        plusButton.onClick.AddListener(IncreaseQuantity);
        minusButton.onClick.AddListener(DecreaseQuantity);
    }

    private void Start()
    {
        quantityInput.onValueChanged.AddListener(HandleInputFieldChange); // 입력 필드 값 변경 이벤트 핸들러
        quantityInput.text = MinItemCount.ToString(); // 초기 수량 설정
    }

    // 슬라이더 값 변경 처리
    private void HandleSliderChange(float sliderValue)
    {
        uint quantity = (uint)Mathf.Lerp(MinItemCount, MaxItemCount, sliderValue); // 슬라이더 값과 수량 매핑
        quantityInput.text = quantity.ToString(); // 입력 필드에 수량 반영
    }

    // 입력 필드 값 변경 처리
    private void HandleInputFieldChange(string text)
    {
        if (uint.TryParse(text, out uint value))
        {
            value = (uint)Mathf.Clamp(value, MinItemCount, MaxItemCount); // 값 범위 제한
            quantityInput.text = value.ToString(); // 입력 필드 업데이트
        }
    }

    // 수량 증가 처리
    private void IncreaseQuantity()
    {
        if (uint.TryParse(quantityInput.text, out uint currentQuantity) && currentQuantity < MaxItemCount)
        {
            currentQuantity++;
            quantityInput.text = currentQuantity.ToString();
        }
    }

    // 수량 감소 처리
    private void DecreaseQuantity()
    {
        if (uint.TryParse(quantityInput.text, out uint currentQuantity) && currentQuantity > MinItemCount)
        {
            currentQuantity--;
            quantityInput.text = currentQuantity.ToString();
        }
    }

    // 입력 시스템 활성화 및 비활성화
    private void OnEnable()
    {
        inputActions.UI.Enable(); // 입력 시스템 활성화
        inputActions.UI.Click.performed += OnClick; // 클릭 이벤트 핸들러 추가
    }

    private void OnDisable()
    {
        inputActions.UI.Disable(); // 입력 시스템 비활성화
        inputActions.UI.Click.performed -= OnClick; // 클릭 이벤트 핸들러 제거
    }

    // 아이템 슬롯 UI 열기
    public void Open(ItemSlot target)
    {
        if (!target.IsEmpty)
        {
            currentItemSlot = target;
            gameObject.SetActive(true);

            // 아이템 정보 UI 업데이트
            itemNameText.text = target.ItemData.itemName;
            itemIcon.sprite = target.ItemData.itemImage;
            itemPriceText.text = $"가격 : {target.ItemData.Price}";
            quantityInput.text = "1"; // 초기 수량 설정

            MovePosition(Mouse.current.position.ReadValue()); // 위치 조정
        }
    }

    // UI 닫기
    public void Close()
    {
        gameObject.SetActive(false);
    }

    // 구매 시도
    private void AttemptPurchase()
    {
        if (uint.TryParse(quantityInput.text, out uint quantity) && quantity >= MinItemCount && quantity <= MaxItemCount)
        {
            Debug.Log("수량이 성공적으로 전달되었습니다 : " + quantity);

            if (currentItemSlot == null || currentItemSlot.ItemData == null)
            {
                Debug.LogError("현재 아이템 슬롯 또는 아이템 데이터가 null 입니다.");
                return; // 데이터가 없으므로 추가 처리 중단
            }

            ItemCode purchasedItemCode = currentItemSlot.ItemData.itemId;
            int totalPrice = (int)(currentItemSlot.ItemData.Price * (int)quantity); // 총 가격 계산

            WorldInventory_UI worldInventoryUI = GameManager.Instance.WorldInventory_UI;

            // 플레이어가 충분한 돈을 가지고 있는지 확인
            if (worldInventoryUI.Money >= totalPrice)
            {
                bool allItemsAdded = true;
                for (uint i = 0; i < quantity; i++)
                {
                    bool itemAdded = GameManager.Instance.WorldInventory_UI.WorldInven.AddItem(purchasedItemCode);
                    if (!itemAdded)
                    {
                        allItemsAdded = false;
                        Debug.LogError("월드인벤토리에 아이템을 추가하지 못 했습니다.");
                        break; // 하나라도 추가 실패하면 더 이상 추가하지 않고 중단
                    }
                }

                if (allItemsAdded)
                {
                    worldInventoryUI.Money -= totalPrice; // 플레이어 돈 차감
                    OnBuyItem?.Invoke(currentItemSlot, quantity);
                    Debug.Log("모든 아이템을 성공적으로 인벤토리에 추가했습니다.");
                }
                else
                {
                    Debug.LogError("모든 아이템을 성공적으로 인벤토리에 추가하지 못 했습니다.");
                }
            }
            else
            {
                Debug.LogError("돈이 충분하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("잘못된 수량을 입력하셨습니다.");
        }
    }




    // 클릭 위치 확인
    private void OnClick(InputAction.CallbackContext context)
    {
        if (Mouse.current != null && !MousePointInRect())
        {
            Close(); // UI 닫기
        }
    }

    // 마우스 포인트가 RectTransform 안에 있는지 확인
    private bool MousePointInRect()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 diff = screenPos - (Vector2)transform.position;
        RectTransform rectTransform = (RectTransform)transform;
        return rectTransform.rect.Contains(diff);
    }

    // UI 위치 조정
    public void MovePosition(Vector2 screenPos)
    {
        RectTransform rect = (RectTransform)transform;
        Vector2 adjustedPos = screenPos;
        // 가로 세로 방향 조정
        int horizontalOver = (int)(adjustedPos.x + rect.sizeDelta.x) - Screen.width;
        int verticalOver = (int)(adjustedPos.y + rect.sizeDelta.y) - Screen.height;
        adjustedPos.x -= Mathf.Max(0, horizontalOver);
        adjustedPos.y -= Mathf.Max(0, verticalOver);

        rect.position = adjustedPos; // 최종 위치 설정
    }
}

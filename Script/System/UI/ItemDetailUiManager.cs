using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDetailUiManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryItem item;
    private ItemUIManager uiManager;
    public void Initialize(InventoryItem inventoryItem, ItemUIManager manager)
    {
        item = inventoryItem;
        uiManager = manager;
    }

    // 마우스 커서가 UI 요소 위로 들어왔을 때 (호버 인)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (uiManager != null && item != null)
        {
            // ItemUIManager의 새로운 메서드를 호출하여 상세 정보 표시
            uiManager.ShowItemDetailOnHover(item, this.transform);
            
        }
    }

    // 마우스 커서가 UI 요소에서 벗어났을 때 (호버 아웃)
    public void OnPointerExit(PointerEventData eventData)
    {
        if (uiManager != null)
        {
            uiManager.HideDetailPanelOnHoverExit();
        }
    }
}

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

    // ���콺 Ŀ���� UI ��� ���� ������ �� (ȣ�� ��)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (uiManager != null && item != null)
        {
            // ItemUIManager�� ���ο� �޼��带 ȣ���Ͽ� �� ���� ǥ��
            uiManager.ShowItemDetailOnHover(item, this.transform);
            
        }
    }

    // ���콺 Ŀ���� UI ��ҿ��� ����� �� (ȣ�� �ƿ�)
    public void OnPointerExit(PointerEventData eventData)
    {
        if (uiManager != null)
        {
            uiManager.HideDetailPanelOnHoverExit();
        }
    }
}

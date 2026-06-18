using UnityEngine;
using UnityEngine.EventSystems;
using System;

// SearchableItem supports both legacy ItemTriggerZone owners and new SearchZone owners.
public class SearchableItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private HiddenItemData itemData;

    // Legacy owner (shim)
    private ItemTriggerZone ownerZone;

    // New owner + callback
    private SearchZone ownerSearchZone;
    private Action<HiddenItemData> onFoundCallback;

    public void SetOwnerZone(ItemTriggerZone zone)
    {
        ownerZone = zone;
    }

    public void SetOwner(SearchZone zone, Action<HiddenItemData> onFound)
    {
        ownerSearchZone = zone;
        onFoundCallback = onFound;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameObject.activeSelf)
        {
            OnFound();
        }
        else
        {
            // Игрок кликнул по уже найденному предмету - проигрываем звук промаха
            SoundManager.Instance?.PlayMissSound();
        }
    }

    public void OnFound()
    {
        gameObject.SetActive(false);

        // Проигрываем звук при нахождении предмета
        SoundManager.Instance?.PlayFoundSound();

        if (onFoundCallback != null && itemData != null)
        {
            onFoundCallback.Invoke(itemData);
            SearchManager.Instance?.NotifyItemFound(itemData);
            return;
        }

        if (ownerZone != null && itemData != null)
        {
            ownerZone.OnItemFound(itemData);
            return;
        }
    }

    public void MarkAsFoundInScene()
    {
        gameObject.SetActive(false);
    }

    public HiddenItemData GetItemData()
    {
        return itemData;
    }
}
using UnityEngine;
using System;

[RequireComponent(typeof(SearchZone))]
public class ItemTriggerZone : MonoBehaviour
{
    public static ItemTriggerZone ActiveZone;

    private SearchZone zone;

    // legacy event removed — use SearchZone.OnZoneCompleted instead

    private void Awake()
    {
        zone = GetComponent<SearchZone>();
    }

    public void ShowDetail()
    {
        // mark active for legacy callers
        ActiveZone = this;

        // Проверяем, завершена ли зона - если да, не активируем её
        if (zone.IsZoneCompleted())
        {
            Debug.Log($"ItemTriggerZone: Zone '{zone.name}' is already completed. Skipping activation.");
            return;
        }

        SearchManager.Instance?.ActivateZone(zone);
    }

    public void OnItemFound(HiddenItemData item)
    {
        // forward to SearchZone logic
        zone.OnItemFoundInZoom(item);
        // notify subscribers if needed
    }

    public void OnDetailClosed()
    {
        zone.Close();
        if (ActiveZone == this) ActiveZone = null;
    }

    public int GetTotalItemsCount()
    {
        return zone != null && zone.items != null ? zone.items.Length : 0;
    }

    public bool HasUnfoundItems()
    {
        var items = GetComponentsInChildren<SearchableItem>(true);
        foreach (var it in items) if (it.gameObject.activeSelf) return true;
        return false;
    }

    public SearchableItem GetRandomUnfoundSearchableItem()
    {
        var items = GetComponentsInChildren<SearchableItem>(true);
        var list = new System.Collections.Generic.List<SearchableItem>();
        foreach (var it in items) if (it.gameObject.activeSelf) list.Add(it);
        if (list.Count == 0) return null;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}

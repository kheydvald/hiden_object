// ZoomZoneController.cs
using UnityEngine;

/// <summary>
/// Component attached to the root of the zoom prefab.
/// Responsibilities:
/// - Find the DetailInstanceMarker in parents to learn the `ownerZone`.
/// - On close: call ownerZone?.OnDetailClosed() and then release the instance.
/// - Auto-close when zone is completed.
/// Note: does not use static finds; marker.ownerZone is provided by ItemTriggerZone.
/// </summary>
public class ZoomZoneController : MonoBehaviour
{
    private SearchZone ownerSearchZone;
    private ItemTriggerZone ownerZoneLegacy;

    private void Awake()
    {
        var marker = GetComponentInParent<DetailInstanceMarker>();
        if (marker != null)
        {
            ownerSearchZone = marker.ownerSearchZone;
            ownerZoneLegacy = marker.ownerZone;
        }
    }

    private void OnEnable()
    {
        // Subscribe to zone completion event if we have an owner zone
        if (ownerSearchZone != null)
        {
            ownerSearchZone.OnZoneCompleted += OnOwnerZoneCompleted;

            // Check if zone is already completed when this zoom instance is enabled
            if (ownerSearchZone.IsZoneCompleted())
            {
                Debug.Log($"ZoomZoneController: Zone already completed, auto-closing zoom zone '{name}'...");
                CloseZoomZone();
            }
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from zone completion event
        if (ownerSearchZone != null)
        {
            ownerSearchZone.OnZoneCompleted -= OnOwnerZoneCompleted;
        }
    }

    /// <summary>
    /// Called when the owner zone is completed - auto-close this zoom zone
    /// </summary>
    private void OnOwnerZoneCompleted()
    {
        Debug.Log($"ZoomZoneController: Owner zone completed, auto-closing zoom zone '{name}'...");
        CloseZoomZone();
    }

    /// <summary>
    /// Called by the UI close button on the zoom prefab.
    /// First notifies the owner zone, then releases this instance via Addressables.
    /// </summary>
    public void OnCloseClicked()
    {
        Debug.Log($"ZoomZoneController: Closing zoom zone instance '{name}' by user click...");
        CloseZoomZone();
    }


    /// <summary>
    /// Common method to close the zoom zone
    /// </summary>
    private void CloseZoomZone()
    {
        // If Awake didn't find the marker (for some edge cases), try again now.
        if (ownerSearchZone == null && ownerZoneLegacy == null)
        {
            var foundMarker = GetComponentInParent<DetailInstanceMarker>();
            if (foundMarker != null)
            {
                ownerSearchZone = foundMarker.ownerSearchZone;
                ownerZoneLegacy = foundMarker.ownerZone;
            }
        }

        try
        {
            if (ownerSearchZone != null)
            {
                // Instead of calling Close directly, let SearchManager handle the deactivation
                if (SearchManager.Instance != null)
                {
                    SearchManager.Instance.DeactivateCurrentZone();
                }
            }
            else
            {
                ownerZoneLegacy?.OnDetailClosed();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ZoomZoneController: Exception notifying owner zone: {ex}");
        }

        // 🔥 НЕ уничтожаем объект! Это объект на сцене, который должен остаться.
        // SearchZone.Close() уже деактивировал его через currentDetailInstance.SetActive(false).
        // Просто выходим — объект останется в иерархии и может быть открыт снова.
        
        // Определяем, был ли этот объект создан через Addressables (для legacy)
        var marker2 = GetComponentInParent<DetailInstanceMarker>();
        if (marker2 != null && marker2.createdByAddressables)
        {
            // Для Addressables — уничтожаем (legacy режим)
            Destroy(gameObject);
        }
        // Для обычных объектов на сцене — НЕ уничтожаем, они просто деактивируются в SearchZone.Close()
    }
}



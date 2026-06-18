// CloseZoomButton.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CloseZoomButton : MonoBehaviour, IPointerClickHandler
{
    private Button uiButton;

    private void Awake()
    {
        uiButton = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
        if (uiButton != null)
        {
            uiButton.onClick.AddListener(HandleClose);
            Debug.Log($"CloseZoomButton: Subscribed to Button.onClick on '{uiButton.gameObject.name}'");
        }
    }

    private void OnDestroy()
    {
        if (uiButton != null)
            uiButton.onClick.RemoveListener(HandleClose);
    }

    private void HandleClose()
    {
        Debug.Log($"CloseZoomButton: HandleClose invoked for '{gameObject.name}' (parent '{transform.parent?.name}')");

        // Если у зум-префаба есть контроллер — используем его (он корректно освободит Addressables)
        var zoomCtrl = GetComponentInParent<ZoomZoneController>();
        if (zoomCtrl != null)
        {
            Debug.Log($"CloseZoomButton: Found ZoomZoneController on '{zoomCtrl.gameObject.name}'. Calling OnCloseClicked().");
            zoomCtrl.OnCloseClicked();
            return;
        }

        // Иначе — ищем маркер и уведомляем владельца зоны
        var marker = GetComponentInParent<DetailInstanceMarker>();
        if (marker != null)
        {
            Debug.Log($"CloseZoomButton: Found DetailInstanceMarker on '{marker.gameObject.name}', ownerZone='{marker.ownerZone?.name ?? "null"}'");
            if (marker.ownerZone != null)
            {
                marker.ownerZone.OnDetailClosed();
                return;
            }
        }

        // === НОВОЕ: Просто закрываем через SearchManager ===
        if (SearchManager.Instance != null)
        {
            SearchManager.Instance.DeactivateCurrentZone();
            return;
        }

        // Финальный запасной вариант: скрываем родительский объект (не уничтожаем!)
        if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClose();
    }
}
using UnityEngine;

/// <summary>
/// Marker attached to an instantiated detail/zoom prefab instance.
/// ItemTriggerZone will set `ownerZone` when it instantiates the prefab.
/// ZoomZoneController reads this to call back the owner without using static finds.
/// </summary>
public class DetailInstanceMarker : MonoBehaviour
{
    public ItemTriggerZone ownerZone; // legacy shim (may be null)
    public SearchZone ownerSearchZone; // preferred
    // Indicates whether this instance was created by Addressables.InstantiateAsync
    public bool createdByAddressables = false;
}
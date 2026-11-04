using UnityEngine;

[DisallowMultipleComponent]
public sealed class AnatomicalPart : MonoBehaviour
{
    [SerializeField] private string partId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string details;
    [SerializeField] private Color baseColor = new Color(0.82f, 0.79f, 0.69f);
    [SerializeField] private Vector3 dissectionOffset;

    private Renderer[] cachedRenderers;
    private MaterialPropertyBlock propertyBlock;
    private Vector3 startingLocalPosition;
    private bool startingPositionCached;

    public string PartId => partId;
    public string DisplayName => displayName;
    public string Details => details;

    public void Configure(string partName, string partDetails, Color color, Vector3 explodeOffset)
    {
        if (string.IsNullOrWhiteSpace(partId))
        {
            partId = ToStableId(partName);
        }
        displayName = partName;
        details = partDetails;
        baseColor = color;
        dissectionOffset = explodeOffset;
    }

    private static string ToStableId(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "structure"
            : value.Trim().ToLowerInvariant().Replace(' ', '-');
    }

    private void Awake()
    {
        CacheRenderers();
        CacheStartingPosition();
    }

    public void SetSelected(bool selected)
    {
        CacheRenderers();
        propertyBlock ??= new MaterialPropertyBlock();

        foreach (Renderer partRenderer in cachedRenderers)
        {
            partRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", selected ? new Color(1f, 0.72f, 0.32f) : baseColor);
            partRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    public void SetVisible(bool visible)
    {
        CacheRenderers();
        foreach (Renderer partRenderer in cachedRenderers)
        {
            partRenderer.enabled = visible;
        }
    }

    public void SetDissected(bool dissected)
    {
        CacheStartingPosition();
        transform.localPosition = startingLocalPosition + (dissected ? dissectionOffset : Vector3.zero);
    }

    private void CacheRenderers()
    {
        cachedRenderers ??= GetComponentsInChildren<Renderer>();
    }

    private void CacheStartingPosition()
    {
        if (startingPositionCached)
        {
            return;
        }

        startingLocalPosition = transform.localPosition;
        startingPositionCached = true;
    }
}

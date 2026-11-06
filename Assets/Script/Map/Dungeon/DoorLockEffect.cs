using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class DoorLockEffect : MonoBehaviour
{
    [SerializeField] private Color lockedColor = new Color32(81, 206, 255, 210);
    [SerializeField] private Color flashColor = new Color32(221, 252, 255, 255);
    private const float PulseDuration = 0.32f;

    private SpriteRenderer door;
    private SpriteRenderer pulse;

    private void Awake()
    {
        door = GetComponent<SpriteRenderer>();
        pulse = new GameObject("Lock pulse", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
        pulse.gameObject.layer = gameObject.layer;
        pulse.transform.SetParent(transform, false);
        pulse.sprite = door.sprite;
        pulse.sharedMaterial = door.sharedMaterial;
        pulse.sortingLayerID = door.sortingLayerID;
        pulse.sortingOrder = door.sortingOrder + 1;
    }

    private void OnEnable()
    {
        StartCoroutine(PlayPulse());
    }

    private IEnumerator PlayPulse()
    {
        for (float elapsed = 0f; elapsed < PulseDuration; elapsed += Time.deltaTime)
        {
            SetPulse(elapsed / PulseDuration);
            yield return null;
        }
        SetPulse(1f);
    }

    private void SetPulse(float progress)
    {
        float remaining = 1f - progress;
        door.color = Color.Lerp(lockedColor, flashColor, remaining * remaining);
        pulse.enabled = progress < 1f;
        pulse.transform.localScale = Vector3.one * (1f + 0.2f * progress);
        pulse.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0.65f * remaining);
    }
}

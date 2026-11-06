using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-5)]
[RequireComponent(typeof(Light2D))]
public sealed class DungeonLightMask : MonoBehaviour
{
    private const int MaskSize = 96;
    // URP samples point-light cookies with the same half-texel offset as its 256px lookup texture.
    private const float LookupUvOffset = 0.5f / 256f;
    private static readonly Color32 Lit = new(255, 255, 255, 255);
    private static readonly Color32 Unlit = new(0, 0, 0, 0);

    private Light2D pointLight;
    private Sprite originalCookie;
    private Texture2D maskTexture;
    private Sprite maskCookie;
    private Color32[] pixels;
    private Scene cachedScene;
    private MapVisualizer map;
    private HashSet<Vector2Int> previousFloor;
    private Vector2 previousPosition;
    private Quaternion previousRotation;
    private float previousRadius;

    private void Awake()
    {
        pointLight = GetComponent<Light2D>();
        originalCookie = pointLight.lightCookieSprite;
    }

    private void OnEnable()
    {
        cachedScene = default;
    }

    private void LateUpdate()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene != cachedScene)
        {
            cachedScene = scene;
            map = FindFirstObjectByType<MapVisualizer>();
        }

        ApplyMask(map != null ? map.PlayableFloor : null);
    }

    private void ApplyMask(HashSet<Vector2Int> floor)
    {
        float radius = pointLight.pointLightOuterRadius;
        if (floor == null || floor.Count == 0 || radius <= 0f)
        {
            RestoreCookie();
            return;
        }

        Vector2 position = transform.position;
        Quaternion rotation = transform.rotation;
        if (floor == previousFloor && position == previousPosition && rotation == previousRotation && radius == previousRadius)
            return;

        previousFloor = floor;
        previousPosition = position;
        previousRotation = rotation;
        previousRadius = radius;

        if (CoversFloor(floor, position, Vector2.one * radius))
        {
            pointLight.lightCookieSprite = originalCookie;
            return;
        }

        if (maskTexture == null)
        {
            maskTexture = new Texture2D(MaskSize, MaskSize, TextureFormat.RGBA32, false, true)
            {
                name = "Dungeon light mask",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            maskCookie = Sprite.Create(maskTexture, new Rect(0, 0, MaskSize, MaskSize), new Vector2(0.5f, 0.5f));
            pixels = new Color32[MaskSize * MaskSize];
        }

        float texelSize = 2f * radius / MaskSize;
        Vector2 right = rotation * (Vector3.right * texelSize);
        Vector2 up = rotation * (Vector3.up * texelSize);
        Vector2 firstSample = position + (right + up) * (0.5f - (0.5f + LookupUvOffset) * MaskSize);
        Vector2 halfTexel = new Vector2(Mathf.Abs(right.x) + Mathf.Abs(up.x), Mathf.Abs(right.y) + Mathf.Abs(up.y)) * 0.5f;

        for (int y = 0; y < MaskSize; y++)
        {
            Vector2 sample = firstSample + up * y;
            for (int x = 0; x < MaskSize; x++, sample += right)
                pixels[y * MaskSize + x] = CoversFloor(floor, sample, halfTexel) ? Lit : Unlit;
        }

        maskTexture.SetPixels32(pixels);
        maskTexture.Apply(false);
        pointLight.lightCookieSprite = maskCookie;
    }

    private static bool CoversFloor(HashSet<Vector2Int> floor, Vector2 center, Vector2 halfExtent)
    {
        Vector2Int min = Vector2Int.FloorToInt(center - halfExtent);
        Vector2Int max = Vector2Int.FloorToInt(center + halfExtent);
        for (int y = min.y; y <= max.y; y++)
            for (int x = min.x; x <= max.x; x++)
                if (!floor.Contains(new Vector2Int(x, y))) return false;

        return true;
    }

    private void OnDisable()
    {
        RestoreCookie();
    }

    private void RestoreCookie()
    {
        pointLight.lightCookieSprite = originalCookie;
        previousFloor = null;
    }

    private void OnDestroy()
    {
        Destroy(maskCookie);
        Destroy(maskTexture);
    }
}

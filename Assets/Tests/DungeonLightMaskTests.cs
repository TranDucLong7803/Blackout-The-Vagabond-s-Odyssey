#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DungeonLightMaskTests
{
    [Test]
    public void RoundLightFollowsTheFloorAcrossCorridorTurnsAndRadiusChanges()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Asset/Prefab/Player/Player.prefab");
        var spot = Object.Instantiate(prefab.transform.Find("Light/Spot").gameObject);
        var light = spot.GetComponent(Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.2D.Runtime", true));
        var mask = spot.GetComponent(Type.GetType("DungeonLightMask, Assembly-CSharp", true));
        Assert.That(mask, Is.Not.Null);
        var radius = light.GetType().GetProperty("pointLightOuterRadius");
        var cookie = light.GetType().GetProperty("lightCookieSprite");
        var applyMask = mask.GetType().GetMethod("ApplyMask", BindingFlags.Instance | BindingFlags.NonPublic);

        try
        {
            spot.transform.SetPositionAndRotation(new Vector3(0.5f, 0.5f), Quaternion.identity);
            var horizontal = Floor(-12, 12, -1, 1);
            applyMask.Invoke(mask, new object[] { horizontal });
            var firstCookie = (Sprite)cookie.GetValue(light);
            Assert.That(firstCookie, Is.Not.Null);
            Assert.That(Sample(firstCookie, spot.transform, 2.74f, new Vector2(2.5f, 0.5f)), Is.GreaterThan(0.9f));
            Assert.That(Sample(firstCookie, spot.transform, 2.74f, new Vector2(0.5f, 2.5f)), Is.Zero);

            radius.SetValue(light, 7.2f);
            spot.transform.SetPositionAndRotation(new Vector3(0.7f, 1.5f), Quaternion.Euler(0f, 0f, 37f));
            var vertical = Floor(-1, 1, -12, 12);
            applyMask.Invoke(mask, new object[] { vertical });
            Assert.That(cookie.GetValue(light), Is.SameAs(firstCookie));
            Assert.That(Sample(firstCookie, spot.transform, 7.2f, new Vector2(0.7f, 6.5f)), Is.GreaterThan(0.9f));
            Assert.That(Sample(firstCookie, spot.transform, 7.2f, new Vector2(2.6f, 1.5f)), Is.Zero);
            Assert.That(radius.GetValue(light), Is.EqualTo(7.2f));

            applyMask.Invoke(mask, new object[] { Floor(-12, 12, -12, 12) });
            Assert.That(cookie.GetValue(light), Is.Null);
            applyMask.Invoke(mask, new object[] { vertical });
            applyMask.Invoke(mask, new object[] { null });
            Assert.That(cookie.GetValue(light), Is.Null);
        }
        finally
        {
            Object.DestroyImmediate(spot);
        }
    }

    private static HashSet<Vector2Int> Floor(int left, int right, int bottom, int top)
    {
        var result = new HashSet<Vector2Int>();
        for (int y = bottom; y <= top; y++)
            for (int x = left; x <= right; x++)
                result.Add(new Vector2Int(x, y));
        return result;
    }

    private static float Sample(Sprite cookie, Transform light, float radius, Vector2 position)
    {
        Vector2 local = Quaternion.Inverse(light.rotation) * (Vector3)(position - (Vector2)light.position);
        Vector2 uv = local / (2f * radius) + Vector2.one * (0.5f + 0.5f / 256f);
        return cookie.texture.GetPixel(
            Mathf.FloorToInt(uv.x * cookie.texture.width),
            Mathf.FloorToInt(uv.y * cookie.texture.height)).r;
    }
}
#endif

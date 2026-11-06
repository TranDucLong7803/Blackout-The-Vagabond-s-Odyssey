#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class BulletImpactTests
{
    [UnityTest]
    public IEnumerator PooledBulletCreatesAnIndependentImpactOnWallsAndDamageableTargets()
    {
        var previousSimulationMode = Physics2D.simulationMode;
        var previousTimeScale = Time.timeScale;
        var previousRunInBackground = Application.runInBackground;
        Physics2D.simulationMode = SimulationMode2D.Script;
        Time.timeScale = 1f;
        Application.runInBackground = true;

        var bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapon/RangedWeapon/Bullet.prefab");
        var poolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapon/PoolManager.prefab");
        var pool = Object.Instantiate(poolPrefab).GetComponent(GameType("PoolManagement"));
        var weaponObject = new GameObject("Impact test weapon");
        weaponObject.SetActive(false);
        var weapon = weaponObject.AddComponent(GameType("RangedWeapon"));
        weapon.GetType().GetField("bulletPrefab").SetValue(weapon, bulletPrefab);
        weapon.GetType().GetField("dam").SetValue(weapon, 3);

        var target = new GameObject("Impact test target");
        target.layer = LayerMask.NameToLayer("Wall");
        var wallCollider = target.AddComponent<BoxCollider2D>();
        wallCollider.size = new Vector2(0.6f, 2f);
        var impactType = GameType("BulletImpact");
        GameObject bullet = null;

        try
        {
            target.transform.position = new Vector3(30f, 10f);
            bullet = Fire(pool, bulletPrefab, weapon, target.transform.position);
            var wallImpact = (Component)Object.FindFirstObjectByType(impactType);
            Assert.That(bullet.activeSelf, Is.False);
            Assert.That(wallImpact, Is.Not.Null);
            Assert.That(wallImpact.gameObject.activeSelf, Is.True);
            Assert.That(wallImpact.transform.position.x, Is.EqualTo(29.7f).Within(0.02f));

            var sparks = wallImpact.GetComponent<ParticleSystem>();
            Assert.That(sparks.main.startColor.color, Is.EqualTo((Color)new Color32(255, 191, 75, 255)));
            float emissionDeadline = Time.realtimeSinceStartup + 2f;
            while (sparks.particleCount == 0 && Time.realtimeSinceStartup < emissionDeadline)
                yield return null;
            Assert.That(sparks.particleCount, Is.GreaterThan(0));

            float returnDeadline = Time.realtimeSinceStartup + 2f;
            while (wallImpact.gameObject.activeSelf && Time.realtimeSinceStartup < returnDeadline)
                yield return null;
            Assert.That(wallImpact.gameObject.activeSelf, Is.False);

            target.layer = 0;
            target.transform.position = new Vector3(31f, 10f);
            target.AddComponent<BoxCollider2D>().size = wallCollider.size;
            var damageable = target.AddComponent(GameType("Damageable"));
            var health = damageable.GetType().GetField("health");
            health.SetValue(damageable, 10);

            Assert.That(Fire(pool, bulletPrefab, weapon, target.transform.position), Is.SameAs(bullet));
            Assert.That(bullet.activeSelf, Is.False);
            Assert.That(health.GetValue(damageable), Is.EqualTo(7));
            Assert.That(Object.FindObjectsByType(impactType, FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindFirstObjectByType(impactType), Is.SameAs(wallImpact));
            Assert.That(wallImpact.transform.position.x, Is.EqualTo(30.7f).Within(0.02f));
            Assert.That(sparks.main.startColor.color, Is.EqualTo((Color)new Color32(255, 100, 63, 255)));
        }
        finally
        {
            foreach (var effect in Object.FindObjectsByType(impactType, FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(((Component)effect).gameObject);
            if (bullet != null) Object.DestroyImmediate(bullet);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(weaponObject);
            Object.DestroyImmediate(pool.gameObject);

            Physics2D.simulationMode = previousSimulationMode;
            Time.timeScale = previousTimeScale;
            Application.runInBackground = previousRunInBackground;
        }
    }

    private static GameObject Fire(Component pool, GameObject prefab, Component weapon, Vector3 target)
    {
        var bullet = (GameObject)pool.GetType().GetMethod("GetBullet").Invoke(pool, new object[] { prefab, false });
        var behaviour = bullet.GetComponent(GameType("Bullet"));
        behaviour.GetType().GetField("weapon").SetValue(behaviour, weapon);
        bullet.transform.SetPositionAndRotation(target - Vector3.right * 0.75f, Quaternion.Euler(0f, 0f, -90f));
        bullet.GetComponent<Rigidbody2D>().linearVelocity = Vector2.right * 12f;
        Physics2D.SyncTransforms();
        for (int i = 0; i < 4 && bullet.activeSelf; i++)
            Physics2D.Simulate(0.02f);
        return bullet;
    }

    private static Type GameType(string name) => Type.GetType(name + ", Assembly-CSharp", true);
}
#endif

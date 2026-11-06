#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class DungeonRoomTests
{
    private const string Doors = "Assets/Asset/Prefab/Objects/Door/";
    private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    [Test]
    public void NearbyAndHurtEnemiesStayIdleUntilThePlayerEntersTheirRoom()
    {
        var roomsField = GameType("DungeonData").GetField("rooms");
        var previousRooms = roomsField.GetValue(null);
        var firstRoom = CreateRoom(new Vector2Int(10, 10));
        var secondRoom = CreateRoom(new Vector2Int(16, 10));
        var rooms = Activator.CreateInstance(roomsField.FieldType);
        Invoke(rooms, "Add", firstRoom);
        Invoke(rooms, "Add", secondRoom);
        roomsField.SetValue(null, rooms);

        var player = new GameObject("Room test player") { tag = "Player" };
        player.transform.position = new Vector3(8.25f, 10.5f);
        var controller = new GameObject("Room test dungeon").AddComponent(GameType("DungeonController"));
        var spawnerObject = new GameObject("Room test spawner");
        spawnerObject.transform.position = new Vector3(7.75f, 10.5f);
        var spawner = spawnerObject.AddComponent(GameType("PlayerSpawner"));
        SetField(controller, "playerSpawner", spawner);
        var melee = SpawnEnemy("Assets/Prefabs/Enemy/Stage1/RiderOrc_Melee_Enemy.prefab", new Vector3(10.5f, 10.5f), firstRoom);
        var ranged = SpawnEnemy("Assets/Prefabs/Enemy/RangeEnemy/MageOrc/MageOrc_Enemy_Ranged_Prefab Variant.prefab", new Vector3(16.5f, 10.5f), secondRoom);

        try
        {
            var meleeAi = melee.GetComponent(GameType("Enemy_Movement_AI"));
            var rangedAi = ranged.GetComponent(GameType("Enemy_Movement_AI"));
            var meleeHealth = melee.GetComponent(GameType("EnemyHealth"));
            Invoke(controller, "Update");
            Assert.That(IsActive(firstRoom), Is.False);
            Assert.That(IsLocked(firstRoom), Is.False);
            var spawn = (IEnumerator)Invoke(spawner, "DelayPlayer");
            Assert.That(spawn.MoveNext(), Is.True);
            Assert.That(spawn.MoveNext(), Is.False);
            Assert.That(player.transform.position, Is.EqualTo(spawnerObject.transform.position));
            Assert.That(Invoke(meleeAi, "GetChasePosition", player.transform.position), Is.EqualTo(new Vector3(8.5f, 10.5f)));

            Invoke(meleeHealth, "takeDame", 1);
            Invoke(controller, "Update");
            Invoke(meleeAi, "Update");
            Invoke(melee, "Update");
            Assert.That(IsActive(firstRoom), Is.False);
            Assert.That(GetField(meleeAi, "chasePlayer"), Is.False);
            Assert.That(GetField(melee, "isAttacking"), Is.False);
            Assert.That(melee.GetComponent<Animator>().GetBool("isWalk"), Is.False);

            player.transform.position = new Vector3(8.25f, 10.5f);
            Invoke(controller, "Update");
            Assert.That(IsActive(firstRoom), Is.False, "The player is still crossing the threshold.");
            Assert.That(IsLocked(firstRoom), Is.False);

            player.transform.position = new Vector3(8.75f, 10.5f);
            Invoke(controller, "Update");
            Invoke(meleeAi, "Update");
            Invoke(melee, "Update");
            Invoke(rangedAi, "Update");
            Invoke(ranged, "Update");
            Assert.That(IsActive(firstRoom), Is.True);
            Assert.That(IsLocked(firstRoom), Is.True);
            Assert.That(GetField(meleeAi, "chasePlayer"), Is.True);
            Assert.That(GetField(melee, "isAttacking"), Is.True);
            Assert.That(melee.GetComponent<Animator>().GetBool("isWalk"), Is.True);
            Assert.That(IsActive(secondRoom), Is.False);
            Assert.That(GetField(rangedAi, "chasePlayer"), Is.False);
            Assert.That(GetField(ranged, "isAttacking"), Is.False);

            player.transform.position = new Vector3(13.5f, 10.5f);
            Invoke(controller, "Update");
            Assert.That(IsActive(firstRoom), Is.True);
            Assert.That(IsActive(secondRoom), Is.False);

            player.transform.position = new Vector3(14.75f, 10.5f);
            Invoke(controller, "Update");
            Invoke(rangedAi, "Update");
            Invoke(ranged, "Update");
            Assert.That(IsActive(secondRoom), Is.True);
            Assert.That(GetField(rangedAi, "chasePlayer"), Is.False, "The ranged enemy is already within its firing distance.");
            Assert.That(GetField(ranged, "isAttacking"), Is.True);
        }
        finally
        {
            roomsField.SetValue(null, previousRooms);
            Object.DestroyImmediate(melee.gameObject);
            Object.DestroyImmediate(ranged.gameObject);
            Object.DestroyImmediate(controller.gameObject);
            Object.DestroyImmediate(spawnerObject);
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void GeneratedEntrancesStayOpenAndAnEmptyRoomDoesNotLock()
    {
        var pathField = GameType("DungeonData").GetField("path");
        var previousPath = pathField.GetValue(null);
        var room = CreateRoom(new Vector2Int(10, 10));
        SetField(room, "enemyCount", 0);
        pathField.SetValue(null, new HashSet<Vector2Int> { new(10, 13), new(7, 10) });
        var root = new GameObject("Room test entrances");
        var placer = root.AddComponent(GameType("PropPlacer"));
        SetField(placer, "propParent", root.transform);

        try
        {
            Invoke(placer, "SetRoomEntrances", room);
            Assert.That(GetField(room, "topEntrance"), Is.EqualTo(new Vector2Int(10, 13)));
            Assert.That(GetField(room, "leftEntrance"), Is.EqualTo(new Vector2Int(7, 10)));
            Assert.That(root.transform.childCount, Is.Zero);
            Invoke(room, "LockDoors");
            Assert.That(IsActive(room), Is.False);
            Assert.That(IsLocked(room), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(root);
            pathField.SetValue(null, previousPath);
        }
    }

    [UnityTest]
    public IEnumerator LockingAnEliteRoomStartsItsSpawnAnimation()
    {
        var previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        var room = CreateRoom(new Vector2Int(10, 10));
        SetField(room, "topEntrance", new Vector2Int(10, 13));
        var elite = new GameObject("Test elite");
        room.GetType().GetProperty("eliteReference").SetValue(room, elite);
        var root = new GameObject("Room test elite setup");
        var setup = root.AddComponent(GameType("EliteRoomSetup"));
        SetField(setup, "barriers", new[] { Load(Doors + "Barrier_front.prefab"), Load(Doors + "Barrier_side.prefab") });
        SetField(setup, "triggerPrefab", Load("Assets/Asset/Prefab/Map/EliteTrigger/EliteTrigger.prefab"));
        SetField(setup, "objectParent", root.transform);
        try
        {
            Invoke(setup, "SetupCombatRoom", room);
            var trigger = root.GetComponentInChildren(GameType("EliteTrigger"));
            SetField(trigger, "delaySpawn", 0f);
            var barrier = root.transform.Find("Barrier").GetChild(0).gameObject;
            Assert.That(elite.activeSelf, Is.False);
            Assert.That(barrier.activeSelf, Is.False);
            Assert.That(IsActive(room), Is.False);

            Invoke(room, "LockDoors");
            Assert.That(barrier.activeSelf, Is.True);
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!elite.activeSelf && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(elite.activeSelf, Is.True);
            Assert.That(barrier.activeSelf, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(elite);
            Time.timeScale = previousTimeScale;
        }
    }

    [UnityTest]
    public IEnumerator OpenEntrancesSealOnPlayerEntryAndReopenAfterTheLastKill()
    {
        var roomsField = GameType("DungeonData").GetField("rooms");
        var pathField = GameType("DungeonData").GetField("path");
        var previousRooms = roomsField.GetValue(null);
        var previousPath = pathField.GetValue(null);
        var previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        var room = CreateRoom(new Vector2Int(10, 10));
        var rooms = Activator.CreateInstance(roomsField.FieldType);
        Invoke(rooms, "Add", room);
        roomsField.SetValue(null, rooms);
        pathField.SetValue(null, new HashSet<Vector2Int> { new(10, 13), new(7, 10), new(13, 10) });

        var root = new GameObject("Combat door test");
        var placer = root.AddComponent(GameType("PropPlacer"));
        SetField(placer, "propParent", root.transform);
        var setup = root.AddComponent(GameType("EliteRoomSetup"));
        SetField(setup, "barriers", new[] { Load(Doors + "Barrier_front.prefab"), Load(Doors + "Barrier_side.prefab") });
        SetField(setup, "objectParent", root.transform);
        var controller = (Behaviour)root.AddComponent(GameType("DungeonController"));
        controller.enabled = false;
        var player = new GameObject("Combat door test player") { tag = "Player" };

        try
        {
            Invoke(placer, "SetRoomEntrances", room);
            Assert.That(root.transform.childCount, Is.Zero);
            Invoke(setup, "SetupCombatRoom", room);
            var barriers = root.transform.Find("Barrier");
            Assert.That(barriers.childCount, Is.EqualTo(3));
            foreach (Transform barrier in barriers) Assert.That(barrier.gameObject.activeSelf, Is.False);

            int doorLayer = LayerMask.GetMask("Background");
            Physics2D.SyncTransforms();
            Assert.That(Physics2D.Raycast(new Vector2(12.75f, 10.5f), Vector2.right, 1f, doorLayer).collider, Is.Null);

            player.transform.position = new Vector3(12.75f, 10.5f);
            Invoke(controller, "Update");
            Assert.That(IsActive(room), Is.False);
            Assert.That(IsLocked(room), Is.False);
            player.transform.position = new Vector3(12.25f, 10.5f);
            Invoke(controller, "Update");
            Assert.That(IsLocked(room), Is.True);
            foreach (Transform barrier in barriers)
            {
                Assert.That(barrier.gameObject.activeSelf, Is.True);
                Assert.That(barrier.GetComponent(GameType("IDamageable")), Is.Null);
                Assert.That(barrier.Find("Lock pulse").GetComponent<SpriteRenderer>().enabled, Is.True);
            }

            Physics2D.SyncTransforms();
            Assert.That(Physics2D.Raycast(new Vector2(12.75f, 10.5f), Vector2.right, 1f, doorLayer).collider, Is.Not.Null);
            Invoke(room, "OnEnemyDeath");
            Assert.That(IsLocked(room), Is.True);
            yield return new WaitForSeconds(0.4f);
            foreach (Transform barrier in barriers)
            {
                Assert.That(barrier.gameObject.activeSelf, Is.True);
                Assert.That(barrier.Find("Lock pulse").GetComponent<SpriteRenderer>().enabled, Is.False);
            }

            Invoke(room, "OnEnemyDeath");
            Assert.That(IsLocked(room), Is.False);
            foreach (Transform barrier in barriers) Assert.That(barrier.gameObject.activeSelf, Is.False);
            Physics2D.SyncTransforms();
            Assert.That(Physics2D.Raycast(new Vector2(12.75f, 10.5f), Vector2.right, 1f, doorLayer).collider, Is.Null);
            Invoke(controller, "Update");
            Assert.That(IsLocked(room), Is.False);
        }
        finally
        {
            roomsField.SetValue(null, previousRooms);
            pathField.SetValue(null, previousPath);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(root);
            Time.timeScale = previousTimeScale;
        }
    }

    private static Component SpawnEnemy(string path, Vector3 position, object room)
    {
        var root = Object.Instantiate(Load(path), position, Quaternion.identity);
        var enemy = root.GetComponent(GameType("Enemy"));
        var ai = root.GetComponent(GameType("Enemy_Movement_AI"));
        SetField(enemy, "roomBelong", room);
        Invoke(enemy, "Start");
        Invoke(root.GetComponent(GameType("EnemyHealth")), "Start");
        Invoke(ai, "Start");
        ((MonoBehaviour)ai).CancelInvoke();
        SetField(root.GetComponentInChildren(GameType("EnemyInteractZone")), "isTouchPlayer", true);
        return enemy;
    }

    private static object CreateRoom(Vector2Int center)
    {
        var room = Activator.CreateInstance(GameType("BoxRoom"));
        SetField(room, "center", center);
        SetField(room, "size", new Vector2Int(2, 2));
        SetField(room, "enemyCount", 2);
        var tiles = GameType("ProceduralGeneration").GetMethod("BoxGenerator").Invoke(null, new object[] { center, 2, 2 });
        SetField(room, "roomTiles", tiles);
        return room;
    }

    private static bool IsActive(object room) => (bool)room.GetType().GetProperty("IsCombatActive").GetValue(room);
    private static bool IsLocked(object room) => (bool)room.GetType().GetProperty("IsLocked").GetValue(room);
    private static GameObject Load(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path);
    private static Type GameType(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private static object GetField(object obj, string name) => obj.GetType().GetField(name, InstanceMembers).GetValue(obj);
    private static void SetField(object obj, string name, object value) => obj.GetType().GetField(name, InstanceMembers).SetValue(obj, value);
    private static object Invoke(object obj, string name, params object[] args) => obj.GetType().GetMethod(name, InstanceMembers).Invoke(obj, args);
}
#endif

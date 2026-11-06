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
            var spawn = (IEnumerator)Invoke(spawner, "DelayPlayer");
            Assert.That(spawn.MoveNext(), Is.True);
            Assert.That(spawn.MoveNext(), Is.False);
            Assert.That(player.transform.position, Is.EqualTo(spawnerObject.transform.position));

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
            Invoke(meleeAi, "Update");
            Invoke(melee, "Update");
            Invoke(rangedAi, "Update");
            Invoke(ranged, "Update");
            Assert.That(IsActive(firstRoom), Is.True);
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

            player.transform.position = new Vector3(14.25f, 10.5f);
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

    [UnityTest]
    public IEnumerator AGeneratedDoorOnlyWakesItsRoomWhenBrokenByPlayerDamage()
    {
        var pathField = GameType("DungeonData").GetField("path");
        var previousPath = pathField.GetValue(null);
        var previousRandomState = UnityEngine.Random.state;
        var firstRoom = CreateRoom(new Vector2Int(10, 10));
        var secondRoom = CreateRoom(new Vector2Int(16, 10));
        pathField.SetValue(null, new HashSet<Vector2Int> { new(10, 13), new(16, 13) });
        var root = new GameObject("Room test doors");
        var placer = root.AddComponent(GameType("PropPlacer"));
        SetField(placer, "propParent", root.transform);
        SetField(placer, "doorChance", 1f);
        SetField(placer, "doors", new[] { Load(Doors + "Door_bottom.prefab"), Load(Doors + "Door_side.prefab"), Load(Doors + "Door_top.prefab") });

        try
        {
            Invoke(placer, "SetRoomEntrances", firstRoom);
            Invoke(placer, "SetRoomEntrances", secondRoom);
            Assert.That(root.transform.childCount, Is.EqualTo(2));
            var firstDoor = root.transform.GetChild(0).GetComponent(GameType("Damageable"));
            var secondDoor = root.transform.GetChild(1).GetComponent(GameType("Damageable"));
            var weapon = Enum.Parse(GameType("AttackType"), "Weapon");
            var playerDamage = GameType("AttackContext").GetMethod("DealPlayerDamage");

            playerDamage.Invoke(null, new[] { firstDoor, (object)4, weapon });
            Assert.That(IsActive(firstRoom), Is.False);

            Invoke(secondDoor, "takeDame", 5);
            Assert.That(IsActive(secondRoom), Is.False);

            playerDamage.Invoke(null, new[] { firstDoor, (object)1, weapon });
            Assert.That(IsActive(firstRoom), Is.True);
            Assert.That(IsActive(secondRoom), Is.False);

            float deadline = Time.realtimeSinceStartup + 2f;
            while ((firstDoor != null || secondDoor != null) && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(firstDoor == null && secondDoor == null, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(root);
            pathField.SetValue(null, previousPath);
            UnityEngine.Random.state = previousRandomState;
        }
    }

    [UnityTest]
    public IEnumerator AnAwakenedEliteSpawnsBeforeBarriersCloseOnPlayerEntry()
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
        var player = new GameObject("Room test player") { tag = "Player" };
        var playerCollider = player.AddComponent<BoxCollider2D>();
        var other = new GameObject("Room test non-player");
        var otherCollider = other.AddComponent<BoxCollider2D>();

        try
        {
            Invoke(setup, "SetupEliteRoom", room, null);
            var trigger = root.GetComponentInChildren(GameType("EliteTrigger"));
            SetField(trigger, "delaySpawn", 0f);
            var barrier = root.transform.Find("Barrier").GetChild(0).gameObject;
            Assert.That(elite.activeSelf, Is.False);
            Assert.That(barrier.activeSelf, Is.False);
            Invoke(trigger, "OnTriggerEnter2D", otherCollider);
            Assert.That(IsActive(room), Is.False);

            Invoke(room, "ActivateCombat");
            Assert.That(barrier.activeSelf, Is.False);
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!elite.activeSelf && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(elite.activeSelf, Is.True);
            Assert.That(barrier.activeSelf, Is.False);

            Invoke(trigger, "OnTriggerEnter2D", playerCollider);
            Assert.That(barrier.activeSelf, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(other);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(elite);
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
    private static GameObject Load(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path);
    private static Type GameType(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private static object GetField(object obj, string name) => obj.GetType().GetField(name, InstanceMembers).GetValue(obj);
    private static void SetField(object obj, string name, object value) => obj.GetType().GetField(name, InstanceMembers).SetValue(obj, value);
    private static object Invoke(object obj, string name, params object[] args) => obj.GetType().GetMethod(name, InstanceMembers).Invoke(obj, args);
}
#endif

using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class BoxRoom : Room
{
    public Vector2Int size = new Vector2Int(); //width, height

    // Leave half a tile at each edge so a closing door cannot overlap the player.
    public Rect Interior => new(center.x - size.x + 0.5f, center.y - size.y + 0.5f, size.x * 2f, size.y * 2f);
}

public class Room
{
    public Vector2Int center; //ID, center of the room
    public HashSet<Vector2Int> roomTiles = new();
    public RoomType roomType;
    public HashSet<Vector2Int> propPositions { get; set; } = new HashSet<Vector2Int>();
    public List<GameObject> propObjectReferences { get; set; } = new List<GameObject>();
    public int enemyCount=0;
    public GameObject eliteReference { get; set; } = null;
    public Vector2Int topEntrance, bottomEntrance, leftEntrance, rightEntrance;
    public HashSet<Vector2Int> corners = new();
    public HashSet<Vector2Int> nearTopWall = new();
    public HashSet<Vector2Int> nearBottomWall = new();
    public HashSet<Vector2Int> nearLeftWall = new();
    public HashSet<Vector2Int> nearRightWall = new();
    public HashSet<Vector2Int> inners = new();

    public bool IsCombatActive { get; private set; }
    public bool IsLocked { get; private set; }
    public event Action CombatActivated;
    private readonly List<GameObject> barriers = new();

    public void AddBarrier(GameObject barrier) => barriers.Add(barrier);

    public void OnEnemyDeath()
    {
        if (enemyCount <= 0) return;

        enemyCount--;
        if (enemyCount > 0) return;

        IsLocked = false;
        foreach (var barrier in barriers) SetBlockerActive(barrier, false);

        if (NotificationController.Instance != null) NotificationController.Instance.ShowNotification("Room Cleared");
        if (GameManager.Instance != null) GameManager.Instance.PlayCurrentStageDungeonMusic();
    }

    public void LockDoors()
    {
        if (IsCombatActive || enemyCount <= 0) return;

        IsCombatActive = true;
        IsLocked = true;
        foreach (var barrier in barriers) SetBlockerActive(barrier, true);
        CombatActivated?.Invoke();

        if (roomType == RoomType.Elite && GameManager.Instance != null)
            GameManager.Instance.PlayCurrentStageEliteMusic();
    }

    private static void SetBlockerActive(GameObject blocker, bool active)
    {
        if (blocker == null || blocker.activeSelf == active) return;

        blocker.TryGetComponent<Collider2D>(out var collider);
        Bounds bounds = !active && collider != null ? collider.bounds : default;
        blocker.SetActive(active);
        if (AstarPath.active != null && collider != null)
            AstarPath.active.UpdateGraphs(new GraphUpdateObject(active ? collider.bounds : bounds));
    }
}

public enum RoomType
{
    Spawn,
    Normal, Elite,
    Treasure, Shop,
    // Upgrade,
    Exit
}

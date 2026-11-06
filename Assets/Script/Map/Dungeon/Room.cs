using System;
using System.Collections.Generic;
using UnityEngine;

public class BoxRoom : Room
{
    public Vector2Int size = new Vector2Int(); //width, height
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
    public GameObject barrierReference { get; set; } = null;
    public Vector2Int topEntrance, bottomEntrance, leftEntrance, rightEntrance;
    public HashSet<Vector2Int> corners = new();
    public HashSet<Vector2Int> nearTopWall = new();
    public HashSet<Vector2Int> nearBottomWall = new();
    public HashSet<Vector2Int> nearLeftWall = new();
    public HashSet<Vector2Int> nearRightWall = new();
    public HashSet<Vector2Int> inners = new();

    public bool IsCombatActive { get; private set; }
    public event Action CombatActivated;

    public void ActivateCombat()
    {
        if (IsCombatActive) return;

        IsCombatActive = true;
        CombatActivated?.Invoke();
    }

    public void OnEnemyDeath()
    {
        enemyCount--;
        Debug.Log("Enemy died " + enemyCount);
        if (enemyCount == 0)
        {
            Debug.Log("Room Cleared"); //them ui bao room cleared o day.
            NotificationController.Instance.ShowNotification("Room Cleared");
            DungeonManager.Instance.OpenRoomBarrier(this);

            GameManager.Instance.PlayCurrentStageDungeonMusic();
        }
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

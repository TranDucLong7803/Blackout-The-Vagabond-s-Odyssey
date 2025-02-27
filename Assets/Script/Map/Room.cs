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
    public List<GameObject> enemyObjectReferences { get; set; } = new List<GameObject>();
    public List<GameObject> doorObjectReferences { get; set; } = new List<GameObject>();
    public Vector2Int topEntrance, bottomEntrance, leftEntrance, rightEntrance;
    public HashSet<Vector2Int> corners = new();
    public HashSet<Vector2Int> nearTopWall = new();
    public HashSet<Vector2Int> nearBottomWall = new();
    public HashSet<Vector2Int> nearLeftWall = new();
    public HashSet<Vector2Int> nearRightWall = new();
    public HashSet<Vector2Int> inners = new();
}

public enum RoomType
{
    Spawn,
    Normal, Elite,
    Treasure, Shop,
    // Upgrade,
    Exit
}

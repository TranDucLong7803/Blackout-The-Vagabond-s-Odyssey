using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapVisualizer : MonoBehaviour
{
    [SerializeField] private int floorOffsetRadius = 0;
    [SerializeField] private int wallHeight = 1;
    [SerializeField] private Tilemap foregroundTilemap, floorTilemap, wallTilemap, minimap;
    [SerializeField] private TileBase foregroundTile, floorTile, wallTile;
    [SerializeField] private TileBase minimapFloor, minimapWall;

    [SerializeField] private bool usePerlinNoise = false;
    [SerializeField] private float scale = 7f; // Scale for Perlin noise
    private int noiseOffset = 0; // Offset for Perlin noise
    public HashSet<Vector2Int> PlayableFloor { get; private set; }
    
    public void VisualizeLayout(HashSet<Vector2Int> floor)
    {
        PlayableFloor = floor;
        var floorMap = offsetFloor(floor);
        if(foregroundTilemap != null)
        {
            PlaceTiles(foregroundTilemap, foregroundTile, floorMap, false);
        }
        PlaceTiles(floorTilemap, floorTile, floorMap, usePerlinNoise);
        DrawMinimap(floor, minimapFloor);
        HashSet<Vector2Int> wall = GetAroundFloor(floor, wallHeight);
        PlaceTiles(wallTilemap, wallTile, wall, false);
        wall = ConvertWallToMinimap(floor);
        DrawMinimap(wall, minimapWall);
    }

    private HashSet<Vector2Int> offsetFloor(HashSet<Vector2Int> floorPositions)
    {
        if(floorOffsetRadius < 1) return floorPositions;
        HashSet<Vector2Int> offsetedFloor = new HashSet<Vector2Int>(floorPositions);
        foreach (var pos in floorPositions)
        {
            foreach (var direction in Direction2D.Directions)
            {
                for (int i = 0; i < floorOffsetRadius; i++)
                {
                    if (!floorPositions.Contains(pos + direction * i))
                    {
                        offsetedFloor.Add(pos + direction * i);
                    }
                }
            }
        }
        return offsetedFloor;
    }

    private void PlaceTiles(Tilemap tilemap, TileBase tile, HashSet<Vector2Int> positions, bool usePerlinNoise = false)
    {
        if(usePerlinNoise)
        {
            noiseOffset = Utility.UnseededRng(0,10000);
            foreach (var pos in positions)
            {
                float perlinValue = Mathf.PerlinNoise((pos.x + noiseOffset) / scale, (pos.y + noiseOffset) / scale);
                if (perlinValue > 0.5f)
                {
                    PlaceTile(tilemap, tile, pos);
                }
            }
            return;
        }
        foreach (var pos in positions)
        {
            PlaceTile(tilemap, tile, pos);
        }
    }

    private void PlaceTile(Tilemap floorTilemap, TileBase tile, Vector2Int pos)
    {
        floorTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);
    }

    public void ClearMap()
    {
        PlayableFloor = null;
        if (foregroundTilemap != null)
        {
            foregroundTilemap.ClearAllTiles();
        }
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        minimap.ClearAllTiles();
    }

    public void DrawMinimap(HashSet<Vector2Int> floor, TileBase tile)
    {
        foreach (var pos in floor)
        {
            // PlaceTile(minimap, tile, pos);
            MinimapData.minimapTiles[pos] = tile;
        }
    }

    private HashSet<Vector2Int> GetAroundFloor(HashSet<Vector2Int> floorPositions, int bottomHeight)
    {
        HashSet<Vector2Int> offsetBottomFloor = new HashSet<Vector2Int>();
        foreach (var pos in floorPositions)
        {
            for(int i = 1; i < bottomHeight; i++)
            {
                if (floorPositions.Contains(pos + Vector2Int.up*i)) //move entire floor up 
                {
                    offsetBottomFloor.Add(pos + Vector2Int.up*i);
                }
            }

        }
        
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var pos in offsetBottomFloor) //get tiles around offseted floor
        {
            foreach (var direction in Direction2D.AllDirections)
            {
                if (!offsetBottomFloor.Contains(pos + direction))
                {
                    if(direction.Equals(Vector2Int.right) || direction.Equals(Vector2Int.down))
                    {
                        wallPositions.Add(pos + direction*2);
                    } else {
                        wallPositions.Add(pos + direction);
                    }
                }
            }
        }
        return wallPositions;
    }

    private HashSet<Vector2Int> ConvertWallToMinimap(HashSet<Vector2Int> floorPositions)
    {
        HashSet<Vector2Int> minimapWallPositions = new HashSet<Vector2Int>();
        foreach (var pos in floorPositions) //get tiles around offseted floor
        {
            foreach (var direction in Direction2D.AllDirections)
            {
                if (!floorPositions.Contains(pos + direction))
                {
                    minimapWallPositions.Add(pos + direction);
                }
            }
        }
        return minimapWallPositions;
    }

}

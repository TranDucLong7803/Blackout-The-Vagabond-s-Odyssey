using System;
using System.Collections.Generic;
using System.Linq;
//using Unity.Mathematics;
//using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class PropPlacer : MonoBehaviour
{
    [SerializeField] private List<Prop> props;
    [SerializeField] private List<Prop> lightProps;
    [SerializeField] private List<Prop> trapProps;
    [SerializeField, Range(0,1)] private float cornerPropChance = 0.6f;
    [SerializeField] private Transform propParent;
    [SerializeField] private Prop groundPortal;
    [SerializeField] private Prop shop;
    [SerializeField] private Prop exit;
    [SerializeField] private Prop treasure;

    public void PlaceCornerProps(BoxRoom room)
    {
        List<Prop> cornerProps = props.FindAll(p => p.corner);
        float tmpChance = cornerPropChance;
        foreach(Vector2Int cornerTile in room.corners)
        {
            if(Random.value < tmpChance)
            {
                Prop prop = cornerProps[Random.Range(0, cornerProps.Count)];
                PlacePropAt(room, cornerTile, prop);
                if(prop.placeAsGroup) PlaceCornerGroupProp(room, cornerTile, prop);
            } else {
                tmpChance = Mathf.Clamp01(tmpChance +0.1f); //10% increase next placement when failed
            }
        }
    }

    public void PlaceNearWallProps(BoxRoom room)
    {
        PlaceLeftWallProps(room);
        PlaceRightWallProps(room);
        PlaceTopWallProps(room);
        PlaceBottomWallProps(room);
    }

    public void PlaceInnerProps(BoxRoom room)
    {
        //Place props near RIGHT wall
        List<Prop> innerProps = props
        .Where(x => x.inner)
        .OrderByDescending(x => x.propSize.x * x.propSize.y)
        .ToList();
        PlaceProps(room, innerProps, room.inners);
    }
    
    public void PlaceTraps(BoxRoom room)
    {
        List<Prop> traps = trapProps
        .OrderByDescending(x => x.propSize.x * x.propSize.y)
        .ToList();
        PlaceTraps(room, traps);
    }

    public void PlaceLights(BoxRoom room)
    {
        PlaceProps(room, lightProps, room.nearLeftWall);
        PlaceProps(room, lightProps, room.nearRightWall);
        PlaceProps(room, lightProps, room.nearTopWall);
        PlaceProps(room, lightProps, room.nearBottomWall);
    }

    public void PlaceLeftWallProps(BoxRoom room)
    {
        //Place props near LEFT wall
        List<Prop> leftWallProps = props
        .Where(x => x.nearLeftWall)
        .OrderByDescending(x => x.propSize.x * x.propSize.y)
        .ToList();
        PlaceProps(room, leftWallProps, room.nearLeftWall);
    }

    public void PlaceRightWallProps(BoxRoom room)
    {
        //Place props near RIGHT wall
        List<Prop> rightWallProps = props
        .Where(x => x.nearRightWall)
        .OrderByDescending(x => x.propSize.x * x.propSize.y)
        .ToList();
        PlaceProps(room, rightWallProps, room.nearRightWall);
    }

    public void PlaceTopWallProps(BoxRoom room)
    {
        //Place props near TOP wall
        List<Prop> topWallProps = props
        .Where(x => x.nearTopWall)
        .OrderByDescending(x => x.propSize.x * x.propSize.y)
        .ToList();
        PlaceProps(room, topWallProps, room.nearTopWall);
    }

    public void PlaceBottomWallProps(BoxRoom room)
    {
        //Place props near BOTTOM wall
        List<Prop> bottomWallProps = props
        .Where(x => x.nearBottomWall)
        .OrderByDescending(x => x.propSize.x * x.propSize.y)
        .ToList();
        PlaceProps(room, bottomWallProps, room.nearBottomWall);
    }

    private void PlaceProps(BoxRoom room, List<Prop> nearWallProps, HashSet<Vector2Int> nearWallTiles)
    {
        //avoid placement in entrances path
        HashSet<Vector2Int> tempPositons = new HashSet<Vector2Int>(nearWallTiles);
        if(!nearWallProps[0].inner) tempPositons.ExceptWith(DungeonData.path);

        foreach (Prop propToPlace in nearWallProps)
        {
            //We want to place only certain quantity of each prop
            int quantity = Random.Range(propToPlace.minQuantity, propToPlace.maxQuantity +1);
            int diam = (room.size.x + room.size.y)/2;
            if(diam > 13) quantity++;
            if(diam > 16) quantity++;
            
            for (int i = 0; i < quantity; i++)
            {
                tempPositons.ExceptWith(room.propPositions);
                List<Vector2Int> availablePositions = tempPositons.ToList();
                Utility.UnseededShuffle(availablePositions);
  
                //If the prop cant be placed, stop trying to place the same prop again
                if (TryPlacingPropBruteForce(room, propToPlace, availablePositions) == false) break;
            }
        }
    }

    private bool TryPlacingPropBruteForce(BoxRoom room, Prop propToPlace, List<Vector2Int> availablePositions)
    {
         //try placing the objects starting from the corner specified by the placement parameter
        for (int i = 0; i < availablePositions.Count; i++)
        {
            //select the specified position (but it can be already taken after placing the corner props as a group)
            Vector2Int position = availablePositions[i];
            bool taken = false;

            if (room.propPositions.Contains(position) || taken)
                continue;

            //check if there is enough space around to fit the prop
            List<Vector2Int> freePositionsAround = new();
            if(propToPlace.inner) 
            {
                float roomSizeAverage = (room.size.x + room.size.y) / 2f;
                roomSizeAverage = Mathf.Round(roomSizeAverage);
                int offset = 2; //space between 2 objects
                if(roomSizeAverage < 12) offset = 1;
                freePositionsAround = TryToFitInnerProp(propToPlace, availablePositions, position, offset);
            }
            else
            {
                freePositionsAround = TryToFitProp(propToPlace, availablePositions, position);
            }
            //If we have enough spaces place the prop
            if (freePositionsAround.Count == propToPlace.propSize.x * propToPlace.propSize.y)
            {
                //Place the gameobject
                PlacePropAt(room, position, propToPlace);
                //Lock all the positions recquired by the prop (based on its size)
                foreach (Vector2Int pos in freePositionsAround)
                {
                    //Hashest will ignore duplicate positions
                    room.propPositions.Add(pos);
                }

                //Deal with groups
                if (propToPlace.placeAsGroup)
                {
                    if(propToPlace.inner) PlaceInnerGroupProp(room, position, propToPlace);
                    else PlaceGroupProp(room, position, propToPlace);
                }
                availablePositions.Except(room.propPositions);
                return true;
            }
        }

        return false;
    }

    private List<Vector2Int> TryToFitInnerProp(
        Prop prop, List<Vector2Int> availablePositions, 
        Vector2Int originPosition, int offset)
    {
        List<Vector2Int> freePositions = new();

        for (int xOffset = 0; xOffset < prop.propSize.x; xOffset++)
        {
            for (int yOffset = 0; yOffset < prop.propSize.y; yOffset++)
            {
                bool adjacent = false;
                Vector2Int tempPos = originPosition + new Vector2Int(xOffset, yOffset);
                
                if (availablePositions.Contains(tempPos) )
                {
                    for(int x=-offset; x<=offset; x++)
                    {
                        for(int y=-offset; y<=offset; y++)
                        {
                            Vector2Int checkPos = tempPos + new Vector2Int(x, y);
                            if(!availablePositions.Contains(checkPos))
                            {
                                adjacent = true;
                                break;
                            }
                        }
                        if(adjacent) break;
                    }
                    if(!adjacent) freePositions.Add(tempPos); 
                }
            }
        }
        return freePositions;
    }

    private List<Vector2Int> TryToFitProp(
        Prop prop,
        List<Vector2Int> availablePositions,
        Vector2Int originPosition)
    {
        List<Vector2Int> freePositions = new();

        for (int xOffset = 0; xOffset < prop.propSize.x; xOffset++)
        {
            for (int yOffset = 0; yOffset < prop.propSize.y; yOffset++)
            {
                Vector2Int tempPos = originPosition + new Vector2Int(xOffset, yOffset);
                if (availablePositions.Contains(tempPos) )
                {
                    freePositions.Add(tempPos); 
                }
            }
        }
        return freePositions;
    }

    private void PlaceTraps(BoxRoom room, List<Prop> traps)
    {
        //avoid placement in entrances path
        HashSet<Vector2Int> tempPositons = new HashSet<Vector2Int>(room.roomTiles);

        foreach (Prop propToPlace in traps)
        {
            //We want to place only certain quantity of each prop
            int quantity = Random.Range(propToPlace.minQuantity, propToPlace.maxQuantity +1);
            int diam = (room.size.x + room.size.y)/2;
            if(diam > 13) quantity++;
            if(diam > 16) quantity++;
            
            for (int i = 0; i < quantity; i++)
            {
                tempPositons.ExceptWith(room.propPositions);
                List<Vector2Int> availablePositions = tempPositons.ToList();
                Utility.UnseededShuffle(availablePositions);
  
                //If the prop cant be placed, stop trying to place the same prop again
                if (TryPlacingTrapBruteForce(room, propToPlace, availablePositions) == false) break;
            }
        }
    }

    private bool TryPlacingTrapBruteForce(BoxRoom room, Prop propToPlace, List<Vector2Int> availablePositions)
    {
         //try placing the objects starting from the corner specified by the placement parameter
        for (int i = 0; i < availablePositions.Count; i++)
        {
            //select the specified position (but it can be already taken after placing the corner props as a group)
            Vector2Int position = availablePositions[i];
            if (room.propPositions.Contains(position))  continue;

            //check if there is enough space around to fit the prop            
            float roomSizeAverage = (room.size.x + room.size.y) / 2f;
            roomSizeAverage = Mathf.Round(roomSizeAverage);
            int offset = 2; //space between 2 objects
            if(roomSizeAverage < 12) offset = 1;
            List<Vector2Int> freePositionsAround = TryToFitInnerProp(propToPlace, availablePositions, position, offset);
            

            //If we have enough spaces place the prop
            if (freePositionsAround.Count == propToPlace.propSize.x * propToPlace.propSize.y)
            {
                //Place the gameobject
                PlaceTrapAt(room, position, propToPlace);
                //Lock all the positions recquired by the prop (based on its size)
                foreach (Vector2Int pos in freePositionsAround)
                {
                    //Hashest will ignore duplicate positions
                    room.propPositions.Add(pos);
                }

                //Deal with groups
                if (propToPlace.placeAsGroup)
                {
                    PlaceGroupTrap(room, position, propToPlace);
                }
                availablePositions.Except(room.propPositions);
                return true;
            }
        }

        return false;
    }


    private void PlaceCornerGroupProp(BoxRoom room, Vector2Int startingPos, Prop prop)
    {   //fixed offset and placement position can make bigger prop poorly placed
        int count = (int) Utility.UnseededRng(prop.minGroupSize, prop.maxGroupSize)-1;
        //find valid space around group placement startingPos
        List<Vector2Int> availableSpaces = new();
        var searchOffset = Mathf.Max(Mathf.CeilToInt(room.size.x / 10f), Mathf.CeilToInt(room.size.y / 10f));
        for (int xOffset = -searchOffset; xOffset <= searchOffset; xOffset++)
        {
            for (int yOffset = -searchOffset; yOffset <= searchOffset; yOffset++)
            {
                // Check if the position is within the diamond shape
                var absCheck =Math.Abs(xOffset) + Math.Abs(yOffset);
                if (absCheck <= searchOffset)
                {
                    Vector2Int checkPos = startingPos + new Vector2Int(xOffset, yOffset);
                    if (room.roomTiles.Contains(checkPos) && 
                        !DungeonData.path.Contains(checkPos))
                    {
                        if(room.propPositions.Contains(checkPos))
                        {
                            count--;
                            if(count == 0) return;
                        }
                        else
                        {
                            availableSpaces.Add(checkPos);
                        }
                    }
                }
            }
        }

        //shuffle valid spaces list
        Utility.UnseededShuffle(availableSpaces); //No seed, always new

        //clamp group object size to available spaces
        int validCount = count < availableSpaces.Count ? count : availableSpaces.Count;
        // Debug.Log("Count: "+count+", Valid count:"+validCount+" Available: "+ String.Join(", ", availableSpaces));
        
        for (int i = 0; i < validCount; i++)
        {
            PlacePropAt(room, availableSpaces[i], prop);
        }
    }

    private void PlaceGroupProp(BoxRoom room, Vector2Int startingPos, Prop prop)
    {   //fixed offset and placement position can make bigger prop poorly placedd
        int count = (int) Utility.UnseededRng(prop.minGroupSize, prop.maxGroupSize+1);
        //find valid space around group placement startingPos
        List<Vector2Int> availableSpaces = new();
        var searchOffset = Mathf.Max(1,Mathf.FloorToInt(room.size.x / 10f), Mathf.FloorToInt(room.size.y / 10f));
        
        for(int xOffset = -searchOffset; xOffset <= searchOffset; xOffset++)
        {
            for(int yOffset = -searchOffset; yOffset <= searchOffset; yOffset++)
            {
                Vector2Int checkPos = startingPos + new Vector2Int(xOffset, yOffset);

                if(room.roomTiles.Contains(checkPos) && 
                !DungeonData.path.Contains(checkPos))
                {
                    if(room.propPositions.Contains(checkPos))
                    {
                        count--;
                        if(count == 0) return;
                    }
                    else
                    {
                        availableSpaces.Add(checkPos);
                    }
                }
            }
        }
        
        //shuffle valid spaces list
        Utility.UnseededShuffle(availableSpaces); //No seed, always new

        //clamp group object size to available spaces
        int validCount = count < availableSpaces.Count ? count : availableSpaces.Count;
        // Debug.Log("Count: "+count+", Valid count:"+validCount+" Available: "+ String.Join(", ", availableSpaces));
        
        for (int i = 0; i < validCount; i++)
        {
            PlacePropAt(room, availableSpaces[i], prop);
        }
    }

    private void PlaceInnerGroupProp(BoxRoom room, Vector2Int startingPos, Prop prop)
    {   //fixed offset and placement position can make bigger prop poorly placedd
        int count = (int) Utility.UnseededRng(prop.minGroupSize, prop.maxGroupSize+1)-1;
        //find valid space around group placement startingPos
        List<Vector2Int> availableSpaces = new();
        var searchOffset = Mathf.Max(1,Mathf.FloorToInt(room.size.x / 10f), Mathf.FloorToInt(room.size.y / 10f));
        
        for(int xOffset = -searchOffset*prop.propSize.x; xOffset < searchOffset*prop.propSize.x; xOffset+=prop.propSize.x)
        {
            for(int yOffset = -searchOffset*prop.propSize.y; yOffset < searchOffset*prop.propSize.y; yOffset+=prop.propSize.y)
            {
                Vector2Int checkPos = startingPos + new Vector2Int(xOffset, yOffset);
                bool taken = false;
                for(int x = 0; x < prop.propSize.x; x++)
                {
                    for(int y = 0; y < prop.propSize.y; y++)
                    {
                        if(room.propPositions.Contains(checkPos + new Vector2Int(x, y)))
                        {
                            taken = true;
                            break;
                        }
                    }
                    if(taken) break;
                }
                if(room.inners.Contains(checkPos) &&
                !room.propPositions.Contains(checkPos) && !taken)
                {                   
                    availableSpaces.Add(checkPos);   
                }
            }
        }
        
        //shuffle valid spaces list
        Utility.UnseededShuffle(availableSpaces); //No seed, always new

        //clamp group object size to available spaces
        int validCount = count < availableSpaces.Count ? count : availableSpaces.Count;
        // Debug.Log("Count: "+count+", Valid count:"+validCount+" Available: "+ String.Join(", ", availableSpaces));
        
        for (int i = 0; i < validCount; i++)
        {
            PlacePropAt(room, availableSpaces[i], prop);
        }
    }

    private void PlaceGroupTrap(BoxRoom room, Vector2Int startingPos, Prop trap)
    {
        int count = (int) Utility.UnseededRng(trap.minGroupSize, trap.maxGroupSize);
        //find valid space around group placement startingPos
        List<Vector2Int> availableSpaces = new();
        
        for(int i=0; i<=count; i++)
        {
            bool taken = false;
            for(int x = 0; x < trap.propSize.x; x++)
            {
                for(int y = 0; y < trap.propSize.y; y++)
                {
                    if(room.propPositions.Contains(startingPos + new Vector2Int(x, y)))
                    {
                        taken = true;
                        break;
                    }
                }
                if(taken) break;
            }
            if(room.roomTiles.Contains(startingPos) && 
            !DungeonData.path.Contains(startingPos) &&
            !room.propPositions.Contains(startingPos) && !taken)
            {                   
                availableSpaces.Add(startingPos);   
            }
            while(true)
            {
                startingPos += Direction2D.GetRandomDirection();
                if(availableSpaces.Contains(startingPos))
                {
                    startingPos = availableSpaces[Utility.UnseededRng(0, availableSpaces.Count)];
                    continue;
                }
                break;
            }
        }
        
        int validCount = count < availableSpaces.Count ? count : availableSpaces.Count;
        
        for (int i = 0; i < validCount; i++)
        {
            PlaceTrapAt(room, availableSpaces[i], trap);
        }
    }

    private GameObject PlaceTrapAt(BoxRoom room, Vector2Int placementPostion, Prop prop)
    {
        GameObject propObject = Instantiate(prop.prefabsVariant[Utility.UnseededRng(0, prop.prefabsVariant.Count)]);
        propObject.transform.position = (Vector2)placementPostion;
        propObject.transform.SetParent(propParent);
        for(int x = 0; x < prop.propSize.x; x++)
        {
            for(int y = 0; y < prop.propSize.y; y++)
            {
                room.propPositions.Add(placementPostion + new Vector2Int(x, y));
            }
        }
        room.propObjectReferences.Add(propObject);
        return propObject;
    }

    private GameObject PlacePropAt(BoxRoom room, Vector2Int placementPostion, Prop prop)
    {
        GameObject propObject = Instantiate(prop.prefabsVariant[Utility.UnseededRng(0, prop.prefabsVariant.Count)]);
        propObject.transform.position = (Vector2)placementPostion + new Vector2(0, 0.25f);
        propObject.transform.SetParent(propParent);
        for(int x = 0; x < prop.propSize.x; x++)
        {
            for(int y = 0; y < prop.propSize.y; y++)
            {
                room.propPositions.Add(placementPostion + new Vector2Int(x, y));
            }
        }
        room.propObjectReferences.Add(propObject);
        return propObject;
    }

    private void PlacePropCenterAt(BoxRoom room, Vector2 posToCenter, Prop prop)
    {
        var propCenter = new Vector2(prop.propSize.x/2f, prop.propSize.y/2f);
        var centerProp = posToCenter - propCenter + new Vector2(0.5f,0.5f);  
        GameObject propObject = Instantiate(prop.prefabsVariant[Utility.UnseededRng(0, prop.prefabsVariant.Count)]);
        propObject.transform.position = centerProp;
        propObject.transform.SetParent(propParent);
        var takenPos = new Vector2Int(Mathf.FloorToInt(centerProp.x), Mathf.FloorToInt(centerProp.y));
        for(int x = 0; x < prop.propSize.x; x++)
        {
            for(int y = 0; y < prop.propSize.y; y++)
            {
                room.propPositions.Add(takenPos + new Vector2Int(x, y));
            }
        }
        room.propObjectReferences.Add(propObject);
    }

    public void Reset()
    {
        while (propParent.childCount > 0)
        {
            DestroyImmediate(propParent.GetChild(0).gameObject);
        }
    }

    public void PlaceSpawnRoomProps(BoxRoom room)
    {

        if(groundPortal) PlacePropCenterAt(room, Vector2.zero, groundPortal);
    }

    public void PlaceShopRoomProps(BoxRoom room)
    {
        if(shop) PlacePropCenterAt(room, room.center, shop);
    }

    public void PlaceExitPortal(BoxRoom room)
    {
        PlacePropCenterAt(room, room.center, exit);
    }

    public void PlaceTreasure(BoxRoom room)
    {
        PlacePropCenterAt(room, room.center, treasure);
    }

    public void SetRoomEntrances(BoxRoom room)
    {
        var center = room.center;
        var size = room.size;
        var path = DungeonData.path;

        var entrances = new Dictionary<int, Vector2Int>
        {
            { 01, new Vector2Int(center.x, center.y + size.y + 1) },    //mid top
            { 0-1, new Vector2Int(center.x, center.y - size.y - 1) }, //mid bottom
            { -10, new Vector2Int(center.x - size.x - 1, center.y) },   //mid left
            { 10, new Vector2Int(center.x + size.x + 1, center.y) }   //mid right
        };
        foreach (var entrance in entrances)
        {
            if (DungeonData.path.Contains(entrance.Value))
            {
                var beforeEntrance = entrance.Value;
                switch (entrance.Key)
                {
                    case 01:
                        room.topEntrance = entrance.Value;
                        beforeEntrance = entrance.Value+Vector2Int.down;
                        path.Add(beforeEntrance);
                        path.Add(beforeEntrance+Vector2Int.left);
                        path.Add(beforeEntrance+Vector2Int.right);
                        break;
                    case 0-1:
                        room.bottomEntrance = entrance.Value;
                        beforeEntrance = entrance.Value+Vector2Int.up;
                        path.Add(beforeEntrance);
                        path.Add(beforeEntrance+Vector2Int.left);
                        path.Add(beforeEntrance+Vector2Int.right);
                        break;
                    case -10:
                        room.leftEntrance = entrance.Value;
                        beforeEntrance = entrance.Value+Vector2Int.right;
                        path.Add(beforeEntrance);
                        path.Add(beforeEntrance+Vector2Int.up);
                        path.Add(beforeEntrance+Vector2Int.down);
                        break;
                    case 10:
                        room.rightEntrance = entrance.Value;
                        beforeEntrance = entrance.Value+Vector2Int.left;
                        path.Add(beforeEntrance);
                        path.Add(beforeEntrance+Vector2Int.up);
                        path.Add(beforeEntrance+Vector2Int.down);
                        break;
                }
            }
        }
        
    }

}

using System;
using System.Collections.Generic;
using UnityEngine;

public class EliteRoomSetup : MonoBehaviour
{
    public GameObject[] barriers; //0:front 1:side
    public GameObject triggerPrefab;
    public Transform objectParent;
    private BoxRoom eliteRoom;
    
    public void SetupEliteRoom(BoxRoom room, PlayerSpawner playerSpawner)
    {
        eliteRoom = room;
        var barrierParent = new GameObject("Barrier").transform; //contains this room barriers
        barrierParent.SetParent(objectParent);
        eliteRoom.barrierReference = barrierParent.gameObject; //reference to room
        var barrierList = PlaceBarriers(barrierParent); //barriers reference for trigger

        PlaceTrigger(barrierList, playerSpawner);

        if(room.eliteReference != null) //wait spawn animation
        {
            room.eliteReference.SetActive(false);
        }
    }

    private void PlaceTrigger(List<GameObject> barrierList, PlayerSpawner playerSpawner)
    {
        var trigger = PlaceCenterCell(triggerPrefab, eliteRoom.center, objectParent);
        var scale = new Vector3(eliteRoom.size.x*2-1, eliteRoom.size.y*2-1, 1);
        trigger.transform.localScale = scale;

        var triggerScript = trigger.GetComponent<EliteTrigger>();
        triggerScript.Initialize(eliteRoom, barrierList, playerSpawner);
    }

    private List<GameObject> PlaceBarriers(Transform barrierParent)
    {
        var barrierList = new List<GameObject>();
        var barrierPlace = new[]
        {
            (eliteRoom.topEntrance, barriers[0]),
            (eliteRoom.bottomEntrance, barriers[0]),
            (eliteRoom.leftEntrance, barriers[1]),
            (eliteRoom.rightEntrance, barriers[1])
        };

        foreach (var (entrance, prefab) in barrierPlace)
        {
            if (entrance == Vector2Int.zero) continue;
            var placedBarrier = PlaceCenterCell(prefab, entrance, barrierParent);
            placedBarrier.SetActive(false);
            barrierList.Add(placedBarrier);
        }
        return barrierList;
    }

    private GameObject PlaceCenterCell(GameObject obj, Vector2Int position, Transform parent)
    {
        var pos = new Vector3(position.x + 0.5f, position.y +0.5f, 0);
        var o = Instantiate(obj, pos, Quaternion.identity, parent);
        return o;
    }

    public void Reset()
    {
        while (objectParent.childCount > 0)
        {
            DestroyImmediate(objectParent.GetChild(0).gameObject);
        }
    }
}

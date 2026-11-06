using UnityEngine;

public class EliteRoomSetup : MonoBehaviour
{
    public GameObject[] barriers; //0:front 1:side
    public GameObject triggerPrefab;
    public Transform objectParent;
    public void SetupCombatRoom(BoxRoom room)
    {
        var barrierParent = new GameObject("Barrier").transform;
        barrierParent.SetParent(objectParent, false);
        barrierParent.gameObject.SetActive(false);
        PlaceBarriers(room, barrierParent);
        barrierParent.gameObject.SetActive(true);

        if (room.eliteReference != null)
        {
            room.eliteReference.SetActive(false);
            var trigger = PlaceCenterCell(triggerPrefab, room.center, objectParent);
            trigger.GetComponent<EliteTrigger>().Initialize(room);
        }
    }

    private void PlaceBarriers(BoxRoom room, Transform barrierParent)
    {
        var barrierPlace = new[]
        {
            (room.topEntrance, barriers[0]),
            (room.bottomEntrance, barriers[0]),
            (room.leftEntrance, barriers[1]),
            (room.rightEntrance, barriers[1])
        };

        foreach (var (entrance, prefab) in barrierPlace)
        {
            if (entrance == Vector2Int.zero) continue;
            var placedBarrier = PlaceCenterCell(prefab, entrance, barrierParent);
            placedBarrier.SetActive(false);
            room.AddBarrier(placedBarrier);
        }
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

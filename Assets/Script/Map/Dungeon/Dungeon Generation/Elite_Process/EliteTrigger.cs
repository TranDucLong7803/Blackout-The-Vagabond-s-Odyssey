using System.Collections;
using UnityEngine;

public class EliteTrigger : MonoBehaviour
{
    public GameObject spawnAnimation;
    [SerializeField] private float delaySpawn = 0.5f;
    private BoxRoom eliteRoom;

    public void Initialize(BoxRoom room)
    {
        eliteRoom = room;
        eliteRoom.CombatActivated += SpawnElite;
    }

    private void SpawnElite()
    {
        var elite = eliteRoom.eliteReference;
        if (elite == null) return;

        var animation = Instantiate(spawnAnimation, elite.transform.position, Quaternion.identity);
        StartCoroutine(ShowEliteAfterDelay(elite));
        Destroy(animation, delaySpawn + 0.5f);
    }

    private IEnumerator ShowEliteAfterDelay(GameObject elite)
    {
        yield return new WaitForSeconds(delaySpawn);
        if (elite != null) elite.SetActive(true);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (eliteRoom != null) eliteRoom.CombatActivated -= SpawnElite;
    }
}

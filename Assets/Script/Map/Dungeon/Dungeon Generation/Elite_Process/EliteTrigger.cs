using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteTrigger : MonoBehaviour
{
    public GameObject spawnAnimation;
    [SerializeField] private float delaySpawn = 0.5f;
    private BoxRoom eliteRoom;
    private List<GameObject> barriers;
    private PlayerSpawner playerSpawner;
    private bool playerEntered;

    public void Initialize(BoxRoom room, List<GameObject> roomBarriers, PlayerSpawner spawner)
    {
        eliteRoom = room;
        barriers = roomBarriers;
        playerSpawner = spawner;
        eliteRoom.CombatActivated += SpawnElite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerEntered || eliteRoom == null || !other.CompareTag("Player")
            || (playerSpawner != null && !playerSpawner.HasSpawned)) return;

        playerEntered = true;
        eliteRoom.ActivateCombat();
        if (eliteRoom.enemyCount > 0)
        {
            foreach (var barrier in barriers)
                barrier.SetActive(true);

            if (GameManager.Instance != null) GameManager.Instance.PlayCurrentStageEliteMusic();
        }
        Destroy(gameObject, delaySpawn + 1f);
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
    }

    private void OnDestroy()
    {
        if (eliteRoom != null) eliteRoom.CombatActivated -= SpawnElite;
    }
}

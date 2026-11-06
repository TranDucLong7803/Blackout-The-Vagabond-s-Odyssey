using System;
using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnEffect;
    [SerializeField] private float delaySpawn = 1f;
    [SerializeField] private float delayPlayer = 0.2f;
    private GameObject[] players;
    public bool HasSpawned { get; private set; }

    void Awake()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    
    void Start()
    {
        StartCoroutine(DelaySpawn());
    }

    private IEnumerator DelaySpawn()
    {
        yield return new WaitForSeconds(delaySpawn);
        var anime = Instantiate(spawnEffect, this.transform.position + new Vector3(0,-0.25f,0), Quaternion.identity);
        Destroy(anime, 5);
        StartCoroutine(DelayPlayer());

        GameManager.Instance.PlayCurrentStageDungeonMusic();
    }

    private IEnumerator DelayPlayer()
    {
        yield return new WaitForSeconds(delayPlayer);
        foreach (var p in players)
        {
            var ps = p.GetComponentsInChildren<SpriteRenderer>();
            foreach(var s in ps) s.enabled = true;
            p.transform.position = this.transform.position;
        }
        HasSpawned = true;
    }

}

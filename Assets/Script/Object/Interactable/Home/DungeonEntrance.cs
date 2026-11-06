using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonEntrance : MonoBehaviour
{
    [SerializeField] private GameObject animationPrefab;
    [SerializeField] private float delayStart = 1.5f;
    [SerializeField] private float delayHide = 0.2f;
    [SerializeField] private Vector3 animationOffset = new Vector3(0.25f,0.25f,0);

    private readonly HashSet<Collider2D> playerColliders = new();
    private HomeMovement player;
    private bool enteringDungeon;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var body = collision.attachedRigidbody;
        if (body == null) return;

        var movement = body.GetComponentInChildren<HomeMovement>();
        if (movement == null) return;

        player = movement;
        playerColliders.Add(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        playerColliders.Remove(collision);
        if (playerColliders.Count == 0) player = null;
    }

    private void Update()
    {
        if (enteringDungeon || player == null || !InputManager.Instance.TryConsumeInteract()) return;

        enteringDungeon = true;
        player.enabled = false;
        Instantiate(animationPrefab, player.transform.position + animationOffset, Quaternion.identity);
        StartCoroutine(HidePlayer(player.transform.root.gameObject));
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene(){
        yield return new WaitForSeconds(delayStart);
        GameManager.Instance.StartDungeon();
    }

    private IEnumerator HidePlayer(GameObject obj)
    {
        yield return new WaitForSeconds(delayHide);
        obj.SetActive(false);
    }
}

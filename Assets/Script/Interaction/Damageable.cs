using System;
using System.Collections;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Damageable : MonoBehaviour, IDamageable
{
    public int health = 3;
    private Lootable lootable;
    private bool isBreaking;

    public event Action BrokenByPlayer;

    private void Awake()
    {
        lootable = GetComponent<Lootable>();
    }

    public virtual void takeDame(int damage)
    {
        if (isBreaking) return;

        health -= damage;
        if (health > 0) return;

        isBreaking = true;
        if (lootable != null) lootable.DropLoot();
        StartCoroutine(UpdateGraphAndDestroy());
    }

    public void TakePlayerDamage(int damage)
    {
        bool wasBreaking = isBreaking;
        takeDame(damage);
        if (!wasBreaking && isBreaking)
            BrokenByPlayer?.Invoke();
    }

    private IEnumerator UpdateGraphAndDestroy()
    {
        yield return null;

        if (TryGetComponent<Collider2D>(out var collider))
        {
            Bounds updateBounds = collider.bounds;
            collider.enabled = false;
            if (AstarPath.active != null)
                AstarPath.active.UpdateGraphs(new GraphUpdateObject(updateBounds));
        }
        Destroy(gameObject);
    }
}

using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<Player>(out var player)) return;

        var enemy = GetComponentInParent<Enemy>();
        if (enemy.CanAct)
            player.healthController.takeDame(enemy.damage);
    }
}

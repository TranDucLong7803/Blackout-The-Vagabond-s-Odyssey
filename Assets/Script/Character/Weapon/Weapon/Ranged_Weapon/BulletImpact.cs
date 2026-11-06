using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BulletImpact : MonoBehaviour
{
    [SerializeField] private Color obstacleColor = new Color32(255, 191, 75, 255);
    [SerializeField] private Color damageableColor = new Color32(255, 100, 63, 255);

    private ParticleSystem sparks;
    private PoolManagement pool;
    private GameObject prefab;

    private void Awake()
    {
        sparks = GetComponent<ParticleSystem>();
    }

    public void Play(PoolManagement owner, GameObject prefabKey, Vector3 position, bool damageable)
    {
        pool = owner;
        prefab = prefabKey;
        transform.position = position;

        var main = sparks.main;
        main.startColor = damageable ? damageableColor : obstacleColor;
        sparks.Play(true);
    }

    private void OnParticleSystemStopped()
    {
        if (pool == null) return;

        var owner = pool;
        pool = null;
        owner.ReturnBullet(gameObject, prefab);
    }
}

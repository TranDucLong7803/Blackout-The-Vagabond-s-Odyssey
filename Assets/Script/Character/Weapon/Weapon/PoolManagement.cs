using System.Collections.Generic;
using UnityEngine;

public class PoolManagement : MonoBehaviour
{
    public static PoolManagement Instance { get; private set; }

    [SerializeField] public List<GameObject> allBulletPrefab;
    private Dictionary<GameObject, Queue<GameObject>> bulletPools;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
        
    }

    private void InitializePools()
    {
        bulletPools = new Dictionary<GameObject, Queue<GameObject>>();
        foreach (GameObject bulletPrefab in allBulletPrefab)
        {
            bulletPools[bulletPrefab] = new Queue<GameObject>();
        }
    }

    public GameObject GetBullet(GameObject bulletPrefab, bool shouldChangeBullet = false)
    {
        if (!bulletPools.ContainsKey(bulletPrefab))
        {
            Debug.LogError("BulletPrefab not found in pool!");
            return null;
        }

        GameObject bullet;
        if (bulletPools[bulletPrefab].Count > 0)
        {
            bullet = bulletPools[bulletPrefab].Dequeue();
            bullet.SetActive(true);
        }
        else
        {
            bullet = Instantiate(bulletPrefab);
        }

        if (shouldChangeBullet)
        {
           
            BulletIdentifier identifier = bullet.GetComponent<BulletIdentifier>();
            if (identifier == null)
            {
                identifier = bullet.AddComponent<BulletIdentifier>();
            }
            
            identifier.bulletPrefabReference = bulletPrefab;
        }

        return bullet;
    }


    public void PlayBulletImpact(BulletImpact prefab, Vector3 position, bool damageable)
    {
        var key = prefab.gameObject;
        if (!bulletPools.ContainsKey(key))
            bulletPools.Add(key, new Queue<GameObject>());

        var effect = GetBullet(key).GetComponent<BulletImpact>();
        effect.Play(this, key, position, damageable);
    }

    public void ReturnBullet(GameObject bullet, GameObject bulletPrefab)
    {
        bullet.SetActive(false);
        //bulletPools[bulletPrefab].Enqueue(bullet);
        if (!bulletPools.ContainsKey(bulletPrefab))
        {
            BulletIdentifier identifier = bullet.GetComponent<BulletIdentifier>();
            if (identifier != null && identifier.bulletPrefabReference != null && bulletPools.ContainsKey(identifier.bulletPrefabReference))
            {
                bulletPools[identifier.bulletPrefabReference].Enqueue(bullet);
            }
            else
            {
                Debug.LogError("dmmmmm");
            }
        }
        else
        {
            bulletPools[bulletPrefab].Enqueue(bullet);
        }

    }
}

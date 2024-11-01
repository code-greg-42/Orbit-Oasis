using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    private List<GameObject> pooledObjects;
    [SerializeField] private int amountToPool = 10;
    [SerializeField] private GameObject objectToPool;

    private void Awake()
    {
        Instance = this;

        if (MainSoundManager.Instance != null)
        {
            AdjustVolumeSettings();
            InitializePool();
        }
        else
        {
            Debug.LogWarning("MainSoundManager = null, using backup initialization coroutine.");
            StartCoroutine(BackupInitialization());
        }
    }

    private IEnumerator BackupInitialization()
    {
        yield return new WaitUntil(() => MainSoundManager.Instance != null);

        AdjustVolumeSettings();
        InitializePool();
    }

    private void InitializePool()
    {
        pooledObjects = new List<GameObject>();
        GameObject obj;
        for (int i = 0; i < amountToPool; i++)
        {
            obj = Instantiate(objectToPool);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    private void AdjustVolumeSettings()
    {
        // set volume on prefab audio source
        if (objectToPool.TryGetComponent(out AudioSource projectileSource))
        {
            projectileSource.volume = MainSoundManager.Instance.MasterVolume * MainSoundManager.Instance.ProjectileVolume;
        }

        if (objectToPool.TryGetComponent(out Projectile projectile))
        {
            if (projectile.DetonationEffect != null && projectile.DetonationEffect.TryGetComponent(out AudioSource detonationSource))
            {
                detonationSource.volume = MainSoundManager.Instance.MasterVolume * MainSoundManager.Instance.DetonationVolume;
            }
        }
    }

    public GameObject GetPooledObject()
    {
        // loop through and return the first inactive enemy
        for (int i = 0; i < amountToPool; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }
}

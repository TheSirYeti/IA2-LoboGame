using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Random = System.Random;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    public List<BaseEnemy> spawnedEnemies = new List<BaseEnemy>();

    [Header("Wave Properties")] 
    [SerializeField] private float _enemiesPerWave;
    private float _waveMultiplier = 1.5f;
    [Space(15)] 
    [SerializeField] private float _enemySpawnCooldown;
    [SerializeField] private float _enemySpawnReducer;
    [SerializeField] private List<GameObject> _enemyPrefabs;
    private int _prefabCount;
    [SerializeField] private List<Transform> _spawnpoints;


    private void Awake()
    {
        if (instance == null) instance = this;
        else 
        {
            Destroy(gameObject);
            return;
        }

        _prefabCount = _enemyPrefabs.Count;
    }

    private void Start()
    {
        StartCoroutine(DoEnemyWave());
    }

    public void AddEnemy(BaseEnemy enemy)
    {
        spawnedEnemies.Add(enemy);
    }

    public void RemoveEnemy(BaseEnemy enemy)
    {
        spawnedEnemies.Remove(enemy);
    }

    public bool ContainsEnemy(BaseEnemy enemy)
    {
        var myEnemy = spawnedEnemies.Select(x => x).Where(x => x == enemy).Take(1);

        if (myEnemy == null) return false;
        return true;
    }

    IEnumerator DoEnemyWave()
    {
        yield return new WaitForSeconds(20f);
        
        for (int i = 0; i < _enemiesPerWave; i++)
        {
            var randEnemy = UnityEngine.Random.Range(0, _prefabCount);
            var enemy = Instantiate(_enemyPrefabs[randEnemy]);

            var randPos = UnityEngine.Random.Range(0, _spawnpoints.Count);
            enemy.transform.position = _spawnpoints[randPos].position;
            
            yield return new WaitForSeconds(_enemySpawnCooldown);

            _enemySpawnCooldown -= _enemySpawnReducer;
            if (_enemySpawnCooldown <= 1.5f)
                _enemySpawnCooldown = 1.5f;
            
            yield return null;
        }

        Village.instance.Win();
        _enemiesPerWave *= _waveMultiplier;
        yield return null;
    }
}

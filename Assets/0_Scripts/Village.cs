using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Village : MonoBehaviour
{
    public static Village instance;
    [SerializeField] private GameObject gameOverSign, winSign;
    [SerializeField] private GameObject bomberPrefab;
    [SerializeField] private Transform bomberSpawnpoint;
    
    List<IEntity> allVillagers = new List<IEntity>();

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Update()
    {
        if (allVillagers.Count <= 0)
        {
            gameOverSign.SetActive(true);
        }
        else gameOverSign.SetActive(false);

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void AddVillager(IEntity villager)
    {
        allVillagers.Add(villager);
    }
    
    public void RemoveVillager(IEntity villager)
    {
        allVillagers.Remove(villager);
    }

    public void SpawnBomber()
    {
        var bomber = Instantiate(bomberPrefab);
        bomber.transform.position = bomberSpawnpoint.position;
    }

    public void Win()
    {
        if(!gameOverSign.activeSelf)
            winSign.SetActive(true);
    }

    private bool isFastForwarding = false;
    public void ToggleFastForward()
    {
        if (!isFastForwarding)
        {
            Time.timeScale = 10f;
        }
        else Time.timeScale = 1f;

        isFastForwarding = !isFastForwarding;
    }
}

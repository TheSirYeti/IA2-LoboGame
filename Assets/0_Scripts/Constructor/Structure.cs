using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Structure : MonoBehaviour, IEntity
{
    List<MeshRenderer> myMeshes;
    [SerializeField] private float hp;
    private float originalHP;

    private void Awake()
    {
        //Consigo las mesh para poder apagarlas mientras no esten construidas
        myMeshes = GetComponentsInChildren<MeshRenderer>().ToList();

        StructureManager.instance.availablesStructures.Add(this);

        GetComponent<BoxCollider>().enabled = false;

        foreach (var mesh in myMeshes)
        {
            mesh.enabled = false;
        }

        originalHP = hp;
    }

    public void OnBuild()
    {
        StructureManager.instance.availablesStructures.Remove(this);
        StructureManager.instance.unavailablesStructures.Add(this);

        foreach (var mesh in myMeshes)
        {
            mesh.enabled = true;
        }

        GetComponent<BoxCollider>().enabled = true;
        
        NodeManager.instance.CalculateNodeNeighbours();
    }

    public void OnDestroyed()
    {
        StructureManager.instance.availablesStructures.Add(this);
        StructureManager.instance.unavailablesStructures.Remove(this);

        foreach (var mesh in myMeshes)
        {
            mesh.enabled = false;
        }

        GetComponent<BoxCollider>().enabled = false;
        
        NodeManager.instance.CalculateNodeNeighbours();
    }

    #region IENTITY

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public float Health
    {
        get
        {
            return hp;
        }
        set
        {
            hp = value;
        }
    }

    public GameObject myGameObject
    {
        get
        {
            return gameObject;
        }
    }

    public bool IsEnemy
    {
        get
        {
            return false;
        }
    }
    public void TakeDamage(float damage)
    {
        Health -= damage;

        if (hp <= 0)
        {
            OnDestroyed();
            hp = originalHP;
        }
    }

    private bool isInGrid = false;
    public bool onGrid
    {
        get
        {
            return isInGrid;
        }
        set
        {
            isInGrid = value;
        }
    }
    public event Action<IEntity> OnMove;

    #endregion
}

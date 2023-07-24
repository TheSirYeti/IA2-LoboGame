using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyTest : MonoBehaviour, IEntity
{
    private float _hp = 15f;

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
            return _hp;
        }
        set
        {
            _hp = value;
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
        get { return false; }
    }

    public void TakeDamage(float damage)
    {
        _hp -= damage;

        if (_hp <= 0)
        {
            gameObject.SetActive(false);
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

    public event Action<IEntity> OnMove = delegate {};
}

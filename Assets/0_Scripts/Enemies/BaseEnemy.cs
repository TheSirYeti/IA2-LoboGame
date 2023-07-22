using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour, IEntity
{
    [Header("Base Properties")] [SerializeField]
    protected float _hp;
    [SerializeField] protected Animator _animator;

    public Vector3 Position { get; }

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
        get
        {
            return true;
        }
    }


    public void TakeDamage(float damage)
    {
        _hp -= damage;
    }

    public abstract void Attack();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Base Properties")] [SerializeField]
    protected float _hp;
    [SerializeField] protected Animator _animator;
    
    public void TakeDamage(float damage)
    {
        _hp -= damage;
    }

    public abstract void Attack();
}

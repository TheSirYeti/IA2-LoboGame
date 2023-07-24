using System;
using UnityEngine;

public interface IEntity
{
    public Vector3 Position { get; }
    public float Health { get; set; }
    public GameObject myGameObject { get; }
    public bool IsEnemy { get; }
    public void TakeDamage(float damage);
    
    public bool onGrid { get; set; }
    public event Action<IEntity> OnMove;
}

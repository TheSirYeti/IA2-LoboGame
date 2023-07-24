using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class BaseEnemy : MonoBehaviour, IEntity
{
    [Header("Base Properties")] 
    [SerializeField] protected float _hp;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected float searchRange;

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

    public IEntity FindNearestTarget(Vector3 position)
    {
        //TODO: hacer con Query

        var objectsInRange = Physics.OverlapSphere(transform.position, searchRange);

        var finalEntity = objectsInRange.Aggregate(FList.Create<IEntity>(), (flist, listObject) =>
        {
            listObject.TryGetComponent(out IEntity entity);
            
            flist = entity != null ? flist + entity : flist;
            return flist;
        }).OrderBy(x => Vector3.Distance(x.Position, transform.position)).FirstOrDefault();

        return finalEntity;

    }
    
    public void TakeDamage(float damage)
    {
        _hp -= damage;
    }

    public abstract void Attack();
}

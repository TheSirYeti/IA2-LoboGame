using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Arrows : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField] float _arrowSpeed;
    [SerializeField] float _damage;
    [SerializeField] Vector3 _dir;
    float dirX, dirY, dirZ;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    void Start()
    {
        dirX = Random.Range(-.2f, .2f);
        dirY = Random.Range(.5f, 1f);
        dirZ = Random.Range(0.5f, 1.5f);
        _dir = new Vector3(dirX, dirY, dirZ);
        transform.forward = _dir;
        _rb.AddForce(_dir * _arrowSpeed, ForceMode.Acceleration);
        
        Destroy(gameObject, 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        var entity = other.GetComponent<IEntity>();

        if (entity != null && entity.IsEnemy)
        {
            entity.TakeDamage(_damage);
        }
    }
}

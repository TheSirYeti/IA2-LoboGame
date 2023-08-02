using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BombarderBbombs : MonoBehaviour
{
    [SerializeField] ParticleSystem _ps;
    [SerializeField] float _lifetime;
    [SerializeField] bool _collided;
    [SerializeField] float _damage;
    

    private void Update()
    {
        if (_collided)
        {
            _lifetime -= Time.deltaTime;
            if (_lifetime < 0)
                Destroy(gameObject);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponent<BaseEnemy>();
        if (enemy)
        {
            enemy.TakeDamage(_damage);
            _ps.Play();
            _collided = true;
        }
        
        //if(other.gameObject.layer == 9)
        //{
        //    _ps.Play();
        //    GetComponent<Rigidbody>().isKinematic = true;
        //    _collided = true;
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 9)
        {
            _ps.Play();
            //IA2-LINQ
            var enemies = Physics.OverlapSphere(transform.position, 5f)
                .Select(x => x.gameObject.GetComponent<IEntity>())
                .Where(x => x != null && x.IsEnemy)
                .ToList();

            if (enemies.Count() >= 5)
            {
                enemies = enemies.OrderBy(x => Vector3.Distance(x.Position, transform.position)).Take(5).ToList();
            }
                
            foreach (var enemy in enemies)
            {
                enemy.TakeDamage(_damage);
            }
            
            _collided = true;
        }
    }
}

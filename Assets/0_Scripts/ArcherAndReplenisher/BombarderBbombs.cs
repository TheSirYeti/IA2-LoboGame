using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombarderBbombs : MonoBehaviour
{
    [SerializeField] ParticleSystem _ps;
    [SerializeField] float _lifetime;
    [SerializeField] bool _collided;


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
            
            _collided = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrows : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField] float _arrowSpeed;
    [SerializeField] Vector3 _dir;
    float dirX, dirY, dirZ;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        dirX = Random.Range(-.4f, .4f);
        dirY = Random.Range(.5f, 1f);
        dirZ = Random.Range(1f, 3f);
        _dir = new Vector3(dirX, dirY, dirZ);
        transform.forward = _dir;
        _rb.AddForce(_dir * _arrowSpeed, ForceMode.Acceleration);
    }
}

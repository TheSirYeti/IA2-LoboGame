using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BombLogic : MonoBehaviour
{
    public Vector3 target;
    
    [SerializeField] private float _speed;
    [SerializeField] private float _arcHeight;
    [SerializeField] private float _stepScale;
    
    Vector3 _startPosition;
    float _progress;

    void Start() {
        _startPosition = transform.position;
        float distance = Vector3.Distance(_startPosition, target);
        
        _stepScale = _speed / distance;
    }

    void Update() {
        _progress = Mathf.Min(_progress + Time.deltaTime * _stepScale, 1.0f);
        float parabola = 1.0f - 4.0f * (_progress - 0.5f) * (_progress - 0.5f);
        Vector3 nextPos = Vector3.Lerp(_startPosition, target, _progress);
        nextPos.y += parabola * _arcHeight;
        
        transform.LookAt(nextPos, transform.forward);
        transform.position = nextPos;
        
        if (_progress == 1.0f) ;
            //Cosas
    }
}

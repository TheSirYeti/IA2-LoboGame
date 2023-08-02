using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class BombLogic : MonoBehaviour
{
    public Vector3 target;
    
    [SerializeField] private float _speed;
    [SerializeField] private float _damage;
    [SerializeField] private float _arcHeight;
    [SerializeField] private float _stepScale;
    
    Vector3 _startPosition;
    float _progress;

    [SerializeField] private Queries query;

    void Start()
    {
        query.targetGrid = SpatialGrid.instance;
        
        _startPosition = transform.position;
        float distance = Vector3.Distance(_startPosition, target);
        
        _stepScale = _speed / distance;
    }

    void Update()
    {
        _progress = Mathf.Min(_progress + Time.deltaTime * _stepScale, 1.0f);
        float parabola = 1.0f - 4.0f * (_progress - 0.5f) * (_progress - 0.5f);
        Vector3 nextPos = Vector3.Lerp(_startPosition, target, _progress);
        nextPos.y += parabola * _arcHeight;
        
        transform.LookAt(nextPos, transform.forward);
        transform.position = nextPos;
        
        //IA2-LINQ-GRID
        if (_progress >= 1.0f)
        {
            var entities = query.Query().Where(x => !x.IsEnemy);
            Debug.Log(entities.Count());

            if (entities.Any())
            {
                foreach (var ent in entities)
                {
                    ent.TakeDamage(_damage);
                }
            }

            Destroy(gameObject);
        }
    }
}

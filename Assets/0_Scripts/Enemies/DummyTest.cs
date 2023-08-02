using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;

public class DummyTest : MonoBehaviour, IEntity
{
    [SerializeField] private float _hp = 15f;
    private float _maxHP;
    [SerializeField] private ParticleSystem tears;
    [SerializeField] private float minDistanceToEnemy;
    public enum DummyInputs { IDLE, SCARED, DIE }
    private EventFSM<DummyInputs> _fsm;

    private void Awake()
    {
        _maxHP = _hp;
        SetupFSM();
    }

    private void Start()
    {
        Village.instance.AddVillager(this);
    }

    void SetupFSM()
    {
        var idle = new State<DummyInputs>("IDLE");
        var scared = new State<DummyInputs>("SCARED");
        var die = new State<DummyInputs>("DIE");
        
        StateConfigurer.Create(idle)
            .SetTransition(DummyInputs.SCARED, scared)
            .SetTransition(DummyInputs.DIE, die)
            .Done();
        
        StateConfigurer.Create(scared)
            .SetTransition(DummyInputs.IDLE, idle)
            .SetTransition(DummyInputs.DIE, die)
            .Done();
        
        StateConfigurer.Create(die)
            .Done();

        idle.OnUpdate += () =>
        {
            if (_hp <= 0)
            {
                _fsm.SendInput(DummyInputs.DIE);
                return;
            }

            var nearestEnemy = EnemyManager.instance.GetClosestEnemy(transform.position);
            if (nearestEnemy != null && Vector3.Distance(transform.position, nearestEnemy.transform.position) <=
                minDistanceToEnemy)
            {
                _fsm.SendInput(DummyInputs.SCARED);
            }

            _hp += Time.deltaTime;

            if (_hp >= _maxHP)
                _hp = _maxHP;
        };

        scared.OnEnter += x =>
        {
            tears.Play();
        };

        scared.OnUpdate += () =>
        {
            var nearestEnemy = EnemyManager.instance.GetClosestEnemy(transform.position);
            if (nearestEnemy == null || Vector3.Distance(transform.position, nearestEnemy.transform.position) >=
                minDistanceToEnemy)
            {
                _fsm.SendInput(DummyInputs.IDLE);
            }
            
            if (_hp <= 0)
            {
                _fsm.SendInput(DummyInputs.DIE);
                return;
            }
        };

        scared.OnExit += x =>
        {
            tears.Stop();
        };

        die.OnEnter += x =>
        {
            Village.instance.RemoveVillager(this);
            gameObject.SetActive(false);
        };

        
        _fsm = new EventFSM<DummyInputs>(idle);
    }

    private void Update()
    {
        _fsm.Update();
    }

    #region IENTITY

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

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
        get { return false; }
    }

    public void TakeDamage(float damage)
    {
        _hp -= damage;
    }

    private bool isInGrid = false;
    public bool onGrid
    {
        get
        {
            return isInGrid;
        }
        set
        {
            isInGrid = value;
        }
    }

    public event Action<IEntity> OnMove = delegate { };

    #endregion
}

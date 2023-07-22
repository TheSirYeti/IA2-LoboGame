using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IA2;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BearEnemy : BaseEnemy
{
    [Header("Idle Properties")] 
    private IEntity _target;
    [SerializeField] private float searchRange; //TODO: hacer con Spatial Grid
    
    [Header("Move Properties")] 
    [SerializeField] private float _speed;
    
    [Header("Attack Properties")] 
    [SerializeField] private float _attackValue;
    [SerializeField] private float _attackCooldown = 1.5f;
    private float _currentAttackCooldown = 0f;
    [SerializeField] private float _minAttackRange;

    public enum BearInputs { IDLE, MOVE, ATTACK, DIE }
    private EventFSM<BearInputs> _fsm;

    private void Start()
    {
        SetupFSMStates();
    }

    void SetupFSMStates()
    {
        #region DECLARATIONS
        
        var idle = new State<BearInputs>("IDLE");
        var move = new State<BearInputs>("MOVE");
        var attack = new State<BearInputs>("ATTACK");
        var die = new State<BearInputs>("DIE");
        
        StateConfigurer.Create(idle)
            .SetTransition(BearInputs.MOVE, move)
            .SetTransition(BearInputs.ATTACK, attack)
            .SetTransition(BearInputs.DIE, die)
            .Done();
        
        StateConfigurer.Create(move)
            .SetTransition(BearInputs.IDLE, idle)
            .SetTransition(BearInputs.ATTACK, attack)
            .SetTransition(BearInputs.DIE, die)
            .Done();
        
        StateConfigurer.Create(attack)
            .SetTransition(BearInputs.IDLE, idle)
            .SetTransition(BearInputs.DIE, die)
            .Done();
        
        StateConfigurer.Create(die)
            .Done();
        
        #endregion

        #region LOGIC

        #region IDLE

        idle.OnEnter += x =>
        {
            _target = FindNearestTarget(transform.position);
        };

        idle.OnUpdate += () =>
        {
            _fsm.SendInput(BearInputs.MOVE);
        };

        #endregion

        #region MOVE

        move.OnEnter += x =>
        {
            _animator.SetFloat("movementSpeed", _speed);
        };
        
        move.OnUpdate += () =>
        {
            if (_hp <= 0)
            {
                _fsm.SendInput(BearInputs.DIE);
                return;
            }

            if (_target == null)
            {
                Debug.Log("NULLLLLL");
                _fsm.SendInput(BearInputs.IDLE);
                return;
            }

            if (Vector3.Distance(_target.Position, transform.position) <= _minAttackRange)
            {
                _fsm.SendInput(BearInputs.ATTACK);
                return;
            }

            transform.forward = _target.Position - transform.position;
            transform.forward = new Vector3(transform.forward.x, 0, transform.forward.z);

            transform.position += transform.forward * _speed * Time.deltaTime;
        };
        
        move.OnExit += x =>
        {
            _animator.SetFloat("movementSpeed", 0);
        };

        #endregion

        #region ATTACK

        attack.OnEnter += x =>
        {
            _currentAttackCooldown = 0f;
        };

        attack.OnUpdate += () =>
        {
            if (_hp <= 0)
            {
                _fsm.SendInput(BearInputs.DIE);
                return;
            }

            if (_target == null)
            {
                _fsm.SendInput(BearInputs.IDLE);
                return;
            }

            if (Vector3.Distance(_target.Position, transform.position) >= _minAttackRange)
            {
                _fsm.SendInput(BearInputs.MOVE);
                return;
            }

            _currentAttackCooldown -= Time.deltaTime;
            if (_currentAttackCooldown >= 0) return;
            
            Attack();
            _currentAttackCooldown = _attackCooldown;
        };

        attack.OnExit += x =>
        {
            _animator.Play("Movement");
            _currentAttackCooldown = 0f;
        };

        #endregion

        #region DIE

        die.OnEnter += x =>
        {
            _animator.Play("Bear_Death");
        };

        #endregion

        #endregion
        
        _fsm = new EventFSM<BearInputs>(idle);
    }

    private void Update()
    {
        _fsm.Update();
    }

    public override void Attack()
    {
        StopCoroutine(AttackCoroutine());
        StartCoroutine(AttackCoroutine());
    }

    public IEnumerator AttackCoroutine()
    {
        int rand = Random.Range(1, 5);
        _animator.Play("Bear_Attack" + rand);

        yield return new WaitForSeconds(1f);
        
        _target.TakeDamage(_attackValue);
        _target = null;
        
        yield return null;
    }

    IEntity FindNearestTarget(Vector3 position)
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
}

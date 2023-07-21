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
    private GameObject _target;
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
                _fsm.SendInput(BearInputs.IDLE);
                return;
            }

            if (Vector3.Distance(_target.transform.position, transform.position) <= _minAttackRange)
            {
                _fsm.SendInput(BearInputs.ATTACK);
                return;
            }

            transform.forward = _target.transform.position - transform.position;
            transform.forward = new Vector3(transform.forward.x, transform.position.y, transform.forward.z);

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

            if (Vector3.Distance(_target.transform.position, transform.position) >= _minAttackRange)
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
    
    public void TakeDamage(float damage)
    {
        _hp -= damage;
    }

    public override void Attack()
    {
        int rand = Random.Range(0, 4);
        _animator.Play("Bear_Attack" + rand);
    }

    GameObject FindNearestTarget(Vector3 position)
    {
        //TODO: hacer con Query

        var nearestEntity = Physics.OverlapSphere(transform.position, searchRange)
            .Select(x => x.gameObject.GetComponent<IEntity>())
            .Where(x => x != null && !x.IsEnemy)
            .OrderByDescending(x => Vector3.Distance(x.Position, transform.position))
            .FirstOrDefault();

        return nearestEntity.myGameObject;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IA2;
using UnityEditorInternal;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BearEnemy : BaseEnemy
{
    [Header("Idle Properties")] 
    private IEntity _target;

    [Header("Move Properties")] 
    [SerializeField] private float _speed;

    [Header("Attack Properties")] 
    [SerializeField] private Queries _query;
    [SerializeField] private float _attackValue;
    [SerializeField] private float _attackCooldown = 1.5f;
    private float _currentAttackCooldown = 0f;
    [SerializeField] private float _minAttackRange;

    public enum BearInputs { IDLE, MOVE, PATHFIND, ATTACK, DIE }
    private EventFSM<BearInputs> _fsm;

    private void Start()
    {
        EnemyManager.instance.spawnedEnemies.Add(this);
        SetupFSMStates();
        
        _query.targetGrid = SpatialGrid.instance;
    }

    void SetupFSMStates()
    {
        #region DECLARATIONS
        
        var idle = new State<BearInputs>("IDLE");
        var move = new State<BearInputs>("MOVE");
        var pathfind = new State<BearInputs>("PATHFIND");
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
            .SetTransition(BearInputs.PATHFIND, pathfind)
            .SetTransition(BearInputs.DIE, die)
            .Done();
        
        StateConfigurer.Create(pathfind)
            .SetTransition(BearInputs.IDLE, idle)
            .SetTransition(BearInputs.ATTACK, attack)
            .SetTransition(BearInputs.MOVE, move)
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
                _fsm.SendInput(BearInputs.IDLE);
                return;
            }

            if (!InSight(_target.Position, transform.position))
            {
                _fsm.SendInput(BearInputs.PATHFIND);
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

        #region PATHFIND

        pathfind.OnEnter += x =>
        {
            int startNodeID = NodeManager.instance.GetClosestNode(transform);
            int endNodeID = NodeManager.instance.GetClosestNode(_target.myGameObject.transform);
            
            CalculatePathfinding(NodeManager.instance.nodes[startNodeID],
                NodeManager.instance.nodes[endNodeID]);
            
            if(currentPath == null || !currentPath.Any())
                _fsm.SendInput(BearInputs.MOVE);
        };

        pathfind.OnUpdate += () =>
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

            if (InSight(_target.Position, transform.position))
            {
                _fsm.SendInput(BearInputs.MOVE);
                return;
            }
            
            if (Vector3.Distance(_target.Position, transform.position) <= _minAttackRange)
            {
                _fsm.SendInput(BearInputs.ATTACK);
                return;
            }
            
            transform.forward = currentPath[currentNode].transform.position - transform.position;
            transform.forward = new Vector3(transform.forward.x, 0, transform.forward.z);
            
            transform.position += transform.forward * _speed * Time.deltaTime;
            
            if (Vector3.Distance(transform.position, currentPath[currentNode].transform.position) <= minDistanceToNode)
            {
                currentNode++;

                if (currentNode >= currentPath.Count)
                {
                    _fsm.SendInput(BearInputs.MOVE);
                }
            }
        };

        pathfind.OnExit += x =>
        {
            currentNode = 0;
            currentPath = new List<Node>();
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

            if (_target == null || _target.myGameObject == null)
            {
                _fsm.SendInput(BearInputs.IDLE);
                return;
            }
            
            if (!InSight(_target.Position, transform.position))
            {
                _fsm.SendInput(BearInputs.PATHFIND);
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
            
            EnemyManager.instance.RemoveEnemy(this);
            EnemyManager.instance.spawnedEnemies.Remove(this);
            Destroy(gameObject, 5f);
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

        //IA2-GRID
        var villagers = _query.Query().Where(x => !x.IsEnemy).ToList();

        if (villagers.Count() != 0)
        {
            foreach (var villager in villagers)
            {
                villager.TakeDamage(_attackValue);
            }
        }

        _fsm.SendInput(BearInputs.IDLE);
        yield return null;
    }
}

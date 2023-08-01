using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IA2;
using UnityEngine.Serialization;

public class BombEnemy : BaseEnemy
{
    [Header("Idle Properties")] 
    private IEntity _target;

    [Header("Move Properties")] 
    [SerializeField] private float _speed;
    
    [Header("Attack Properties")] 
    [SerializeField] private float _attackValue;
    [SerializeField] private float _attackCooldown = 1.5f;
    private float _currentAttackCooldown = 0f;
    [SerializeField] private float _minAttackRange;
    [SerializeField] private GameObject _bombPrefab;

    public enum BombInputs { IDLE, MOVE, PATHFIND, PREPARE_LAUNCH, SHOOT, DIE }
    private EventFSM<BombInputs> _fsm;

    private void Start()
    {
        SetupFSMStates();
    }

    void SetupFSMStates()
    {
        #region DECLARATIONS
        
        var idle = new State<BombInputs>("IDLE");
        var move = new State<BombInputs>("MOVE");
        var pathfind = new State<BombInputs>("PATHFIND");
        var launch = new State<BombInputs>("PREPARE_LAUNCH");
        var shoot = new State<BombInputs>("SHOOT");
        var die = new State<BombInputs>("DIE");
        
        StateConfigurer.Create(idle)
            .SetTransition(BombInputs.MOVE, move)
            .SetTransition(BombInputs.PREPARE_LAUNCH, launch)
            .SetTransition(BombInputs.DIE, die)
            .Done();
        
        StateConfigurer.Create(move)
            .SetTransition(BombInputs.IDLE, idle)
            .SetTransition(BombInputs.PREPARE_LAUNCH, launch)
            .SetTransition(BombInputs.PATHFIND, pathfind)
            .SetTransition(BombInputs.DIE, die)
            .Done();
        
        StateConfigurer.Create(pathfind)
            .SetTransition(BombInputs.IDLE, idle)
            .SetTransition(BombInputs.PREPARE_LAUNCH, launch)
            .SetTransition(BombInputs.MOVE, move)
            .SetTransition(BombInputs.DIE, die)
            .Done();

        StateConfigurer.Create(launch)
            .SetTransition(BombInputs.IDLE, idle)
            .SetTransition(BombInputs.SHOOT, shoot)
            .SetTransition(BombInputs.DIE, die)
            .Done();
        
        StateConfigurer.Create(shoot)
            .SetTransition(BombInputs.IDLE, idle)
            .SetTransition(BombInputs.PREPARE_LAUNCH, launch)
            .SetTransition(BombInputs.DIE, die)
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
            _fsm.SendInput(BombInputs.MOVE);
        };

        #endregion

        #region MOVE

        move.OnEnter += x =>
        {
            _animator.SetFloat("movementSpeed", _speed);
        };
        
        move.OnUpdate += () =>
        {
            Debug.Log("MOVE?");
            if (_hp <= 0)
            {
                _fsm.SendInput(BombInputs.DIE);
                return;
            }

            if (_target == null)
            {
                Debug.Log("NULLLLLL");
                _fsm.SendInput(BombInputs.IDLE);
                return;
            }

            if (!InSight(_target.Position, transform.position))
            {
                _fsm.SendInput(BombInputs.PATHFIND);
                return;
            }
            
            if (Vector3.Distance(_target.Position, transform.position) <= _minAttackRange)
            {
                _fsm.SendInput(BombInputs.PREPARE_LAUNCH);
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
                _fsm.SendInput(BombInputs.MOVE);
        };

        pathfind.OnUpdate += () =>
        {
            Debug.Log("PF");
            
            if (_hp <= 0)
            {
                _fsm.SendInput(BombInputs.DIE);
                return;
            }

            if (_target == null)
            {
                _fsm.SendInput(BombInputs.IDLE);
                return;
            }

            if (InSight(_target.Position, transform.position))
            {
                _fsm.SendInput(BombInputs.MOVE);
                return;
            }
            
            if (Vector3.Distance(_target.Position, transform.position) <= _minAttackRange)
            {
                _fsm.SendInput(BombInputs.PREPARE_LAUNCH);
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
                    _fsm.SendInput(BombInputs.MOVE);
                }
            }
        };

        pathfind.OnExit += x =>
        {
            currentNode = 0;
            currentPath = new List<Node>();
        };

        #endregion

        #region PREPARE_LAUNCH
        
        launch.OnEnter += x =>
        {
            _currentAttackCooldown = _attackCooldown;
        };
        
        launch.OnUpdate += () =>
        {
            if (_hp <= 0)
            {
                _fsm.SendInput(BombInputs.DIE);
                return;
            }

            if (_target == null)
            {
                _fsm.SendInput(BombInputs.IDLE);
                return;
            }
            
            if (_currentAttackCooldown >= 0) return;
            _fsm.SendInput(BombInputs.SHOOT);
            
            _currentAttackCooldown -= Time.deltaTime;
            transform.LookAt(new Vector3(_target.Position.x, transform.position.y, _target.Position.z));
        };

        #endregion
        
        #region SHOOT

        shoot.OnEnter += x =>
        {
            Attack();
        };

        shoot.OnExit += x =>
        {
            _animator.Play("Movement");
        };

        #endregion

        #region DIE

        die.OnEnter += x =>
        {
            _animator.Play("Bear_Death");
        };

        #endregion

        #endregion
        
        _fsm = new EventFSM<BombInputs>(idle);
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
        
        var bomb = Instantiate(_bombPrefab);
        bomb.transform.position = transform.position;
        bomb.GetComponent<BombLogic>().target = _target.Position;

        yield return new WaitForSeconds(2f);
        
        _fsm.SendInput(BombInputs.IDLE);
        
        yield return null;
    }
}

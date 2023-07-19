using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System.Linq;
using System;

public class Archer : MonoBehaviour
{

    public enum PlayerInputs { MOVE, IDLE, ATTACK, BOUNDARIES, DIE, TURN }
    private EventFSM<PlayerInputs> _myFsm;


    [Header("IdleState")]
    [SerializeField] float _restingTime; //time spent in idle
    [SerializeField] float _movementSpeed; //time spent in idle
    private float _restingTimeCounter; //counter

    [Header("MoveState")]
    [SerializeField] float _movingTime; //time spent in idle
    private float _movingTimeCounter; //counter
    private Vector3 _movingDir; //direction
    RaycastHit hit; //Hit for boundaries test.

    [Header("AttackState")]
    [SerializeField] float cadence; //Time it takes to shoot again
    float _timeUntilAttack;
    [SerializeField] private float _attackCd;//Cd after shooting
    private float _attackCdCounter;
    [SerializeField] GameObject _bullet;

    [SerializeField] GameObject _bulletSpawner;

    [Header("RotateSpeed")]
    [SerializeField] float _rotationSpeed;


    [Header("TurnState")]
    [SerializeField] Vector3 _turnVector;

    [Header("Boundaries")]
    [SerializeField] float _raycastRange;
    [SerializeField] LayerMask _boundariesLayers;


    private void Awake()
    {
        //_myRb = gameObject.GetComponent<Rigidbody>();
        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var idle = new State<PlayerInputs>("Idle");
        var moving = new State<PlayerInputs>("WaypointState");
        var attacking = new State<PlayerInputs>("Attack");
        var boundaries = new State<PlayerInputs>("Boundaries");
        var dying = new State<PlayerInputs>("Die");
        var turn = new State<PlayerInputs>("Turning");

        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.MOVE, moving)
            .SetTransition(PlayerInputs.TURN, turn)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(moving)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.TURN, turn)
            .SetTransition(PlayerInputs.BOUNDARIES, boundaries)
            .SetTransition(PlayerInputs.DIE, dying)
            .SetTransition(PlayerInputs.ATTACK, attacking)
            .Done();

        StateConfigurer.Create(turn)
        .SetTransition(PlayerInputs.MOVE, moving)
        .SetTransition(PlayerInputs.DIE, dying)
        .Done();


        StateConfigurer.Create(attacking)
            .SetTransition(PlayerInputs.MOVE, moving)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();


        StateConfigurer.Create(boundaries)
            .SetTransition(PlayerInputs.MOVE, moving)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(dying)
            .Done();

        idle.OnEnter += x =>
        {
            _restingTime = UnityEngine.Random.Range(1, 3);
            _restingTimeCounter = _restingTime;

        };



        idle.OnUpdate += () =>
        {
            _restingTimeCounter -= Time.deltaTime;
            if (_restingTimeCounter <= 0)
            {
                SendInputToFSM(PlayerInputs.TURN);
            }

        };

        //For turning
        turn.OnEnter += x =>
        {
            //Randomizes direction
            //Consider: Moving throu either X or Z, means that the other axis has to be 0.
            var xdir = UnityEngine.Random.Range(-1, 2);
            var zdir = UnityEngine.Random.Range(-1, 2);

            if (xdir == _movingDir.x && xdir != 0)
            {

                zdir = 0;
                SendInputToFSM(PlayerInputs.MOVE);
            }

            if (xdir != 0)
            {
                zdir = 0;
            }
            else
            {
                if (zdir == _movingDir.z && zdir != 0)
                {
                    SendInputToFSM(PlayerInputs.MOVE);
                }

                if (zdir == 0)
                    do
                    {
                        zdir = UnityEngine.Random.Range(-1, 2);
                    } while (zdir == 0);
            }

            _movingDir = new Vector3(xdir, 0, zdir);


        };

        turn.OnUpdate += () =>
        {
            transform.Rotate(_turnVector);
            Vector3 eulerAngles = transform.eulerAngles;
            if (_movingDir == transform.forward)
            {
                SendInputToFSM(PlayerInputs.MOVE);
            }
        };

        //MOVING
        moving.OnEnter += x =>
        {
            _movingTime = UnityEngine.Random.Range(1, 4);
            _movingTimeCounter = _movingTime;
        };

        //En el update manejo lo que seria la stamina para ver cuanto se puede mover
        moving.OnUpdate += () =>
        {
            _movingTimeCounter -= Time.deltaTime;

            _timeUntilAttack -= Time.deltaTime;
            //It can go to attacking
            if (_timeUntilAttack < 0)
                SendInputToFSM(PlayerInputs.ATTACK);

            transform.forward = _movingDir;

            transform.position += _movingDir * Time.deltaTime * _movementSpeed;

            //It can go to idle
            if (_movingTimeCounter <= 0)
                SendInputToFSM(PlayerInputs.IDLE);


        };

        //Boundaries
        boundaries.OnEnter += x =>
        {
            _movingDir *= -1;//Changes de direction to the other side.
        };

        //En el update manejo lo que seria la stamina para ver cuanto se puede mover
        boundaries.OnUpdate += () =>
        {
            transform.Rotate(_turnVector);
            Vector3 eulerAngles = transform.eulerAngles;
            if (_movingDir == transform.forward)
            {
                SendInputToFSM(PlayerInputs.MOVE);
            }
        };

        //Attacking
        attacking.OnEnter += x =>
        {
            //Animacion de disparo
            _attackCdCounter = _attackCd;
            EnemyAttack();
        };

        attacking.OnUpdate += () =>
        {
            _attackCdCounter -= Time.deltaTime;
            if (_attackCdCounter < 0)
                SendInputToFSM(PlayerInputs.MOVE);
        };


        attacking.OnExit += x =>
        {
            cadence = UnityEngine.Random.Range(2, 5);
            _timeUntilAttack = cadence;
        };


        //Attacking
        dying.OnEnter += x =>
        {
            Destroy(gameObject);
        };

        //Choose first state.
        _myFsm = new EventFSM<PlayerInputs>(idle);
    }




    //Armo la lista de enemigos
    public void Start()
    {

    }

    private void SendInputToFSM(PlayerInputs inp)
    {
        _myFsm.SendInput(inp);
    }

    private void Update()
    {
        _myFsm.Update();
        BoundariesFunction();

        //HealthTestingForDying

        //if (Input.GetKeyDown(KeyCode.A))
        //    LoseHp(10);
    }

    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }

    public void BoundariesFunction() //Triple checking directions.
    {
        Vector3 direction = Vector3.forward + Vector3.left * .5f;
        Vector3 direction2 = Vector3.forward + Vector3.right * .5f;
        Vector3 direction3 = Vector3.forward;

        Ray ray = new Ray(transform.position, transform.TransformDirection(direction.normalized * _raycastRange));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction.normalized * _raycastRange), Color.red);

        Ray ray2 = new Ray(transform.position, transform.TransformDirection(direction2.normalized * _raycastRange));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction2.normalized * _raycastRange), Color.red);

        Ray ray3 = new Ray(transform.position, transform.TransformDirection(direction3.normalized * _raycastRange));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction3.normalized * _raycastRange), Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, _raycastRange))
        {
            if (hit.transform.gameObject.layer == 6 || hit.transform.gameObject.layer == 7
                || hit.transform.gameObject.layer == 8)//Enemies or Obstacles layer
            {
                SendInputToFSM(PlayerInputs.BOUNDARIES);
            }
        }
        else if (Physics.Raycast(ray2, out RaycastHit hit2, _raycastRange))
        {
            if (hit2.transform.gameObject.layer == 6 || hit2.transform.gameObject.layer == 7
                || hit2.transform.gameObject.layer == 8)//Enemies Obstacles or boundaries.
            {
                SendInputToFSM(PlayerInputs.BOUNDARIES);
            }
        }
        else if (Physics.Raycast(ray3, out RaycastHit hit3, _raycastRange))
        {
            if (hit3.transform.gameObject.layer == 6 || hit3.transform.gameObject.layer == 7
                || hit3.transform.gameObject.layer == 8)//Enemies or Obstacles layer
            {
                SendInputToFSM(PlayerInputs.BOUNDARIES);
            }
        }

    }

    //Whenever this func is called it will attack
    public void EnemyAttack()
    {
        if (!_bulletSpawner)
            return;

        var instantiateBullet = Instantiate(_bullet, _bulletSpawner.transform.position, transform.rotation);
    }

}

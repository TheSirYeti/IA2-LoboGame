using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System.Linq;
using System;

public class Bombarder : MonoBehaviour
{

    public enum PlayerInputs { DESTRUCTION, MOVE, ATTACK }
    private EventFSM<PlayerInputs> _myFsm;


    [Header("MovingState")]
    public List<Transform> allWaypoints = new List<Transform>(); //Lista de waypoints en los cuales se va a mover la IA
    public List<Tuple<Waypoint, bool>> safeWaypoints = new List<Tuple<Waypoint, bool>>();

    [SerializeField] int _currentWaypoint;
    [SerializeField] float _waypointSpeed;

    public WaypointSafety wpSafety;

    [Header("AttackState")]

    //Listas de flechas.
    public List<BombarderBbombs> _bombs = new List<BombarderBbombs>();//Lista de las bombas que va a tirar.
    [SerializeField] private int _bombsAmount;
    [SerializeField] BombarderBbombs _bombPrefab;

    //Posicion del spawner de bombas
    [SerializeField] GameObject _bombSpawner;


    [Header("DestructionState")]
    [SerializeField] BaseEnemy _target;

    private void Awake()
    {
        //_myRb = gameObject.GetComponent<Rigidbody>();
        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var destruct = new State<PlayerInputs>("Destruction");
        var attacking = new State<PlayerInputs>("Attack");
        var moving = new State<PlayerInputs>("Move");

        StateConfigurer.Create(destruct)
            .Done();


        StateConfigurer.Create(moving)
            .SetTransition(PlayerInputs.ATTACK, attacking)
            .Done();


        StateConfigurer.Create(attacking)
            .SetTransition(PlayerInputs.DESTRUCTION, destruct)
            .SetTransition(PlayerInputs.MOVE, moving)
            .Done();



        moving.OnEnter += x =>
        {

        };

        moving.OnUpdate += () =>
        {
            //Esto es el comportamiento de waypoints
            Vector3 dir = safeWaypoints[_currentWaypoint].Item1.transform.position - transform.position;
            transform.forward = dir;
            transform.position += transform.forward * _waypointSpeed * Time.deltaTime;

            if (dir.magnitude < 0.15f)
            {
                _currentWaypoint++;
                if (_currentWaypoint > allWaypoints.Count - 1)
                {
                    _currentWaypoint = 0;
                    SendInputToFSM(PlayerInputs.ATTACK);

                }
                SendInputToFSM(PlayerInputs.ATTACK);
            }

        };


        //Estado de recarga: El arquero no tiene mas flechas por lo tanto
        //se encarga de recargar desde _reloadingArrows.

        //Attacking
        attacking.OnEnter += x =>
        {
            Shoot();
        };

        attacking.OnUpdate += () =>
        {


        };

        attacking.OnExit += x =>
        {

        };

        destruct.OnEnter += x =>
        {
            _target = TargetEnemy();
        };

        destruct.OnUpdate += () =>
        {
            Vector3 dir = _target.transform.position - transform.position;
            transform.forward = dir;
            transform.position += transform.forward * _waypointSpeed * Time.deltaTime;

            if (dir.magnitude < 0.15f)
                Destroy(gameObject);
        };

        destruct.OnExit += x =>
        {

        };

        //Choose first state.
        _myFsm = new EventFSM<PlayerInputs>(moving);
    }


    private void SendInputToFSM(PlayerInputs inp)
    {
        _myFsm.SendInput(inp);
    }

    private void Start()
    {

        //Creo la lista de safe waypoints
        safeWaypoints = SafeWaypoint(wpSafety).ToList();

        _bombs = BombLoading(_bombPrefab, _bombsAmount).ToList();

        //Way
        allWaypoints = WpCreator(safeWaypoints).ToList();

    }

    private void Update()
    {
        _myFsm.Update();
    }

    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }


    //Crea la lista de waypoints de los safe waypoints
    IEnumerable<Transform> WpCreator(List<Tuple<Waypoint, bool>> tuple)
    {
        var myCol = tuple.Select(x => x.Item1.transform);
        return myCol;
    }


    //Esta funcion toma tantas flechas como pueda de el arrowholder
    //IEnumerable<Arrows> ArrowsCounter(List<Arrows> arrows)
    //{
    //    //var myCol = arrows.Take(_arrowsAmount);
    //    //return myCol;
    //}

    //Esta funcion hace que cada vez que dispare pierda flechas
    IEnumerable<BombarderBbombs> DecreasingAmmo(List<BombarderBbombs> arrows, int ammo)
    {
        var myCol = arrows.Skip(ammo);
        return myCol;
    }

    public void Shoot()
    {
        if (!_bombs.Any())
            SendInputToFSM(PlayerInputs.DESTRUCTION);
        else
        {
            var instantiateBullet = Instantiate(_bombs.FirstOrDefault(), _bombSpawner.transform.position, transform.rotation);
            _bombs = DecreasingAmmo(_bombs, 1).ToList(); //Cuando disparo, baja el ammo de la lista.
            SendInputToFSM(PlayerInputs.MOVE);
        }
    }

    //Generator//
    public IEnumerable<BombarderBbombs> BombLoading(BombarderBbombs bombs, int ammount)
    {
        for (int i = 0; i < ammount; i++)
        {
                yield return bombs;
        }
    }


    IEnumerable<Tuple<Waypoint, bool>> SafeWaypoint(WaypointSafety wpList)
    {
        //Obtengo una lista de waypoints y ahi es donde creo la tupla booleanos / waypoints, de aca saco los waypoints safe
        //osea por los que me quiero mover (los que tienen is safe)
        var myCol = wpList.waypoints.Zip(wpList.canEnter, (booleans, waypoints) => Tuple.Create(booleans, waypoints))
              .Where(x => x.Item2);

        return myCol;
    }



    BaseEnemy TargetEnemy()
    {
        var sphere = Physics.OverlapSphere(transform.position, 30, 11); //Una esfera de colisiones

        var colTest = sphere.Select(x => x.GetComponent<BaseEnemy>());

        var myCol = colTest.Aggregate(new List<Tuple<Vector3, float, BaseEnemy>>(), (acum, current) =>
        {
            var dir = current.gameObject.transform.position - transform.position;
            var tuple = Tuple.Create(dir, dir.magnitude, current);
            acum.Add(tuple);

            return acum;

        }).OrderBy(x => x.Item2);

        return  myCol.FirstOrDefault().Item3;
    }

}

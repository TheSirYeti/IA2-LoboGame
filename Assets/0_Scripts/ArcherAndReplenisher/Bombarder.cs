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
    [SerializeField] float _damage;
    [SerializeField] BaseEnemy _target;
    [SerializeField] float _destructionSpeed;
    [SerializeField] ParticleSystem _explosionParticles;
    [SerializeField] GameObject _mesh;
    [SerializeField] float _destroyCounter;

    //[SerializeField] List<BaseEnemy> test = new List<BaseEnemy>();//Esta lista es para ver si el generator agarra bien

    private void Awake()
    {
        wpSafety = FindObjectOfType<WaypointSafety>();
        if(wpSafety == null) Destroy(gameObject);

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
            _target = TargetEnemy(PossibleTargets().ToList());

            if(_target == null || _target.gameObject == null)
                Destroy(gameObject);
            
            //test = PossibleTargets().ToList();
            _destroyCounter = 3;
        };

        destruct.OnUpdate += () =>
        {
            if(_target == null || _target.gameObject == null)
                Destroy(gameObject);
            
            Vector3 dir = _target.transform.position - transform.position;
            transform.forward = dir;
            transform.position += transform.forward * _destructionSpeed * Time.deltaTime;

            if (dir.magnitude < 0.15f)
            {
                var enemies = Physics.OverlapSphere(transform.position, 10f)
                    .Select(x => x.gameObject.GetComponent<IEntity>())
                    .Where(x => x != null && x.IsEnemy)
                    .ToList();

                if (enemies.Count() >= 5)
                {
                    enemies = enemies.OrderBy(x => Vector3.Distance(x.Position, transform.position)).Take(5).ToList();
                }
                
                foreach (var enemy in enemies)
                {
                    enemy.TakeDamage(_damage);
                }

                _destructionSpeed = 0;
                _mesh.SetActive(false);
                _explosionParticles.Play();
                _destroyCounter -= Time.deltaTime;
                
            }

            if (_destroyCounter <= 0)
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

        //Carga las bombas
        _bombs = BombLoading(_bombPrefab, _bombsAmount).ToList();

        //Wps
        allWaypoints = WpCreator(safeWaypoints).ToList();
    }

    private void Update()
    {
        _myFsm.Update();
        
    }

    private void OnDrawGizmos() //Esfera de colision
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 30);
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


    //Saca bombas
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

    //Generator lista de enemigos? masterclass?
    public IEnumerable<BaseEnemy> PossibleTargets()
    {
        var sphere = Physics.OverlapSphere(transform.position, 30, 11); //Una esfera de colisiones

        for (int i = 0; i < sphere.Length-1; i++)
        {
            var enemy = sphere[i].GetComponent<BaseEnemy>();
            if (enemy != null) //Si el nodo no es si mismo, lo agrega al a lista de vecinos
                yield return enemy;
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



    BaseEnemy TargetEnemy(List<BaseEnemy> possibleTargets)
    {
        var myCol = possibleTargets.Aggregate(new List<Tuple<Vector3, float, BaseEnemy>>(), (acum, current) =>
        {
            var dir = current.gameObject.transform.position - transform.position;
            var tuple = Tuple.Create(dir, dir.magnitude, current);
            acum.Add(tuple);

            return acum;

        }).OrderBy(x => x.Item2);

        return  myCol.FirstOrDefault().Item3;
    }

}

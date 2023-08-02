using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System.Linq;
using System;

public class Archer : MonoBehaviour
{

    public enum PlayerInputs { RELOAD, IDLE, ATTACK, DIE, PANIC }
    private EventFSM<PlayerInputs> _myFsm;

    [SerializeField] Animator _anim;

    [Header("IdleState")]
    [SerializeField] float _restingTime; //time spent in idle
    private float _restingTimeCounter; //counter

    [Header("ReloadState")]
    [SerializeField] private float _reloadCd;//Cd after shooting
    private float _reloadCounter;
    [SerializeField] GameObject _bow, _reloadQuiver, _quiver;

    [Header("AttackState")]

    [SerializeField] BaseEnemy _target;

    //Listas de flechas.
    public List<Arrows> _arrows = new List<Arrows>();//Lista de las flechas.
    [SerializeField] private int _arrowsAmount;
    public List<Arrows> _reloadingArrows = new List<Arrows>();//Lista de las flechas de recarga.
    public List<Arrows> _auxArrowsList = new List<Arrows>();//Lista del refiller momentanea

    //Posicion del spawner de flechas
    [SerializeField] GameObject _arrowsSpawner;

    [SerializeField] private float _attackCd;//Cd after shooting
    private float _attackCdCounter;



    public bool isPanic;


    private void Awake()
    {
        //_myRb = gameObject.GetComponent<Rigidbody>();
        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var idle = new State<PlayerInputs>("Idle");
        var attacking = new State<PlayerInputs>("Attack");
        var dying = new State<PlayerInputs>("Die");
        var reloading = new State<PlayerInputs>("Reload");
        var panic = new State<PlayerInputs>("Panic");

        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.ATTACK, attacking)
            .SetTransition(PlayerInputs.DIE, dying)
            .SetTransition(PlayerInputs.IDLE, idle)
            .Done();


        StateConfigurer.Create(reloading)
            .SetTransition(PlayerInputs.DIE, dying)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.PANIC, panic)
            .SetTransition(PlayerInputs.ATTACK, attacking)
            .Done();


        StateConfigurer.Create(attacking)
            .SetTransition(PlayerInputs.RELOAD, reloading)
            .SetTransition(PlayerInputs.ATTACK, attacking)
            .SetTransition(PlayerInputs.PANIC, panic)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();


        StateConfigurer.Create(panic)
            .SetTransition(PlayerInputs.RELOAD, reloading)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(dying)
            .Done();



        idle.OnEnter += x =>
        {
            _anim.Play("Idle");
            _restingTimeCounter = _restingTime;
            if (EnemyManager.instance.spawnedEnemies.Count==0)
                _target = null;
            else _target = TargetEnemy(EnemyManager.instance.spawnedEnemies);
            
        };

        idle.OnUpdate += () =>
        {
            _restingTimeCounter -= Time.deltaTime;
            if (_restingTimeCounter <= 0)
            {
                if(_target != null)
                    SendInputToFSM(PlayerInputs.ATTACK);
                else
                {
                    SendInputToFSM(PlayerInputs.IDLE);
                }
            }

        };


        //Estado de recarga: El arquero no tiene mas flechas por lo tanto
        //se encarga de recargar desde _reloadingArrows.
        reloading.OnEnter += x =>
        {
            _anim.Play("Reload");
            _reloadCounter = _reloadCd;
            _bow.SetActive(false);
            _reloadQuiver.SetActive(true);
            _quiver.SetActive(false);
        };

        reloading.OnUpdate += () =>
        {
            _reloadCounter -= Time.deltaTime;

            if (_reloadCounter < 0)
            {
                if (_reloadingArrows.Any())
                {
                    _arrows = ArrowsCounter(_reloadingArrows).ToList();
                    Debug.Log(_reloadingArrows.Count());
                    _reloadingArrows = DecreasingAmmo(_reloadingArrows, 5).ToList();
                    SendInputToFSM(PlayerInputs.IDLE);
                }
                else SendInputToFSM(PlayerInputs.PANIC);
            }

        };

        reloading.OnExit += x =>
        {
            _bow.SetActive(true);
            _reloadQuiver.SetActive(false);
            _quiver.SetActive(true);
        };

        //Attacking
        attacking.OnEnter += x =>
        {
            if (_target == null || _target.gameObject == null)
            {
                SendInputToFSM(PlayerInputs.IDLE);
                return;
            }
                //Debug.Log("entre a attack");
            Vector3 arrowDir = _target.transform.position - _arrowsSpawner.transform.position;
            _arrowsSpawner.transform.forward = arrowDir;
            transform.forward = arrowDir +  new Vector3(0, 3, 0);
            //Cada vez que ataco, elimino las flechas
            if (!_arrows.Any())
                SendInputToFSM(PlayerInputs.RELOAD);

            _attackCdCounter = _attackCd;
            _anim.Play("Fire");

        };


        panic.OnEnter += x =>
        {
            _anim.Play("Panic");
            isPanic = true;
        };

        panic.OnUpdate += () =>
        {
            if (!isPanic)
            {
                SendInputToFSM(PlayerInputs.RELOAD);
            }
        };



        dying.OnEnter += x =>
        {
            Destroy(gameObject);
        };

        //Choose first state.
        _myFsm = new EventFSM<PlayerInputs>(reloading);
    }


    private void SendInputToFSM(PlayerInputs inp)
    {
        _myFsm.SendInput(inp);
    }

    private void Update()
    {
        _myFsm.Update();
        //if (Input.GetKeyDown(KeyCode.A))
        //    _reloadingArrows = ArrowsRefilling(_arrows).ToList();
    }

    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }




    //Esta funcion toma tantas flechas como pueda de el arrowholder
    IEnumerable<Arrows> ArrowsCounter(List<Arrows> arrows)
    {
        var myCol = arrows.Take(_arrowsAmount);
        return myCol;
    }

    //Esta funcion hace que cada vez que dispare pierda flechas
    IEnumerable<Arrows> DecreasingAmmo(List<Arrows> arrows, int ammo)
    {
        var myCol = arrows.Skip(ammo);
        return myCol;
    }

    public void Shoot()
    {
        if (!_arrows.Any())
            SendInputToFSM(PlayerInputs.RELOAD);
        else
        {
            var instantiateBullet = Instantiate(_arrows.FirstOrDefault(), _arrowsSpawner.transform.position, _arrowsSpawner.transform.rotation);
            _arrows = DecreasingAmmo(_arrows, 1).ToList(); //Cuando disparo, baja el ammo de la lista.

            SendInputToFSM(PlayerInputs.ATTACK);
        }
    }

    public void TestReload()
    {
        if (_reloadingArrows.Any())
        {
            _arrows = ArrowsCounter(_reloadingArrows).ToList();
            Debug.Log(_reloadingArrows.Count());
            _reloadingArrows = DecreasingAmmo(_reloadingArrows, 5).ToList();
            SendInputToFSM(PlayerInputs.IDLE);
        }
        else SendInputToFSM(PlayerInputs.PANIC);
    }

    // Final IA-2 - Aggregate - First Or Default - Order By - //
    BaseEnemy TargetEnemy(List<BaseEnemy> possibleTargets)
    {
        var myCol = possibleTargets.Aggregate(new List<Tuple<Vector3, float, BaseEnemy>>(), (acum, current) =>
        {
            var dir = current.gameObject.transform.position - transform.position;
            var tuple = Tuple.Create(dir, dir.magnitude, current);
            acum.Add(tuple);

            return acum;

        }).OrderBy(x => x.Item2);

        return myCol.FirstOrDefault().Item3;
    }
}

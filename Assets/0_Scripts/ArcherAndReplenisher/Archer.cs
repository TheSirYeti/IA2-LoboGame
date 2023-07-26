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


    [Header("IdleState")]
    [SerializeField] float _restingTime; //time spent in idle
    private float _restingTimeCounter; //counter

    [Header("AttackState")]

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
            .Done();


        StateConfigurer.Create(reloading)
            .SetTransition(PlayerInputs.DIE, dying)
            .SetTransition(PlayerInputs.PANIC, panic)
            .SetTransition(PlayerInputs.IDLE, idle)
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
            _restingTime = UnityEngine.Random.Range(1, 3);
            _restingTimeCounter = _restingTime;
            Debug.Log("entre a idle");
        };

        idle.OnUpdate += () =>
        {
            _restingTimeCounter -= Time.deltaTime;
            if (_restingTimeCounter <= 0)
            {
                SendInputToFSM(PlayerInputs.ATTACK);
            }

        };


        //Estado de recarga: El arquero no tiene mas flechas por lo tanto
        //se encarga de recargar desde _reloadingArrows.
        reloading.OnEnter += x =>
        {
            Debug.Log("entre a reload");
            //Cuando recargo vuelvo a generar mi lista con flechas y ammo.
            if (_reloadingArrows.Any())
            {
                _arrows = ArrowsCounter(_reloadingArrows).ToList();
                Debug.Log(_reloadingArrows.Count());
                _reloadingArrows = DecreasingAmmo(_reloadingArrows, 5).ToList();
                SendInputToFSM(PlayerInputs.IDLE);
            }
            else SendInputToFSM(PlayerInputs.PANIC);


        };

        reloading.OnExit += x =>
        {
        };

        //Attacking
        attacking.OnEnter += x =>
        {
            Debug.Log("entre a attack");
            //Cada vez que ataco, elimino las flechas
            if (!_arrows.Any())
                SendInputToFSM(PlayerInputs.RELOAD);

            _attackCdCounter = _attackCd;
            Shooting();

        };

        attacking.OnUpdate += () =>
        {
            _attackCdCounter -= Time.deltaTime;
            if (_attackCdCounter < 0)
            {
                if (!_arrows.Any())
                    SendInputToFSM(PlayerInputs.RELOAD);
                else
                    SendInputToFSM(PlayerInputs.ATTACK);
            }


        };

        attacking.OnExit += x =>
        {

        };

        panic.OnEnter += x =>
        {
            isPanic = true;
        };

        panic.OnUpdate += () =>
        {
            if (!isPanic)
            {
                _reloadingArrows = ArrowsRefilling(_auxArrowsList).ToList();
                SendInputToFSM(PlayerInputs.RELOAD);
            }
        };

        panic.OnExit += x =>
        {

        };


        dying.OnEnter += x =>
        {
            Destroy(gameObject);
        };

        //Choose first state.
        _myFsm = new EventFSM<PlayerInputs>(idle);
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

    //Shooting
    public void Shooting()
    {
        var instantiateBullet = Instantiate(_arrows.FirstOrDefault(), _arrowsSpawner.transform.position, transform.rotation);
        _arrows = DecreasingAmmo(_arrows, 1).ToList(); //Cuando disparo, baja el ammo de la lista.
    }


    //Refill flechas esta funcion la tengo que pasar al que refillea las flechas justamente

    public IEnumerable<Arrows> ArrowsRefilling(List<Arrows> arrows)
    {
        var myCol = arrows.Aggregate(new List<Arrows>(), (acum, current) =>
        {
            acum.Add(current);
            return acum;
        });
        return myCol;
    }

}

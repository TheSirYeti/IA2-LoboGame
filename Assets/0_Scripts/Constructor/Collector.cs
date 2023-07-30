using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System.Linq;
using System;

public class Collector : MonoBehaviour
{

    public enum PlayerInputs { LOOT, RUN, IDLE, RUN_WITH_STONE, RUN_WITH_WOOD, PUT_DOWN_RESOURCE, DIE }
    private EventFSM<PlayerInputs> _myFsm;

    [SerializeField] Animator _anim;

    [Header("Physics")]
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody rb;

    [Header("IdleState")]
    [SerializeField] float _restingTime; //time spent in idle
    private float _restingTimeCounter; //counter


    [SerializeField] private float _attackCd;//Cd after shooting
    private float _attackCdCounter;



    public bool isPanic;


    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var idle = new State<PlayerInputs>("Idle");
        var run = new State<PlayerInputs>("Run");
        var dying = new State<PlayerInputs>("Die");
        var looting = new State<PlayerInputs>("Looting");
        var runWithStone = new State<PlayerInputs>("RunStone");
        var runWithWood = new State<PlayerInputs>("RunWood");
        var putDownResource = new State<PlayerInputs>("PutDownResource");

        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.RUN, run)
            .Done();


        StateConfigurer.Create(run)
            .SetTransition(PlayerInputs.DIE, dying)
            .SetTransition(PlayerInputs.LOOT, looting)
            .Done();

        StateConfigurer.Create(looting)
            .SetTransition(PlayerInputs.RUN_WITH_STONE, runWithStone)
            .SetTransition(PlayerInputs.RUN_WITH_WOOD, runWithWood)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();


        StateConfigurer.Create(runWithStone)
            .SetTransition(PlayerInputs.PUT_DOWN_RESOURCE, putDownResource)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();


        StateConfigurer.Create(runWithWood)
            .SetTransition(PlayerInputs.PUT_DOWN_RESOURCE, putDownResource)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(runWithWood)
            .SetTransition(PlayerInputs.PUT_DOWN_RESOURCE, putDownResource)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(putDownResource)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(dying)
            .Done();



        idle.OnEnter += x =>
        {
            //_anim.Play("Idle");
            //_restingTimeCounter = _restingTime;
            //Debug.Log("entre a idle");
        };

        idle.OnUpdate += () =>
        {
            //_restingTimeCounter -= Time.deltaTime;
            //if (_restingTimeCounter <= 0)
            //{
            //    SendInputToFSM(PlayerInputs.ATTACK);
            //}

        };


        //Estado de recarga: El arquero no tiene mas flechas por lo tanto
        //se encarga de recargar desde _reloadingArrows.
        run.OnEnter += x =>
        {
            //_anim.Play("Reload");
            //_reloadCounter = _reloadCd;
            //_bow.SetActive(false);
            //_reloadQuiver.SetActive(true);
            //_quiver.SetActive(false);
        };

        run.OnUpdate += () =>
        {
            //_reloadCounter -= Time.deltaTime;

            //if (_reloadCounter < 0)
            //{
            //    if (_reloadingArrows.Any())
            //    {
            //        _arrows = ArrowsCounter(_reloadingArrows).ToList();
            //        Debug.Log(_reloadingArrows.Count());
            //        _reloadingArrows = DecreasingAmmo(_reloadingArrows, 5).ToList();
            //        SendInputToFSM(PlayerInputs.ATTACK);
            //    }
            //    else SendInputToFSM(PlayerInputs.PANIC);
            //}

        };

        run.OnExit += x =>
        {
            //_bow.SetActive(true);
            //_reloadQuiver.SetActive(false);
            //_quiver.SetActive(true);
        };

        //Attacking
        looting.OnEnter += x =>
        {
            Debug.Log("entre a attack");

            //Cada vez que ataco, elimino las flechas
            //if (!_arrows.Any())
            //    SendInputToFSM(PlayerInputs.RELOAD);

            //_attackCdCounter = _attackCd;
            //_anim.Play("Fire");

        };

        looting.OnUpdate += () =>
        {


        };

        looting.OnExit += x =>
        {

        };

        runWithStone.OnEnter += x =>
        {
            _anim.Play("Panic");
            isPanic = true;
        };

        runWithStone.OnUpdate += () =>
        {
            //if (!isPanic)
            //{
            //SendInputToFSM(PlayerInputs.RELOAD);
            //}
        };

        runWithStone.OnExit += x =>
        {

        };


        dying.OnEnter += x =>
        {
            //Destroy(gameObject);
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




    ////Esta funcion toma tantas flechas como pueda de el arrowholder
    //IEnumerable<Arrows> ArrowsCounter(List<Arrows> arrows)
    //{
    //    var myCol = arrows.Take(_arrowsAmount);
    //    return myCol;
    //}

    ////Esta funcion hace que cada vez que dispare pierda flechas
    //IEnumerable<Arrows> DecreasingAmmo(List<Arrows> arrows, int ammo)
    //{
    //    var myCol = arrows.Skip(ammo);
    //    return myCol;
    //}

    public void Shoot()
    {
        //if (!_arrows.Any())
        //SendInputToFSM(PlayerInputs.RELOAD);
        //else
        //{
        //var instantiateBullet = Instantiate(_arrows.FirstOrDefault(), _arrowsSpawner.transform.position, transform.rotation);
        //_arrows = DecreasingAmmo(_arrows, 1).ToList(); //Cuando disparo, baja el ammo de la lista.

        //SendInputToFSM(PlayerInputs.ATTACK);
        //}
    }

    public void TestReload()
    {
        //if (_reloadingArrows.Any())
        //{
        //_arrows = ArrowsCounter(_reloadingArrows).ToList();
        //Debug.Log(_reloadingArrows.Count());
        //_reloadingArrows = DecreasingAmmo(_reloadingArrows, 5).ToList();
        //SendInputToFSM(PlayerInputs.IDLE);
        //}
        //else SendInputToFSM(PlayerInputs.PANIC);
    }
}

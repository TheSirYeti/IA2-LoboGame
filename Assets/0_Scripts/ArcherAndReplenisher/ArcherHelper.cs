using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System.Linq;
using System;

public class ArcherHelper : MonoBehaviour
{

    public enum PlayerInputs { REFILL, CRAFT, DELIVER, IDLE, DIE }
    private EventFSM<PlayerInputs> _myFsm;

    [SerializeField] Archer _myArcher;
    [SerializeField] Animator _anim;

    [SerializeField] GameObject _reloadQuiver, _craftQuiver, _hammer;


    [Header("DeliverState")]
    [SerializeField] Transform _craftPos;
    [SerializeField] Transform _deliverPos;
    [SerializeField] bool _isDelivering;
    [SerializeField] float _moveSpeed;

    [Header("CraftingState")]
    //[SerializeField] List<Arrows> _auxArrowsList = new List<Arrows>();//AuxList siempre filleada es para craftear
    [SerializeField] List<Arrows> _craftedArrows = new List<Arrows>();//Lista de flechas creadas
    [SerializeField] float _craftTime;

    [SerializeField] Arrows _arrow;
    [SerializeField] int _failChance;
    [SerializeField] int _craftAttempts;
    private float _craftCounter;

    [Header("RefllingState")]
    [SerializeField] float _refillTime;
    private float _refillTimeCounter;


    private void Awake()
    {
        //_myRb = gameObject.GetComponent<Rigidbody>();
        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var idle = new State<PlayerInputs>("Idle");
        var refilling = new State<PlayerInputs>("Refill");
        var dying = new State<PlayerInputs>("Die");
        var crafting = new State<PlayerInputs>("Craft");
        var delivering = new State<PlayerInputs>("Deliver");

        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.DELIVER, delivering)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();


        StateConfigurer.Create(refilling)
            .SetTransition(PlayerInputs.DIE, dying)
            .SetTransition(PlayerInputs.DELIVER, delivering)
            .Done();


        StateConfigurer.Create(crafting)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();


        StateConfigurer.Create(delivering)
            .SetTransition(PlayerInputs.REFILL, refilling)
            .SetTransition(PlayerInputs.CRAFT, crafting)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(dying)
            .Done();



        idle.OnEnter += x =>
        {
            Debug.Log("enter idle");
            Debug.Log(_myArcher.isPanic);
        };

        idle.OnUpdate += () =>
        {
            _anim.Play("Idle");
            //Debug.Log(_myArcher.isPanic);
            if (_myArcher.isPanic)
            {
                _isDelivering = true;
                SendInputToFSM(PlayerInputs.DELIVER);
            }
                

        };
        idle.OnExit += x =>
        {
            Debug.Log("sali:?");
        };


        //Estado de recarga: El arquero no tiene mas flechas por lo tanto
        //se encarga de recargar desde _reloadingArrows.
        delivering.OnEnter += x =>
        {
            _craftQuiver.SetActive(false);
            if(_isDelivering)
                _reloadQuiver.SetActive(true);
            else _reloadQuiver.SetActive(false);

        };

        delivering.OnFixedUpdate += () =>
        {
            _anim.Play("Run");
            Debug.Log("enter delivery");
            if (_isDelivering) //Si hace deliver entonces esta yendo
            {
                
                Vector3 dir = _deliverPos.transform.position - transform.position;
                transform.forward = dir;
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;

                if (dir.magnitude < 0.15f)
                {
                    SendInputToFSM(PlayerInputs.REFILL);
                }
                
            }
            else //Si no esta haciendo deliver significa que esta volviendo basicamente y cuando llega va a craft
            {
                Vector3 dir = _craftPos.transform.position - transform.position;
                transform.forward = dir;
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;

                if (dir.magnitude < 0.15f)
                {
                    SendInputToFSM(PlayerInputs.CRAFT);
                }
            }

            

        };

        delivering.OnExit += x =>
        {
        };

        //Le hace refill a las flechas del arquero que tiene guardadas
        refilling.OnEnter += x =>
        {
            _anim.Play("Throw");
            Debug.Log("enter refill");
            _isDelivering = false;
            //Hago un concat entre las flechas que tenga en el reloading en este momento y las flechas que craftie
            _myArcher._reloadingArrows = _myArcher._reloadingArrows.Concat(ArrowCrafter(_craftedArrows)).ToList();

            _craftedArrows = ArrowTaker(_craftedArrows).ToList();

            _refillTimeCounter = _refillTime;
        };

        refilling.OnUpdate += () =>
        {
            _refillTimeCounter -= Time.deltaTime;

            //Corre la animacion y al final de la animacion cambia a delivering pero con delivering false
            //entonces hace la vuelta a crafting
            if (_refillTimeCounter<0)
                SendInputToFSM(PlayerInputs.DELIVER);

        };

        //Le saca el panic al arquero
        refilling.OnExit += x =>
        {
            _myArcher.isPanic = false;
            
        };

        crafting.OnEnter += x =>
        {
            Debug.Log("enter ctafting");
            //Esto es si se quiere usar con aggregate y una lista auxiliar
            //_craftedArrows = ArrowCrafter(_auxArrowsList).ToList();

            //Este es con el generator
            _craftedArrows = CraftingTime(_arrow, _failChance, _craftAttempts).ToList();

            //Crafting Time
            _craftCounter = _craftTime;
            _anim.Play("Hammer");
            _hammer.SetActive(true);

        };

        crafting.OnUpdate += () =>
        {

            _craftCounter -= Time.deltaTime;
            //Corre la animacion de crafting
            if (_craftCounter<0)
                SendInputToFSM(PlayerInputs.IDLE);
            
        };

        crafting.OnExit += x =>
        {
            _hammer.SetActive(false);
            _craftQuiver.SetActive(true);
        };


        dying.OnEnter += x =>
        {
            Destroy(gameObject);
        };

        //Choose first state.
        _myFsm = new EventFSM<PlayerInputs>(crafting);
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

    //Refill flechas esta funcion la tengo que pasar al que refillea las flechas justamente


    //Este es para recargar con aggregate y una lista auxiliar.
    public IEnumerable<Arrows> ArrowCrafter(List<Arrows> arrows)
    {
        var myCol = arrows.Aggregate(new List<Arrows>(), (acum, current) =>
        {
            acum.Add(current);
            return acum;
        });
        return myCol;
    }

    public IEnumerable<Arrows> ArrowTaker(List<Arrows> arrows)
    {
        var myCol = arrows.Skip(arrows.Count());
        return myCol;
    }

    //Generator//
    public IEnumerable<Arrows> CraftingTime(Arrows arrows, float failChance, int attempts)
    {
        for (int i = 0; i < attempts; i++)
        {
            var randomSuccessChance = UnityEngine.Random.Range(0, 101);
            if (randomSuccessChance > failChance)
                yield return arrows;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System.Linq;
using System;

public class Collector : MonoBehaviour, IEntity
{

    public enum PlayerInputs { LOOT, IDLE, RUN_WITH_STONE, RUN_WITH_WOOD, PUT_DOWN_RESOURCE, DIE }
    private EventFSM<PlayerInputs> _myFsm;

    [SerializeField] Animator _anim;
    [SerializeField] float hp;
    private float originalHP;

    [Header("Physics")]
    [SerializeField] private float _speed;
    [SerializeField] private Rigidbody _rb;
    Vector3 dir;

    [Header("IdleState")]
    [SerializeField] float _restTime;
    [SerializeField] float _restTimer;

    [Header("LootingState")]
    [SerializeField] Collectable _lootTarget;
    [SerializeField] float _rangeToLoot;
    [SerializeField] float _timeToLootStone;
    [SerializeField] float _timeToLootWood;
    [SerializeField] float _lootingTimer;

    [Header("LeaveResourcesState")]
    [SerializeField] float _speedWithLoot;
    [SerializeField] float _rangeToDropLoot;
    [SerializeField] float _timeToDrop;
    [SerializeField] float _timerToDrop;
    [SerializeField] Container _containerTarget;


    private void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
        //PARTE 1: SETEO INICIAL
        //Time.timeScale *= 5;
        //Creo los estados
        var idle = new State<PlayerInputs>("Idle");
        var dying = new State<PlayerInputs>("Die");
        var looting = new State<PlayerInputs>("Looting");
        var runWithStone = new State<PlayerInputs>("RunStone");
        var runWithWood = new State<PlayerInputs>("RunWood");
        var putDownResource = new State<PlayerInputs>("PutDownResource");

        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.LOOT, looting)
            .SetTransition(PlayerInputs.DIE, dying)
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


        //Idle
        idle.OnEnter += x =>
        {
            _anim.Play("Idle");
            _rb.velocity = Vector3.zero;
        };

        idle.OnUpdate += () =>
        {
            _restTimer += Time.deltaTime;
           
            if (_restTimer >= _restTime)
            {
                if ((CollectableManager.instance.availablesTrees.Any() || CollectableManager.instance.availablesStones.Any()) &&
                    (ContainerManager.instance.freeStoneContainers.Any() || ContainerManager.instance.freeWoodContainers.Any())) //SI NO HAY ARBOLES O PIEDRAS PARA LOOTEAR RESETEO EL IDLE
                    SendInputToFSM(PlayerInputs.LOOT);
                else
                    _restTimer = 0;
            }
        };

        idle.OnExit += x =>
        {
            _restTimer = 0;
        };


        //Looting
        looting.OnEnter += x =>
        {
            var treeToLoot = CollectableManager.instance.availablesTrees.OrderBy(x => Vector3.Distance(gameObject.transform.position, x.gameObject.transform.position)).FirstOrDefault();
            var stoneToLoot = CollectableManager.instance.availablesStones.OrderBy(x => Vector3.Distance(gameObject.transform.position, x.gameObject.transform.position)).FirstOrDefault();

            _anim.Play("Run");

            if (ResourcesManager.instance.woodAmount <= ResourcesManager.instance.stoneAmount) //SI HAY MENOS WOOD QUE STONE, BUSCO WOOD
            {
                _lootTarget = treeToLoot;
            }
            else
            {
                _lootTarget = stoneToLoot;
            }

            if (_lootTarget == null || _lootTarget.gameObject == null)
            {
                SendInputToFSM(PlayerInputs.IDLE);
                return;
            }
            
            dir = (transform.position - _lootTarget.transform.position).normalized * -1;
        };

        looting.OnUpdate += () =>
        {
            transform.forward = dir;

            if (_lootTarget == null || _lootTarget.gameObject == null)
            {
                SendInputToFSM(PlayerInputs.IDLE);
                return;
            }

            if (Vector3.Distance(transform.position, _lootTarget.gameObject.transform.position) > _rangeToLoot) //SI TODAV�A NO ESTOY EN RANGO, SIGO CORRIENDO
            {
                _rb.velocity = dir * _speed;
            }
            else if (Vector3.Distance(transform.position, _lootTarget.gameObject.transform.position) <= _rangeToLoot) //SI YA ESTOY CERCA,COMIENZO A LOOTEAR
            {
                _rb.velocity = Vector3.zero;
                _anim.Play("Collect");
                _lootingTimer += Time.deltaTime;

                if (_lootTarget.GetComponent<Tree>()) //SI ESTOY LOOTEANDO MADERA
                {
                    if (_lootingTimer >= _timeToLootWood) //Y YA TERMINO EL TIEMPO DE ESTAR LOOTEANDO
                    {
                        SendInputToFSM(PlayerInputs.RUN_WITH_WOOD);
                        _lootTarget.GetComponent<Collectable>().OnLooted();
                    }
                }
                else if (_lootTarget.GetComponent<Stone>()) //SI ESTOY LOOTEANDO PIEDRA
                {
                    if (_lootingTimer >= _timeToLootStone) //Y YA TERMINO EL TIEMPO DE ESTAR LOOTEANDO
                    {
                        SendInputToFSM(PlayerInputs.RUN_WITH_STONE);
                        _lootTarget.GetComponent<Collectable>().OnLooted();
                    }
                }
            }
        };

        looting.OnExit += x =>
        {
            _lootingTimer = 0;
        };


        runWithStone.OnEnter += x =>
        {
            _anim.Play("RunWithStone");
            //BUSCO UN CONTAINER DE PIEDRAS
            _containerTarget = ContainerManager.instance.freeStoneContainers.OrderBy(x => Vector3.Distance(gameObject.transform.position, x.gameObject.transform.position)).FirstOrDefault();

            dir = (transform.position - _containerTarget.transform.position).normalized * -1;
            transform.forward = dir;
        };

        runWithStone.OnUpdate += () =>
        {
            transform.forward = dir;

            if (_containerTarget == null)
                SendInputToFSM(PlayerInputs.IDLE);

            if (Vector3.Distance(transform.position, _containerTarget.gameObject.transform.position) > _rangeToDropLoot) //SI TODAV�A NO ESTOY EN RANGO, SIGO CORRIENDO
                _rb.velocity = dir * _speedWithLoot;
            else
            {
                _rb.velocity = Vector3.zero;
                SendInputToFSM(PlayerInputs.PUT_DOWN_RESOURCE);
            }
        };




        runWithWood.OnEnter += x =>
        {
            _anim.Play("RunWithWood");

            //BUSCO UN CONTAINER DE MADERA
            _containerTarget = ContainerManager.instance.freeWoodContainers.OrderBy(x => Vector3.Distance(gameObject.transform.position, x.gameObject.transform.position)).FirstOrDefault();


            dir = (transform.position - _containerTarget.transform.position).normalized * -1;
            transform.forward = dir;
        };

        runWithWood.OnUpdate += () =>
        {
            transform.forward = dir;



            if (Vector3.Distance(transform.position, _containerTarget.gameObject.transform.position) > _rangeToDropLoot) //SI TODAV�A NO ESTOY EN RANGO, SIGO CORRIENDO
                _rb.velocity = dir * _speedWithLoot;
            else
            {
                _rb.velocity = Vector3.zero;
                SendInputToFSM(PlayerInputs.PUT_DOWN_RESOURCE);
            }
        };

        putDownResource.OnEnter += x =>
        {
            _timerToDrop = 0;
            _anim.Play("PutDownResource");
        };

        putDownResource.OnUpdate += () =>
        {
            transform.forward = dir;

            _timerToDrop += Time.deltaTime;

            if (_timerToDrop >= _timeToDrop)
            {
                _containerTarget.GetComponent<Container>().OnGetResource();
                SendInputToFSM(PlayerInputs.IDLE);
            }
        };

        putDownResource.OnExit += x =>
        {
            _timerToDrop = 0;
        };

        dying.OnEnter += x =>
        {
            Village.instance.RemoveVillager(this);
            gameObject.SetActive(false);
        };

        //Choose first state.
        _myFsm = new EventFSM<PlayerInputs>(idle);
    }

    private void Start()
    {
        Village.instance.AddVillager(this);
    }
    
    private void SendInputToFSM(PlayerInputs inp)
    {
        _myFsm.SendInput(inp);
    }

    private void Update()
    {
        _myFsm.Update();
    }

    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }

    
    #region IENTITY

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public float Health
    {
        get
        {
            return hp;
        }
        set
        {
            hp = value;
        }
    }

    public GameObject myGameObject
    {
        get
        {
            return gameObject;
        }
    }

    public bool IsEnemy
    {
        get
        {
            return false;
        }
    }
    public void TakeDamage(float damage)
    {
        Health -= damage;

        if (hp <= 0)
        {
            _myFsm.SendInput(PlayerInputs.DIE);
            hp = originalHP;
        }
    }

    private bool isInGrid = false;
    public bool onGrid
    {
        get
        {
            return isInGrid;
        }
        set
        {
            isInGrid = value;
        }
    }
    public event Action<IEntity> OnMove;

    #endregion
}

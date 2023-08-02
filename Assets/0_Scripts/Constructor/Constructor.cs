using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System.Linq;
using System;

public class Constructor : MonoBehaviour, IEntity
{

    public enum PlayerInputs { BUILD, IDLE, RUN_WITH_STONE, RUN_WITH_WOOD, GET_RESOURCE, DIE, REST }
    private EventFSM<PlayerInputs> _myFsm;

    [SerializeField] Animator _anim;
    [SerializeField] float hp;
    private float originalHP;
    [SerializeField] int buildStage;

    [Header("Physics")]
    [SerializeField] private float _speed;
    [SerializeField] private Rigidbody _rb;
    Vector3 dir;
    Vector3 startPos;

    [Header("IdleState")]
    [SerializeField] float _restTime;
    [SerializeField] float _restTimer;

    [Header("BuildingState")]
    [SerializeField] Structure _structureTarget;
    [SerializeField] float _speedWithResource;
    [SerializeField] float _rangeToBuild;
    [SerializeField] float _timeToBuildStone;
    [SerializeField] float _timeToBuildWood;
    [SerializeField] float _buildTimer;

    [Header("GetResources")]
    [SerializeField] float _rangeToGetResource;
    [SerializeField] float _timeToGetResource;
    [SerializeField] float _timerToGetResource;
    [SerializeField] Container _containerTarget;


    private void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
        buildStage = 0;
        originalHP = hp;

        startPos = transform.position;

        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var idle = new State<PlayerInputs>("Idle");
        var getResource = new State<PlayerInputs>("GetResource");
        var dying = new State<PlayerInputs>("Die");
        var building = new State<PlayerInputs>("Building");
        var runWithStone = new State<PlayerInputs>("RunStone");
        var rest = new State<PlayerInputs>("Rest");
        var runWithWood = new State<PlayerInputs>("RunWood");

        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.GET_RESOURCE, getResource)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(getResource)
            .SetTransition(PlayerInputs.RUN_WITH_STONE, runWithStone)
            .SetTransition(PlayerInputs.RUN_WITH_WOOD, runWithWood)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(rest)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();


        StateConfigurer.Create(runWithWood)
            .SetTransition(PlayerInputs.BUILD, building)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(runWithStone)
            .SetTransition(PlayerInputs.BUILD, building)
            .SetTransition(PlayerInputs.REST, rest)
            .SetTransition(PlayerInputs.DIE, dying)
            .Done();

        StateConfigurer.Create(building)
            .SetTransition(PlayerInputs.GET_RESOURCE, getResource)
            .SetTransition(PlayerInputs.DIE, dying)
            .SetTransition(PlayerInputs.REST, rest)
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



            if (buildStage == 0)
            {
                if (_restTimer >= _restTime )
                {
                    if (ContainerManager.instance.takenWoodContainers.Any() && StructureManager.instance.availablesStructures.Any()) //SI NO HAY ARBOLES O PIEDRAS PARA LOOTEAR RESETEO EL IDLE, TAMBIEN CHEQUEO QUE HAYA LUGAR PARA CONSTRUIR
                        SendInputToFSM(PlayerInputs.GET_RESOURCE);
                    else
                        _restTimer = 0;
                }
            }
            else
            {
                if (ContainerManager.instance.takenStoneContainers.Any())
                    SendInputToFSM(PlayerInputs.GET_RESOURCE);
            }

        };

        idle.OnExit += x =>
        {
            _restTimer = 0;
        };


        //Looting
        getResource.OnEnter += x =>
        {
            _anim.Play("Run");

            //EN EL STAGE 0 LLEVA LA MADERA, DESPUES LAS PIEDRAS
            if (buildStage == 0)
            {
                _containerTarget = ContainerManager.instance.takenWoodContainers.OrderBy(x => Vector3.Distance(gameObject.transform.position, x.gameObject.transform.position)).FirstOrDefault();
                _structureTarget = StructureManager.instance.availablesStructures.OrderBy(x => Vector3.Distance(gameObject.transform.position, x.gameObject.transform.position)).FirstOrDefault();
            }
            else
                _containerTarget = ContainerManager.instance.takenStoneContainers.OrderBy(x => Vector3.Distance(gameObject.transform.position, x.gameObject.transform.position)).FirstOrDefault();

            dir = (transform.position - _containerTarget.transform.position).normalized * -1;
        };

        getResource.OnUpdate += () =>
        {
            transform.forward = dir;

            if (Vector3.Distance(transform.position, _containerTarget.gameObject.transform.position) > _rangeToGetResource) //SI TODAV�A NO ESTOY EN RANGO, SIGO CORRIENDO
                _rb.velocity = dir * _speed;
            else
            {
                _rb.velocity = Vector3.zero;
                _anim.Play("GrabResource");
                _timerToGetResource += Time.deltaTime;

                if (_timerToGetResource >= _timeToGetResource)
                    if (_containerTarget.GetComponent<WoodContainer>())
                        SendInputToFSM(PlayerInputs.RUN_WITH_WOOD);
                    else
                        SendInputToFSM(PlayerInputs.RUN_WITH_STONE);

            }
        };

        getResource.OnExit += x =>
        {
            _timerToGetResource = 0;
        };


        runWithStone.OnEnter += x =>
        {
            _anim.Play("RunWithStone");
            _containerTarget.GetComponent<Container>().OnTakenResource();

            dir = (transform.position - _structureTarget.transform.position).normalized * -1;
        };

        runWithStone.OnUpdate += () =>
        {
            transform.forward = dir;

            if (Vector3.Distance(transform.position, _structureTarget.gameObject.transform.position) > _rangeToBuild) //SI TODAV�A NO ESTOY EN RANGO, SIGO CORRIENDO
                _rb.velocity = dir * _speedWithResource;
            else
            {
                _rb.velocity = Vector3.zero;
                SendInputToFSM(PlayerInputs.BUILD);
            }
        };

        runWithWood.OnEnter += x =>
        {

            _anim.Play("RunWithWood");
            _containerTarget.GetComponent<Container>().OnTakenResource();

            dir = (transform.position - _structureTarget.transform.position).normalized * -1;
        };

        runWithWood.OnUpdate += () =>
        {
            transform.forward = dir;

            if (Vector3.Distance(transform.position, _structureTarget.gameObject.transform.position) > _rangeToBuild) //SI TODAV�A NO ESTOY EN RANGO, SIGO CORRIENDO
                _rb.velocity = dir * _speedWithResource;
            else
            {
                _rb.velocity = Vector3.zero;
                SendInputToFSM(PlayerInputs.BUILD);
            }

        };

        building.OnEnter += x =>
        {
            _anim.Play("Construct");
        };

        building.OnUpdate += () =>
        {
            transform.forward = dir;

            _buildTimer += Time.deltaTime;
            if (buildStage == 0)
            {
                if (_buildTimer >= _timeToBuildWood)
                {
                    buildStage++;
                    SendInputToFSM(PlayerInputs.GET_RESOURCE);
                }
            }
            else
            {
                if (_buildTimer >= _timeToBuildStone)
                {
                    buildStage--;
                    _structureTarget.OnBuild();
                    SendInputToFSM(PlayerInputs.REST);
                }
            }

        };

        building.OnExit += x =>
        {
            _buildTimer = 0;
        };

        rest.OnEnter += x =>
        {
            dir = (transform.position - startPos).normalized * -1;
            _anim.Play("Run");
        };

        rest.OnUpdate += () =>
        {
            transform.forward = dir;

            if (Vector3.Distance(transform.position, startPos) > 0.5f)
            {
                _rb.velocity = dir * _speed;
            }
            else
            {
                SendInputToFSM(PlayerInputs.IDLE);
            }
        };

        dying.OnEnter += x =>
         {
             //Destroy(gameObject);
             gameObject.SetActive(false);
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


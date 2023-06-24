using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype _cc;

    [SerializeField] private Ball _prefabBall;
    [SerializeField] private PhysxBall _prefabPhysxBall;
    private Vector3 _forward;

    [Networked] private TickTimer delay { get; set; }

    [Networked(OnChanged = nameof(OnBallSpawned))]
    public NetworkBool spawned { get; set; }

    private Material _material;
    private Text _messages;

    Material material
    {
        get
        {
            if(_material == null)
                _material = GetComponentInChildren<MeshRenderer>().material;
            return _material;
        }
    }

    public static void OnBallSpawned(Changed<Player> changed)
    {
        changed.Behaviour.material.color = Color.white;
    }


    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
        _forward = transform.forward;
    }

    //��� �ùķ��̼� ƽ���� ȣ���
    //Fusion�� ������ Ȯ�ε� ��Ʈ��ũ ���¸� �����Ѵ��� ���� �ش� ƽ���� ����(������) ���� ƽ����
    //�ٽ� �ùķ��̼��ϱ� ������ ������ �����Ӵ� ������ �߻��� �� ����
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData data))
        {
            //���������� �����ϱ� ���� ����ȭ ��
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if(data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if(delay.ExpiredOrNotRunning(Runner))
            {
                if((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                    Runner.Spawn(_prefabBall, transform.position + _forward, Quaternion.LookRotation(_forward),
                        Object.InputAuthority, (runner, o) => {
                            //Ball�� ����ȭ �Ǳ��� Init()�Լ� ȣ�� �ʿ�
                            o.GetComponent<Ball>().Init();
                        });

                    spawned = !spawned;
                }
                else if((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                    Runner.Spawn(_prefabPhysxBall, transform.position + _forward, Quaternion.LookRotation(_forward),
                        Object.InputAuthority, (runner, o) => {
                            //Ball�� ����ȭ �Ǳ��� Init()�Լ� ȣ�� �ʿ�
                            o.GetComponent<PhysxBall>().Init(10*_forward);
                        });                    
                }
            }
        }
    }

    public override void Render()
    {
        material.color = Color.Lerp(material.color, Color.blue, Time.deltaTime);
    }

    private void Update()
    {
        if(Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        Debug.Log(info.Source);

        if (_messages == null)
            _messages = FindObjectOfType<Text>();        

        if (info.Source == Runner.Simulation.LocalPlayer)
            message = $"You said: {message}\n";
        else
            message = $"Some other player said: {message}\n";            
        _messages.text += message;
    }
}

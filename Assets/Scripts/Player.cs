using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype _cc;

    [SerializeField] private Ball _prefabBall;
    [SerializeField] private PhysxBall _prefabPhysxBall;
    private Vector3 _forward;

    [Networked] private TickTimer delay { get; set; }


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
}

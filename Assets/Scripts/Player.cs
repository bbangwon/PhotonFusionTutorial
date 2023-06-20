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

    //모든 시뮬레이션 틱에서 호출됨
    //Fusion이 이전에 확인된 네트워크 상태를 적용한다음 다음 해당 틱에서 현재(예측된) 로컬 틱까지
    //다시 시뮬레이션하기 때문에 렌더링 프레임당 여러번 발생할 수 있음
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData data))
        {
            //부정행위를 방지하기 위해 정규화 함
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
                            //Ball이 동기화 되기전 Init()함수 호출 필요
                            o.GetComponent<Ball>().Init();
                        });
                }
                else if((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                    Runner.Spawn(_prefabPhysxBall, transform.position + _forward, Quaternion.LookRotation(_forward),
                        Object.InputAuthority, (runner, o) => {
                            //Ball이 동기화 되기전 Init()함수 호출 필요
                            o.GetComponent<PhysxBall>().Init(10*_forward);
                        });
                }
            }
        }
    }
}

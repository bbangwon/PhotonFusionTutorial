using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype _cc;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
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
        }
    }
}

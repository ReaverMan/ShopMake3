using System.Collections;
using UnityEngine;

public class Speed : BuffBase
{
    public override void Use()
    {
        if (player != null) // BuffBase에서 상속된 player 필드를 사용
        {
            player.moveSpeed += amountBuff;
            Destroy(this.gameObject);
            CoroutineManager.Instance.StartManagedCoroutine(Duration());
            Debug.Log("Speed가 증가하였습니다.");
            base.Use();
        }
        else
        {
            Debug.LogError("Player is not assigned when trying to use Speed.");
        }
    }

    private IEnumerator Duration()
    {
        Debug.Log("Duration() 시작"); // 코루틴 시작 시 메시지 출력
        float endTime = Time.realtimeSinceStartup + Maxduration;
        while (Time.realtimeSinceStartup < endTime)
        {
            yield return null;
        }
        Debug.Log("Duration() 대기 완료"); // 대기 완료 후 메시지 출력
        player.moveSpeed -= amountBuff;
        Debug.Log("Speed 감소 적용"); // 속도 감소 적용 후 메시지 출력
    }

#if UNITY_EDITOR
    public void Test_Use()
    {
        Use();
    }
#endif
}

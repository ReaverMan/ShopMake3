using System.Collections;
using UnityEngine;

public class Strength : BuffBase
{
    public override void Use()
    {
        if (player != null)
        {
            player.limitWeight += amountBuff;
            player.MaxWeight += amountBuff;

            Destroy(this.gameObject);
            CoroutineManager.Instance.StartManagedCoroutine(Duration());
            base.Use();
        }
        else
        {
            Debug.LogError("Player is not assigned when trying to use Strength.");
        }
    }

    private IEnumerator Duration()
    {
        Debug.Log("Strength Duration() 시작"); // 코루틴 시작 시 메시지 출력
        yield return new WaitForSeconds(Maxduration);
        Debug.Log("Strength Duration() 대기 완료"); // 대기 완료 후 메시지 출력

        player.limitWeight -= amountBuff;
        player.MaxWeight -= amountBuff;
        Debug.Log("Strength 버프 종료"); // 버프 종료 메시지 출력
    }

#if UNITY_EDITOR
    public void Test_Use()
    {
        Use();
    }
#endif
}

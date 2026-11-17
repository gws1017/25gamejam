using System.Collections;
using UnityEngine;

// 폼 1 — 기본형: 플레이어를 조준한 단발을 일정 간격으로 연사한다. 가장 읽기 쉬운 폼.
public partial class AlienBoss
{
    private IEnumerator Basic(AlienBody body)
    {
        for (int i = 0; i < settings.basicShots; i++)
        {
            Fire(body, body.transform.position, PlayerPosition());
            yield return new WaitForSeconds(settings.basicInterval);
        }
    }
}

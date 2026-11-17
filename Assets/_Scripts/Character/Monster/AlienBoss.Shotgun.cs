using System.Collections;
using UnityEngine;

// 폼 4 — 산탄형: 눈 3개에서 한 발씩, 왼쪽 → 가운데 → 오른쪽 순서로 쏜다 (묶음당 총 3발).
// 일반 몹(스피터)은 3발을 동시에 부채꼴로 쏘고, 보스는 시차를 두고 쏴서 "타-타-탁" 패링 리듬이 생긴다.
// 눈마다 그 순간의 플레이어를 다시 조준하므로, 움직이는 중에도 한 발씩 따라온다.
public partial class AlienBoss
{
    private IEnumerator Shotgun(AlienBody body)
    {
        for (int volley = 0; volley < settings.shotgunVolleys; volley++)
        {
            for (int eye = -1; eye <= 1; eye++)
            {
                Vector2 eyePosition = (Vector2)body.transform.position +
                                      new Vector2(eye * settings.eyeSpacing, settings.eyeHeight);
                Fire(body, eyePosition, PlayerPosition());
                yield return new WaitForSeconds(settings.eyeDelay);
            }
            yield return new WaitForSeconds(settings.volleyInterval);
        }
    }
}

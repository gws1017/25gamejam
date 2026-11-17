using System.Collections;
using UnityEngine;

// 폼 2 — 촉수형: 플레이어 발밑에 전조 장판 → 촉수가 솟구침, 이를 연속으로 반복한다.
// 장판이 뜰 때마다 그 시점의 플레이어 위치를 새로 잡는다. 장판 자체는 따라오지 않는다
// (실시간으로 따라오면 피할 수 없음). 보스1 솟구침과 같은 전조 장판을 재사용한다.
public partial class AlienBoss
{
    private IEnumerator Tentacle(AlienBody body)
    {
        for (int i = 0; i < settings.tentacleStrikes; i++)
        {
            bool resolved = false;

            // watch = 본체: 보스가 죽으면 장판이 발동 없이 사라진다
            var warning = AttackTelegraph.CreateEllipse(PlayerFeet(), settings.tentacleRadius, GroundSquash,
                                                        settings.tentacleWarning, transform);
            warning.OnComplete += center =>
            {
                resolved = true;
                TentacleStrike(center, body.tentacleFrames);
            };

            yield return new WaitUntil(() => resolved || !IsLive);
            if (!IsLive) yield break;

            yield return new WaitForSeconds(settings.tentacleInterval);
        }
    }

    /// <param name="frames">공격한 몸의 촉수 이펙트 (파랑/빨강 색이 다름)</param>
    private void TentacleStrike(Vector2 center, Sprite[] frames)
    {
        if (frames != null && frames.Length > 0 && frames[0] != null)
        {
            // 이펙트가 가장 넓게 퍼졌을 때 장판 폭과 같아지도록 배율 계산 (보스1 솟구침과 같은 방식)
            float peakWidthWorld = settings.tentacleEffectPeakWidthPx / frames[0].pixelsPerUnit;
            float scale = (settings.tentacleRadius * 2f) / peakWidthWorld;
            SpriteFlipbook.Play(frames, center, 0f, new Vector3(scale, scale, 1f), 16f);
        }

        var player = PlayerCharacter.Instance;
        if (player == null || player.IsDead) return;

        // 플레이어 발밑이 장판 타원 안이면 피해 (보이는 장판 = 판정 범위)
        Vector2 offset = PlayerFeet() - center;
        float nx = offset.x / settings.tentacleRadius;
        float ny = offset.y / (settings.tentacleRadius * GroundSquash);
        if (nx * nx + ny * ny <= 1f)
            player.ApplyDamage(damage);
    }
}

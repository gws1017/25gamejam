using System.Collections;
using UnityEngine;

// 폼 3 — 돌진형: 도로(바닥) 영역을 비스듬한 줄로 나누고, 한쪽 끝 줄부터 반대쪽 끝 줄까지 차례로
// "전조 장판 → 보스가 그 줄을 지나감"을 반복한다. 파도 진행 방향(왼→오 / 오→왼)은 매번 랜덤,
// 줄마다 위→아래 / 아래→위를 번갈아 지그재그로 지나간다.
// 회피법: 보스가 이미 지나간 줄로 넘어간다.
//
// 줄 모양: 바닥에 비스듬히 누운 사각형(평행사변형). 윗변·아랫변은 가로로 평평하고 옆변만 기운다.
//   윗변 = 도로 윗선(laneTopViewportY), 아랫변 = laneBottomViewportY, 기울기 = laneSlant
//      ┌────┐
//     /    /
//    /    /
//   └────┘
// 보스는 줄의 중심선을 따라 비스듬히 지나가고, 장판은 보스가 지나갈 방향으로 차오른다.
//
// 예고 장판은 "보스가 여기를 지나간다"는 표시일 뿐 판정이 아니다. 피해는 지나가는 보스 몸에
// 닿았을 때만, 일반 근접 몹과 같은 접촉 피해 경로(PlayerCharacter.DamageDelayCoroutine)로 들어간다.
// 접촉은 닿는 순간 한 번 + 쿨타임이라 한 줄에서 여러 번 깎이지 않는다.
//
// 예고는 줄마다 laneInterval 간격으로 "미리" 띄운다. 한 줄 돌진이 끝나야 다음 예고를 띄우면
// 화면 전체를 쓸기까지 줄 수 × (예고 + 돌진)이 걸려 너무 늘어지기 때문이다.
//   기존 방식: [예고━━][돌진] [예고━━][돌진] ...
//   이 방식  : [예고━━][돌진]
//                 [예고━━][돌진]
//                    [예고━━][돌진] ...   ← 장판 여러 개가 동시에 차오르는 파도 모양
public partial class AlienBoss
{
    private struct DashLane
    {
        public Vector2 start;   // 보스가 출발하는 변의 중심
        public Vector2 end;     // 보스가 도착하는 변의 중심
    }

    private IEnumerator Dash(AlienBody body)
    {
        float topY = ViewportY(settings.laneTopViewportY);
        float bottomY = ViewportY(settings.laneBottomViewportY);
        float slant = settings.laneSlant;

        // 줄이 기울어 있어서, 아랫변 기준 x 범위를 기울기만큼 넓혀야 윗변 쪽 화면 끝까지 빈틈이 없다.
        // (아랫변이 [left, right]를 덮으면 윗변은 [left + slant, right + slant]를 덮는다)
        float left = Mathf.Min(viewMin.x, viewMin.x - slant);
        float right = Mathf.Max(viewMax.x, viewMax.x - slant);
        float coverWidth = right - left;

        int laneCount = Mathf.Max(1, Mathf.CeilToInt(coverWidth / settings.laneWidth));
        float laneWidth = coverWidth / laneCount;   // 빈틈없이 나누도록 실제 폭 재계산
        bool leftToRight = Random.value < 0.5f;

        var lanes = new DashLane[laneCount];
        for (int i = 0; i < laneCount; i++)
        {
            int column = leftToRight ? i : laneCount - 1 - i;
            float bottomX = left + (column + 0.5f) * laneWidth;
            Vector2 top = new Vector2(bottomX + slant, topY);
            Vector2 bottom = new Vector2(bottomX, bottomY);
            bool downward = (i % 2 == 0);
            lanes[i] = new DashLane
            {
                start = downward ? top : bottom,
                end = downward ? bottom : top,
            };
        }

        // 다음 줄 예고가 다 찼을 때 보스가 이미 그 줄 시작점에 와 있어야 한다.
        // 한 줄 지나가는 시간 + 다음 줄 시작점으로 순간이동하는 시간(사라짐+나타남)보다 간격이 짧으면 자동으로 늘린다.
        float travelTime = Vector2.Distance(lanes[0].start, lanes[0].end) / settings.dashSpeed;
        float teleportTime = settings.laneHopFadeTime * 2f;
        float interval = Mathf.Max(settings.laneInterval, travelTime + teleportTime + 0.02f);

        var ready = new bool[laneCount];
        StartCoroutine(ScheduleDashWarnings(body, lanes, laneWidth, interval, ready));

        for (int i = 0; i < laneCount; i++)
        {
            int lane = i; // 람다 캡처용
            // 줄 시작점으로는 순간이동. 첫 줄은 대기 위치에서 오므로 보통 속도, 이후 옆 줄은 빠르게
            float hopFade = (i == 0) ? settings.teleportFadeTime : settings.laneHopFadeTime;
            yield return TeleportTo(body, lanes[lane].start, hopFade);
            yield return new WaitUntil(() => ready[lane] || !IsLive);
            if (!IsLive) yield break;

            yield return MoveTo(body, lanes[lane].end, settings.dashSpeed);     // 돌진 자체는 공격이라 실제로 이동 (순간이동 X)
        }
    }

    /// <summary>줄마다 interval 간격으로 전조 장판을 미리 띄운다. 다 찬 줄은 ready에 표시.</summary>
    private IEnumerator ScheduleDashWarnings(AlienBody body, DashLane[] lanes, float laneWidth, float interval, bool[] ready)
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            int lane = i;
            var warning = AttackTelegraph.CreateParallelogram(lanes[lane].start, lanes[lane].end, laneWidth, settings.dashWarning);
            warning.OnComplete += _ => ready[lane] = true;
            body.spawnedWarnings.Add(warning.gameObject);   // 보스 사망 시 정리 대상 (이 예고는 보스를 감시하지 않으므로)

            yield return new WaitForSeconds(interval);
        }
    }
}

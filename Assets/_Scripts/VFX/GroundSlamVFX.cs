using System.Collections.Generic;
using UnityEngine;

/*****************************************************************************************
 * 파일: GroundSlamVFX.cs
 * 역할: 내려찍기 충격 연출 — 땅이 갈라지고 흙먼지가 튄다. 피해 판정은 없다 (연출 전용).
 *       피해는 호출하는 쪽의 트리거 콜라이더가 정한다 (대악마: stompColl).
 * 사용법:
 *   GroundSlamVFX.Play(center, radiusX, crackFrames, dustFrames);
 *   frames가 비어 있으면 그 부분만 건너뛴다 (에셋 연결 전에도 로직은 돌아가도록).
 * 동작:
 *   - 균열: 단계 이미지 4장이 growTime 동안 차례로 갈라지고, 마지막 모습을 유지하다가 옅어지며 사라진다.
 *     마지막 단계의 가로 폭이 장판 지름(2 × radiusX)과 같아지도록 배율을 맞춘다.
 *   - 흙먼지: 덩어리 여러 개가 중심 근처에서 바깥/위로 흩어지며, 6프레임을 넘기고 커지다가 사라진다.
 * 에셋 (Assets/_Graphics/VFX/GroundSlam): groundCrack 4단계 / groundDust 6프레임 (피벗: 균열 중심, 먼지 아래 가운데)
 *****************************************************************************************/

public class GroundSlamVFX : MonoBehaviour
{
    private const float CrackGrowTime = 0.15f;     // 균열이 갈라지는 데 걸리는 시간(초)
    private const float CrackHoldTime = 0.9f;      // 다 갈라진 채 유지되는 시간(초)
    private const float CrackFadeTime = 0.6f;      // 옅어지며 사라지는 시간(초)
    private const int DustCount = 10;              // 흙먼지 덩어리 개수
    private const float DustMinLife = 0.55f, DustMaxLife = 0.9f;
    private const int CrackSortingOrder = 0;       // 바닥 장판(0)과 같은 높이 — 캐릭터(1) 아래
    private const int DustSortingOrder = 3;        // 캐릭터 위

    private class Dust
    {
        public Transform tf;
        public SpriteRenderer sr;
        public Vector2 velocity;
        public float life, age, baseScale;
    }

    private SpriteRenderer crackSr;
    private Sprite[] crackFrames;
    private Sprite[] dustFrames;
    private readonly List<Dust> dusts = new List<Dust>();
    private float elapsed, totalTime;

    /// <summary>center: 충격 지점(장판 중심), radiusX: 전조 장판의 가로 반지름.</summary>
    public static void Play(Vector2 center, float radiusX, Sprite[] crackFrames, Sprite[] dustFrames)
    {
        bool hasCrack = crackFrames != null && crackFrames.Length > 0;
        bool hasDust = dustFrames != null && dustFrames.Length > 0;
        if (!hasCrack && !hasDust) return;

        var go = new GameObject("GroundSlamVFX");
        go.transform.position = new Vector3(center.x, center.y, -0.1f);
        go.AddComponent<GroundSlamVFX>().Init(radiusX, hasCrack ? crackFrames : null, hasDust ? dustFrames : null);
    }

    private void Init(float radiusX, Sprite[] crackFrames, Sprite[] dustFrames)
    {
        this.crackFrames = crackFrames;
        this.dustFrames = dustFrames;
        totalTime = CrackGrowTime + CrackHoldTime + CrackFadeTime;

        if (crackFrames != null)
        {
            var child = new GameObject("Crack");
            child.transform.SetParent(transform, false);
            // 마지막 단계(가장 넓은 균열)의 가로 폭이 장판 지름과 같아지게
            float fullWidth = crackFrames[crackFrames.Length - 1].bounds.size.x;
            float scale = radiusX * 2f / Mathf.Max(0.01f, fullWidth);
            child.transform.localScale = new Vector3(scale, scale, 1f);
            crackSr = child.AddComponent<SpriteRenderer>();
            crackSr.sprite = crackFrames[0];
            crackSr.sortingOrder = CrackSortingOrder;
        }

        if (dustFrames != null)
        {
            float sizeFactor = radiusX / 2.4f;   // 장판이 클수록 먼지도 크게 (기준 반지름 2.4)
            for (int i = 0; i < DustCount; i++)
            {
                // 장판 안쪽 임의 지점에서 시작해 바깥(가로)과 위로 흩어진다
                float angle = Random.value * Mathf.PI * 2f;
                float dist = Random.Range(0.15f, 0.8f);
                Vector2 start = new Vector2(Mathf.Cos(angle) * radiusX * dist, Mathf.Sin(angle) * radiusX * 0.35f * dist);

                var child = new GameObject("Dust");
                child.transform.SetParent(transform, false);
                child.transform.localPosition = start;
                var sr = child.AddComponent<SpriteRenderer>();
                sr.sprite = dustFrames[0];
                sr.sortingOrder = DustSortingOrder;

                Vector2 outward = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.35f).normalized;
                dusts.Add(new Dust
                {
                    tf = child.transform,
                    sr = sr,
                    velocity = outward * Random.Range(0.8f, 1.8f) * sizeFactor + Vector2.up * Random.Range(0.5f, 1.1f),
                    life = Random.Range(DustMinLife, DustMaxLife),
                    baseScale = Random.Range(0.7f, 1.2f) * sizeFactor,
                });
            }
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        UpdateCrack();
        bool dustAlive = UpdateDust();
        if (elapsed >= totalTime && !dustAlive) Destroy(gameObject);
    }

    private void UpdateCrack()
    {
        if (crackSr == null) return;

        // 단계 이미지를 차례로 (갈라지는 중) → 마지막 모습 유지 → 옅어짐
        int index = Mathf.Min(crackFrames.Length - 1, (int)(elapsed / CrackGrowTime * crackFrames.Length));
        crackSr.sprite = crackFrames[index];

        float fadeStart = CrackGrowTime + CrackHoldTime;
        float alpha = elapsed < fadeStart ? 1f : Mathf.Clamp01(1f - (elapsed - fadeStart) / CrackFadeTime);
        crackSr.color = new Color(1f, 1f, 1f, alpha);
        if (alpha <= 0f) crackSr.enabled = false;
    }

    /// <summary>먼지를 움직인다. 아직 보이는 먼지가 있으면 true.</summary>
    private bool UpdateDust()
    {
        bool alive = false;
        foreach (var d in dusts)
        {
            if (d.sr == null || !d.sr.enabled) continue;

            d.age += Time.deltaTime;
            float t = d.age / d.life;
            if (t >= 1f) { d.sr.enabled = false; continue; }
            alive = true;

            d.tf.localPosition += (Vector3)(d.velocity * Time.deltaTime);
            d.velocity *= 1f - 2.2f * Time.deltaTime;   // 점점 느려진다

            int frame = Mathf.Min(dustFrames.Length - 1, (int)(t * dustFrames.Length));
            d.sr.sprite = dustFrames[frame];
            float grow = d.baseScale * (0.8f + 0.5f * t);   // 퍼지면서 조금 커진다
            d.tf.localScale = new Vector3(grow, grow, 1f);
            d.sr.color = new Color(1f, 1f, 1f, 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.6f) / 0.4f)));   // 끝 40%에서 옅어짐
        }
        return alive;
    }
}

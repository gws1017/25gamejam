using System;
using UnityEngine;

/*****************************************************************************************
 * 파일: AttackTelegraph.cs
 * 역할: 공격 예고(텔레그래프) — 테두리(윤곽)만 보이다가 안쪽이 서서히 차오르고,
 *       다 차면 OnComplete를 발행한 뒤 사라진다. 보스 패턴2(솟구침/브레스)에서 사용.
 * 사용법: 프리팹/에디터 세팅 없이 코드에서 정적 팩토리로 바로 생성한다. (SpriteRenderer + 런타임 생성 스프라이트)
 *   var tg = AttackTelegraph.CreateEllipse(pos, radiusX, 0.35f, 1.2f, bossTransform);   // 바닥에 눕힌 타원
 *   var bm = AttackTelegraph.CreateBeam(origin, dir, length, width, 1.2f, bossTransform); // 방향성 직사각형
 *   tg.OnComplete += center => { // 여기서 실제 공격 판정 처리 };
 * 메모:
 *   - 색은 단색 빨강 placeholder. 추후 스프라이트를 교체하고 싶으면 GetXxxSprite()만 바꾸면 된다.
 *   - watch Transform이 파괴/비활성화되면(= 보스 사망) 발동하지 않고 조용히 취소된다.
 *****************************************************************************************/

public class AttackTelegraph : MonoBehaviour
{
    private static readonly Color OutlineColor = new Color(1f, 0.15f, 0.15f, 0.95f);
    private static readonly Color FillColor = new Color(1f, 0.15f, 0.15f, 0.5f);
    private static readonly Color BeamAreaColor = new Color(1f, 0.15f, 0.15f, 0.2f);

    private static Sprite discSprite, ringSprite, squareSprite;

    /// <summary>다 차올랐을 때 발행. 인자는 그 시점의 텔레그래프 위치(원=중심, 빔=시작점).</summary>
    public event Action<Vector2> OnComplete;

    private Transform fill;
    private bool isEllipse;
    private float fullSize;          // 타원: 가로 지름 / 빔: 길이
    private float fullHeight;        // 타원: 세로 지름 / 빔: 두께
    private float duration;
    private float elapsed;
    private Transform watch;
    private bool hasWatch;   // 생성 시점에 감시 대상이 있었는지 (대상이 파괴돼 null처럼 보여도 취소 검사를 하기 위함)
    private bool follow;

    #region Factory
    /// <summary>
    /// 바닥에 눕힌 타원 예고. radiusX는 가로 반지름, squash는 세로/가로 비율(1=정원, 0.35=원근감 있게 납작).
    /// </summary>
    public static AttackTelegraph CreateEllipse(Vector2 center, float radiusX, float squash, float duration, Transform watch = null)
    {
        var go = new GameObject("Telegraph_Ellipse");
        go.transform.position = new Vector3(center.x, center.y, -0.1f);
        var tg = go.AddComponent<AttackTelegraph>();
        tg.isEllipse = true;
        tg.duration = Mathf.Max(0.01f, duration);
        tg.fullSize = radiusX * 2f;
        tg.fullHeight = radiusX * 2f * squash;
        tg.watch = watch;
        tg.hasWatch = watch != null;

        AddSprite(go.transform, "Ring", GetRingSprite(), OutlineColor, new Vector2(tg.fullSize, tg.fullHeight), 0, -0.01f);
        tg.fill = AddSprite(go.transform, "Fill", GetDiscSprite(), FillColor, Vector2.zero, 0, 0f);
        return tg;
    }

    /// <summary>origin에서 dir 방향으로 length만큼 뻗는 직사각형 예고. follow를 주면 그 Transform을 따라다닌다(보스 이동 추적).</summary>
    public static AttackTelegraph CreateBeam(Vector2 origin, Vector2 dir, float length, float width, float duration, Transform follow = null)
    {
        var go = new GameObject("Telegraph_Beam");
        go.transform.position = new Vector3(origin.x, origin.y, -0.1f);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        var tg = go.AddComponent<AttackTelegraph>();
        tg.isEllipse = false;
        tg.duration = Mathf.Max(0.01f, duration);
        tg.fullSize = length;
        tg.fullHeight = width;
        tg.watch = follow;
        tg.hasWatch = follow != null;
        tg.follow = follow != null;

        AddSprite(go.transform, "Area", GetSquareSprite(), BeamAreaColor, new Vector2(length, width), 0, 0f);
        tg.fill = AddSprite(go.transform, "Fill", GetSquareSprite(), FillColor, new Vector2(0f, width), 0, -0.01f);
        return tg;
    }

    private static Transform AddSprite(Transform parent, string name, Sprite sprite, Color color, Vector2 scale, int order, float z)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = new Vector3(0f, 0f, z);
        child.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        var sr = child.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return child.transform;
    }
    #endregion

    private void Update()
    {
        // 보스가 죽거나 사라지면 발동 없이 취소
        if (hasWatch)
        {
            if (watch == null || !watch.gameObject.activeInHierarchy)
            {
                Destroy(gameObject);
                return;
            }
            if (follow)
                transform.position = new Vector3(watch.position.x, watch.position.y, -0.1f);
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        // 타원: 중심에서 바깥으로 차오름 / 빔: 시작점에서 길이 방향으로 차오름
        if (isEllipse) fill.localScale = new Vector3(fullSize * t, fullHeight * t, 1f);
        else fill.localScale = new Vector3(fullSize * t, fullHeight, 1f);

        if (t >= 1f)
        {
            OnComplete?.Invoke(transform.position);
            Destroy(gameObject);
        }
    }

    #region Runtime Sprites
    private static Sprite GetDiscSprite()
    {
        //꽉찬 원, 128 == 안티엘리어싱 계수
        //0.5를 기준으로 불투명 / 투명을 나눔
        if (discSprite == null)
            discSprite = MakeCircleSprite(128, (dist) => Mathf.Clamp01((0.5f - dist) * 128f * 0.5f));
        return discSprite;
    }

    private static Sprite GetRingSprite()
    {
        //속이 빈 링
        if (ringSprite == null)
            ringSprite = MakeCircleSprite(128, (dist) => Mathf.Clamp01((0.05f - Mathf.Abs(dist - 0.45f)) * 128f * 0.5f));
        return ringSprite;
    }

    /// <summary>1유닛 크기, 왼쪽 가운데가 피벗인 단색 사각형 (빔 시작점 기준으로 길이 방향으로 늘림).</summary>
    private static Sprite GetSquareSprite()
    {
        if (squareSprite == null)
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var px = new Color32[16];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            squareSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0f, 0.5f), 4f);
        }
        return squareSprite;
    }

    private static Sprite MakeCircleSprite(int size, Func<float, float> alphaByDist)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size - 0.5f;  // 가로 위치를 -0.5 ~ +0.5로 변환 
                float v = (y + 0.5f) / size - 0.5f;
                float dist = Mathf.Sqrt(u * u + v * v); // 중심에서의 거리
                byte a = (byte)(Mathf.Clamp01(alphaByDist(dist)) * 255f); //거리에 따른 투명도
                px[y * size + x] = new Color32(255, 255, 255, a);
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    #endregion
}

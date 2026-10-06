using UnityEngine;

/*****************************************************************************************
 * 파일: SpriteFlipbook.cs
 * 역할: 잘라놓은 스프라이트 배열(프레임 시퀀스)을 한 번 재생하고 스스로 사라지는 일회성 VFX.
 *       Animator/애니메이션 클립/프리팹 없이 코드에서 바로 띄울 수 있다.
 * 사용법:
 *   SpriteFlipbook.Play(frames, position, rotationZ, scale, 16f);
 *   frames가 비어있으면 아무 것도 안 하고 null을 반환한다 (에셋 연결 전에도 로직은 돌아가도록).
 *****************************************************************************************/

public class SpriteFlipbook : MonoBehaviour
{
    private Sprite[] frames;
    private float fps;
    private float time;
    private SpriteRenderer sr;

    public static SpriteFlipbook Play(Sprite[] frames, Vector3 position, float rotationZ, Vector3 scale,
                                      float fps = 16f, int sortingOrder = 10)
    {
        if (frames == null || frames.Length == 0) return null;

        var go = new GameObject("Flipbook");
        go.transform.SetPositionAndRotation(new Vector3(position.x, position.y, -0.1f), Quaternion.Euler(0f, 0f, rotationZ));
        go.transform.localScale = scale;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = frames[0];
        renderer.sortingOrder = sortingOrder;

        var fb = go.AddComponent<SpriteFlipbook>();
        fb.frames = frames;
        fb.fps = Mathf.Max(1f, fps);
        fb.sr = renderer;
        return fb;
    }

    private void Update()
    {
        time += Time.deltaTime;
        int index = (int)(time * fps);

        if (index >= frames.Length)
        {
            Destroy(gameObject);
            return;
        }

        sr.sprite = frames[index];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hearts : MonoBehaviour
{
    [SerializeField] private Heart[] hearts;

    // 마지막에 ‘꺼진’ 하트 기록(힐 시 되돌리기)
    private Stack<int> offStack = new Stack<int>();

    public void Awake()
    {
        SetHearts();
        RebuildOffStack(); // 초기 스택 구성
    }

    public void SetHearts()
    {
        if (hearts != null && hearts.Length > 0) return;

        int length = transform.childCount;
        hearts = new Heart[length];
        for (int i = 0; i < length; i++)
            hearts[i] = transform.GetChild(i).GetComponent<Heart>();
    }

    public Heart[] GetHearts() => hearts;

    // 현재 하트 배열 상태로부터 offStack 재구성(오른쪽이 먼저 복구되도록 오른쪽부터 푸시)
    public void RebuildOffStack()
    {
        offStack.Clear();
        if (hearts == null) return;

        for (int i = hearts.Length - 1; i >= 0; i--)
        {
            if (hearts[i] == null) continue;
            if (!hearts[i].IsOn) offStack.Push(i);
        }
    }

    public int CountOn()
    {
        int count = 0;
        if (hearts == null) return 0;
        foreach (var heart in hearts) if (heart != null && heart.IsOn) count++;
        return count;
    }

    public int CountTotal() => hearts?.Length ?? 0;

    // 왼쪽(인덱스 0)부터 최초의 ‘켜진’ 하트를 찾아 끈다.
    // 성공 시 true, 끌 하트 없으면 false.
    public bool TurnOffFirstOn()
    {
        if (hearts == null) return false;

        for (int i = 0; i < hearts.Length; i++)
        {
            var heart = hearts[i];
            if (heart == null) continue;
            if (heart.IsOn && heart.TrySetOn(false))
            {
                offStack.Push(i);
                return true;
            }
        }
        return false;
    }

    // 마지막에 꺼졌던 하트를 다시 켠다(스택 기반 복구). 성공 시 true
    public bool TurnOnLastOff()
    {
        if (hearts == null) return false;

        // 스택이 비었으면 현재 상태로 재구성 시도(안전장치)
        if (offStack.Count == 0) RebuildOffStack();
        if (offStack.Count == 0) return false;

        int idx = offStack.Pop();
        var heart = hearts[idx];
        if (heart != null && !heart.IsOn)
            return heart.TrySetOn(true);

        return false;
    }

    // Player의 HP로 초기 표시 동기화(선택적): left-to-right 켜기
    public void SyncFromHP(int currentHP, int maxHP)
    {
        SetHearts();
        if (hearts == null || hearts.Length == 0) return;

        int total = Mathf.Clamp(maxHP, 0, hearts.Length);
        int on = Mathf.Clamp(currentHP, 0, total);

        for (int i = 0; i < hearts.Length; i++)
        {
            bool shouldOn = (i < on);
            if (hearts[i] != null) hearts[i].TrySetOn(shouldOn);
        }
        RebuildOffStack();
    }

    /// <summary>
    /// 여러 칸을 한 프레임에 꺼야 할 때, 왼쪽→오른쪽(또는 오른쪽→왼쪽)으로
    /// stepDelay 간격으로 순서대로 꺼지는 "도미노" 연출.
    /// HP/로직은 기존대로 처리하고, UI만 순차 토글할 때 사용.
    /// </summary>
    public Coroutine TurnOffDomino(int count, float stepDelay = 0.03f, bool leftToRight = true, bool useUnscaledTime = false)
    {
        if (hearts == null || count <= 0) return null;

        // 현재 켜져있는 하트들의 인덱스를 미리 뽑아둔다(애니 지연과 무관하게 정확한 대상 확보)
        var onIndices = new List<int>();
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null && hearts[i].IsOn) onIndices.Add(i);
        }

        if (onIndices.Count == 0) return null;

        // 방향 설정
        if (!leftToRight) onIndices.Reverse();

        int n = Mathf.Min(count, onIndices.Count);
        var targets = onIndices.GetRange(0, n);

        return StartCoroutine(CoDominoToggle(targets, turnOn: false, stepDelay, useUnscaledTime));
    }

    /// <summary>
    /// 여러 칸을 한 프레임에 켜야 할 때(대량 힐), 마지막에 꺼진 순서대로 도미노 켜짐.
    /// </summary>
    public Coroutine TurnOnDomino(int count, float stepDelay = 0.03f, bool useUnscaledTime = false)
    {
        if (hearts == null || count <= 0) return null;

        // 스택에서 먼저 팝해서 “어떤 칸을 켤지” 리스트를 확보해 둔다.
        if (offStack.Count == 0) RebuildOffStack();
        if (offStack.Count == 0) return null;

        var targets = new List<int>();
        int n = Mathf.Min(count, offStack.Count);
        for (int i = 0; i < n; i++)
        {
            targets.Add(offStack.Pop());
        }

        // 켜질 때는 오른쪽(최근 꺼진 것)부터 도미노 느낌이 자연스러우니 순서를 뒤집어도 좋음.
        targets.Reverse();

        return StartCoroutine(CoDominoToggle(targets, turnOn: true, stepDelay, useUnscaledTime));
    }

    private IEnumerator CoDominoToggle(List<int> indices, bool turnOn, float stepDelay, bool useUnscaledTime)
    {
        for (int i = 0; i < indices.Count; i++)
        {
            int idx = indices[i];
            var hearts = (idx >= 0 && idx < this.hearts.Length) ? this.hearts[idx] : null;
            if (hearts != null)
            {
                // TrySetOn이 내부 애니를 알아서 처리(펀치/바운스/페이드 등)
                if (turnOn)
                {
                    // 켜짐: 스택은 TurnOnDomino 호출 시점에 미리 Pop했으므로 여기선 스택 수정 X
                    hearts.TrySetOn(true);
                }
                else
                {
                    // 꺼짐: 스택은 실제 꺼질 때 푸시(치명적 동시성 방지는 간단히 이 타이밍에 처리)
                    if (hearts.TrySetOn(false))
                        offStack.Push(idx);
                }
            }

            if (i < indices.Count - 1)
            {
                if (useUnscaledTime) yield return new WaitForSecondsRealtime(stepDelay);
                else yield return new WaitForSeconds(stepDelay);
            }
        }
    }
}
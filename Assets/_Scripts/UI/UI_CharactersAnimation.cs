using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharactersAnimation : MonoBehaviour
{
    [Header("Sprites (targets)")]
    [SerializeField] private SpriteRenderer mainNPC;   // The primary character sprite to bob
    [SerializeField] private SpriteRenderer subNPC;    // A secondary character sprite to bob

    [Header("Motion Settings")]
    [SerializeField] private bool useLocalSpace = true;      // true: use localPosition (relative to parent); false: world position
    [SerializeField] private bool updateWhilePaused = false; // true: keep animating when timeScale==0 (menus/pauses)

    [Tooltip("How far the MAIN sprite moves vertically (world units/meters).")]
    [SerializeField] private float mainYOffset = 0.10f;

    [Tooltip("Seconds for one half-cycle (down or up) of MAIN breathing.")]
    [SerializeField] private float mainDuration = 1.5f;

    [Tooltip("How far the SUB sprite moves vertically (world units/meters).")]
    [SerializeField] private float subYOffset = 0.07f;

    [Tooltip("Seconds for one half-cycle (down or up) of SUB breathing.")]
    [SerializeField] private float subDuration = 1.0f;

    [Tooltip("Easing curve for both tweens (InOutSine feels natural).")]
    [SerializeField] private Ease ease = Ease.InOutSine;

    // Cached transforms and original positions so we can restore on stop/teardown
    private Transform mainT, subT;
    private Vector3 mainOriginalPos, subOriginalPos;

    // Currently running tweens (kept so we can Kill() them explicitly)
    private Tween mainTween, subTween;

    private void Start()
    {
        // Cache transforms if sprites were assigned
        if (mainNPC) mainT = mainNPC.transform;
        if (subNPC) subT = subNPC.transform;

        // Remember where each sprite started (local or world space)
        if (mainT) mainOriginalPos = useLocalSpace ? mainT.localPosition : mainT.position;
        if (subT) subOriginalPos = useLocalSpace ? subT.localPosition : subT.position;

        // Kick off the looping animation
        StartBreathing();
    }

    private void OnDisable()
    {
        // When the object is disabled (e.g., scene change or parent turned off),
        // stop tweens and restore positions to avoid dangling tweens targeting destroyed objects.
        StopBreathing();
    }

    private void OnDestroy()
    {
        // Redundant safety: in case OnDisable didn't run, ensure we clean up
        StopBreathing();
    }

    /// <summary>
    /// Starts/resumes breathing on available sprites.
    /// Safe to call multiple times; it kills/replaces any existing tweens.
    /// </summary>
    public void StartBreathing()
    {
        // MAIN sprite breathing
        if (mainT)
        {
            // Kill any previous tween on this transform to prevent stacking
            mainTween?.Kill();

            // Compute target Y based on original position and offset
            float targetY = mainOriginalPos.y - mainYOffset;

            // Pick the correct move function for world/local mode
            mainTween = (useLocalSpace
                ? mainT.DOLocalMoveY(targetY, mainDuration)
                : mainT.DOMoveY(targetY, mainDuration))
                .SetEase(ease)                     // natural smooth in/out
                .SetLoops(-1, LoopType.Yoyo)       // loop forever: down→up→down...
                .SetUpdate(updateWhilePaused)      // ignore timeScale if desired
                .SetLink(mainT.gameObject, LinkBehaviour.KillOnDestroy); // auto-kill when object is destroyed
        }

        // SUB sprite breathing
        if (subT)
        {
            subTween?.Kill();

            float targetY = subOriginalPos.y - subYOffset;

            subTween = (useLocalSpace
                ? subT.DOLocalMoveY(targetY, subDuration)
                : subT.DOMoveY(targetY, subDuration))
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(updateWhilePaused)
                .SetLink(subT.gameObject, LinkBehaviour.KillOnDestroy);
        }
    }

    /// <summary>
    /// Stops breathing on both sprites and restores original positions.
    /// Safe to call even if no tweens are active.
    /// </summary>
    public void StopBreathing()
    {
        // Kill tweens (if they exist) to stop updates immediately
        if (mainTween != null) { mainTween.Kill(); mainTween = null; }
        if (subTween != null) { subTween.Kill(); subTween = null; }

        // Restore each sprite to its starting position (important for consistent re-entry)
        if (mainT)
        {
            if (useLocalSpace) mainT.localPosition = mainOriginalPos;
            else mainT.position = mainOriginalPos;
        }

        if (subT)
        {
            if (useLocalSpace) subT.localPosition = subOriginalPos;
            else subT.position = subOriginalPos;
        }
    }

    // 🔧 Optional helpers if you want to swap sprites at runtime:

    /// <summary>
    /// Assigns a new main sprite target at runtime and restarts breathing.
    /// </summary>
    public void SetMain(SpriteRenderer newMain, bool restart = true)
    {
        StopBreathing();
        mainNPC = newMain;
        mainT = newMain ? newMain.transform : null;
        if (mainT) mainOriginalPos = useLocalSpace ? mainT.localPosition : mainT.position;
        if (restart) StartBreathing();
    }

    /// <summary>
    /// Assigns a new sub sprite target at runtime and restarts breathing.
    /// </summary>
    public void SetSub(SpriteRenderer newSub, bool restart = true)
    {
        StopBreathing();
        subNPC = newSub;
        subT = newSub ? newSub.transform : null;
        if (subT) subOriginalPos = useLocalSpace ? subT.localPosition : subT.position;
        if (restart) StartBreathing();
    }

}

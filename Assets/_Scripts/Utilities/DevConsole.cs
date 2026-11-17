#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

/*****************************************************************************************
 * 파일: DevConsole.cs
 * 역할: 인게임 개발자용 치트 패널. F2로 열고 닫는다. 열려 있는 동안 게임은 일시정지.
 *       - 위쪽 버튼: 자주 쓰는 치트를 한 번에 실행
 *       - 아래 입력창: "stage 10"처럼 직접 입력 (Enter 실행, ↑↓ 이전 명령)
 * 구조:
 *   - IMGUI(OnGUI)로 그려서 씬/프리팹/폰트 설정이 필요 없다. 게임 시작 시 자동 생성된다.
 *   - 파일 전체가 에디터/개발용 빌드에서만 컴파일된다 → 배포 빌드에는 포함되지 않는다.
 *   - IMGUI 기본 폰트는 한글이 깨지므로 표시는 영어로 한다.
 * 새 치트 추가: RegisterCommands()에 Register(...) 한 줄 + 실행 함수. 버튼은 QuickButtons에 문자열 추가.
 *****************************************************************************************/

public class DevConsole : MonoBehaviour
{
    /// <summary>패널이 열려 있는가. PlayerController가 이 값으로 게임 입력(E 상점, Space 사격 등)을 막는다.</summary>
    public static bool IsOpen { get; private set; }

    private class Command
    {
        public string name;
        public string usage;
        public string description;
        public Action<string[]> run;
    }

    private const KeyCode ToggleKey = KeyCode.F2;
    private const string InputControl = "DevConsoleInput";
    private const int MaxLogLines = 60;
    private const int VisibleLogLines = 10;
    private const int ButtonsPerRow = 4;

    /// <summary>입력한 것처럼 실행되는 빠른 버튼 목록.</summary>
    private static readonly string[] QuickButtons =
    {
        "god", "heal", "boss", "stage 10",
        "phase 2", "phase 3", "killboss", "killall",
        "gold 500", "exp 100", "speed 1", "speed 2",
    };

    private static DevConsole instance;

    private readonly List<Command> commands = new List<Command>();
    private readonly List<string> log = new List<string>();
    private readonly List<string> history = new List<string>();
    private int historyIndex;
    private string input = "";
    private bool focusInput;
    private float resumeTimeScale = 1f;   // 패널을 닫을 때 되돌릴 시간 배율 (speed 명령으로 바뀜)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (instance != null) return;
        var go = new GameObject("DevConsole");
        DontDestroyOnLoad(go);
        go.AddComponent<DevConsole>();
    }

    private void Awake()
    {
        instance = this;
        RegisterCommands();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Log("F2: close | type 'help' for commands");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (IsOpen) Close();
        if (instance == this) instance = null;
    }

    // 씬이 바뀌면 일시정지가 풀린 채로 남지 않게 닫는다
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsOpen) Close();
    }

    private void Update()
    {
        if (Input.GetKeyDown(ToggleKey))
        {
            if (IsOpen) Close();
            else Open();
        }
    }

    private void Open()
    {
        IsOpen = true;
        resumeTimeScale = Time.timeScale;   // 이미 일시정지(0)였으면 닫을 때도 0으로 복귀
        Time.timeScale = 0f;
        focusInput = true;
    }

    private void Close()
    {
        IsOpen = false;
        Time.timeScale = resumeTimeScale;
    }

    #region Commands
    private void RegisterCommands()
    {
        Register("help", "help", "List commands", _ => ShowHelp());
        Register("god", "god", "Toggle invincibility (same as F1)", _ => CmdGod());
        Register("heal", "heal [n]", "Heal n hearts (default: full)", CmdHeal);
        Register("stage", "stage <n>", "Jump to stage n", CmdStage);
        Register("boss", "boss", "Jump to nearest boss stage", _ => CmdBoss());
        Register("phase", "phase <n>", "Set boss HP to enter phase n", CmdPhase);
        Register("killboss", "killboss", "Kill the boss (advances stage)", _ => CmdKillBoss());
        Register("killall", "killall", "Kill all non-boss monsters (counts as kills)", _ => CmdKillAll());
        Register("gold", "gold <n>", "Add gold", CmdGold);
        Register("exp", "exp <n>", "Add exp", CmdExp);
        Register("speed", "speed <x>", "Game speed after closing (0.1~10)", CmdSpeed);
    }

    private void Register(string name, string usage, string description, Action<string[]> run)
    {
        commands.Add(new Command { name = name, usage = usage, description = description, run = run });
    }

    private void ShowHelp()
    {
        foreach (var c in commands) Log($"{c.usage}  -  {c.description}");
    }

    private void CmdGod()
    {
        var player = Player();
        if (player == null) return;
        player.ToggleCheatInvincible();
        Log($"god {(player.IsCheatInvincible ? "ON" : "OFF")}");
    }

    private void CmdHeal(string[] args)
    {
        var player = Player();
        if (player == null || player.IsDead) return;

        int count = TryInt(args, 0, out int n) ? n : int.MaxValue;
        int healed = 0;
        while (healed < count && healed < 20 && player.CurrentHP < player.MaxHP)
        {
            player.Heal(1f);
            healed++;
        }
        Log($"healed {healed} (HP {player.CurrentHP}/{player.MaxHP})");
    }

    private void CmdStage(string[] args)
    {
        if (!TryInt(args, 0, out int stage)) { Log("usage: stage <n>"); return; }
        if (StageManager.Instance == null) { Log("no StageManager"); return; }
        StageManager.Instance.SetStage(stage);
        Log($"stage {StageManager.Instance.CurrentStage}");
    }

    private void CmdBoss()
    {
        if (StageManager.Instance == null) { Log("no StageManager"); return; }
        StageManager.Instance.JumpToNearestBossStage();
        Log($"stage {StageManager.Instance.CurrentStage} (boss)");
    }

    private void CmdPhase(string[] args)
    {
        if (!TryInt(args, 0, out int phase)) { Log("usage: phase <n>"); return; }
        var boss = FindFirstObjectByType<Boss>();
        if (boss == null) { Log("no boss in scene"); return; }
        boss.DebugSetPhase(phase);
        Log($"boss phase {boss.CurrentPhase} (HP {boss.CurrentHP:0}/{boss.MaxHP:0})");
    }

    private void CmdKillBoss()
    {
        var boss = FindFirstObjectByType<Boss>();
        if (boss == null) { Log("no boss in scene"); return; }
        boss.TakeDamage(boss.MaxHP * 10f, boss.transform.position);
        Log("boss killed");
    }

    private void CmdKillAll()
    {
        int count = 0;
        foreach (var monster in FindObjectsByType<MonsterCharacter>(FindObjectsSortMode.None))
        {
            if (monster.IsBoss || !monster.IsLive) continue;
            monster.TakeDamage(monster.MaxHP * 10f, monster.transform.position);
            count++;
        }
        Log($"killed {count} monsters");
    }

    private void CmdGold(string[] args)
    {
        var player = Player();
        if (player == null || !TryInt(args, 0, out int amount)) { Log("usage: gold <n>"); return; }
        player.PlayerWallet.AddGold(amount);
        Log($"gold {player.PlayerWallet.CurrentGold}");
    }

    private void CmdExp(string[] args)
    {
        var player = Player();
        if (player == null || !TryInt(args, 0, out int amount)) { Log("usage: exp <n>"); return; }
        player.IncreamentExp(amount);
        Log($"exp +{amount} (Lv {player.Level})");
    }

    private void CmdSpeed(string[] args)
    {
        if (args.Length == 0 ||
            !float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            Log("usage: speed <x>");
            return;
        }
        resumeTimeScale = Mathf.Clamp(value, 0.1f, 10f);   // 패널이 열려 있는 동안은 정지 상태라 닫을 때 적용
        Log($"speed x{resumeTimeScale} (applies on close)");
    }

    private PlayerCharacter Player()
    {
        var player = PlayerCharacter.Instance;
        if (player == null) Log("no player");
        return player;
    }

    private static bool TryInt(string[] args, int index, out int value)
    {
        value = 0;
        return args.Length > index && int.TryParse(args[index], out value);
    }
    #endregion

    #region Execute / History
    private void Execute(string line)
    {
        line = line.Trim();
        if (line.Length == 0) return;

        Log("> " + line);
        if (history.Count == 0 || history[history.Count - 1] != line) history.Add(line);
        historyIndex = history.Count;

        string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string name = parts[0].ToLowerInvariant();
        string[] args = new string[parts.Length - 1];
        Array.Copy(parts, 1, args, 0, args.Length);

        var command = commands.Find(c => c.name == name);
        if (command == null) { Log($"unknown command '{name}' (try help)"); return; }

        try { command.run(args); }
        catch (Exception e) { Log("error: " + e.Message); }
    }

    private void MoveHistory(int step)
    {
        if (history.Count == 0) return;
        historyIndex = Mathf.Clamp(historyIndex + step, 0, history.Count);
        input = historyIndex == history.Count ? "" : history[historyIndex];
    }

    private void Log(string message)
    {
        log.Add(message);
        if (log.Count > MaxLogLines) log.RemoveAt(0);
    }
    #endregion

    #region GUI
    private void OnGUI()
    {
        if (!IsOpen) return;

        // 해상도가 높아도 글자가 작지 않게 720p 기준으로 배율을 맞춘다
        float scale = Mathf.Max(1f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

        var panel = new Rect(10f, 10f, 480f, Screen.height / scale - 20f);
        GUI.Box(panel, "DEV CONSOLE  (F2: close)");
        GUILayout.BeginArea(new Rect(panel.x + 8f, panel.y + 26f, panel.width - 16f, panel.height - 34f));

        DrawStatus();
        GUILayout.Space(6f);
        DrawQuickButtons();
        GUILayout.Space(6f);
        DrawInput();
        GUILayout.Space(6f);
        DrawLog();

        GUILayout.EndArea();
    }

    private void DrawStatus()
    {
        var player = PlayerCharacter.Instance;
        string stage = StageManager.Instance != null ? StageManager.Instance.CurrentStage.ToString() : "-";
        string god = player != null && player.IsCheatInvincible ? "ON" : "OFF";
        string hp = player != null ? $"{player.CurrentHP}/{player.MaxHP}" : "-";
        GUILayout.Label($"Stage {stage}   God {god}   HP {hp}   Speed x{resumeTimeScale}");
    }

    private void DrawQuickButtons()
    {
        for (int i = 0; i < QuickButtons.Length; i += ButtonsPerRow)
        {
            GUILayout.BeginHorizontal();
            for (int j = i; j < i + ButtonsPerRow && j < QuickButtons.Length; j++)
            {
                if (GUILayout.Button(QuickButtons[j])) Execute(QuickButtons[j]);
            }
            GUILayout.EndHorizontal();
        }
    }

    private void DrawInput()
    {
        Event e = Event.current;
        bool inputFocused = GUI.GetNameOfFocusedControl() == InputControl;

        // 키 처리는 텍스트 필드를 그리기 전에 해야 Enter/화살표가 필드에 먹히지 않는다
        if (e.type == EventType.KeyDown && inputFocused)
        {
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                Execute(input);
                input = "";
                focusInput = true;
                e.Use();
            }
            else if (e.keyCode == KeyCode.UpArrow) { MoveHistory(-1); e.Use(); }
            else if (e.keyCode == KeyCode.DownArrow) { MoveHistory(1); e.Use(); }
        }

        GUI.SetNextControlName(InputControl);
        input = GUILayout.TextField(input);

        if (focusInput && e.type == EventType.Repaint)
        {
            GUI.FocusControl(InputControl);
            focusInput = false;
        }
    }

    private void DrawLog()
    {
        int start = Mathf.Max(0, log.Count - VisibleLogLines);
        for (int i = start; i < log.Count; i++) GUILayout.Label(log[i]);
    }
    #endregion
}
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class snowflakes : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] buttons;
    public TextMesh[] displays;
    public Color defaultColor;
    public Color strikeColor;
    public Color solveColor;

    private int currentPosition;
    private int target;
    #pragma warning disable 414
    private int startingPosition; // for Souvenir
    #pragma warning restore 414

    private static readonly string[] walls = new string[] {
        "DR", "DL", "DR", "L", "DR", "RLD", "LR", "LD", "R", "RLD", "LR", "LD", "D",
        "UD", "UDR", "LU", "DR", "LU", "RU", "DL", "RU", "LR", "UDL", "D", "UD", "UD",
        "UD", "UR", "LD", "UR", "LR", "DL", "UD", "DR", "LD", "UD", "UDR", "UL", "UD",
        "UR", "L", "UR", "LR", "LR", "UL", "U", "U", "UR", "LU", "UR", "LR", "UL"
    };
    private static readonly string[] alphabet = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
    private Coroutine submittingWait;
    private bool hasMoved;
    private bool animating;
    private bool activated;
    private bool hitWall;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        module.OnActivate += Activate;
        foreach (KMSelectable button in buttons)
            button.OnInteract += delegate () { PressButton(button); return false; };
    }

    void Start()
    {
        target = new GregorianCalendar().GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        if (target == 53)
            target = 52;
        Debug.LogFormat("[Snowflakes #{0}] It is currently week {1}, so go to {2}.", moduleId, target, Coordinate(target - 1));
        target--;
        GenerateStart();
    }

    void GenerateStart()
    {
        hasMoved = false;
        currentPosition = Enumerable.Range(0, 52).Where(x => x != target).PickRandom();
        startingPosition = currentPosition;
        Debug.LogFormat("[Snowflakes #{0}] You started in {1}.", moduleId, Coordinate(currentPosition));
    }

    void Activate()
    {
        activated = true;
        for (int i = 0; i < 4; i++)
            displays[i].text = alphabet[currentPosition + ScreenText(i)];
    }

    int ScreenText(int i)
    {
        switch (i)
        {
            case 0:
                return currentPosition / 13 == 0 ? 39 : -13;
            case 1:
                return currentPosition % 13 == 12 ? -12 : 1;
            case 2:
                return currentPosition / 13 == 3 ? -39 : 13;
            default:
                return currentPosition % 13 == 0 ? 12 : -1;
        }
    }

    void PressButton(KMSelectable button)
    {
        var ix = Array.IndexOf(buttons, button);
        button.AddInteractionPunch(.5f);
        if (moduleSolved || animating || !activated)
            return;
        if (!hasMoved)
        {
            submittingWait = StartCoroutine(CountUp());
            hasMoved = true;
        }
        else
        {
            StopCoroutine(submittingWait);
            submittingWait = StartCoroutine(CountUp());
        }
        audio.PlaySoundAtTransform("beep", transform);
        if (!walls[currentPosition].Contains("URDL"[ix]))
        {
            hitWall = true;
            module.HandleStrike();
            Debug.LogFormat("[Snowflakes #{0}] You ran into a wall. Strike!", moduleId);
            StopCoroutine(submittingWait);
            Submit();
        }
        else
            currentPosition += new int[] { -13, 1, 13, -1 }[ix];
    }

    IEnumerator CountUp()
    {
        yield return new WaitForSeconds(5f);
        Submit();
    }

    void Submit()
    {
        if (currentPosition != target || hitWall)
        {
            if (!hitWall) {
                module.HandleStrike();
                Debug.LogFormat("[Snowflakes #{0}] You stopped at {1}. That is not the target. Strike!", moduleId, Coordinate(currentPosition));
                hitWall = false;
            }
            Debug.LogFormat("[Snowflakes #{0}] Resetting...", moduleId);
            StartCoroutine(StrikeAnimation());
        }
        else
        {
            module.HandlePass();
            moduleSolved = true;
            audio.PlaySoundAtTransform("solve", transform);
            Debug.LogFormat("[Snowflakes #{0}] You stopped at {1}. That is the target. Module solved!", moduleId, Coordinate(currentPosition));
            StartCoroutine(SolveAnimation());
        }
    }

    IEnumerator StrikeAnimation()
    {
        animating = true;
        foreach (TextMesh display in displays)
            display.color = strikeColor;
        yield return new WaitForSeconds(2f);
        foreach (TextMesh display in displays)
            display.color = defaultColor;
        GenerateStart();
        Activate();
        animating = false;
    }

    IEnumerator SolveAnimation()
    {
        foreach (TextMesh display in displays)
            display.color = solveColor;
        var elapsed = 0f;
        var duration = 5f;
        while (elapsed < duration)
        {
            foreach (TextMesh display in displays)
            {
                display.color = new Color(
                    solveColor.r, solveColor.g, solveColor.b,
                    Mathf.Lerp(solveColor.a, 0f, elapsed / duration)
                );
            }
            yield return null;
            elapsed += Time.deltaTime;
        }
    }

    string Coordinate(int x)
    {
        var s1 = "ABCDEFGHIJKLM"[x % 13].ToString();
        var s2 = ((x / 13) + 1).ToString();
        return s1 + s2;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} <l/r/u/d> [Presses that button, can be chained, e.g. 'lrrdlu']";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string input)
    {
        var cmd = input.ToLowerInvariant().ToCharArray();
        var directions = new Char[] { 'u', 'r', 'd', 'l' };
        if (cmd.Any(x => !directions.Contains(x)))
            yield break;
        yield return null;
        var tpPosition = currentPosition;
        for (int i = 0; i < cmd.Length; i++)
            tpPosition += new int[] { -13, 1, 13, -1 }[Array.IndexOf(directions, cmd[i])];
        if (tpPosition != target)
            yield return "strike";
        else
            yield return "solve";
        for (int i = 0; i < cmd.Length; i++)
        {
            buttons[Array.IndexOf(directions, cmd[i])].OnInteract();
            yield return new WaitForSeconds(.1f);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        var q = new Queue<int>();
        var allMoves = new List<Movement>();
        q.Enqueue(currentPosition);
        while (q.Count > 0)
        {
            var next = q.Dequeue();
            if (next == target)
                goto readyToSubmit;
            var cell = walls[next];
            var allDirections = "URDL";
            var offsets = new int[] { -13, 1, 13, -1 };
            for (int i = 0; i < 4; i++)
            {
                if (cell.Contains(allDirections[i]) && !allMoves.Any(x => x.start == next + offsets[i]))
                {
                    q.Enqueue(next + offsets[i]);
                    allMoves.Add(new Movement { start = next, end = next + offsets[i], direction = i });
                }
            }
        }
        throw new InvalidOperationException("There is a bug in maze generation.");
        readyToSubmit:
        if (allMoves.Count != 0) // Checks for position already being target
        {
            var lastMove = allMoves.First(x => x.end == target);
            var relevantMoves = new List<Movement> { lastMove };
            while (lastMove.start != currentPosition)
            {
                lastMove = allMoves.First(x => x.end == lastMove.start);
                relevantMoves.Add(lastMove);
            }
            for (int i = 0; i < relevantMoves.Count; i++)
            {
                buttons[relevantMoves[relevantMoves.Count - 1 - i].direction].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
        }
    }

    class Movement
    {
        public int start;
        public int end;
        public int direction;
    }
}

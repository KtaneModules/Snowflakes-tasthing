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
    private int startingPosition; // for Souvenir

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
        target = new GregorianCalendar().GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
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
                return currentPosition % 13 == 13 ? -12 : 1;
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
            module.HandleStrike();
            Debug.LogFormat("[Snowflakes #{0}] You ran into a wall. Strike!", moduleId);
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
        if (currentPosition != target)
        {
            Debug.LogFormat("[Snowflakes #{0}] You stopped at {1}. That is not the target. Strike!", moduleId, Coordinate(currentPosition));
            module.HandleStrike();
            Debug.LogFormat("[Snowflakes #{0}] Resetting...", moduleId);
            StartCoroutine(StrikeAnimation());
        }
        else
        {
            module.HandlePass();
            audio.PlaySoundAtTransform("solve", transform);
            Debug.LogFormat("[Snowflakes #[0]] You stopped at {1}. That is the target. Module solved!", moduleId, Coordinate(currentPosition));
            moduleSolved = true;
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
}

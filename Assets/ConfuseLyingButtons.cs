using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using LyingBtnEnums;

public class ConfuseLyingButtons : MonoBehaviour {

	public KMBombModule module;
	public KMAudio Audio;
	public KMColorblindMode colorblindHandler;

	public Transform moduleTransform;

	public KMSelectable[] toggleButtons;
	public KMSelectable[] buttonSelectables;
	public KMSelectable mainSelectable;

	public MeshRenderer[] screens;
	public MeshRenderer[] buttonMeshes;
	public TextMesh[] toggleTextMeshes, cbBtnTextMeshes, cbLClueTextMeshes, cbRClueTextMeshes;
	public MeshRenderer[] clamps;

	public Material[] buttonColors;
	public Material[] clueMats;

	public AudioClip solveSfx;
	public AudioClip toggleSfx;

	private int moduleId;
	private static int moduleIdCounter = 1;

	private int NUM_ROWS = 3;
	private int NUM_COLS = 3;
	private int MIN_UNSAFE = 2;
	private int MAX_UNSAFE = 2;
	private int MIN_CHOICE = 1;
	private int MAX_CHOICE = 1;
	private int NUM_COLORS = 3;

	const string COLUMNS = "ABC";
	const string ROWS = "123";

	const string COL_COORD = "LMR";
	const string ROW_COORD = "TMB";
	[SerializeField]
	private string CBColors = "RYB";

	private int numLiars;
	private List<int> possNumLiars;

	bool requireColorblind = false;
	bool solving = false;
	bool focused = false;
	List<int> idxesHL = new List<int>();

	private int numPressed;
	private int[] toggleIndexes;
	private readonly string toggleCycleChars = " 12345OX?";
	bool[] safeButtonsAmbiguousPuzzle = null;
	bool[] buttonsPressed;

	private ButtonComplex[] buttons;
	//private PuzzleGenComplex usedPuzzle;

	private Dictionary<string, Material> clueMatDict = new Dictionary<string, Material>();

	void Awake()
	{
		moduleId = moduleIdCounter++;
		try
        {
			requireColorblind = colorblindHandler.ColorblindModeActive;
        }
		catch
        {
			requireColorblind = false;
        }
		foreach (Material clueMat in clueMats)
		{
			//Debug.LogFormat("{0}", clueMat.name);
			clueMatDict.Add(clueMat.name, clueMat);
		}
		//Debug.LogFormat("{0}", clueMatDict.Count);
		toggleIndexes = new int[toggleTextMeshes.Length];
		generatePossNumLiars();
		Debug.Log($"[Confused Lying Buttons #{moduleId}]: Any amount of these unsafe buttons are selected as clues: {string.Join(" ", possNumLiars.Select(x => x.ToString()).ToArray())}");
		for(int i = 0; i < clamps.Length; i++)
			clamps[i].enabled = possNumLiars.Contains(i + 1);
		clamps.Last().enabled = true;
		buttons = generatePuzzle();
		HandleColorblindModeToggle(requireColorblind);
		mainSelectable.OnFocus += delegate { focused = true; };
		mainSelectable.OnDefocus += delegate { focused = false; };
	}
	private void generatePossNumLiars()
	{
		possNumLiars = new List<int>();
		for (int i = MIN_UNSAFE; i <= MAX_UNSAFE; i++)
			possNumLiars.Add(i);
		int numChoices = Random.Range(0, MAX_CHOICE - MIN_CHOICE + 1) + MIN_CHOICE;
		possNumLiars = possNumLiars.Shuffle();
		while (possNumLiars.Count > numChoices)
			possNumLiars.RemoveAt(0);
		/*
		possNumLiars.Clear();
		possNumLiars.Add(2);
		possNumLiars.Add(3);
		possNumLiars.Add(4);
		*/
		possNumLiars.Sort();
	}
	private ButtonComplex[] generatePuzzle()
	{
		ButtonComplex[] buttons = GenerateButtons();
		var puzzleGenerator = new PuzzleGenComplex();
		var buttonCount = buttons.Length;
		buttons = puzzleGenerator.GeneratePuzzle(buttons, possNumLiars, NUM_COLORS, PuzzleType.Confuse);
		buttonsPressed = new bool[buttonCount];
		while (buttons == null)
		{
			buttons = GenerateButtons();
			puzzleGenerator = new PuzzleGenComplex();
			buttons = puzzleGenerator.GeneratePuzzle(buttons, possNumLiars, NUM_COLORS, PuzzleType.Confuse);
			if (!puzzleGenerator.IsSolutionUnique) continue;
		}
        for (int i = 0; i < buttonCount; i++)
        {
            Debug.Log($"[Confused Lying Buttons #{moduleId}]: {buttons[i].toString()}");
            screens[i].material = clueMatDict[buttons[i].clue.Code];
            var btnColorIdx = (int)buttons[i].buttonColor;
            buttonMeshes[i].material = buttonColors[btnColorIdx];
        }
		// Commented out since might as well force the loop to continue if the solution is not unique.
		/*var allPossibleSolutions = puzzleGenerator.GetAllPossiblePositionsDoubt();
		Debug.Log(allPossibleSolutions.Select(a => a.Select(b => b ? "!" : "X").Join("")).Join(","));
		if (!puzzleGenerator.IsSolutionUnique)
		{
			// If for whatever reason the module generated an ambiguous puzzle, do this.
			Debug.Log($"[Confused Lying Buttons #{moduleId}]: Watch out! The module has generated an ambiguous case!");
			safeButtonsAmbiguousPuzzle = new bool[buttonCount];
			for (var x = 0; x < buttons.Length; x++)
				safeButtonsAmbiguousPuzzle[x] = allPossibleSolutions.All(combo => combo[x]);
			Debug.Log($"[Confused Lying Buttons #{moduleId}]: Certainly safe buttons: {Enumerable.Range(0, buttonCount).Where(a => safeButtonsAmbiguousPuzzle[a]).Select(a => string.Format("{0}{1}", ROW_COORD[a / 3], COL_COORD[a % 3])).Join(", ")}");
		}
		else
			safeButtonsAmbiguousPuzzle = null;
		*/
		int[] indexes = new int[toggleIndexes.Length];
		for (int i = 0; i < indexes.Length; i++)
			indexes[i] = i;
		foreach (int index in indexes)
		{
			toggleButtons[index].OnInteract = delegate { pressedToggle(index); return false; };
			toggleButtons[index].OnHighlight = delegate { idxesHL.Add(index); };
			toggleButtons[index].OnHighlightEnded = delegate { idxesHL.Remove(index); };
			buttonSelectables[index].OnInteract = delegate { pressedButton(index); return false; };
			buttonSelectables[index].OnHighlight = delegate { idxesHL.Add(index); };
			buttonSelectables[index].OnHighlightEnded = delegate { idxesHL.Remove(index); };
		}
		return buttons;
	}
	private ButtonComplex[] GenerateButtons()
	{
		var totalButtons = NUM_ROWS * NUM_COLS;
		ButtonComplex[] buttons = new ButtonComplex[totalButtons];
		numLiars = possNumLiars.PickRandom();
		//numLiars = 1;
		var liars = Enumerable.Range(0, totalButtons).ToArray().Shuffle().Take(numLiars).ToArray();
		bool[] truth = Enumerable.Repeat(true, totalButtons).ToArray();
		bool[] isSafe = Enumerable.Repeat(true, totalButtons).ToArray();
		int[] idxPickedColors = Enumerable.Range(0, totalButtons).Select(a => Random.Range(0, NUM_COLORS)).ToArray();
		foreach (var liar in liars)
		{
			isSafe[liar] = false;
			truth[liar] = false;
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			ButtonColor color = (ButtonColor)idxPickedColors[i];
			buttons[i] = new ButtonComplex(color, truth[i], isSafe[i], COLUMNS[i % NUM_COLS] + "" + ROWS[i / NUM_COLS]);
		}
		// Apply Confuse ruleset.
		var pickedIdxSafeBtn = Enumerable.Range(0, totalButtons).Where(a => buttons[a].isSafe).PickRandom();
		buttons[pickedIdxSafeBtn].isTruth = false;

		return buttons;
	}
	
	private void UpdateToggleScns()
    {
		for (var index = 0; index < toggleTextMeshes.Length; index++)
			toggleTextMeshes[index].text = toggleCycleChars[toggleIndexes[index]].ToString();
    }

	private void pressedToggle(int index)
	{
		Audio.PlaySoundAtTransform(toggleSfx.name, toggleButtons[index].transform);
		toggleIndexes[index] = (toggleIndexes[index] + 1) % toggleCycleChars.Length;
		UpdateToggleScns();
	}
	private void pressedButton(int index)
	{
		buttonSelectables[index].AddInteractionPunch(0.2f);
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttonSelectables[index].transform);
		//Debug.Log($"[Doubted Lying Buttons #{moduleId}]: Defuser pressed the {ROW_COORD[index / NUM_COLS]}{COL_COORD[index % NUM_COLS]} Button");
		if (buttons[index].isSafe)
		{
			buttonsPressed[index] = true;
			buttonSelectables[index].OnInteract = null;
			buttonSelectables[index].transform.localPosition = new Vector3(buttonSelectables[index].transform.localPosition.x, 0.014f, buttonSelectables[index].transform.localPosition.z);
			numPressed++;
			if(numPressed == (buttons.Length - numLiars))
			{
				Debug.Log($"[Confused Lying Buttons #{moduleId}]: All safe buttons have been pressed.");
				foreach (KMSelectable button in buttonSelectables)
					button.OnInteract = null;
				foreach (KMSelectable button in toggleButtons)
					button.OnInteract = null;
				StartCoroutine(solveAnimation());
			}
		}
		else if (safeButtonsAmbiguousPuzzle == null)
		{
			Debug.Log($"[Confused Lying Buttons #{moduleId}]: Strike! {ROW_COORD[index / NUM_COLS]}{COL_COORD[index % NUM_COLS]} was not safe! Regenerating Puzzle!");
			foreach (KMSelectable button in buttonSelectables)
				button.OnInteract = null;
			foreach (KMSelectable button in toggleButtons)
				button.OnInteract = null;
			numPressed = 0;
			foreach (MeshRenderer buttonMesh in buttonMeshes)
				buttonMesh.transform.localPosition = new Vector3(buttonMesh.transform.localPosition.x, 0.0159f, buttonMesh.transform.localPosition.z);
			for(int i = 0; i < toggleIndexes.Length; i++)
			{
				toggleIndexes[i] = 0;
				toggleTextMeshes[i].text = toggleCycleChars[toggleIndexes[i]] + "";
			}
			module.HandleStrike();
			buttons = generatePuzzle();
			HandleColorblindModeToggle(requireColorblind);
		}
		else
        {
			var idxesCertainlySafe = Enumerable.Range(0, buttons.Length).Where(a => safeButtonsAmbiguousPuzzle[a]);
			if (idxesCertainlySafe.All(a => buttonsPressed[a]))
            {
				Debug.Log($"[Confused Lying Buttons #{moduleId}]: Normally the module would strike. But since you pressed all buttons that are certainly safe, and the module generated an ambiguous puzzle, the module will solve in response.");
				foreach (KMSelectable button in buttonSelectables)
					button.OnInteract = null;
				foreach (KMSelectable button in toggleButtons)
					button.OnInteract = null;
				StartCoroutine(solveAnimation());
			}
			else
            {
				Debug.Log($"[Confused Lying Buttons #{moduleId}]: Strike! {ROW_COORD[index / NUM_COLS]}{COL_COORD[index % NUM_COLS]} was not safe! At least 1 certainly safe button was also not pressed! Regenerating Puzzle!");
				foreach (KMSelectable button in buttonSelectables)
					button.OnInteract = null;
				foreach (KMSelectable button in toggleButtons)
					button.OnInteract = null;
				numPressed = 0;
				foreach (MeshRenderer buttonMesh in buttonMeshes)
					buttonMesh.transform.localPosition = new Vector3(buttonMesh.transform.localPosition.x, 0.0159f, buttonMesh.transform.localPosition.z);
				for (int i = 0; i < toggleIndexes.Length; i++)
				{
					toggleIndexes[i] = 0;
					toggleTextMeshes[i].text = toggleCycleChars[toggleIndexes[i]] + "";
				}
				module.HandleStrike();
				buttons = generatePuzzle();
				HandleColorblindModeToggle(requireColorblind);
			}
        }
	}
	private IEnumerator solveAnimation()
	{
		solving = true;
		yield return new WaitForSeconds(0.0f);
		Audio.PlaySoundAtTransform(solveSfx.name, transform);
		while (moduleTransform.transform.localScale.x > 0f)
		{
			float x = Mathf.Max(moduleTransform.transform.localScale.x - 0.02f, 0f);
			float z = Mathf.Max(moduleTransform.transform.localScale.z - 0.02f, 0f);
			moduleTransform.transform.localScale = new Vector3(x, moduleTransform.transform.localScale.y, z);
			yield return new WaitForSeconds(0.01f);
		}
		moduleTransform.transform.localScale = new Vector3(0f, 0f, 0f);
		module.HandlePass();
	}
	void Update()
	{
		var possibleKeys = new[] {
			new[] { KeyCode.Backspace, KeyCode.Delete }, // Blank
			new[] { KeyCode.Keypad1, KeyCode.Alpha1 }, // 1
			new[] { KeyCode.Keypad2, KeyCode.Alpha2 }, // 2
			new[] { KeyCode.Keypad3, KeyCode.Alpha3 }, // 3
			new[] { KeyCode.Keypad4, KeyCode.Alpha4 }, // 4
			new[] { KeyCode.Keypad5, KeyCode.Alpha5 }, // 5
			new[] { KeyCode.Keypad0, KeyCode.Alpha0, KeyCode.O }, // O
			new[] { KeyCode.X }, // X
			new[] { KeyCode.T, KeyCode.Question }, // ?
		};
		if (!focused) return;
		var firstIdxPressed = Enumerable.Range(0, possibleKeys.Length).IndexOf(a => possibleKeys[a].Any(b => Input.GetKeyDown(b)));
		if (firstIdxPressed != -1 && idxesHL.Any())
        {
			foreach (var idxHL in idxesHL)
			{
				toggleIndexes[idxHL] = firstIdxPressed;
				Audio.PlaySoundAtTransform(toggleSfx.name, toggleButtons[idxHL].transform);
			}
			UpdateToggleScns();
        }

	}
	private void HandleColorblindModeToggle(bool enableColorblind = false)
    {
		for (int i = 0; i < buttons.Length; i++)
		{
			var curClueName = clueMatDict[buttons[i].clue.Code].name;
			var rgxMatchClueClrCnt = Regex.Match(curClueName, @"^GDN[BRY]LGD\d\d\d");
			var rgxMatchClueClrComp = Regex.Match(curClueName, @"^GDN[BRY]LGDN[BRY]L");
			if (rgxMatchClueClrCnt.Success)
            {
				var clrL = rgxMatchClueClrCnt.Value[3].ToString();
				cbLClueTextMeshes[i].text = enableColorblind ? clrL : "";
				cbLClueTextMeshes[i].color = enableColorblind && clrL == "Y" ? Color.black : Color.white;
				cbRClueTextMeshes[i].text = "";
            }
			else if (rgxMatchClueClrComp.Success)
            {
				var clrL = rgxMatchClueClrComp.Value[3].ToString();
				var clrR = rgxMatchClueClrComp.Value[8].ToString();
				cbLClueTextMeshes[i].text = enableColorblind ? clrL : "";
				cbLClueTextMeshes[i].color = enableColorblind && clrL == "Y" ? Color.black : Color.white;
				cbRClueTextMeshes[i].text = enableColorblind ? clrR : "";
				cbRClueTextMeshes[i].color = enableColorblind && clrR == "Y" ? Color.black : Color.white;
			}
			else
			{
				cbLClueTextMeshes[i].text = "";
				cbRClueTextMeshes[i].text = "";
			}
			var btnColorIdx = (int)buttons[i].buttonColor;
			cbBtnTextMeshes[i].text = enableColorblind ? CBColors[btnColorIdx].ToString() : "";
			cbBtnTextMeshes[i].color = enableColorblind && btnColorIdx == 1 ? Color.black : Color.white;
		}
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = "\"!{0} (T)oggle [position] (12345OXB?)\" will press the toggle button in the position until the desired text shows up, with \"B\" corresponding to blank. \"!{0} (P)ress [positions]\" will press the buttons listed in the positions you provided. The list of valid positions is as follows: TL/1 TM/2 TR/3 ML/4 MM/5 MR/6 BL/7 BM/8 BR/9.";
	readonly string[] allowedBtnPos = new[] { "TL", "TM", "TR", "ML", "MM", "MR", "BL", "BM", "BR", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
	readonly string refValidNotesOrdered = "B12345OX?";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
	{
		if (solving) {
			yield return "sendtochaterror The module is no longer accepting any more inputs.";
			yield break;
		}
		var rgxToggleCmd = Regex.Match(command, string.Format(@"^\s*T(OGGLE)?(\s({0})\s[12345OX\?B])+\s*$", allowedBtnPos.Join("|")), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		var rgxPressCmd = Regex.Match(command, string.Format(@"^\s*P(RESS)?(\s({0}))+\s*$", allowedBtnPos.Join("|")), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		var rgxColorblindCmd = Regex.Match(command, @"^colou?rblind$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (rgxColorblindCmd.Success)
        {
			yield return null;
			requireColorblind ^= true;
			HandleColorblindModeToggle(requireColorblind);
        }
		else if (rgxPressCmd.Success)
        {
            var pressCmdVal = rgxPressCmd.Value.ToUpperInvariant().Trim().Split().Skip(1).ToArray();
			yield return null;
            for (var x = 0; x < pressCmdVal.Count(); x++)
            {
				var curIdxPressCmd = allowedBtnPos.IndexOf(a => a == pressCmdVal[x]);
				buttonSelectables[curIdxPressCmd].OnInteract();
				yield return "trywaitcancel 0.1 Button interactions have been canceled!";
				if (solving)
				{
					yield return "solve";
					yield break;
				}
            }
		}
		else if (rgxToggleCmd.Success)
        {
			var toggleCmdVal = rgxToggleCmd.Value.ToUpperInvariant().Trim().Split().Skip(1).ToArray();
			yield return null;
            for (var x = 0; x < toggleCmdVal.Length; x += 2)
            {
				var refButton = allowedBtnPos.IndexOf(a => a == toggleCmdVal[x]);
				var refNoteIdx = refValidNotesOrdered.IndexOf(toggleCmdVal[x + 1]);
				while (refNoteIdx != toggleIndexes[refButton])
                {
					toggleButtons[refButton].OnInteract();
					yield return "trywaitcancel 0.1 Toggle interactions have been canceled!";
                }

			}
        }
		else
			yield return "sendtochat An error occurred because {0} inputted something wrong. Check the command again for any typos.";
	}
    IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
		for(int i = 0; i < buttons.Length; i++)
		{
			if(buttons[i].isSafe && buttonSelectables[i].OnInteract != null)
			{
				buttonSelectables[i].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}
	}
}

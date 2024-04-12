using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class LyingButtons : MonoBehaviour {

	public KMBombModule module;
	public KMAudio Audio;

	public Transform moduleTransform;

	public KMSelectable[] toggleButtons;
	public KMSelectable[] buttonSelectables;

	public MeshRenderer[] screens;
	public MeshRenderer[] buttonMeshes;
	public TextMesh[] toggleTextMeshes;
	public MeshRenderer[] clamps;

	public Material[] buttonColors;
	public Material[] clueMats;

	public AudioClip solveSfx;
	public AudioClip toggleSfx;

	private int moduleId;
	private static int moduleIdCounter = 1;

	private int NUM_ROWS = 3;
	private int NUM_COLS = 3;
	private int MIN_LIARS = 1;
	private int MAX_LIARS = 4;
	private int MIN_CHOICE = 1;
	private int MAX_CHOICE = 3;
	private int NUM_COLORS = 3;

	private string COLUMNS = "ABC";
	private string ROWS = "123";

	private string COL_COORD = "LMR";
	private string ROW_COORD = "TMB";

	private int numLiars;
	private List<int> possNumLiars;

	

	private int numPressed;
	private int[] toggleIndexes;
	private readonly string toggleCycleChars = " 12345OX";

	private Button[] buttons;

	private Dictionary<string, Material> clueMatDict = new Dictionary<string, Material>();

	void Awake()
	{
		moduleId = moduleIdCounter++;
		foreach (Material clueMat in clueMats)
		{
			Debug.LogFormat("{0}", clueMat.name);
			clueMatDict.Add(clueMat.name, clueMat);
		}
		toggleIndexes = new int[toggleTextMeshes.Length];
		generatePossNumLiars();
		Debug.Log($"[Lying Buttons #{moduleId}]: Possible number of liars: {string.Join(" ", possNumLiars.Select(x => x.ToString()).ToArray())}");
		for(int i = 0; i < clamps.Length; i++)
		{
			if (!possNumLiars.Contains(i + 1))
				clamps[i].transform.localScale = new Vector3(0f, 0f, 0f);
		}
		buttons = generatePuzzle();
	}
	private void generatePossNumLiars()
	{
		possNumLiars = new List<int>();
		for (int i = MIN_LIARS; i <= MAX_LIARS; i++)
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
	private Button[] generatePuzzle()
	{
		Button[] buttons = GenerateButtons();
		buttons = new PuzzleGen().generatePuzzle(buttons, possNumLiars, NUM_COLORS);
		while (buttons == null)
		{
			buttons = GenerateButtons();
			buttons = new PuzzleGen().generatePuzzle(buttons, possNumLiars, NUM_COLORS);
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			Debug.Log($"[Lying Buttons #{moduleId}]: {buttons[i].toString()}");
			screens[i].material = clueMatDict[buttons[i].clue.getCode()];
			buttonMeshes[i].material = buttonColors[(int)buttons[i].buttonColor];
		}
		int[] indexes = new int[toggleIndexes.Length];
		for (int i = 0; i < indexes.Length; i++)
			indexes[i] = i;
		foreach (int index in indexes)
		{
			toggleButtons[index].OnInteract = delegate { pressedToggle(index); return false; };
			buttonSelectables[index].OnInteract = delegate { pressedButton(index); return false; };
		}
		return buttons;
	}
	private Button[] GenerateButtons()
	{
		Button[] buttons = new Button[NUM_ROWS * NUM_COLS];
		string liars = "";
		for (int i = 0; i < buttons.Length; i++)
			liars += i;
		numLiars = possNumLiars[Random.Range(0, possNumLiars.Count)];
		//numLiars = 1;
		liars = new string(liars.ToCharArray().Shuffle()).Substring(0, numLiars);
		bool[] truth = { true, true, true, true, true, true, true, true, true };
		foreach (char liar in liars)
			truth[liar - '0'] = false;
		for (int i = 0; i < buttons.Length; i++)
		{
			ButtonColor color = (ButtonColor)Random.Range(0, NUM_COLORS);
			buttons[i] = new Button(color, truth[i], COLUMNS[i % NUM_COLS] + "" + ROWS[i / NUM_COLS]);
		}
		return buttons;
	}
	
	private void pressedToggle(int index)
	{
		Audio.PlaySoundAtTransform(toggleSfx.name, toggleButtons[index].transform);
		toggleIndexes[index] = (toggleIndexes[index] + 1) % toggleCycleChars.Length;
		toggleTextMeshes[index].text = toggleCycleChars[toggleIndexes[index]] + "";
	}
	private void pressedButton(int index)
	{
		buttonSelectables[index].AddInteractionPunch(0.2f);
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttonSelectables[index].transform);
		Debug.Log($"[Lying Buttons #{moduleId}]: Defuser pressed the {ROW_COORD[index / NUM_COLS]}{COL_COORD[index % NUM_COLS]} Button");
		if (buttons[index].isTruth)
		{
			buttonSelectables[index].OnInteract = null;
			buttonSelectables[index].transform.localPosition = new Vector3(buttonSelectables[index].transform.localPosition.x, 0.014f, buttonSelectables[index].transform.localPosition.z);
			numPressed++;
			if(numPressed == (buttons.Length - numLiars))
			{
				foreach (KMSelectable button in buttonSelectables)
					button.OnInteract = null;
				foreach (KMSelectable button in toggleButtons)
					button.OnInteract = null;
				StartCoroutine(solveAnimation());
			}
		}
		else
		{
			Debug.Log($"[Lying Buttons #{moduleId}]: Strike! Regenerating Puzzle!");
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
		}

	}
	private IEnumerator solveAnimation()
	{
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
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} (T)oggle [position] (12345OXB) will press the toggle button in the position until the desired text shows up. !{0} (P)ress [positions] will press the buttons listed in the positions you provided. The list of valid positions is as follows: TL/1 TM/2 TR/3 ML/4 MM/5 MR/6 BL/7 BM/8 BR/9.";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] param = command.ToUpper().Split(' ');
		if ((Regex.IsMatch(param[0], @"^\s*TOGGLE\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(param[0], @"^\s*T\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) && param.Length == 3)
		{
			yield return null;
			var pos = posToNum(param[1]);
			var text = param[2].Replace("B", " ");
			if (pos == -1 || !isValidText(text))
				yield return "sendtochat An error occured because the user inputted something wrong.";
			while(!toggleTextMeshes[pos].text.Equals(text))
			{
				toggleButtons[pos].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}
		else if (Regex.IsMatch(param[0], @"^\s*PRESS\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(param[0], @"^\s*P\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			for(int i = 1; i < param.Length; i++)
			{
				var pos = posToNum(param[i]);
				if (pos >= 0 && buttonSelectables[pos].OnInteract != null)
				{
					buttonSelectables[pos].OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
		else
			yield return "sendtochat An error occured because the user inputted something wrong.";
		yield return null;
	}
	private int posToNum(string str)
	{
		switch(str)
		{
			case "TL":
			case "1":
				return 0;
			case "TM":
			case "2":
				return 1;
			case "TR":
			case "3":
				return 2;
			case "ML":
			case "4":
				return 3;
			case "MM":
			case "5":
				return 4;
			case "MR":
			case "6":
				return 5;
			case "BL":
			case "7":
				return 6;
			case "BM":
			case "8":
				return 7;
			case "BR":
			case "9":
				return 8;
		}
		return -1;
	}
	private bool isValidText(string str)
	{
		if (str.Length == 1)
			return toggleCycleChars.Contains(str);
		return false;
	}
	IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
		for(int i = 0; i < buttons.Length; i++)
		{
			if(buttons[i].isTruth && buttonSelectables[i].OnInteract != null)
			{
				buttonSelectables[i].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LyingBtnEnums;
namespace LyingBtnEnums
{
    public enum PuzzleType {
        Normal,
        Doubt,
        Confuse,
        Quizzer
    };
}

public class PuzzleGenComplex
{
    static Dictionary<string, List<List<int>>> storedCombinations = new Dictionary<string, List<List<int>>>();
    private readonly int NUM_TOTAL_RETRIES = 1000;
    private List<ClueObj> allTrueClues = new List<ClueObj>(), allFalseClues = new List<ClueObj>();
    private ButtonComplex[] storedButtons;
    private int[] possibleLiarsStored;
    private bool solutionUnique = false;
    public bool IsSolutionUnique => solutionUnique;
    public PuzzleType usedPuzzle;

    public ButtonComplex[] GeneratePuzzle(ButtonComplex[] buttons, List<int> possNumLiars, int numColors, PuzzleType puzzle = PuzzleType.Normal)
    {
        usedPuzzle = puzzle;
        List<ClueObj> allPossibleClues = GetAllPossibleClueIds(buttons, possNumLiars, numColors);
        storedButtons = buttons;
        possibleLiarsStored = possNumLiars.ToArray();
        //Debug.LogFormat("Number of clues: {0}", allPossibleClues.Count);
        ClueTesterComplex clueTester = new ClueTesterComplex();
        foreach (ClueObj clue in allPossibleClues)
        {
            if (clueTester.testClue(clue, buttons))
                allTrueClues.Add(clue);
            else
                allFalseClues.Add(clue);
        }
        /*
        Debug.LogFormat("All true clues: {0}", allTrueClues.Count);
        foreach(ClueObj clue in allTrueClues)
            Debug.LogFormat("{0}", clue.toString);
        Debug.LogFormat("All false clues: {0}", allFalseClues.Count);
        foreach (ClueObj clue in allFalseClues)
            Debug.LogFormat("{0}", clue.toString);
        */
        return GetValidPuzzle(buttons, clueTester, possNumLiars, allTrueClues, allFalseClues);
    }
	private List<ClueObj> GetAllPossibleClueIds(ButtonComplex[] buttons, List<int> possNumLiars, int numColors)
    {
        possNumLiars.Sort();
        int maxLiars = possNumLiars[possNumLiars.Count - 1];
        string[] locIds = { "RA", "RB", "RC", "CA", "CB", "CC" };
        string[] locNames = { "top row", "middle row", "bottom row", "left column", "middle column", "right column" };
        List<ClueObj> clues = new List<ClueObj>();

        // Generate clues based on the amount of unsafe buttons on each 
        List<ClueObj> locClues = new List<ClueObj>();
        ClueTesterComplex clueTester = new ClueTesterComplex();
        for (int i = 0; i <= maxLiars && i <= 3; i++)
        {
            for(int j = 0; j < locIds.Length; j++)
                locClues.Add(new CEqual($"The {locNames[j]} contains exactly {i} unsafe buttons", locIds[j], "LLL", i));
        }
        for(int i = 1; i < maxLiars && i < 3; i++)
        {
            for (int j = 0; j < locIds.Length; j++)
            {
                locClues.Add(new CLeast($"The {locNames[j]} contains at least {i} unsafe button(s)", locIds[j], "LLL", i));
                locClues.Add(new CFewer($"The {locNames[j]} contains {i} or fewer unsafe buttons", locIds[j], "LLL", i));
            }
        }
        for(int i = 0; i < locIds.Length; i++)
        {
            for (int j = i + 1; j < locIds.Length; j++)
                locClues.Add(new CEqual($"The {locNames[i]} contains an equal number of unsafe buttons as the {locNames[j]}", locIds[i], "LLL", locIds[j], "LLL"));
        }
        for (int i = 0; i < locIds.Length; i++)
        {
            for (int j = 0; j < locIds.Length; j++)
            {
                if(i != j)
                    locClues.Add(new CGreater($"The {locNames[i]} contains more unsafe buttons than the {locNames[j]}", locIds[i], "LLL", locIds[j], "LLL"));
            }
        }
        List<ClueObj> trueLocClues = new List<ClueObj>(), falseLocClues = new List<ClueObj>();
        foreach(ClueObj clue in locClues)
        {
            if (clueTester.testClue(clue, buttons))
                trueLocClues.Add(clue);
            else
                falseLocClues.Add(clue);
        }
        trueLocClues = trueLocClues.Shuffle();
        falseLocClues = falseLocClues.Shuffle();
        while (trueLocClues.Count > 9)
            trueLocClues.RemoveAt(0);
        while (falseLocClues.Count > 9)
            falseLocClues.RemoveAt(0);
        clues.AddRange(trueLocClues);
        clues.AddRange(falseLocClues);

        for (int i = 1; i <= maxLiars && i <= 3; i++)
        {
            clues.Add(new CEqual($"The number of rows that contains an unsafe button is exactly {i}", "GD", "DRL", i));
            clues.Add(new CEqual($"The number of columns that contains an unsafe button is exactly {i}", "GD", "DCL", i));
        }
        if (possNumLiars.Count > 1)
        foreach(int i in possNumLiars)
            clues.Add(new CEqual($"There are exactly {i} unsafe buttons", "GD", "LLL", i));
        
        for(int i = 0; i <= maxLiars; i++)
        {
            if(i != 1)
                clues.Add(new CEqual($"There are exactly {i} unsafe buttons that are orthogonally adjacent to one another", "GD", "AJL", i));
        }
        
        locIds = new string[] { "A1", "B1", "C1", "A2", "B2", "C2", "A3", "B3", "C3" };
        locNames = new string[] { "top left", "top middle", "top right", "middle left", "center", "middle right", "bottom left", "bottom middle", "bottom right" };
        for (int i = 0; i < 9; i++)
        {
            clues.Add(new CEqual($"The {locNames[i]} button is safe", locIds[i], "LLL", 0));
            clues.Add(new CEqual($"The {locNames[i]} button is not safe", locIds[i], "LLL", 1));
        }

        /*
        int[] maxNumAdj = { 2, 3, 2, 3, 4, 3, 2, 3, 2 };
        for (int i = 0; i < 9; i++)
        {
            for(int j = 0; j <= maxNumAdj[i] && j <= maxLiars; j++)
                clues.Add(new CEqual("There is exactly " + j + " liars that are orthogonally adjacent to the " + locNames[i] + " button", "J" + (i + 1), "LLL", j));
        }
        for (int i = 0; i < 9; i++)
        {
            for (int j = 1; j < maxNumAdj[i] && j < maxLiars; j++)
            {
                clues.Add(new CLeast("There is at least " + j + " liar(s) that are orthogonally adjacent to the " + locNames[i] + " button", "J" + (i + 1), "LLL", j));
                clues.Add(new CFewer("There are " + j + " or fewer liars that are orthogonally adjacent to the " + locNames[i] + " button", "J" + (i + 1), "LLL", j));
            }  
        }
        */
        
        int[] colorSums = new int[numColors];
        foreach (ButtonComplex button in buttons)
            colorSums[(int)button.buttonColor]++;
        List<ClueObj> colorClues = new List<ClueObj>();
        for(int i = 0; i < numColors; i++)
        {
            if(colorSums[i] > 1)
            {
                for(int j = 0; j < numColors; j++)
                {
                    if (i != j)
                        colorClues.Add(new CGreater($"There are more {GetColorName((ButtonColor)i)} unsafe buttons than {GetColorName((ButtonColor)j)} unsafe buttons", "GD", "N" + GetColorCode((ButtonColor)i) + "L", "N" + GetColorCode((ButtonColor)j) + "L"));
                }
            }
        }
        for (int i = 0; i < numColors; i++)
        {
            if (colorSums[i] > 0)
            {
                for (int j = i + 1; j < numColors; j++)
                {
                    if (colorSums[j] > 0)
                        colorClues.Add(new CEqual($"The number of {GetColorName((ButtonColor)i)} unsafe buttons are equal to the number of {GetColorName((ButtonColor)j)} unsafe buttons", "GD", "N" + GetColorCode((ButtonColor)i) + "L", "GD", "N" + GetColorCode((ButtonColor)j) + "L"));
                }
            }
        }
        for(int i = 0; i < numColors; i++)
        {
            if(colorSums[i] > 0)
            {
                for(int j = 0; j <= colorSums[i] && j <= maxLiars; j++)
                    colorClues.Add(new CEqual($"The {GetColorName((ButtonColor)i)} buttons contains exactly {j} unsafe button(s)", "GD", "N" + GetColorCode((ButtonColor)i) + "L", j));
            }
        }
        for (int i = 0; i < numColors; i++)
        {
            for (int j = 1; j < colorSums[i] && j < maxLiars; j++)
            {
                colorClues.Add(new CLeast($"The {GetColorName((ButtonColor)i)} buttons contains at least {j} unsafe button(s)", "GD", "N" + GetColorCode((ButtonColor)i) + "L", j));
                colorClues.Add(new CFewer($"The {GetColorName((ButtonColor)i)} buttons contains {j} or fewer unsafe buttons", "GD", "N" + GetColorCode((ButtonColor)i) + "L", j));
            }
        }
        List<ClueObj> trueColorClues = new List<ClueObj>(), falseColorClues = new List<ClueObj>();
        foreach(ClueObj clue in colorClues)
        {
            if (clueTester.testClue(clue, buttons))
                trueColorClues.Add(clue);
            else
                falseColorClues.Add(clue);
        }
        trueColorClues = trueColorClues.Shuffle();
        while (trueColorClues.Count > 9)
            trueColorClues.RemoveAt(0);
        falseColorClues = falseColorClues.Shuffle();
        while (falseColorClues.Count > 9)
            falseColorClues.RemoveAt(0);
        clues.AddRange(trueColorClues);
        clues.AddRange(falseColorClues);
        int dc = 0;
        foreach(int colorSum in colorSums)
        {
            if (colorSum > 0)
                dc++;
        }
        if(dc > 1)
        {
            for (int i = 1; i <= maxLiars && i <= dc; i++)
                clues.Add(new CEqual("The number of colors that contains an unsafe button is exactly " + i, "GD", "CDL", i));
        }
        
        return clues;
    }
    private ButtonComplex[] GetValidPuzzle(ButtonComplex[] buttons, ClueTesterComplex clueTester, List<int> possNumLiars, List<ClueObj> trueClues, List<ClueObj> falseClues)
    {
        for (int z = 0; z < NUM_TOTAL_RETRIES && !solutionUnique; z++)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].isTruth)
                    buttons[i].clue = trueClues.PickRandom();
                else
                    buttons[i].clue = falseClues.PickRandom();
            }
            solutionUnique = CanSolveAny(realButtons: buttons, clueTester: clueTester, possNumLiars: possNumLiars);
        }
        return buttons;
    }
    private bool CanSolveAny(ButtonComplex[] realButtons, List<int> possNumLiars, ClueTesterComplex clueTester = null)
    {
        if (clueTester == null)
            clueTester = new ClueTesterComplex();
        ButtonComplex[] buttons = Copy(realButtons);
        var possibleBtnColors = buttons.Select(a => a.buttonColor).Distinct().ToArray();
        var buttonsTotal = realButtons.Length;
        var possibleIdxUnsafePos = possNumLiars.SelectMany(a => CreateIdxCombinations(buttonsTotal, a)).ToList();
        int solCount = 0;
        for (var x = 0; x < possibleIdxUnsafePos.Count() && solCount < 2; x++)
        {
            var curIdxLying = possibleIdxUnsafePos[x];
            var arraySafeBtns = Enumerable.Range(0, buttonsTotal).Select(a => !curIdxLying.Contains(a)).ToArray();
            for (var n = 0; n < buttonsTotal; n++)
                buttons[n].isSafe = arraySafeBtns[n];
            // Reset this so that the conditions are followed later.
            foreach (var btn in buttons)
                btn.isTruth = btn.isSafe;
            // Default rule: a button that is safe is telling the truth.
            switch (usedPuzzle)
            {
                case PuzzleType.Quizzer:
                case PuzzleType.Confuse: // Apply Confuse/Quizzer ruleset.
                    {
                        // Use below condition if the puzzle is a Quizzer ruleset.
                        if (usedPuzzle == PuzzleType.Quizzer && buttons.All(btnClue => clueTester.testClue(btnClue.clue, buttons) == btnClue.isTruth))
                            solCount++;
                        // Throw a random confused button and check for each safe button's case.
                        int[] curComboSafeBtns = Enumerable.Range(0, buttons.Length).Where(a => buttons[a].isSafe).ToArray();
                        for (int i = 0; i < curComboSafeBtns.Length && solCount < 2; i++)
                        {
                            int idx = curComboSafeBtns[i];
                            var copiedButtons = Copy(buttons);
                            copiedButtons[idx].isTruth = false;
                            var solutionValid = copiedButtons.All(btnClue => clueTester.testClue(btnClue.clue, buttons) == btnClue.isTruth);
                            if (solutionValid)
                                solCount++;
                        }
                    }
                    break;
                case PuzzleType.Doubt: // Apply Doubt ruleset.
                    { // Unsafe buttons tell the truth, but safe buttons that are the same color as an unsafe button lies.
                        foreach (var curBtnColor in possibleBtnColors)
                        {
                            var sharedbuttonIdxesClr = Enumerable.Range(0, buttonsTotal).Where(a => buttons[a].buttonColor == curBtnColor).ToArray();
                            if (sharedbuttonIdxesClr.Any(a => !buttons[a].isSafe))
                                foreach (var idx in sharedbuttonIdxesClr)
                                    buttons[idx].isTruth ^= true;
                        }
                        var solutionValid = buttons.All(btnClue => clueTester.testClue(btnClue.clue, buttons) == btnClue.isTruth);
                        if (solutionValid)
                            solCount++;
                    }
                    break;
                default: // Normal Lying Buttons ruleset.
                    {
                        var solutionValid = buttons.All(btnClue => clueTester.testClue(btnClue.clue, buttons) == btnClue.isTruth);
                        if (solutionValid)
                            solCount++;
                    }
                    break;
            }
            

        }
        return solCount == 1;
    }
    public List<ButtonComplex[]> GetAllPossiblePositions()
    {
        var clueTester = new ClueTesterComplex();
        ButtonComplex[] buttons = Copy(storedButtons);
        var possibleBtnColors = buttons.Select(a => a.buttonColor).Distinct().ToArray();
        var buttonsTotal = storedButtons.Length;
        var possibleUnsafePos = possibleLiarsStored.SelectMany(a => CreateIdxCombinations(buttonsTotal, a)).ToList();
        var output = new List<ButtonComplex[]>();
        for (var x = 0; x < possibleUnsafePos.Count(); x++)
        {
            var curIdxLying = possibleUnsafePos[x];
            var arraySafeBtns = Enumerable.Range(0, buttonsTotal).Select(a => !curIdxLying.Contains(a)).ToArray();
            for (var n = 0; n < buttonsTotal; n++)
                buttons[n].isSafe = arraySafeBtns[n];
            // Reset this so that the conditions are followed later.
            foreach (var btn in buttons)
                btn.isTruth = btn.isSafe;
            // Default rule: A button that is safe is telling the truth. A button that is not safe is not telling the truth.
            switch (usedPuzzle)
            {
                default:
                    {
                        var solutionValid = buttons.All(btnClue => clueTester.testClue(btnClue.clue, buttons) == btnClue.isTruth);
                        if (solutionValid)
                            output.Add(Copy(buttons));
                    }
                    break;
                case PuzzleType.Doubt:
                    {
                        foreach (var curBtnColor in possibleBtnColors)
                        {
                            var sharedbuttonIdxesClr = Enumerable.Range(0, buttonsTotal).Where(a => buttons[a].buttonColor == curBtnColor).ToArray();
                            if (sharedbuttonIdxesClr.Any(a => !buttons[a].isSafe))
                                foreach (var idx in sharedbuttonIdxesClr)
                                    buttons[idx].isTruth ^= true;
                        }
                        var solutionValid = buttons.All(btnClue => clueTester.testClue(btnClue.clue, buttons) == btnClue.isTruth);
                        if (solutionValid)
                            output.Add(Copy(buttons));
                    }
                    break;
                case PuzzleType.Confuse:
                    {
                        foreach (var idx in Enumerable.Range(0, buttons.Length).Where(a => buttons[a].isSafe).ToArray())
                        {
                            var copiedButtons = Copy(buttons);
                            copiedButtons[idx].isTruth = false;
                            var solutionValid = buttons.All(btnClue => clueTester.testClue(btnClue.clue, buttons) == btnClue.isTruth);
                            if (solutionValid)
                                output.Add(Copy(copiedButtons));
                        }
                    }
                    break;
            }
            

        }
        return output;
    }
    private List<List<int>> CreateIdxCombinations(int itemCount, int pickCount = 0)
    {
        var searchString = string.Format("{0},{1}", itemCount, pickCount);
        if (storedCombinations.ContainsKey(searchString))
            return storedCombinations[searchString];
        // Generate a set of combinations based on the amount of items.
        var output = new List<List<int>> { new List<int>() }; // Start with an empty list, as that is N choose 0. Needed to iterate for later items.
        for (var x = 0; x < pickCount; x++)
        {
            var nextOutput = new List<List<int>>();
            foreach (var curList in output)
            {
                var possibleValues = Enumerable.Range(0, itemCount).Where(a => !(curList.Contains(a) || curList.Any() && a < curList.Max())).ToArray();
                // Check if the value is not present in the list, AND if the list is not empty, the value is larger than the biggest value stored.
                foreach (var value in possibleValues)
                    nextOutput.Add(curList.Concat(new[] { value }).ToList());
            }
            output = nextOutput;
        }
        storedCombinations.Add(searchString, output.ToList());
        return output;
    }

    private string GetColorCode(ButtonColor color)
    {
        switch(color)
        {
            case ButtonColor.RED:
                return "R";
            case ButtonColor.YELLOW:
                return "Y";
            case ButtonColor.BLUE:
                return "B";
        }
        return null;
    }
    private string GetColorName(ButtonColor color)
    {
        switch (color)
        {
            case ButtonColor.RED:
                return "red";
            case ButtonColor.YELLOW:
                return "yellow";
            case ButtonColor.BLUE:
                return "blue";
        }
        return null;
    }
    private ButtonComplex[] Copy(ButtonComplex[] buttons)
    {
        ButtonComplex[] copy = new ButtonComplex[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            copy[i] = new ButtonComplex(buttons[i].buttonColor, buttons[i].isTruth, buttons[i].isSafe, buttons[i].coord);
            copy[i].clue = buttons[i].clue;
        }
        return copy;
    }
}

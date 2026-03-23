using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueTesterComplex
{
	public bool testClue(ClueObj clue, ButtonComplex[] buttons)
	{
		string code = clue.Code;
		switch(code.Substring(code.Length - 2))
		{
			case "EQ": return evalEqualClue(code, buttons);
			case "GR": return evalGreaterClue(code, buttons);
			case "LT": return evalLeastClue(code, buttons);
			case "FR": return evalFewerClue(code, buttons);
		}
		return false;
	}
	private bool evalEqualClue(string code, ButtonComplex[] buttons)
	{
		ButtonComplex[] loc1 = getButtons(code.Substring(0, 2), buttons);
		ButtonComplex[] loc2 = getButtons(code.Substring(5, 2), buttons);
		int s1 = getAttributeCount(loc1, code.Substring(2, 3), buttons);
		int s2;
		if(isNum(code.Substring(7, 3)))
			s2 = int.Parse(code.Substring(7, 3));
		else
			s2 = getAttributeCount(loc2, code.Substring(7, 3), buttons);
		return s1 == s2;
	}
	private bool evalGreaterClue(string code, ButtonComplex[] buttons)
	{
		ButtonComplex[] loc1 = getButtons(code.Substring(0, 2), buttons);
		ButtonComplex[] loc2 = getButtons(code.Substring(5, 2), buttons);
		int s1 = getAttributeCount(loc1, code.Substring(2, 3), buttons);
		int s2 = getAttributeCount(loc2, code.Substring(7, 3), buttons);
		return s1 > s2;
	}
	private bool evalLeastClue(string code, ButtonComplex[] buttons)
	{
		ButtonComplex[] loc1 = getButtons(code.Substring(0, 2), buttons);
		int s1 = getAttributeCount(loc1, code.Substring(2, 3), buttons);
		int s2 = int.Parse(code.Substring(7, 3));
		return s1 >= s2;
	}
	private bool evalFewerClue(string code, ButtonComplex[] buttons)
	{
		ButtonComplex[] loc1 = getButtons(code.Substring(0, 2), buttons);
		int s1 = getAttributeCount(loc1, code.Substring(2, 3), buttons);
		int s2 = int.Parse(code.Substring(7, 3));
		return s1 <= s2;
	}
	private ButtonComplex[] getButtons(string locId, ButtonComplex[] buttons)
	{
		switch(locId)
		{
			case "GD": return buttons; // All Buttons
			case "RA": return new ButtonComplex[] { buttons[0], buttons[1], buttons[2] }; // Row 1
			case "RB": return new ButtonComplex[] { buttons[3], buttons[4], buttons[5] }; // Row 2
			case "RC": return new ButtonComplex[] { buttons[6], buttons[7], buttons[8] }; // Row 3
			case "CA": return new ButtonComplex[] { buttons[0], buttons[3], buttons[6] }; // Col A
			case "CB": return new ButtonComplex[] { buttons[1], buttons[4], buttons[7] }; // Col B
			case "CC": return new ButtonComplex[] { buttons[2], buttons[5], buttons[8] }; // Col C
			case "J1": return new ButtonComplex[] { buttons[1], buttons[3]}; // Buttons adjacent to TL
			case "J2": return new ButtonComplex[] { buttons[0], buttons[2], buttons[4] }; // Buttons adjacent to TM
			case "J3": return new ButtonComplex[] { buttons[1], buttons[5] }; // Buttons adjacent to TR
			case "J4": return new ButtonComplex[] { buttons[0], buttons[4], buttons[6] }; // Buttons adjacent to ML
			case "J5": return new ButtonComplex[] { buttons[1], buttons[3], buttons[5], buttons[7] }; // Buttons adjacent to MM
			case "J6": return new ButtonComplex[] { buttons[2], buttons[4], buttons[8] }; // Buttons adjacent to MR
			case "J7": return new ButtonComplex[] { buttons[3], buttons[7] }; // Buttons adjacent to BL
			case "J8": return new ButtonComplex[] { buttons[4], buttons[6], buttons[8] }; // Buttons adjacent to BM
			case "J9": return new ButtonComplex[] { buttons[5], buttons[7] }; // Buttons adjacent to BR
			case "A1": return new ButtonComplex[] { buttons[0] }; // TL
			case "B1": return new ButtonComplex[] { buttons[1] }; // TM
			case "C1": return new ButtonComplex[] { buttons[2] }; // TR
			case "A2": return new ButtonComplex[] { buttons[3] }; // ML
			case "B2": return new ButtonComplex[] { buttons[4] }; // MM
			case "C2": return new ButtonComplex[] { buttons[5] }; // MR
			case "A3": return new ButtonComplex[] { buttons[6] }; // BL
			case "B3": return new ButtonComplex[] { buttons[7] }; // BM
			case "C3": return new ButtonComplex[] { buttons[8] }; // BR
		}
		return null;
	}
	private int getAttributeCount(ButtonComplex[] loc, string attrID, ButtonComplex[] buttons)
	{
		int sum = 0;
		HashSet<int> set = new HashSet<int>();
		
		switch(attrID)
		{
			case "LLL":
				foreach(ButtonComplex button in loc)
				{
					if (!button.isSafe)
						sum++;
				}
				break;
			case "NRL":
				foreach (ButtonComplex button in loc)
				{
					if (!button.isSafe && button.buttonColor == ButtonColor.RED)
						sum++;
				}
				break;
			case "NYL":
				foreach (ButtonComplex button in loc)
				{
					if (!button.isSafe && button.buttonColor == ButtonColor.YELLOW)
						sum++;
				}
				break;
			case "NBL":
				foreach (ButtonComplex button in loc)
				{
					if (!button.isSafe && button.buttonColor == ButtonColor.BLUE)
						sum++;
				}
				break;
			case "CDL":
				foreach (ButtonComplex button in loc)
				{
					if (!button.isSafe)
						set.Add((int)button.buttonColor);
				}
				sum = set.Count;
				break;
			case "DCL":
				foreach (ButtonComplex button in loc)
				{
					if (!button.isSafe)
						set.Add(button.coord[0]);
				}
				sum = set.Count;
				break;
			case "DRL":
				foreach (ButtonComplex button in loc)
				{
					if (!button.isSafe)
						set.Add(button.coord[1]);
				}
				sum = set.Count;
				break;
			case "AJL":
				for(int i = 0; i < buttons.Length; i++)
				{
					if(!buttons[i].isSafe)
					{
						ButtonComplex[] newLoc = getButtons("J" + (i + 1), buttons);
						foreach(ButtonComplex button in newLoc)
						{
							if(!button.isSafe)
							{
								sum++;
								break;
							}
						}
					}
				}
				break;
		}
		return sum;
	}
	private bool isNum(string str)
	{
		foreach(char c in str)
		{
			if (!"0123456789".Contains(c + ""))
				return false;
		}
		return true;
	}
}

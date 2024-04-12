using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueTester 
{
	
	public bool testClue(ClueObj clue, Button[] buttons)
	{
		string code = clue.getCode();
		switch(code.Substring(code.Length - 2))
		{
			case "EQ": return evalEqualClue(code, buttons);
			case "GR": return evalGreaterClue(code, buttons);
			case "LT": return evalLeastClue(code, buttons);
			case "FR": return evalFewerClue(code, buttons);
		}
		return false;
	}
	private bool evalEqualClue(string code, Button[] buttons)
	{
		Button[] loc1 = getButtons(code.Substring(0, 2), buttons);
		Button[] loc2 = getButtons(code.Substring(5, 2), buttons);
		int s1 = getAttributeCount(loc1, code.Substring(2, 3), buttons);
		int s2;
		if(isNum(code.Substring(7, 3)))
			s2 = Int32.Parse(code.Substring(7, 3));
		else
			s2 = getAttributeCount(loc2, code.Substring(7, 3), buttons);
		return (s1 == s2);
	}
	private bool evalGreaterClue(string code, Button[] buttons)
	{
		Button[] loc1 = getButtons(code.Substring(0, 2), buttons);
		Button[] loc2 = getButtons(code.Substring(5, 2), buttons);
		int s1 = getAttributeCount(loc1, code.Substring(2, 3), buttons);
		int s2 = getAttributeCount(loc2, code.Substring(7, 3), buttons);
		return (s1 > s2);
	}
	private bool evalLeastClue(string code, Button[] buttons)
	{
		Button[] loc1 = getButtons(code.Substring(0, 2), buttons);
		int s1 = getAttributeCount(loc1, code.Substring(2, 3), buttons);
		int s2 = Int32.Parse(code.Substring(7, 3));
		return (s1 >= s2);
	}
	private bool evalFewerClue(string code, Button[] buttons)
	{
		Button[] loc1 = getButtons(code.Substring(0, 2), buttons);
		int s1 = getAttributeCount(loc1, code.Substring(2, 3), buttons);
		int s2 = Int32.Parse(code.Substring(7, 3));
		return (s1 <= s2);
	}
	private Button[] getButtons(string locId, Button[] buttons)
	{
		switch(locId)
		{
			case "GD": return buttons;
			case "RA": return new Button[] { buttons[0], buttons[1], buttons[2] };
			case "RB": return new Button[] { buttons[3], buttons[4], buttons[5] };
			case "RC": return new Button[] { buttons[6], buttons[7], buttons[8] };
			case "CA": return new Button[] { buttons[0], buttons[3], buttons[6] };
			case "CB": return new Button[] { buttons[1], buttons[4], buttons[7] };
			case "CC": return new Button[] { buttons[2], buttons[5], buttons[8] };
			case "J1": return new Button[] { buttons[1], buttons[3]};
			case "J2": return new Button[] { buttons[0], buttons[2], buttons[4] };
			case "J3": return new Button[] { buttons[1], buttons[5] };
			case "J4": return new Button[] { buttons[0], buttons[4], buttons[6] };
			case "J5": return new Button[] { buttons[1], buttons[3], buttons[5], buttons[7] };
			case "J6": return new Button[] { buttons[2], buttons[4], buttons[8] };
			case "J7": return new Button[] { buttons[3], buttons[7] };
			case "J8": return new Button[] { buttons[4], buttons[6], buttons[8] };
			case "J9": return new Button[] { buttons[5], buttons[7] };
			case "A1": return new Button[] { buttons[0] };
			case "B1": return new Button[] { buttons[1] };
			case "C1": return new Button[] { buttons[2] };
			case "A2": return new Button[] { buttons[3] };
			case "B2": return new Button[] { buttons[4] };
			case "C2": return new Button[] { buttons[5] };
			case "A3": return new Button[] { buttons[6] };
			case "B3": return new Button[] { buttons[7] };
			case "C3": return new Button[] { buttons[8] };
		}
		return null;
	}
	private int getAttributeCount(Button[] loc, string attrID, Button[] buttons)
	{
		int sum = 0;
		HashSet<int> set = new HashSet<int>();
		
		switch(attrID)
		{
			case "LLL":
				foreach(Button button in loc)
				{
					if (!button.isTruth)
						sum++;
				}
				break;
			case "NRL":
				foreach (Button button in loc)
				{
					if (!button.isTruth && button.buttonColor == ButtonColor.RED)
						sum++;
				}
				break;
			case "NYL":
				foreach (Button button in loc)
				{
					if (!button.isTruth && button.buttonColor == ButtonColor.YELLOW)
						sum++;
				}
				break;
			case "NBL":
				foreach (Button button in loc)
				{
					if (!button.isTruth && button.buttonColor == ButtonColor.BLUE)
						sum++;
				}
				break;
			case "CDL":
				foreach (Button button in loc)
				{
					if (!button.isTruth)
						set.Add((int)button.buttonColor);
				}
				sum = set.Count;
				break;
			case "DCL":
				foreach (Button button in loc)
				{
					if (!button.isTruth)
						set.Add(button.coord[0]);
				}
				sum = set.Count;
				break;
			case "DRL":
				foreach (Button button in loc)
				{
					if (!button.isTruth)
						set.Add(button.coord[1]);
				}
				sum = set.Count;
				break;
			case "AJL":
				for(int i = 0; i < buttons.Length; i++)
				{
					if(!buttons[i].isTruth)
					{
						Button[] newLoc = getButtons("J" + (i + 1), buttons);
						foreach(Button button in newLoc)
						{
							if(!button.isTruth)
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

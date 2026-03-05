using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonComplex
{
	public ButtonColor buttonColor;
	public bool isTruth;
	public bool isSafe;
	public ClueObj clue;
	public string coord;
	public ButtonComplex(ButtonColor buttonColor, bool isTruth, bool isSafe, string coord)
	{
		this.buttonColor = buttonColor;
		this.isTruth = isTruth;
		this.isSafe = isSafe;
		this.coord = coord;
	}

	public string toString()
	{

		string endCond = isTruth ? "Honest" : "Lying";
		string endState = isSafe ? "Safe" : "Not Safe";

		string clueStr = ":";
		if (clue != null)
			clueStr = clue.toString;
		return $"{getRow(coord[1])}{getCol(coord[0])} {buttonColor} {clueStr} ({endCond}, {endState})";
	}
	private char getRow(char r)
	{
		switch(r)
		{
			case '1': return 'T';
			case '2': return 'M';
			case '3': return 'B';
		}
		return '-';
	}
	private char getCol(char r)
	{
		switch (r)
		{
			case 'A': return 'L';
			case 'B': return 'M';
			case 'C': return 'R';
		}
		return '-';
	}
}

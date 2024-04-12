using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueObj
{
	private string clue;
	private string code;
	public ClueObj(string clue, string code)
	{
		this.clue = clue;
		this.code = code;
	}
	public string getCondition()
	{
		return clue;
	}
	public string getCode()
	{
		return code;
	}
	public string toString()
	{
		return code + ": " + clue;
	}

}
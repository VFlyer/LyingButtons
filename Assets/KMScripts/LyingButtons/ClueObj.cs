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
    public string Condition => clue;
    public string Code => code;
    public string toString => code + ": " + clue;

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGenResponse  
{
    private string[] trueClueIds;
    private string[] falseClueIds;

    public PuzzleGenResponse(string[] trueClueIds, string[] falseClueIds)
    {
        this.trueClueIds = trueClueIds;
        this.falseClueIds = falseClueIds;
    }
    public string[] getTrueClueIds()
    {
        return trueClueIds;
    }
    public string[] getFalseClueIds()
    {
        return falseClueIds;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEqual : ClueObj
{

    public CEqual(string clue, string locID, string attrID, int sum)
    : base(clue, locID + attrID + locID + "00" + sum + "EQ")
    {

    }
    public CEqual(string clue, string locID1, string attrID1, string locID2, string attrID2)
    : base(clue, locID1 + attrID1 + locID2 + attrID2 + "EQ")
    {

    }
}

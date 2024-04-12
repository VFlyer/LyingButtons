using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFewer : ClueObj
{
    public CFewer(string clue, string locID, string attrID, int sum)
    : base(clue, locID + attrID + locID + "00" + sum + "FR")
    {

    }
}

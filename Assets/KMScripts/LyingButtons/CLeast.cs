using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLeast : ClueObj
{

    public CLeast(string clue, string locID, string attrID, int sum)
    : base(clue, locID + attrID + locID + "00" + sum + "LT")
    {

    }
}

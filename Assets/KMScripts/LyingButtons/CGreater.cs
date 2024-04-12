using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGreater : ClueObj
{
    public CGreater(string clue, string locID, string attrID1, string attrID2)
    : base(clue, locID + attrID1 + locID + attrID2 + "GR")
    {

    }
    public CGreater(string clue, string locID1, string attrID1, string locID2, string attrID2)
    : base(clue, locID1 + attrID1 + locID2 + attrID2 + "GR")
    {

    }
}

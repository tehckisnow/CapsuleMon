using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Condition
{
    public ConditionID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }

    public Action<Mon> OnStart { get; set; }
    public Func<Mon, bool> OnBeforeMove { get; set; }
    public Action<Mon> OnAfterTurn { get; set; }
}

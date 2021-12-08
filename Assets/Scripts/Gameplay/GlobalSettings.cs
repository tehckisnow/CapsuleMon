using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GlobalSettings : MonoBehaviour
{
    [Header("Font")]
    [SerializeField] TMP_FontAsset font;
    public TMP_FontAsset Font => font;
    [Header("Text Colors")]
    [SerializeField] Color highlightedColor;
    public Color HighlightedColor => highlightedColor;
    
    [SerializeField] Color unhighlightedColor;
    public Color UnhighlightedColor => unhighlightedColor;

    [SerializeField] Color reorderColor;
    public Color ReorderColor => reorderColor;
    [SerializeField] Color reorderColorHighlighted;
    public Color ReorderColorHighlighted => reorderColorHighlighted;

    [Header("Status Colors")]
    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;
    Dictionary<ConditionID, Color> statusColors;
    public Dictionary<ConditionID, Color> StatusColors => statusColors;

    public static GlobalSettings i { get; private set; }

    private void Awake()
    {
        i = this;

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.slp, slpColor},
            {ConditionID.par, parColor},
            {ConditionID.frz, frzColor}
        };
    }
}

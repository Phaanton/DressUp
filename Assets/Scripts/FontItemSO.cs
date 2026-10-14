
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "NewFontItem", menuName = "DressUp/Font Item")]
public class FontItemSO : ScriptableObject
{
    public string itemID; // Para usar o mesmo sistema de save do Model
    public string fontName;
    public TMP_FontAsset fontAsset;
    public bool requiresAdToUnlock;
}
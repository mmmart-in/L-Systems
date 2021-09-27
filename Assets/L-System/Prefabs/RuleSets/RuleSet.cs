using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RuleSet", fileName ="RuleSet")]
public class RuleSet : ScriptableObject
{      
    public Dictionary<char, string> dic = new Dictionary<char, string>();

    //Instance specific variables
    [Header("Color")]
    public Color leafColor, trunkColor, flowerColor;
    [Range(0.0F, 1.0F)] public float colorVariance;
    
    [Header("Generation")]
    [Range(0, 5)] public int minGenerations;
    [Range(0, 7)] public int maxGenerations;
    public float length;
    public string axiom;
    
    [Header("Angles")]
    public float branchAngle;
    public float yAxis;
    
    [Header("Size")]
    [Range(0.1F, 10)] public float treeStartSize;
    [Range(0.1F, 1.5F)] public float branchSizeMultiplier;

    //Rules
    [Header("Rules")]
    public string f;
    public string x;
    public string l;
    public string minus;
    public string plus;
    public string star;
    public string slash;
    public string a;
    public string b;
    public string c;
    public void EnableRuleSet()
    {
        dic.Clear();
        dic.Add('F', f);
        dic.Add('X', x);
        dic.Add('A', a);
        dic.Add('B', b);
        dic.Add('C', c);
        dic.Add('-', minus);
        dic.Add('+', plus);
        dic.Add('*', star);
        dic.Add('/', slash);
        dic.Add('L', l);

    }
}

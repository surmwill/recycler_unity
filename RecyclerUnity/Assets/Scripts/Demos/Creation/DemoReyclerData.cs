using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoRecyclerData : IRecyclerScrollRectData<string>
{
    public string Key => Word;
    
    public string Word { get; private set; }
    
    public Color BackgroundColor { get; private set; }

    public DemoRecyclerData(string word, Color backgroundColor)
    {
        Word = word;
        BackgroundColor = backgroundColor;
    }
}

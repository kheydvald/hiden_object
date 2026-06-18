using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Story/Story Data")]
public class StoryData : ScriptableObject
{
    [TextArea(3, 10)]
    public List<string> lines;
}


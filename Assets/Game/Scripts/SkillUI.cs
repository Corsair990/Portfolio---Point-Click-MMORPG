using TMPro;
using UnityEngine;

public class SkillUI : MonoBehaviour
{
    public TMP_Text skillNameText;
    public TMP_Text skillLevel;
    public TMP_Text skillXPText;

    public void Setup(string _skillName, int _level, float _xp)
    {
        if (skillNameText == null || skillLevel == null || skillXPText == null) return;

        skillNameText.text = _skillName;
        skillLevel.text = $"{_level.ToString()} / 100";
        skillXPText.text = $"XP: {_xp.ToString()}";
    }
}

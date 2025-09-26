using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    public uint affectedSkillID;
    public string objectName;
    public int requiredLevel;

    public float chanceOfSuccess = 0.20f;
    public float xpOnSuccess;
    public float xpOnFail;

    public AudioClip[] successClips;
    public AudioClip[] failedClips;
}
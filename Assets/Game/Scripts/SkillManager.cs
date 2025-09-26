using Mirror;
using System;
using UnityEngine;

public class SkillManager : NetworkBehaviour
{
    public readonly SyncList<Skill> skills = new SyncList<Skill>();

    public override void OnStartServer()
    {
        skills.Add(new Skill() { id = 1, skillName = "Woodcutting", level = 1, maxLevel = 100, currentXP = 0, xpNeededToLevel = 83d, xpMultiplier = 1.1f, xpRemaining = 83d });
        skills.Add(new Skill() { id = 2, skillName = "Mining", level = 1, maxLevel = 100, currentXP = 0, xpNeededToLevel = 83d, xpMultiplier = 1.1f, xpRemaining = 83d });
        skills.Add(new Skill() { id = 3, skillName = "Smithing", level = 1, maxLevel = 100, currentXP = 0, xpNeededToLevel = 83d, xpMultiplier = 1.1f, xpRemaining = 83d });
        skills.Add(new Skill() { id = 4, skillName = "Fletching", level = 1, maxLevel = 100, currentXP = 0, xpNeededToLevel = 83d, xpMultiplier = 1.1f, xpRemaining = 83d });
        skills.Add(new Skill() { id = 5, skillName = "Crafting", level = 1, maxLevel = 100, currentXP = 0, xpNeededToLevel = 83d, xpMultiplier = 1.1f, xpRemaining = 83d });
    }

    public override void OnStartAuthority()
    {
        skills.OnChange += OnSkillsListChanged;

        CanvasManager.instance.skillsPanel.SetActive(true);

        foreach (Skill skill in skills)
        {
            GameObject skillUIPrefab = Instantiate(CanvasManager.instance.skillUIPrefab, CanvasManager.instance.skillsTransform);

            skillUIPrefab.name = skill.skillName;

            SkillUI skillUI = skillUIPrefab.GetComponent<SkillUI>();

            if (skillUI != null)
            { 
                skillUI.Setup(skill.skillName, skill.level, (float)skill.currentXP);
            }
        }
    }

    [Server]
    public void AddXP(uint skillID, double xpToAdd)
    {
        // It's safer to work with the index when modifying structs in a list.
        int index = skills.FindIndex(s => s.id == skillID);
        if (index == -1) return;

        Skill skill = skills[index];
        skill.currentXP += xpToAdd * skill.xpMultiplier;

        // Loop to handle gaining multiple levels at once.
        while (skill.currentXP >= skill.xpNeededToLevel && skill.level < skill.maxLevel)
        {
            // Subtract the XP for the level we just completed.
            skill.currentXP -= skill.xpNeededToLevel;

            // Increment the level directly.
            skill.level++;

            // Calculate the XP needed for the new next level.
            skill.xpNeededToLevel = Mathf.Round(83f * Mathf.Pow(1.05f, skill.level));
        }

        // Update the remaining XP for the UI after all level ups.
        skill.xpRemaining = skill.xpNeededToLevel - skill.currentXP;

        // Because Skill is a struct, you MUST write the modified copy 
        // back to the SyncList to trigger the network update.
        skills[index] = skill;
    }

    [Server]
    public void RemoveXP(uint _skillID, double _xpToRemove)
    {

    }

    private void OnSkillsListChanged(SyncList<Skill>.Operation _op, int _index, Skill _skill)
    {
        switch (_op)
        {
            case SyncList<Skill>.Operation.OP_ADD:
                // An item was added to the list.
                // You can get the new item with skills[index]
                Debug.Log($"Added new skill {_skill.skillName} at index {_index}");
                break;
            case SyncList<Skill>.Operation.OP_INSERT:
                // An item was inserted at a specific index.
                UpdateSkillsUI(_skill);
                break;
            case SyncList<Skill>.Operation.OP_REMOVEAT:
                // An item was removed at a specific index.
                break;
            case SyncList<Skill>.Operation.OP_SET:
                // An item's value was changed. This is what will be called when
                // a skill's XP or level changes.
                // The updated item is skills[index].
                UpdateSkillsUI(_skill);
                Debug.Log($"Skill {_skill.skillName} was updated. You now have {_skill.currentXP} XP.");
                break;
            case SyncList<Skill>.Operation.OP_CLEAR:
                // The entire list was cleared.
                break;
        }
    }

    private void UpdateSkillsUI(Skill _skill)
    {
        foreach (Transform t in CanvasManager.instance.skillsTransform)
        {
            if (t.name == _skill.skillName)
            {
                SkillUI skillUI = t.GetComponent<SkillUI>();

                skillUI.Setup(_skill.skillName, _skill.level, (float)_skill.currentXP);

                return;
            }
        }
    }
}



[Serializable]
public struct Skill
{
    public int id;
    public string skillName;
    public int level;
    public int maxLevel;
    public double currentXP;
    public double xpNeededToLevel;
    public double xpRemaining;
    public float xpMultiplier;

    public Skill(int _id, string _name, int _level, int _maxLevel, double _currentXP, double _xpNeeded, double _xpRemaining, float _xpMultiplier)
    {
        id = _id;
        skillName = _name;
        level = _level;
        maxLevel = _maxLevel;
        currentXP = _currentXP;
        xpNeededToLevel = _xpNeeded;
        xpRemaining = _xpRemaining;
        xpMultiplier = _xpMultiplier;
    }
}

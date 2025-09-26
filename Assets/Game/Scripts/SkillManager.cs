using Mirror;
using System;
using TMPro;
using UnityEngine;

public class SkillManager : NetworkBehaviour
{
    public SyncList<Skill> skills = new SyncList<Skill>();

    public override void OnStartServer()
    {
        skills.Add(new Skill() { id = 1, skillName = "Woodcutting", level = 1, currentXP = 0, xpNeededToLevel = 83 });
        skills.Add(new Skill() { id = 2, skillName = "Mining", level = 1, currentXP = 0, xpNeededToLevel = 83 });
        skills.Add(new Skill() { id = 3, skillName = "Smithing", level = 1, currentXP = 0, xpNeededToLevel = 83 });
        skills.Add(new Skill() { id = 4, skillName = "Fletching", level = 1, currentXP = 0, xpNeededToLevel = 83 });
        skills.Add(new Skill() { id = 5, skillName = "Crafting", level = 1, currentXP = 0, xpNeededToLevel = 83 });
    }

    public override void OnStartAuthority()
    {
        skills.OnChange += OnSkillsListChanged;

        CanvasManager.instance.skillsPanel.SetActive(true);

        foreach (Skill skill in skills)
        {
            //GameObject skillUI = Instantiate(CanvasManager.instance.skillUIPrefab, CanvasManager.instance.skillsTransform);

            //skillUI.GetComponentInChildren<TextMeshPro>().text = $"{skill.skillName} / {skill.currentXP}";
        }
    }

    [Server]
    public void AddXP(uint _skillID, double _xpToAdd)
    {
        Skill skillToUpdate = skills.Find(s => s.id == _skillID);

        // Add the new XP, multiplied by the multiplier
        skillToUpdate.currentXP += _xpToAdd * skillToUpdate.xpMultiplier;

        // Recalculate remaining XP
        skillToUpdate.xpRemaining = skillToUpdate.xpNeededToLevel - skillToUpdate.currentXP;

        // Check for level-up and loop until all levels are gained
        while (skillToUpdate.currentXP >= skillToUpdate.xpNeededToLevel && skillToUpdate.level < skillToUpdate.maxLevel)
        {
            LevelUp(_skillID, skillToUpdate.level++);
        }
    }

    [Server]
    public void RemoveXP(uint _skillID, double _xpToRemove)
    {

    }

    [Server]
    public void LevelUp(uint _skillID, int _newLevel)
    {
        Skill skillToUpdate = skills.Find(s => s.id == _skillID);

        // First, subtract the XP needed to get to the next level
        skillToUpdate.currentXP -= skillToUpdate.xpNeededToLevel;

        // Then, increment the level
        skillToUpdate.level = _newLevel;

        // Calculate the XP needed for the next level
        skillToUpdate.xpNeededToLevel = Mathf.Round(83f * Mathf.Pow(1.05f, skillToUpdate.level));

        // Update remaining XP after the level up
        skillToUpdate.xpRemaining = skillToUpdate.xpNeededToLevel - skillToUpdate.currentXP;
    }


    [Command]
    public void CmdAddXP(int _skillID, double _xpToAdd)
    {
        Skill skillToUpdate = skills.Find(s => s.id == _skillID);
        
        if (skillToUpdate != null)
        {
            skillToUpdate.currentXP += _xpToAdd;
        }
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
                break;
            case SyncList<Skill>.Operation.OP_REMOVEAT:
                // An item was removed at a specific index.
                break;
            case SyncList<Skill>.Operation.OP_SET:
                // An item's value was changed. This is what will be called when
                // a skill's XP or level changes.
                // The updated item is skills[index].

                Debug.Log($"Skill {_skill.skillName} was updated. You now have {_skill.currentXP} XP.");
                break;
            case SyncList<Skill>.Operation.OP_CLEAR:
                // The entire list was cleared.
                break;
        }
    }

    private void UpdateSkillsUI(Skill _skill)
    {
        GameObject skillUI = Instantiate(CanvasManager.instance.skillUIPrefab, CanvasManager.instance.skillsTransform);

        skillUI.GetComponentInChildren<TextMeshPro>().text = $"{_skill.skillName} / {_skill.currentXP}";
    }
}



[Serializable]
public class Skill
{
    public int id;
    public string skillName;
    public int level = 0;
    public int maxLevel = 100;
    public double currentXP = 0;
    public double xpNeededToLevel = 0;
    public double xpRemaining = 0;
    public float xpMultiplier = 1.10f;
}

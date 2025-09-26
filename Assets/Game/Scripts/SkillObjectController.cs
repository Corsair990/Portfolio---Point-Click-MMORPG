using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class SkillObjectController : NetworkBehaviour
{
    [SerializeField] private SkillManager skillManager;

    private List<SkillData> skillObjectList = new List<SkillData>();

    public void Awake()
    {
        skillObjectList.AddRange(Resources.LoadAll<SkillData>("SkillData"));
    }

    public override void OnStartServer()
    {
        if (skillManager == null && isServer)
        { 
            skillManager = GetComponent<SkillManager>();
        }
    }

    [Server]
    public void UseSkillObject(NetworkIdentity _netID)
    {
        if (_netID == null) return;

        if (_netID.TryGetComponent(out SkillObject SO))
        {
            int index = skillObjectList.IndexOf(SO.skillData);

            if (index == -1)
            {
                Debug.LogError($"[Server]: SkillData '{SO.skillData.name}' not found in the database.");
                return;
            }

            SkillData skillData = skillObjectList[index];

            float roll = Random.value;

            if (skillData != null && roll <= skillData.chanceOfSuccess)
            {
                Debug.Log($"[Server]: Player with ID {netId} successfully used the skill.");

                skillManager.AddXP(skillData.affectedSkillID, skillData.xpOnSuccess);
                UsedSkillObject(connectionToClient, skillData.affectedSkillID, skillData.xpOnSuccess);

                PlaySound(index, true);
            }

            else
            {
                if (SO.skillData == null)
                {
                    Debug.Log($"[Server]: Skill Data is null. Exiting Use Skill Object.");
                    return;
                }

                Debug.Log($"[Server]: Player with ID {netId} failed attempt to use the skill.");

                skillManager.AddXP(skillData.affectedSkillID, skillData.xpOnFail);
                UsedSkillObject(connectionToClient, skillData.affectedSkillID, skillData.xpOnFail);
                PlaySound(index, false);
            }
        }
    }

    [TargetRpc]
    public void UsedSkillObject(NetworkConnectionToClient _conn, uint _skillID, float _xpGiven)
    {
        Debug.Log($"[Server]: You gained {_xpGiven} in skill {_skillID}.");
    }

    [ClientRpc]
    public void PlaySound(int _index, bool _isSuccess)
    {
        SkillData skillData = skillObjectList[_index];

        if (skillData == null) return;

        if (skillData.successClips.Length <= 0 || skillData.failedClips.Length <= 0)
        {
            return;
        }

        if (_isSuccess)
        {
            AudioClip clip = skillData.successClips[Random.Range(0, skillData.successClips.Length)];

            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }

        else
        {
            AudioClip clip = skillData.failedClips[Random.Range(0, skillData.failedClips.Length)];

            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
    }
}

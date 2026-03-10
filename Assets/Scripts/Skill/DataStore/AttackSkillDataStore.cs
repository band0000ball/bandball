using System;
using System.Collections.Generic;
using Skill.DataBase;
using Skill.SkillData;
using UnityEngine;

namespace Skill.DataStore
{
    public class AttackSkillDataStore : SkillDataStore<AttackSkillData>
    {
        private const string OriginalSkillsKey = "OriginalAttackSkillsData";

        // private void Awake()
        // {
        //     OriginalSkills ??= PlayerPrefs.HasKey(OriginalSkillsKey)
        //         ? JsonUtility.FromJson<List<AttackSkillData>>(PlayerPrefs.GetString(OriginalSkillsKey))
        //         : null;
        // }
    }
}
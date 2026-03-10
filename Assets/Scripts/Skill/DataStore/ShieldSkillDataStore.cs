using System.Collections.Generic;
using Skill.DataBase;
using Skill.SkillData;
using UnityEngine;

namespace Skill.DataStore
{
    public class ShieldSkillDataStore : SkillDataStore<ShieldSkillData>
    {
        private const string OriginalSkillsKey = "OriginalShieldSkillsData";

        // private void Awake()
        // {
        //     OriginalSkills ??= PlayerPrefs.HasKey(OriginalSkillsKey)
        //         ? JsonUtility.FromJson<List<ShieldSkillData>>(PlayerPrefs.GetString(OriginalSkillsKey))
        //         : null;
        // }
    }
}
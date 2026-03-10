using System.Collections.Generic;
using Skill.DataBase;
using Skill.SkillData;
using UnityEngine;

namespace Skill.DataStore
{
    public class BuffSkillDataStore : SkillDataStore<BuffSkillData>
    {
        private const string OriginalSkillsKey = "OriginalBuffSkillsData";

        // private void Awake()
        // {
        //     OriginalSkills ??= PlayerPrefs.HasKey(OriginalSkillsKey)
        //         ? JsonUtility.FromJson<List<BuffSkillData>>(PlayerPrefs.GetString(OriginalSkillsKey))
        //         : null;
        // }
    }
}
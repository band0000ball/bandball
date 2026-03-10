using System.Collections.Generic;
using UnityEngine;

namespace Skill.DataBase
{
    public abstract class SkillDataBase<T> : ScriptableObject where T : SkillData.SkillData
    {
        [SerializeField]
        private List<T> skillList = new();
        
        public List<T> SkillList => skillList;
    }
}
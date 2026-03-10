using Skill.SkillData;
using UnityEngine;

namespace Skill.DataBase
{
    [CreateAssetMenu(fileName = "AttackSkillDataBase", menuName = "ScriptableObject/DB/AttackSkillDB")]
    public class AttackSkillDataBase : SkillDataBase<AttackSkillData>
    { }
}
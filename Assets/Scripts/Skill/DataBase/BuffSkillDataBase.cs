using Skill.SkillData;
using UnityEngine;

namespace Skill.DataBase
{
    [CreateAssetMenu(fileName = "BuffSkillDataBase", menuName = "ScriptableObject/DB/BuffSkillDB", order = 0)]
    public class BuffSkillDataBase : SkillDataBase<BuffSkillData>
    { }
}
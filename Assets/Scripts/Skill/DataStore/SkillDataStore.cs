using System.Collections.Generic;
using Databrain;
using JetBrains.Annotations;
using Skill.DataBase;
using UnityEngine;

namespace Skill.DataStore
{
    public abstract class SkillDataStore<T> : MonoBehaviour where T: SkillData.SkillData
    {
        public DataLibrary data;
        
        protected List<T> dataBase => data.GetAllInitialDataObjectsByType<T>();
        public List<T> DataBase => dataBase;

        public T FindWithName(string skillName)
        {
            return string.IsNullOrEmpty(skillName) ? null : dataBase.Find(x => x.skillName == skillName);
        }

        public T FindWithID(int id)
        {
            return dataBase.Find(x => x.skillID == id);
        }

        public T FindWithGuid(string guid)
        {
            return (T)data.GetInitialDataObjectByGuid(guid);
        }
    }
}
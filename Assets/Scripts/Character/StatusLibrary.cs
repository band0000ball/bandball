using System;
using System.Collections.Generic;
using System.Linq;
using Databrain;
using UnityEngine;

namespace Character
{
    [Serializable]
    public class StatusLibrary
    {
        private DataLibrary data;
        
        public List<BehaviourStatus> behaviourStatusData;
        public List<AttributeStatus> attributeStatusData;
        public List<BattleStatus> battleStatusData;
        public List<MetaStatus> metaStatusData;

        public StatusLibrary(DataLibrary dataLibrary)
        {
            data = dataLibrary;
            behaviourStatusData = data.GetAllInitialDataObjectsByType<BehaviourStatus>();
            attributeStatusData = data.GetAllInitialDataObjectsByType<AttributeStatus>();
            battleStatusData = data.GetAllInitialDataObjectsByType<BattleStatus>();
            metaStatusData = data.GetAllInitialDataObjectsByType<MetaStatus>();
        }

        public MetaStatus SelectMeta(int targetId)
        {
            MetaStatus targetMeta = metaStatusData.Select(x => x).FirstOrDefault(x => x.id == targetId);
            return targetMeta;
        }

        /*public BehaviourStatus SelectBehaviour(int targetId)
        {
            BehaviourStatus targetBehaviour = behaviourStatusData.Select(x => x)
                .FirstOrDefault(x => x.behaviourId == targetId);
            return targetBehaviour;
        }

        public AttributeStatus SelectAttribute(int targetId)
        {
            AttributeStatus targetAttribute = attributeStatusData.Select(x => x)
                .FirstOrDefault(x => x.attributeId == targetId);
            return targetAttribute;
        }

        public BattleStatus SelectBattle(int targetId)
        {
            #if UNITY_EDITOR
            BattleStatus battleValue = battleStatusData.Select(x => x)
                .FirstOrDefault(x => x.battleId == targetId);
            BattleStatus targetBattle = ScriptableObject.CreateInstance<BattleStatus>().Clone(battleValue);
            #else
            BattleStatus targetBattle = battleStatusData.Select(x => x)
                .FirstOrDefault(x => x.battleId == targetId);
            #endif
            return targetBattle;
        }*/

        /*public void Save(string dataPath)
        {
            ScriptableObjectJson.SaveToJSON(dataPath, this);
        }

        public StatusData Load(string dataPath)
        {
            string fullPath = ScriptableObjectJson.GetFullPath(dataPath);

            // ファイルからデータを読み込む
            string jsonStr = System.IO.File.ReadAllText(fullPath);

            try
            {

                StatusData data = CreateInstance<StatusData>();
                JsonUtility.FromJsonOverwrite(jsonStr, data);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load settings data: " + e.Message);
                return null;
            }
        }*/
    }
}
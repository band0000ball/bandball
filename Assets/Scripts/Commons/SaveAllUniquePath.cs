using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Manager;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Commons
{
    /// <summary>
    /// 保存時に、シーンに設定されているユニークID保持対象のPathを設定（ベイク）する
    /// </summary>
    public class SaveAllUniquePath : AssetModificationProcessor
    {
        /// <summary>
        /// アセットが保存される直前のイベント
        /// </summary>
        /// <param name="paths">保存される対象アセットのパス</param>
        [Obsolete("Obsolete")]
        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                Scene scene = SceneManager.GetSceneByPath(path);

                if (!scene.IsValid()) continue;
                GameObject[] roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    RecursiveUpdateUniquePath(root.transform);
                }
            }
        
            UniqueIDManager[] uidms = Object.FindObjectsByType<UniqueIDManager>(FindObjectsSortMode.None);
            UniqueIDManager uidm;
            if (uidms.Length <= 0) return null;
            if (uidms.Length > 1)
            {
                List<int> versions = uidms.Select(u => u.GetVersion()).ToList();
                int argmax = versions.Select((x, i) => new { x, i }).Aggregate((max, xi) => xi.x > max.x ? xi : max).i;
                uidm = uidms[argmax];
            }
            else
            {
                uidm = uidms[0];
            }

            uidm.IncrementVersion();

            return paths;

        }

        /// <summary>
        /// 再帰的にUniquePathを更新する
        /// </summary>
        static void RecursiveUpdateUniquePath(Transform target)
        {
            CharacterControl upt = target.GetComponent<CharacterControl>();
            if (upt != null)
            {
                upt.SetUniquePathAndHash();
            }

            for (int i = 0; i < target.childCount; i++)
            {
                RecursiveUpdateUniquePath(target.GetChild(i));
            }
        }
    }
}
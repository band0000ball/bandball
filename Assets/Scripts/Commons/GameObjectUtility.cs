using UnityEngine;
using UnityEngine.SceneManagement;


namespace Commons
{
    public class GameObjectUtility : MonoBehaviour
    {
        public static string GetHierarchyPath(GameObject target)
        {
            string path = "";
            Transform current = target.transform;
            while (current is not null)
            {
                // 同じ階層に同名のオブジェクトがある場合があるので、それを回避する
                int index = current.GetSiblingIndex();
                path = "/" + current.name + index + path;
                current = current.parent;
            }

            Scene belongScene = target.GetBelongsScene();

            return "/" + belongScene.name + path;
        }

        public static int GetUniqueId(GameObject target)
        {
            string uniqueId = GetHierarchyPath(target);
            return uniqueId.GetHashCode();
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Commons
{
    public static class GameObjectExtension
    {
        public static Scene GetBelongsScene(this GameObject target)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid())
                {
                    continue;
                }

                if (!scene.isLoaded)
                {
                    continue;
                }
        
                GameObject[] roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    if (root == target.transform.root.gameObject)
                    {
                        return scene;
                    }
                }
            }

            return default;
        }
    
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return GetOrAddComponent<T>(component.gameObject);
        }
    
        /// <summary>
        /// 拡張対象のGameObjectを複製(生成)して返す
        /// </summary>
        public static GameObject Instantiate(this GameObject gameObject)
        {
            return Object.Instantiate(gameObject);
        }

        /// <summary>
        /// 生成後に親となるTransformを指定して、拡張対象のGameObjectを複製(生成)して返す
        /// </summary>
        public static GameObject Instantiate(this GameObject gameObject, Transform parent)
        {
            return Object.Instantiate(gameObject, parent);
        }

        /// <summary>
        /// 生成後の座標及び姿勢を指定して、拡張対象のGameObjectを複製(生成)して返す
        /// </summary>
        public static GameObject Instantiate(this GameObject gameObject, Vector3 pos, Quaternion rot)
        {
            return Object.Instantiate(gameObject, pos, rot);
        }

        /// <summary>
        /// 生成後に親となるTransform、また生成後の座標及び姿勢を指定して、拡張対象のGameObjectを複製(生成)して返す
        /// </summary>
        public static GameObject Instantiate(this GameObject gameObject, Vector3 pos, Quaternion rot, Transform parent)
        {
            return Object.Instantiate(gameObject, pos, rot, parent);
        }

        /// <summary>
        /// 生成後に親となるTransform、また生成後のローカル座標を指定して、拡張対象のGameObjectを複製(生成)して返す
        /// </summary>
        public static GameObject InstantiateWithLocalPosition(this GameObject gameObject, Transform parent, Vector3 localPos)
        {
            var instance = Object.Instantiate(gameObject, parent);
            instance.transform.localPosition = localPos;
            return instance;
        }

        public static T Instantiate<T>(this T component) where T : Component
        {
            return Object.Instantiate(component);
        }

        public static T Instantiate<T>(this T component, Transform parent) where T : Component
        {
            return Object.Instantiate(component, parent);
        }

        public static T Instantiate<T>(this T component, Vector3 pos, Quaternion rot) where T : Component
        {
            return Object.Instantiate(component, pos, rot);
        }

        public static T Instantiate<T>(this T component, Vector3 pos, Quaternion rot, Transform parent) where T : Component
        {
            return Object.Instantiate(component, pos, rot, parent);
        }

        public static T InstantiateWithLocalPosition<T>(this T component, Transform parent,
            Vector3 localPos) where T : Component
        {
            var instance = Object.Instantiate(component, parent);
            instance.transform.localPosition = localPos;
            return instance;
        }

        public static void Destroy(this GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }

        public static void Destroy(this Component component)
        {
            Object.Destroy(component);
        }

        /// <summary>
        /// ComponentがアタッチされているGameObjectを破棄する
        /// </summary>
        public static void DestroyGameObject(this Component component)
        {
            Object.Destroy(component.gameObject);
        }

        public static bool IsInLayerMask(this GameObject gameObject, LayerMask layerMask)
        {
            int objLayerMask = (1 << gameObject.layer);
            return (layerMask.value & objLayerMask) > 0;
        }

        public static bool IsInLayerMask(this Component component, LayerMask layerMask)
        {
            return IsInLayerMask(component.gameObject, layerMask);
        }
    }
}
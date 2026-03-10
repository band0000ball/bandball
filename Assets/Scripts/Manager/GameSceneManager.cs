using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{
    /// <summary>
    /// シーン遷移を管理するシングルトン。
    /// 遷移時にフェード処理と全マネージャーの静的状態クリアを行う。
    /// MainScene の常駐オブジェクトに AddComponent して使用する。
    /// </summary>
    public class GameSceneManager : MonoBehaviour
    {
        public static GameSceneManager Instance { get; private set; }

        /// <summary>シーン名定数</summary>
        public static class SceneNames
        {
            public const string Main      = "MainScene";
            public const string PlayStage = "PlayStage";
        }

        [SerializeField] private float _fadeDuration = 0.3f;

        private CanvasGroup _fadeCanvasGroup;
        private bool _isTransitioning;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _fadeCanvasGroup = CreateFadeCanvas();
        }

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------

        /// <summary>フェード付きシーン遷移</summary>
        public void LoadScene(string sceneName)
        {
            if (_isTransitioning) return;
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        // ---------------------------------------------------------------
        // Internal
        // ---------------------------------------------------------------

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            _isTransitioning = true;

            yield return StartCoroutine(Fade(1f));   // フェードアウト

            ClearAllManagers();                       // 静的リストクリア

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
                yield return null;

            yield return StartCoroutine(Fade(0f));   // フェードイン

            _isTransitioning = false;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            _fadeCanvasGroup.blocksRaycasts = true;
            float startAlpha = _fadeCanvasGroup.alpha;
            float elapsed    = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / _fadeDuration);
                yield return null;
            }

            _fadeCanvasGroup.alpha       = targetAlpha;
            _fadeCanvasGroup.blocksRaycasts = targetAlpha > 0.01f;
        }

        /// <summary>
        /// シーン遷移前に全マネージャーの静的状態をクリアする。
        /// 新しい静的マネージャーを追加した場合はここに追記する。
        /// </summary>
        private static void ClearAllManagers()
        {
            DamageManager.ClearAll();
            Stage.StageManager.Instance?.ClearStage();
        }

        /// <summary>フェード用フルスクリーン Canvas を自動生成する</summary>
        private CanvasGroup CreateFadeCanvas()
        {
            var root   = new GameObject("FadeCanvas");
            root.transform.SetParent(transform);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            root.AddComponent<GraphicRaycaster>();

            var imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(root.transform, false);
            var img = imageGo.AddComponent<Image>();
            img.color = Color.black;
            var rect = img.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var cg = root.AddComponent<CanvasGroup>();
            cg.alpha          = 0f;
            cg.blocksRaycasts = false;
            return cg;
        }
    }
}

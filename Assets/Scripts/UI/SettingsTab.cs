using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 設定タブ。サウンド・グラフィック設定を管理する。
    ///
    /// 設定値は PlayerPrefs に保存する（#38 LocalSettingsManager 実装後に移行予定）。
    /// Time.timeScale は変更しない（リアルタイム動作）。
    ///
    /// depends on: なし（#38 LocalSettingsManager で置換予定）
    /// </summary>
    public class SettingsTab : MonoBehaviour
    {
        private const string KeyMasterVolume = "Settings_MasterVolume";
        private const string KeyBgmVolume    = "Settings_BgmVolume";
        private const string KeySeVolume     = "Settings_SeVolume";

        [Header("サウンド設定")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Slider _seVolumeSlider;
        [SerializeField] private TextMeshProUGUI _masterVolumeValueText;
        [SerializeField] private TextMeshProUGUI _bgmVolumeValueText;
        [SerializeField] private TextMeshProUGUI _seVolumeValueText;

        [Header("グラフィック設定")]
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private Slider _brightnessSlider;
        [SerializeField] private TextMeshProUGUI _brightnessValueText;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_masterVolumeSlider != null)
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (_bgmVolumeSlider != null)
                _bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            if (_seVolumeSlider != null)
                _seVolumeSlider.onValueChanged.AddListener(OnSeVolumeChanged);
            if (_fullscreenToggle != null)
                _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            if (_brightnessSlider != null)
                _brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>タブ表示時に MenuCanvas から呼ばれる。保存値をスライダーに反映する。</summary>
        public void Refresh()
        {
            float master = PlayerPrefs.GetFloat(KeyMasterVolume, 1.0f);
            float bgm    = PlayerPrefs.GetFloat(KeyBgmVolume, 0.8f);
            float se     = PlayerPrefs.GetFloat(KeySeVolume, 0.8f);

            SetSliderSilent(_masterVolumeSlider, master);
            SetSliderSilent(_bgmVolumeSlider, bgm);
            SetSliderSilent(_seVolumeSlider, se);

            UpdateVolumeText(_masterVolumeValueText, master);
            UpdateVolumeText(_bgmVolumeValueText, bgm);
            UpdateVolumeText(_seVolumeValueText, se);

            if (_fullscreenToggle != null)
                _fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            if (_brightnessSlider != null)
            {
                float brightness = PlayerPrefs.GetFloat("Settings_Brightness", 1.0f);
                SetSliderSilent(_brightnessSlider, brightness);
                UpdateVolumeText(_brightnessValueText, brightness);
            }

            ApplyAudioSettings(master, bgm, se);
        }

        // ── スライダーコールバック ─────────────────────────────────────────────

        private void OnMasterVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(KeyMasterVolume, value);
            UpdateVolumeText(_masterVolumeValueText, value);
        }

        private void OnBgmVolumeChanged(float value)
        {
            // TODO: BGM AudioSource の音量に反映（AudioManager 実装後）
            PlayerPrefs.SetFloat(KeyBgmVolume, value);
            UpdateVolumeText(_bgmVolumeValueText, value);
        }

        private void OnSeVolumeChanged(float value)
        {
            // TODO: SE AudioSource の音量に反映（AudioManager 実装後）
            PlayerPrefs.SetFloat(KeySeVolume, value);
            UpdateVolumeText(_seVolumeValueText, value);
        }

        private void OnFullscreenChanged(bool value)
        {
            Screen.fullScreen = value;
        }

        private void OnBrightnessChanged(float value)
        {
            // TODO: ポストプロセス Exposure / Gamma 調整（実装後）
            PlayerPrefs.SetFloat("Settings_Brightness", value);
            UpdateVolumeText(_brightnessValueText, value);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private static void ApplyAudioSettings(float master, float bgm, float se)
        {
            AudioListener.volume = master;
            // TODO: BGM / SE AudioSource は AudioManager 実装後に連携
        }

        /// <summary>コールバックを発火させずにスライダー値をセットする。</summary>
        private static void SetSliderSilent(Slider slider, float value)
        {
            if (slider == null) return;
            slider.SetValueWithoutNotify(Mathf.Clamp(value, slider.minValue, slider.maxValue));
        }

        private static void UpdateVolumeText(TextMeshProUGUI label, float value)
        {
            if (label != null) label.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }
}

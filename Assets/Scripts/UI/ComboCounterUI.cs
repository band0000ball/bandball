using Character;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// コンボ数をUIに表示するコンポーネント
    /// </summary>
    public class ComboCounterUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private GameObject _comboPanel;
        [SerializeField] private CharacterControl _character;

        private int _lastCombo;

        private void Update()
        {
            if (_character is null) return;

            int combo = _character.GetComboCount();
            if (combo == _lastCombo) return;

            _lastCombo = combo;
            UpdateDisplay(combo);
        }

        private void UpdateDisplay(int combo)
        {
            _comboPanel?.SetActive(combo > 1);

            if (_comboText is not null)
                _comboText.text = $"{combo} Combo";
        }
    }
}
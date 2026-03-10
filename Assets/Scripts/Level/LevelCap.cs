using Databrain;
using Databrain.Attributes;
using Databrain.Inventory;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Level
{
    public class LevelRarity : ItemRarityData
    {
        public int minLevel;
        public int maxLevel;

#if UNITY_EDITOR
        public override VisualElement EditorGUI(SerializedObject _serializedObject, DatabrainEditorWindow _editorWindow)
        {
            /*var _root = new VisualElement();

#if DATABRAIN_LOCALIZATION

            var _localizedTitle = new PropertyField();
            _localizedTitle.BindProperty(_serializedObject.FindProperty(nameof(localizedTitle)));

            _root.Add(_localizedTitle);
#endif

            var _valueProperty = new IntegerField();
            _valueProperty.label = "Value";
            _valueProperty.BindProperty(_serializedObject.FindProperty(nameof(rarityValue)));
            
            _root.Add(_valueProperty);*/
            
            var _root = base.EditorGUI(_serializedObject, _editorWindow);

            var _minLevelProperty = new IntegerField();
            _minLevelProperty.label = "Min Level";
            _minLevelProperty.BindProperty(_serializedObject.FindProperty(nameof(minLevel)));
            
            _root.Add(_minLevelProperty);
            
            var _maxLevelProperty = new IntegerField();
            _maxLevelProperty.label = "Max Level";
            _maxLevelProperty.BindProperty(_serializedObject.FindProperty(nameof(maxLevel)));
            
            _root.Add(_maxLevelProperty);

            return _root;
        }
#endif
    }
}
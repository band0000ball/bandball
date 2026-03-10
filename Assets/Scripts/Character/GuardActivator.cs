using UnityEngine;

namespace Character
{
    public class GuardActivator : MonoBehaviour
    {
        [Header("Fire rate")]
        public GameObject shieldPrefab;
        private GameObject _guardShield;
        private GuardComponent _guardComponent;
        private int _defaultLayer;

        [HideInInspector] public CharacterControl ctl;

        void Start()
        {
            ctl = GetComponent<CharacterControl>();
            _defaultLayer = gameObject.layer;
            Debug.Log("position :" + ctl.GetPosition());
            _guardShield = Instantiate(shieldPrefab, ctl.GetPosition(), Quaternion.identity, transform);
            float scale = ctl.GetHeight() / 2;
            _guardShield.transform.localScale = new Vector3(scale, scale, scale);
            _guardShield.SetActive(false);
            _guardComponent = _guardShield.GetComponent<GuardComponent>();
        }

        void Update()
        {
            if (ctl.GetGuard() && ctl.GetMinGuardCooldown() <= 0)
            {
                Commons.Tools.Invoke(this, () => ShieldActivate(true), 1f / (0.9f + 0.1f * ctl.GetControl()));
            }
            else
            {
                Commons.Tools.Invoke(this, () => ShieldActivate(false), 1f / (0.9f + 0.1f * ctl.GetControl()));
            }
        }

        void ShieldActivate(bool active)
        {
            _guardShield.SetActive(active);
            gameObject.layer = active ? LayerMask.NameToLayer("Shield") : _defaultLayer;
        }
    }
}
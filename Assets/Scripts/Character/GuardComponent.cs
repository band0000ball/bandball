using System.Collections.Generic;
using Commons;
using UnityEngine;

namespace Character
{
    public class GuardComponent : MonoBehaviour
    {
        public GameObject[] effectsOnCollision;
        public CharacterControl parent;
        public float destroyTimeDelay = 5;
        public float offset;
        public Vector3 rotationOffset = new(0,0,0);
        public bool useWorldSpacePosition;
        public bool useOnlyRotationOffset = true;
        public bool useFirePointRotation;
        public bool destroyMainEffect;
        private ParticleSystem part;
        private List<ParticleCollisionEvent> collisionEvents = new();

        void Awake()
        {
            part = GetComponent<ParticleSystem>();
            transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            parent = transform.parent.GetComponent<CharacterControl>();
            gameObject.layer = transform.parent.gameObject.layer;
        }

        private void OnParticleCollision(GameObject other)
        {
            int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);     
            for (int i = 0; i < numCollisionEvents; i++)
            {
                foreach (var effect in effectsOnCollision)
                {
                    var instance = Instantiate(effect, collisionEvents[i].intersection + collisionEvents[i].normal * offset, new Quaternion());
                    if (!useWorldSpacePosition) instance.transform.parent = transform;
                    if (useFirePointRotation) { instance.transform.LookAt(transform.position); }
                    else if (rotationOffset != Vector3.zero && useOnlyRotationOffset) { instance.transform.rotation = Quaternion.Euler(rotationOffset); }
                    else
                    {
                        instance.transform.LookAt(collisionEvents[i].intersection + collisionEvents[i].normal);
                        instance.transform.rotation *= Quaternion.Euler(rotationOffset);
                    }
                    Destroy(instance, destroyTimeDelay);
                }
            }
        }

        public float DamageInflicted(float damage, float cooldown = GameBalance.DEFAULT_GUARD_COOLDOWN) => parent.GuardInflicted(damage, cooldown);
        public float GetLuck() => parent.GetLuck();
    }
}
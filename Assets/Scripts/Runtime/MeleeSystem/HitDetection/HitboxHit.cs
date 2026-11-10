using UnityEngine;

namespace GP
{
    public readonly struct HitboxHit
    {
        public IDamageable Target { get; }
        public Vector3 Point { get; }

        public HitboxHit(IDamageable target, Vector3 point)
        {
            Target = target;
            Point = point;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    public interface IHitboxQuery
    {
        bool IsDetecting { get; }
        void BeginDetection(Transform origin);
        void EndDetection();
        IReadOnlyList<HitboxHit> Detect(Transform origin, LayerMask layerMask);
        void DrawGizmos(Transform origin);
    }
}

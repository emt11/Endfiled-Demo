using UnityEngine;

namespace GP
{
    public interface ITrigger
    {
        void TriggerEnter(Collider collider);
        void TriggerStay(Collider collider);
        void TriggerExit(Collider collider);
    }
}

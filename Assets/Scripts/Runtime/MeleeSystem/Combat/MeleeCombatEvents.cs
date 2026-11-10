using UnityEngine;

namespace GP
{
    public readonly struct HitboxWindowEvent : IEventContext
    {
        public readonly bool IsOpen;
        public readonly WeaponAttachmentPoint AttachmentPoints;
        public readonly HitDirection Direction;
        public readonly GameObject Sender;
        public readonly object Window;

        public HitboxWindowEvent(bool isOpen, WeaponAttachmentPoint attachmentPoints,
            HitDirection direction, GameObject sender, object window)
        {
            IsOpen = isOpen;
            AttachmentPoints = attachmentPoints;
            Direction = direction;
            Sender = sender;
            Window = window;
        }
    }

    public readonly struct ComboInputWindowEvent : IEventContext
    {
        public readonly bool IsOpen;
        public readonly GameObject Sender;
        public readonly object Window;

        public ComboInputWindowEvent(bool isOpen, GameObject sender, object window)
        {
            IsOpen = isOpen;
            Sender = sender;
            Window = window;
        }
    }

    public readonly struct ClearHitTargetsEvent : IEventContext
    {
        public readonly GameObject Sender;
        public ClearHitTargetsEvent(GameObject sender) => Sender = sender;
    }
}

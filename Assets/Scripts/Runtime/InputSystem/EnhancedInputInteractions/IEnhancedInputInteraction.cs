using System;


namespace GP
{
    public interface IEnhancedInputInteraction
    {
        event Action InteractionStarted;
        event Action InteractionTriggered;

        bool BeginInteraction();
        bool EndInteraction();
        void ResetInteraction();
    }

    public interface IEnhancedInputInteractionTick : IEnhancedInputInteraction
    {
        bool IsPendingRemoval { get; set; }

        void TickInteraction();
    }
}

// This is not a component. It's a contract.
// Any class that implements this interface MUST have these methods.
public interface IInteractable
{
    // A property to get the name for the UI prompt.
    string InteractionPrompt { get; }

    // A method that returns true if an interaction is possible.
    bool Interact(PlayerInteractor interactor);
}

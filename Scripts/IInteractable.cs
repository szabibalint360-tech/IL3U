using Godot;

public interface IInteractable
{
    // Returns the text prompt to display on screen (e.g., "Press E to Open", "Pick up Key")
    string GetInteractionPrompt();

    // The core action executed when the player interacts with the object
    void Interact(Node3D interactor);

    // Optional: Check if the object is currently interactive
    bool CanInteract() => true;
}
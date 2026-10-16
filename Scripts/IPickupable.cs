using Godot;

public interface IPickupable : IInteractable
{
    bool IsHeld { get; }
    void PickUp(Node3D holdNode);
    void Drop();
}
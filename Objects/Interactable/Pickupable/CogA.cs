using Godot;

public partial class CogA : RigidBody3D, IPickupable
{
    [Export] private int _gearCount = 25;
    [Export] private float _followSpeed = 15.0f;

    private bool _isHeld = false;
    private Node3D _holdNode;

    public bool IsHeld => _isHeld;

    public string GetInteractionPrompt()
    {
        if (_isHeld) return string.Empty;
        return $"Pick up CogA (Gear Count: {_gearCount})";
    }

    public void Interact(Node3D interactor)
    {
        if (!_isHeld)
        {
            var holdNode = interactor.GetNodeOrNull<Node3D>("Head/HoldPoint");
            if (holdNode != null)
            {
                PickUp(holdNode);
            }
        }
    }

    public void PickUp(Node3D holdNode)
    {
        _isHeld = true;
        _holdNode = holdNode;

        Freeze = true;
        FreezeMode = FreezeModeEnum.Kinematic;

        // Turn off Layer 2 (Interactable) so raycasts stop seeing it while carried
        SetCollisionLayerValue(2, false);
        // Turn off Mask 1 (Physics/World) so it doesn't collide with the environment
        SetCollisionMaskValue(1, false);
    }

    public void Drop()
    {
        _isHeld = false;

        Freeze = false;

        // Re-enable Layer 2 (Interactable) and Mask 1 (Physics)
        SetCollisionLayerValue(2, true);
        SetCollisionMaskValue(1, true);

        ApplyCentralImpulse(-_holdNode.GlobalTransform.Basis.Z * 5.0f);
        _holdNode = null;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isHeld && _holdNode != null)
        {
            GoToHoldPosition(delta);
        }
    }

    private void GoToHoldPosition(double delta)
    {
        GlobalPosition = GlobalPosition.Lerp(_holdNode.GlobalPosition, (float)(_followSpeed * delta));
        GlobalRotation = _holdNode.GlobalRotation;
    }
}
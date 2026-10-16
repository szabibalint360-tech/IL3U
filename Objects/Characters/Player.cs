using Godot;
using System;

public partial class Player : CharacterBody3D
{
    public const float Speed = 5.0f;
    public const float JumpVelocity = 4.5f;

    [Export] private float _mouseSensitivity = 0.0025f;
    [Export] private float _lookSmoothSpeed = 10.0f;

    private Node3D _head;
    private RayCast3D _interactionRayCast;
    private Label _interactionPromptLabel;
    private Label _eventPromptLabel;
    private Node2D _centerPoint;

    private IInteractable _currentInteractable;
    private IPickupable _carriedItem;

    private float _targetYaw;
    private float _targetPitch;
    private float _currentYaw;
    private float _currentPitch;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        // Fail-fast: Enforce that all required structural nodes exist at startup
        _head = GetNodeOrNull<Node3D>("Head")
            ?? throw new InvalidOperationException($"{Name}: Missing required child node 'Head'.");

        _interactionRayCast = GetNodeOrNull<RayCast3D>("Head/InteractionRayCast")
            ?? throw new InvalidOperationException($"{Name}: Missing required child node 'Head/InteractionRayCast'.");

        // HUD nodes
        _interactionPromptLabel = GetNodeOrNull<Label>("HUD/ItemPrompt")
            ?? throw new InvalidOperationException($"{Name}: Missing required UI node 'HUD/ItemPrompt'.");

        _eventPromptLabel = GetNodeOrNull<Label>("HUD/EventPrompt")
            ?? throw new InvalidOperationException($"{Name}: Missing required UI node 'HUD/EventPrompt'.");

        _centerPoint = GetNodeOrNull<Node2D>("HUD/CenterPoint")
            ?? throw new InvalidOperationException($"{Name}: Missing required UI node 'HUD/CenterPoint'.");

        // Initial values
        _interactionPromptLabel.Text = "";
        _eventPromptLabel.Text = "";

        // Initialize look angles to prevent starting snap
        _targetYaw = Rotation.Y;
        _currentYaw = _targetYaw;
        _targetPitch = _head.Rotation.X;
        _currentPitch = _targetPitch;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            _targetYaw -= mouseMotion.Relative.X * _mouseSensitivity;
            _targetPitch -= mouseMotion.Relative.Y * _mouseSensitivity;
            _targetPitch = Mathf.Clamp(_targetPitch, Mathf.DegToRad(-89f), Mathf.DegToRad(89f));
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        if (Input.IsActionJustPressed("interact"))
        {
            if (_carriedItem == null && _currentInteractable is IPickupable pickupable)
            {
                _carriedItem = pickupable;
                _carriedItem.Interact(this);
            }
            else if (_carriedItem == null && _currentInteractable != null)
            {
                _currentInteractable.Interact(this);
            }
        }

        if (Input.IsActionJustPressed("drop") && _carriedItem != null)
        {
            _carriedItem.Drop();
            _carriedItem = null;
        }
    }

    public override void _Process(double delta)
    {
        CheckForInteractables();
        UpdateInteractionPrompt();
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateCameraLook(delta);
        HandleMovement(delta);
    }
    private void HandleMovement(double delta)
    {
        Vector3 velocity = Velocity;
        if (!IsOnFloor()) velocity += GetGravity() * (float)delta;
        if (Input.IsActionJustPressed("jump") && IsOnFloor()) velocity.Y = JumpVelocity;

        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private void UpdateCameraLook(double delta)
    {
        float smoothFactor = 1.0f - Mathf.Exp(-(float)(_lookSmoothSpeed * delta));
        _currentYaw = Mathf.Lerp(_currentYaw, _targetYaw, smoothFactor);
        _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, smoothFactor);

        Rotation = new Vector3(Rotation.X, _currentYaw, Rotation.Z);

        Vector3 headRotation = _head.Rotation;
        headRotation.X = _currentPitch;
        _head.Rotation = headRotation;
    }

    private void CheckForInteractables()
    {
        _interactionRayCast.ForceRaycastUpdate();

        if (_interactionRayCast.IsColliding())
        {
            GodotObject collider = _interactionRayCast.GetCollider();
            if (collider is IInteractable interactable)
            {
                if (_currentInteractable != interactable)
                {
                    _currentInteractable = interactable;
                    GD.Print($"Looking at: {_currentInteractable.GetInteractionPrompt()}");
                }
                return;
            }
        }

        if (_currentInteractable != null)
        {
            _currentInteractable = null;
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (_currentInteractable != null)
        {
            _interactionPromptLabel.Text = _currentInteractable.GetInteractionPrompt();
            _centerPoint.Scale = new Vector2(0.2f, 0.2f);
        }
        else
        {
            _interactionPromptLabel.Text = "";
            _centerPoint.Scale = new Vector2(0.15f, 0.15f);
        }
    }
}
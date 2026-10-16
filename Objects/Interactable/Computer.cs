using Godot;

public partial class Computer : StaticBody3D, IInteractable
{
    [Export] private StandardMaterial3D _offMaterial;
    [Export] private StandardMaterial3D _onMaterial;

    public bool TurnedOn = false;
    private CsgMesh3D _screen;

    public override void _Ready()
    {
        _screen = GetNode<CsgMesh3D>("ScreenMesh");
        UpdateScreenMaterial();
    }

    public string GetInteractionPrompt()
    {
        return TurnedOn ? "Shut down computer" : "Start up computer";
    }

    public void Interact(Node3D interactor)
    {
        TurnedOn = !TurnedOn;
        UpdateScreenMaterial();
    }

    private void UpdateScreenMaterial()
    {
        if (_screen == null) return;

        if (TurnedOn)
        {
            _screen.Material = _onMaterial ?? CreateColorMaterial(new Color(0.05f, 0.1f, 0.3f));
        }
        else
        {
            _screen.Material = _offMaterial ?? CreateColorMaterial(Colors.Black);
        }
    }

    private StandardMaterial3D CreateColorMaterial(Color color)
    {
        var mat = new StandardMaterial3D();
        mat.AlbedoColor = color;
        return mat;
    }
}
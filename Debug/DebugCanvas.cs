using Godot;
using System;
using System.Diagnostics;

public partial class DebugCanvas : CanvasLayer
{
    private Label _debugLabel;
    private float _timeSinceLastUpdate = 0.0f;
    private const float UpdateInterval = 0.2f; // Updates 5 times per second

    // CPU tracking variables
    private Process _currentProcess;
    private TimeSpan _lastCpuTime;
    private long _lastTimeTick;
    private double _cpuUsagePercent = 0.0;

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("debug"))
        {
            Visible = !Visible;
        }
    }
    public override void _Ready()
    {
        _currentProcess = Process.GetCurrentProcess();
        _lastCpuTime = _currentProcess.TotalProcessorTime;
        _lastTimeTick = System.Environment.TickCount64;

        // Create a margin container to position the HUD in the top-left corner
        var marginContainer = new MarginContainer();
        marginContainer.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        marginContainer.OffsetLeft = 15;
        marginContainer.OffsetTop = 15;
        AddChild(marginContainer);

        // Create and style the stats label
        _debugLabel = new Label();
        _debugLabel.Name = "DebugStatsLabel";

        var settings = new LabelSettings
        {
            FontSize = 14,
            FontColor = new Color(0.2f, 1.0f, 0.4f), // Terminal green
            OutlineSize = 4,
            OutlineColor = Colors.Black
        };
        _debugLabel.LabelSettings = settings;

        marginContainer.AddChild(_debugLabel);
    }

    public override void _Process(double delta)
    {
        _timeSinceLastUpdate += (float)delta;
        if (_timeSinceLastUpdate >= UpdateInterval)
        {
            UpdateCpuUsage();
            _timeSinceLastUpdate = 0.0f;
            UpdateStatsText();
        }
    }

    private void UpdateCpuUsage()
    {
        try
        {
            _currentProcess.Refresh();
            TimeSpan currentCpuTime = _currentProcess.TotalProcessorTime;
            long currentTick = System.Environment.TickCount64;

            double cpuTimePassedMs = (currentCpuTime - _lastCpuTime).TotalMilliseconds;
            long wallTimePassedMs = currentTick - _lastTimeTick;

            if (wallTimePassedMs > 0)
            {
                // Normalize CPU usage across all logical processor cores
                _cpuUsagePercent = (cpuTimePassedMs / (wallTimePassedMs * System.Environment.ProcessorCount)) * 100.0;
            }

            _lastCpuTime = currentCpuTime;
            _lastTimeTick = currentTick;
        }
        catch (Exception)
        {
            _cpuUsagePercent = 0.0;
        }
    }

    private void UpdateStatsText()
    {
        if (_debugLabel == null) return;

        double fps = Engine.GetFramesPerSecond();
        double frameTimeMs = Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000.0;
        double physicsTimeMs = Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess) * 1000.0;
        double staticMemoryMb = Performance.GetMonitor(Performance.Monitor.MemoryStatic) / 1024.0 / 1024.0;

        // GPU tracking via VRAM usage on the rendering device
        double vramUsageMb = Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed) / 1024.0 / 1024.0;

        _debugLabel.Text =
            $"=== SYSTEM DEBUG ===\n" +
            $"FPS:           {fps:F2}\n" +
            $"Frame Time:    {frameTimeMs:F2} ms\n" +
            $"Physics Time:  {physicsTimeMs:F2} ms\n" +
            $"Static Mem:    {staticMemoryMb:F2} MB\n" +
            $"App CPU:       {_cpuUsagePercent:F1}%\n" +
            $"GPU VRAM:      {vramUsageMb:F2} MB";
    }
}
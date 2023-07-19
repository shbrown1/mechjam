using Godot;
using System;

public class UserInterface : CanvasLayer
{
    [Export]
    public NodePath ObjectiveLabelNodePath;
    [Export]
    public NodePath MissionAccomplishedNodePath;

    private Label _objectiveLabel;
    private Label _missionAccomplishedLabel;
    private bool _triggeredObjective;
    private bool _triggeredMissionAccomplished;
    private Player _player;

    public override void _Ready()
    {
        _objectiveLabel = GetNode<Label>(ObjectiveLabelNodePath);
        _missionAccomplishedLabel = GetNode<Label>(MissionAccomplishedNodePath);
        _objectiveLabel.Visible = false;
        _objectiveLabel.RectPosition = new Vector2(-1000, _objectiveLabel.RectPosition.y);
        _missionAccomplishedLabel.Visible = false;
        _missionAccomplishedLabel.RectPosition = new Vector2(-1000, _missionAccomplishedLabel.RectPosition.y);
        _player = GetTree().Root.FindNode("Player", true, false) as Player;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        var screenSize = GetTree().Root.GetViewport().Size;
        float speed = screenSize.x / 18f;

        if (_triggeredObjective && _objectiveLabel.RectPosition.x < 2000)
        {
            if (_objectiveLabel.RectPosition.x > 0)
                _player.IsPlayingCinematic = false;

            var getCenter = GetLabelCenter(_objectiveLabel);

            if (getCenter.x < (screenSize.x * 0.4f) || getCenter.x > (screenSize.x * 0.6f))
                _objectiveLabel.RectPosition += new Vector2(speed * 60 * delta, 0);
            else
                _objectiveLabel.RectPosition += new Vector2(speed * delta, 0);
        }

        if (_triggeredMissionAccomplished && _missionAccomplishedLabel.RectPosition.x < 2000)
        {
            if (_missionAccomplishedLabel.RectPosition.x > 0)
                _player.IsPlayingCinematic = false;

            var getCenter = GetLabelCenter(_missionAccomplishedLabel);

            if (getCenter.x < (screenSize.x * 0.4f) || getCenter.x > (screenSize.x * 0.6f))
                _missionAccomplishedLabel.RectPosition += new Vector2(speed * 60 * delta, 0);
            else
                _missionAccomplishedLabel.RectPosition += new Vector2(speed * delta, 0);
        }
    }


    public void ShowObjective()
    {
        _triggeredObjective = true;
        _objectiveLabel.Visible = true;
    }

    public void ShowMissionAccomplished()
    {
        _triggeredMissionAccomplished = true;
        _missionAccomplishedLabel.Visible = true;
    }

    Vector2 GetLabelCenter(Label label)
    {
        var pos = label.RectPosition;
        var size = label.RectSize;
        return pos + (size / 2f);
    }
}

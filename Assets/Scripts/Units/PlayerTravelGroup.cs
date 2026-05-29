using UnityEngine;
using System.Collections.Generic;

public class PlayerTravelGroup : UnitTravelGroup
{
    [SerializeField] private GameObject _light;

    private List<BaseNode> _path = new List<BaseNode>();
    private BaseBuilding _originBuilding;
    private bool _tracksOriginBuilding;

    public void Setup(int units, BaseNode start, BaseNode destination)
    {
        var settings = GameManager.Instance.GetGameSettings();
        float speed = Random.Range(settings.playerSpeedMin, settings.playerSpeedMax);
        float stepDelay = Random.Range(settings.playerStepDelayMin, settings.playerStepDelayMax);
        Color color = settings.playerUnitGroupColor;

        var building = start.GetBuilding();
        _originBuilding = (building != null && building.Side == ConflictSide.Player) ? building : null;
        _tracksOriginBuilding = _originBuilding != null;

        var newPath = PathfindingManager.Instance.GetPath(start, destination);
        if (newPath == null || newPath.Count == 0)
        {
            Debug.LogWarning("PlayerTravelGroup: Path is empty or null.");
            Destroy(gameObject);
            return;
        }

        _path = newPath;
        // First node in path is the start node itself, skip it
        if (_path.Count > 0 && _path[0] == start)
            _path.RemoveAt(0);

        SetupBase(units, start, ConflictSide.Player, speed, stepDelay, color);
    }

    protected override void OnArrivedAtNode()
    {
        // Check if origin building was destroyed
        if (_tracksOriginBuilding && _originBuilding == null)
        {
            DisbandAndDeleteUnits();
            return;
        }
    }

    protected override void OnReadyToMove()
    {
        // Check if origin building was destroyed
        if (_tracksOriginBuilding && _originBuilding == null)
        {
            DisbandAndDeleteUnits();
            return;
        }

        if (_path.Count > 0)
        {
            var next = _path[0];
            _path.RemoveAt(0);
            UpdateLight(next);
            MoveTo(next);
        }
        else
        {
            Disband();
        }
    }

    private void UpdateLight(BaseNode destination)
    {
        if (_light == null) return;
        bool safe = destination.GetOwner() == ConflictSide.Player ||
                    destination.GetAllPlayerUnits() > 0;
        _light.SetActive(!safe);
    }

    protected override void OnEncounterChanged(EncounterInfo info)
    {
        // Player groups stay and fight - no special logic needed
        // They just remain on the node while NodeEncounterController handles combat
    }
}

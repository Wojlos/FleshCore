public enum EncounterType
{
    None,
    Battle,
    Siege,
    NodeConquest
}

public struct EncounterInfo
{
    public BaseNode Node;
    public EncounterType Type;
    public ConflictSide AttackingSide;
    public int OpposingUnits;
    public bool HasOpposingBuilding;
}

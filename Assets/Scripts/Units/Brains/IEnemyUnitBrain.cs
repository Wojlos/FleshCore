public interface IEnemyUnitBrain
{
    void Initialize(EnemyTravelGroup body, BaseNode startNode, BaseBuilding originBuilding);

    // Called by body when it arrives at a node and is ready for next instruction
    BrainOrder DecideNext(BaseNode currentNode);

    // Called by body when an encounter state changes on the current node
    BrainOrder OnEncounterUpdate(EncounterInfo info);

    // Called by body when encounter ends (units survived, opposing side eliminated)
    BrainOrder OnEncounterEnded(BaseNode currentNode);

    // Called when the body is externally redirected (e.g. by LymphNode alert)
    void OnRedirected(BaseNode newDestination);

    // Called when the body is about to be destroyed (for cleanup/reporting)
    void OnBodyDestroyed(BaseNode lastNode);
}

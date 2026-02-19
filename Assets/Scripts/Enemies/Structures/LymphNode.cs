using UnityEngine;
using System.Collections.Generic;

public class LymphNode : BaseStructure {
    public LymphNode() : base(BuildingType.LymphNode)
    {
    }

    private void Awake()
    {
        _structureType = StructureType.LymphNode;
    }

}

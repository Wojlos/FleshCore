public enum BrainInstruction
{
    Wait,
    Move
}

public struct BrainOrder
{
    public BrainInstruction Instruction;
    public BaseNode TargetNode;

    public static BrainOrder WaitOrder() => new BrainOrder { Instruction = BrainInstruction.Wait };
    public static BrainOrder MoveOrder(BaseNode target) => new BrainOrder { Instruction = BrainInstruction.Move, TargetNode = target };
}

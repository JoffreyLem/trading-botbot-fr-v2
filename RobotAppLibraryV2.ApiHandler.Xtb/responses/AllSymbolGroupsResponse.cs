using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

public class AllSymbolGroupsResponse : BaseResponse
{
    public AllSymbolGroupsResponse(string body) : base(body)
    {
    }

    public virtual LinkedList<SymbolGroupRecord> SymbolGroupRecords { get; } = new();
}
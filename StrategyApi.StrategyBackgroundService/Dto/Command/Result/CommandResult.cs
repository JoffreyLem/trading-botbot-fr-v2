namespace StrategyApi.StrategyBackgroundService.Dto.Command.Result;

public abstract record CommandResultBase;

public record CommandExecutedResult : CommandResultBase;

public record CommandExecutedTypedResult<T>(T value) : CommandResultBase;
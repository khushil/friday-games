namespace OXS.Core;

public abstract record Result<T> {
    public bool IsSuccess => this is Success;
    public bool IsFailure => !IsSuccess;

    public sealed record Success(T Value) : Result<T>;
    public sealed record Failure(string Error) : Result<T>;

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string, TOut> onFailure) => this switch {
        Success s => onSuccess(s.Value),
        Failure f => onFailure(f.Error),
        _ => throw new InvalidOperationException()
    };
}

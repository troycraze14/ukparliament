﻿using System.Diagnostics.CodeAnalysis;

namespace People.Api.ApiModels;

public class Result<T>
{
    public T? Value { get; }
    
    public ErrorResponse? Error { get; }
    
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }
    
    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }
    
    private Result(ErrorResponse error)
    {
        Error = error;
        IsSuccess = false;
    }
    
    private static Result<T> Fail(ErrorType type, string error) => new(new ErrorResponse(error, type));
    
    public static Result<T> NotFound(string error) => Fail(ErrorType.NotFound, error);
    
    public static Result<T> Invalid(string error) => Fail(ErrorType.Invalid, error);
    
    public static Result<T> InternalServerError(string error) => Fail(ErrorType.InternalServerError, error);

    public static Result<T> Ok(T value) => new(value);

    public static implicit operator Result<T>(T value) => Ok(value);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ErrorResponse, TResult> onFailure) =>
        IsSuccess 
            ? onSuccess(Value) 
            : onFailure(Error);
}
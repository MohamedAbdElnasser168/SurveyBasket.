namespace SurveyBasket.Api.Abstractions
{
    public class Result
    {
        public bool IsSuccess { get; }
        // A failure result is simply the opposite of a success result, so we can derive it from the IsSuccess property.
        // we can't use IsFailure in the constructor because it would create a circular dependency, so we define it as a separate property that returns the negation of IsSuccess.
        public bool IsFailure => !IsSuccess;
        // Error property to hold the error information when the result is a failure.
        // It is initialized with a default value, and it will be set to the appropriate error when creating a failure result.
        public Error Error { get; } = default!;


        public Result(bool isSuccess, Error error)
        {
            if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
            {
                throw new InvalidOperationException("Invalid result state: " +
                    "A successful result cannot have an error, " +
                    "and a failed result must have an error.");
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        // Factory methods for creating success and failure results
        // without including data
        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);

        
        public static Result<TValue> Success<TValue>(TValue value)
        {
            return new Result<TValue>(value, true, Error.None);
        }

        public static Result<TValue> Failure<TValue>(Error error)
        {
            return new Result<TValue>(default, false, error);
        }
       

        //public static Result<TValue> Success<TValue>(TValue value) => new(value, true,Error.None);
        //public static Result<TValue> Failure<TValue>(Error error) => new( default,false, error);



    }



    // if we want to return Success with data  It will be something like this: Result<User>.Success(user);
    public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        // getter for this property private field _value, but only if the result is successful. If the result is a failure,
        // it will throw an exception when trying to access the Value property.
        public TValue Value
        {
            get
            {
                if (IsSuccess)
                {
                    return _value!;
                }
                else
                {
                    throw new InvalidOperationException("Failure results cannot have value");
                }
            }
        }

        //public TValue Value => IsSuccess 
        // ? _value! 
        // : throw new InvalidOperationException("Cannot access the value of a failed result.");

        public Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
        {
            _value = value;
        }


    }
}

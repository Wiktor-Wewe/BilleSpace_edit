using System.Text.Json.Serialization;

namespace BilleSpace.Domain.Results
{
    public class Result
    {
        [JsonIgnore]
        public ResultCode CodeResult { get; set; }
        public int Code { get; set; }
        public List<string> Errors { get; set; }

        public static Result Ok()
        {
            return new Result()
            {
                CodeResult = ResultCode.Ok,
                Code = 200
            };
        }

        public static Result<T> Ok<T>(T Value)
        {
            return new Result<T>
            {
                Data = Value,
                CodeResult = ResultCode.Ok,
                Code = 200
            };
        }

        public static Result BadRequest(string message)
        {
            var result = new Result
            {
                CodeResult = ResultCode.BadRequest,
                Errors = new List<string> { message },
                Code = 400
            };
            return result;
        }

        public static Result<T> BadRequest<T>(List<string> messages)
        {
            var result = new Result<T>
            {
                CodeResult = ResultCode.BadRequest,
                Errors = messages,
                Code = 400
            };

            return result;
        }

        public static Result<T> NotFound<T>(Guid id)
        {
            var result = new Result<T>
            {
                CodeResult = ResultCode.NotFound,
                Errors = new List<string> { $"There is no object with id: {id}" },
                Code = 404
            };

            return result;
        }

        public static Result NotFound(Guid id)
        {
            var result = new Result
            {
                CodeResult = ResultCode.NotFound,
                Errors = new List<string> { $"There is no object with id: {id}" },
                Code = 404
            };

            return result;
        }

        public static Result<T> NotFound<T>(string error)
        {
            var result = new Result<T>
            {
                CodeResult = ResultCode.NotFound,
                Errors = new List<string> { error },
                Code = 404
            };

            return result;
        }

        public static Result<T> Forbidden<T>(string message)
        {
            var result = new Result<T>
            {
                CodeResult = ResultCode.Forbidden,
                Errors = new List<string> { message },
                Code = 403
            };
            return result;
        }

        public static Result<T> Forbidden<T>(List<string> messages)
        {
            var result = new Result<T>
            {
                CodeResult = ResultCode.Forbidden,
                Errors = messages,
                Code = 403
            };

            return result;
        }

    }
    public class Result<T> : Result
    {
        public T Data { get; set; }
    }

    public enum ResultCode
    {
        Ok,
        BadRequest,
        NotFound,
        Forbidden
    }
}
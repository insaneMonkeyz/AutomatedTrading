using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppComponents.Messaging.Results
{
    public struct Result
    {
        public bool IsSuccess;
        public string? Description;
        public object? Data;

        public static Result Success(object? data = null)
        {
            return new Result 
            { 
                IsSuccess = true,
                Data = data
            };
        }
        public static Result Error(string? description = null, object? data = null)
        {
            return new Result 
            { 
                Description = description,
                Data = data
            };
        }
    }
}

using Application.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldReturnSuccessfulResult()
        {
            const string value = "test";

            var result = Result<string>.Success(value);

            Assert.True(result.IsSuccess);
            Assert.Equal(value, result.Value);
            Assert.Empty(result.Error);
        }

        [Fact]
        public void Failure_ShouldReturnFailedResult()
        {
            const string error = "Something went wrong";

            var result = Result<string>.Failure(error);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(error, result.Error);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Tests;

internal static class QueryableExtensions
{
    public static IQueryable<T> BuildMock<T>(
        this IEnumerable<T> source)
    {
        return new TestAsyncEnumerable<T>(source);
    }
}
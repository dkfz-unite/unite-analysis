namespace Unite.Analysis.Expression.Extensions;

internal static class TaskExtensions
{
    public static async Task Batch<T>(this IEnumerable<T> values, byte batchSize, Func<T, Task> action)
    {
        for (int i = 0; i < values.Count(); i += batchSize)
        {
            var tasks = values
                .Skip(i).Take(batchSize)
                .Select(value => action(value));

            await Task.WhenAll(tasks);
        }
    }
}

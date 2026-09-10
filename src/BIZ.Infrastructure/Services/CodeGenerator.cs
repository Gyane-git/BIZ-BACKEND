using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public static class CodeGenerator
{
    public static async Task<string> NextAsync(
        IQueryable<string> existingCodes,
        string prefix)
    {
        var suffixes = await existingCodes
            .Where(code => code.StartsWith(prefix))
            .Select(code => code.Substring(prefix.Length))
            .ToListAsync();

        var next = suffixes
            .Select(value => int.TryParse(value, out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"{prefix}{next:00000}";
    }
}

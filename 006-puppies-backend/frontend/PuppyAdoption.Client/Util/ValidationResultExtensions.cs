using System;
using System.Linq;
using FluentValidation.Results;

namespace PuppyAdoption.Client.Util;

/// <summary>
///     Extension methods for validation results
/// </summary>
public static class ValidationResultExtensions
{
    extension(ValidationResult result)
    {
        /// <summary>
        ///     Creates a message listing all validation errors, one per line
        /// </summary>
        /// <returns>The message to show to the user</returns>
        public string ToMessage() =>
            string.Join(Environment.NewLine, result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
    }
}

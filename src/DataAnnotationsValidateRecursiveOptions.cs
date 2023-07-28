using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ReHackt.Extensions.Options.Validation
{
    /// <summary>
    /// Implementation of <see cref="IValidateOptions{TOptions}"/> that uses DataAnnotation's <see cref="Validator"/> for validation.
    /// </summary>
    /// <typeparam name="TOptions">The instance being validated.</typeparam>
    public class DataAnnotationsValidateRecursiveOptions<TOptions> : IValidateOptions<TOptions> where TOptions : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        public DataAnnotationsValidateRecursiveOptions(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The options name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Validates a specific named options instance (or all when name is null).
        /// </summary>
        /// <param name="name">The name of the options instance being validated.</param>
        /// <param name="options">The options instance.</param>
        /// <returns>The <see cref="ValidateOptionsResult"/> result.</returns>
        public ValidateOptionsResult Validate(string name, TOptions options)
        {
            // Ignored if not validating this instance.
            if (Name != null && name != Name) return ValidateOptionsResult.Skip;
            
            // Null name is used to configure all named options.
            var validator = new DataAnnotationsValidator();
            var validationResults = new List<ValidationResult>();
            return validator.TryValidateObjectRecursive(options, validationResults) ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(string.Join(Environment.NewLine, validationResults.Select(r => "DataAnnotation validation failed for members " + string.Join(", ", r.MemberNames) + " with the error '" + r.ErrorMessage + "'.")));
        }
    }
}
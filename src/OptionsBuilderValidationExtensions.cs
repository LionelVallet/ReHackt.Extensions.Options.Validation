using Microsoft.Extensions.Options;
using ReHackt.Extensions.Options.Validation;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding configuration related options services to the DI container via <see cref="OptionsBuilder{TOptions}"/>.
    /// </summary>
    public static class OptionsBuilderValidationExtensions
    {
        /// <summary>
        /// Register this options instance for validation of its DataAnnotations.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="optionsBuilder">The options builder to add the services to.</param>
        /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that additional calls can be chained.</returns>
        public static OptionsBuilder<TOptions> ValidateDataAnnotationsRecursively<TOptions>(this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
        {
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));

            optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(new DataAnnotationsValidateRecursiveOptions<TOptions>(optionsBuilder.Name));
            return optionsBuilder;
        }

        /// <summary>
        /// Validates this options instance at startup.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="optionsBuilder">The options builder to add the services to.</param>
        /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that additional calls can be chained.</returns>
        [Obsolete("Use ValidateOnStart instead to follow Microsoft guidelines (https://github.com/dotnet/runtime/issues/36391)")]
        public static OptionsBuilder<TOptions> ValidateEagerly<TOptions>(this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
        {
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));

            return optionsBuilder.ValidateOnStart();
        }

        /// <summary>
        /// Registers an action used to configure a particular type of options and validates this options instance at startup.
        /// Note: These are run before all <seealso cref="PostConfigure{TOptions}(IServiceCollection, Action{TOptions})"/>.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="optionsBuilder">The options builder to add the services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that additional calls can be chained.</returns>
        public static OptionsBuilder<TOptions> ConfigureAndValidate<TOptions>(this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions> configureOptions) where TOptions : class
        {
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));

            return optionsBuilder
                    .Configure(configureOptions)
                    .ValidateDataAnnotationsRecursively()
                    .ValidateOnStart();
        }
    }
}
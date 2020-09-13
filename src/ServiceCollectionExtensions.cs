using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers an action used to configure a particular type of options and validates this options instance at startup.
        /// Note: These are run before all <seealso cref="PostConfigure{TOptions}(IServiceCollection, Action{TOptions})"/>.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection ConfigureAndValidate<TOptions>(this IServiceCollection services, Action<TOptions> configureOptions) where TOptions : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services
                .AddOptions<TOptions>()
                    .ConfigureAndValidate(configureOptions)
                    .Services;
        }
    }
}

# ReHackt.Extensions.Options.Validation
Extends `OptionsBuilder<T>` and `IServiceCollection` with **nested data annotations validation** and **eager validation on start**.

## Get started

Get it on <a href="https://www.nuget.org/packages/ReHackt.Extensions.Options.Validation"><img src="https://www.nuget.org/Content/gallery/img/default-package-icon.svg" height=18 style="height:18px;" /> NuGet</a>

## TL;DR

1) Create your options class(es)
2) Decorate your options with data annotations
3) Call `ConfigureAndValidate<T>(Action<T> configureOptions)` on your `IServiceCollection`

`ConfigureAndValidate` will configure your options (calling the base `Configure` method), but will also check that the built configuration respects the data annotations, otherwise an OptionsValidationException (with details) is thrown as soon as the application is started. No misconfiguration surprise at runtime!

### Example

``` csharp
// Startup configuration

public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureAndValidate<ApplicationOptions>(options => _configuration.Bind(options));
    
    ...
}


// Options classes

public class ApplicationOptions
{
    [Required]
    public EmailOptions Email { get; set; }

    public SecurityOptions Security { get; set; }

    ...
}

public class EmailOptions
{
    [Required]
    public string Host { get; set; }

    [Required]
    public int Port { get; set; }

    public bool UseSsl { get; set; }

    ...
}

public class SecurityOptions
{
    [Range(8, int.MaxValue, ErrorMessage = "The {0} must be greater than {1}.")]
    public int PasswordMinLength { get; set; } = 8;

    ...
}
```

## Use

### ServiceCollection extension

#### `ConfigureAndValidate`

``` csharp
services.ConfigureAndValidate<TOptions>(configureOptions)
```
  
Is syntactic sugar for

``` csharp
services
    .AddOptions<TOptions>()
        .Configure(configureOptions)
        .ValidateDataAnnotationsRecursively()
        .ValidateOnStart()   // or ValidateEagerly()
        .Services
```

### OptionsBuilder extensions

#### `ValidateDataAnnotationsRecursively`

This method register this options instance for validation of its DataAnnotations at the first dependency injection. Nested objects are supported.

#### `ValidateOnStart` *(or `ValidateEagerly` in previous versions)*

This method validates this options instance at application startup rather than at the first dependency injection.

#### `ConfigureAndValidate`

``` csharp
optionsBuilder.ConfigureAndValidate<TOptions>(configureOptions)
```
  
Is syntactic sugar for

``` csharp
optionsBuilder
    .Configure(configureOptions)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart()   // or ValidateEagerly()
```

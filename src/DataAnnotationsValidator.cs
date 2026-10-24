using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace ReHackt.Extensions.Options.Validation
{
    public class DataAnnotationsValidator : IDataAnnotationsValidator
    {
        // Bounds the descent: a computed property that returns a new object on each read, such as a record copy,
        // never revisits an object, so the visited set alone would recurse until the stack overflows.
        private const int MaxDepth = 32;

        public bool TryValidateObject(object obj, ICollection<ValidationResult> results, IDictionary<object, object> validationContextItems = null)
        {
            return Validator.TryValidateObject(obj, new ValidationContext(obj, null, validationContextItems), results, true);
        }

        public bool TryValidateObjectRecursive<T>(T obj, List<ValidationResult> results, IDictionary<object, object> validationContextItems = null)
        {
            return obj == null
                ? throw new ArgumentNullException(nameof(obj))
                // Visited objects are compared by reference: two distinct objects that are equal are both validated.
                : TryValidateObjectRecursive(obj, results, new HashSet<object>(ReferenceComparer.Instance), validationContextItems, 0);
        }

        private bool TryValidateObjectRecursive(object obj, List<ValidationResult> results, ISet<object> validatedObjects, IDictionary<object, object> validationContextItems, int depth)
        {
            // A visited object is not validated twice, which also stops cycles.
            if (depth > MaxDepth || IsNonValidatableType(obj.GetType()) || !validatedObjects.Add(obj))
            {
                return true;
            }

            var result = TryValidateObject(obj, results, validationContextItems);

            foreach (var property in GetNestedProperties(obj.GetType()))
            {
                var value = property.GetValue(obj);
                if (value == null || value is string) continue;

                // The items of a collection are validated, not the collection itself: dictionary entries included.
                foreach (var item in value as IEnumerable ?? new[] { value })
                {
                    // Strings, primitives and enums carry no nested annotation; skipping them keeps a byte array cheap.
                    if (item == null || item is string || item.GetType().IsPrimitive || item.GetType().IsEnum) continue;

                    var nestedResults = new List<ValidationResult>();
                    if (TryValidateObjectRecursive(item, nestedResults, validatedObjects, validationContextItems, depth + 1)) continue;

                    result = false;
                    results.AddRange(nestedResults.Select(r => new ValidationResult(r.ErrorMessage, r.MemberNames.Select(memberName => property.Name + "." + memberName))));
                }
            }

            return result;
        }

        // The most derived readable declaration of each public instance property: a base property hidden with `new` is
        // not read, nor a static property, which often holds a default instance. Strings and value types are not
        // descended; their annotations are validated on the parent.
        private static IEnumerable<PropertyInfo> GetNestedProperties(Type type)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            for (var current = type; current != null; current = current.BaseType)
            {
                foreach (var property in current.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    // A declaration without getter, such as a setter-only override, leaves the name to the readable one.
                    if (property.GetMethod != null
                        && names.Add(property.Name)
                        && property.GetIndexParameters().Length == 0
                        && property.PropertyType != typeof(string)
                        && !property.PropertyType.IsValueType)
                    {
                        yield return property;
                    }
                }
            }
        }

        // These types carry no annotations, and some of their getters throw or never end when read: Type.DeclaringMethod
        // on most types, which delegates reach through their Method, Uri.Segments on a relative URI, the key of some
        // certificates, or FileSystemInfo, whose directories lead to new parent instances up to the root.
        private static bool IsNonValidatableType(Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type)
                   || typeof(MemberInfo).IsAssignableFrom(type)
                   || typeof(ParameterInfo).IsAssignableFrom(type)
                   || typeof(Module).IsAssignableFrom(type)
                   || typeof(Assembly).IsAssignableFrom(type)
                   || typeof(Uri).IsAssignableFrom(type)
                   || typeof(X509Certificate).IsAssignableFrom(type)
                   || typeof(FileSystemInfo).IsAssignableFrom(type);
        }

        // System.Collections.Generic.ReferenceEqualityComparer does not exist on .NET Standard 2.0.
        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

            int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }


    public interface IDataAnnotationsValidator
    {
        bool TryValidateObject(object obj, ICollection<ValidationResult> results, IDictionary<object, object> validationContextItems = null);
        bool TryValidateObjectRecursive<T>(T obj, List<ValidationResult> results, IDictionary<object, object> validationContextItems = null);
    }
}

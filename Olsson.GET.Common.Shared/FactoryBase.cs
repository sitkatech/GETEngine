using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Common.Shared
{
    public abstract class FactoryBase
    {
        // contains the dictionary of overrides provided for this factory instance
        readonly Dictionary<string, object> _overrides = new Dictionary<string, object>();

        // contains the dictionary of types supported by this factory
        readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        /// <summary>
        /// Provides mock override objects for testing purposes 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        public void AddOverride<T>(T obj, string key = "")
        {
            var dictKey = GetDictionaryKey<T>(key);
            if (_overrides.ContainsKey(dictKey))
                _overrides.Remove(dictKey);

            _overrides.Add(dictKey, obj);
        }

        private string GetDictionaryKey<T>(string key)
        {
            return $"{typeof(T).Name}_{key}";
        }

        /// <summary>
        /// Configure the types supported by this factory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        public void AddType<T>(Type obj, string key = "")
        {
            var dictKey = GetDictionaryKey<T>(key);
            if (_types.ContainsKey(dictKey))
                _types.Remove(dictKey);

            _types.Add(dictKey, obj);
        }

        protected T GetInstanceForType<T>(string key = "") where T : class
        {
            var dictKey = GetDictionaryKey<T>(key);
            // Return the override, if one exists for the type T
            if (_overrides.ContainsKey(dictKey))
            {
                return _overrides[dictKey] as T;
            }

            // No override, so return an instance of the type from the configured types
            if (_types.ContainsKey(dictKey))
            {
                var type = _types[dictKey] as Type;
                if (type != null)
                {
                    return Activator.CreateInstance(type) as T;
                }
            }

            // Oops, no override OR configuration for this type
            throw new ArgumentException($"{typeof(T).Name} is not supported by this factory (Key={key})");
        }
    }
}

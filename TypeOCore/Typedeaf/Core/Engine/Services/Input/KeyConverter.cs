using System.Collections.Generic;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Services.Input
    {
        /// <summary>Maps input values to their corresponding key aliases.</summary>
        public class KeyConverter
        {
            /// <summary>Initializes an empty input-to-key mapping.</summary>
            public KeyConverter()
            {
                InputToKeyConverter = new Dictionary<object, object>();
            }

            private Dictionary<object, object> InputToKeyConverter { get; set; }

            /// <summary>Adds a mapping from an input value to a key alias.</summary>
            public KeyConverter SetKeyAlias(object input, object key)
            {
                InputToKeyConverter.Add(input, key);
                return this;
            }

            /// <summary>
            /// Removes the key alias associated with the specified input.
            /// </summary>
            /// <param name="input">The input value whose key alias mapping is removed.</param>
            public void RemoveKeyAlias(object input)
            {
                if(InputToKeyConverter.ContainsKey(input))
                    InputToKeyConverter.Remove(input);
            }

            /// <summary>
            /// Removes all configured input-to-key aliases.
            /// </summary>
            /// <remarks>After this call, no key aliases remain.</remarks>
            public void ClearKeyAliases()
            {
                InputToKeyConverter.Clear();
            }

            /// <summary>Determines whether an input value has a registered alias.</summary>
            public bool ContainsInput(object input)
            {
                return InputToKeyConverter.ContainsKey(input);
            }

            /// <summary>Gets the key alias registered for an input value.</summary>
            public object GetKey(object input)
            {
                return InputToKeyConverter[input];
            }
        }
    }
}

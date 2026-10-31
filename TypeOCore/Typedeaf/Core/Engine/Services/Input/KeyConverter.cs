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

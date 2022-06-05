using System.Collections.Generic;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Services.Input
    {
        public class KeyConverter
        {
            public KeyConverter()
            {
                InputToKeyConverter = new Dictionary<object, object>();
            }

            private Dictionary<object, object> InputToKeyConverter { get; set; }

            public KeyConverter SetKeyAlias(object input, object key)
            {
                InputToKeyConverter.Add(input, key);
                return this;
            }

            public bool ContainsInput(object input)
            {
                return InputToKeyConverter.ContainsKey(input);
            }

            public object GetKey(object input)
            {
                return InputToKeyConverter[input];
            }
        }
    }
}

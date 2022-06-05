using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Services
    {
        [AttributeUsage(AttributeTargets.Property, Inherited = false)]
        public class ServiceId : Attribute
        {
            public ServiceId(string id = "")
            {
                Id = id;
            }

            public string Id { get; set; }
        }
    }
}

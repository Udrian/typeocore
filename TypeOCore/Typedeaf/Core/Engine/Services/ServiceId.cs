using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Services
    {
        /// <summary>Associates a service property with a registered service identifier.</summary>
        [AttributeUsage(AttributeTargets.Property, Inherited = false)]
        public class ServiceId : Attribute
        {
            /// <summary>Initializes the attribute with the identifier of the service to inject.</summary>
            public ServiceId(string id = "")
            {
                Id = id;
            }

            /// <summary>Gets or sets the service identifier.</summary>
            public string Id { get; set; }
        }
    }
}

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Interfaces
    {
        /// <summary>Provides a parent object and its hierarchical identifier path.</summary>
        public interface IHasParent
        {
            /// <summary>Gets the parent object, if one exists.</summary>
            public TypeOObject Parent { get; }

            /// <summary>Gets the identifier path of the parent hierarchy.</summary>
            public string FullParentPath
            {
                get
                {
                    if (Parent == null)
                    {
                        return string.Empty;
                    }
                    else if (Parent is IHasParent parentWithParent)
                    {
                        return parentWithParent.FullParentPath + Parent.ID + "/";
                    }
                    else
                    {
                        return Parent.ID + "/";
                    }
                }
            }
        }

        /// <summary>Provides a parent object of a specific TypeO object type.</summary>
        /// <typeparam name="T">The type of the parent object.</typeparam>
        public interface IHasParent<T> : IHasParent where T : TypeOObject
        {
            TypeOObject IHasParent.Parent { get => Parent; }
            /// <summary>Gets the parent object.</summary>
            public new T Parent { get; }
        }
    }
}

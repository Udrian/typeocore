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
    }
}

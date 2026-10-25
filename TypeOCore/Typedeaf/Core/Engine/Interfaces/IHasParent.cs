namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Interfaces
    {
        public interface IHasParent
        {
            public TypeOObject Parent { get; }

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

        public interface IHasParent<T> : IHasParent where T : TypeOObject
        {
            TypeOObject IHasParent.Parent { get => Parent; }
            public new T Parent { get; }
        }
    }
}

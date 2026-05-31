using System;

namespace ProjectBase.UI.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PresenterTypeAttribute : Attribute
    {
        public Type PresenterType { get; }

        public PresenterTypeAttribute(Type presenterType)
        {
            PresenterType = presenterType;
        }
    }
}

using System;

namespace Rem.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public Type Target { get; }
        public Type Generic { get; }

        public ServiceAttribute(Type target, Type generic)
        {
            if (!target.IsInterface)
                throw new ArgumentException($"{nameof(target)} must be an interface");

            Target = target;
            Generic = generic;
        }

        public ServiceAttribute(Type target) : this(target, null)
        {
        }
    }
}

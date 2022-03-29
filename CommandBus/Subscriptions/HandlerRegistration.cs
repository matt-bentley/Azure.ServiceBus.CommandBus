using System.Reflection;

namespace CommandBus.Subscriptions
{
    public class HandlerRegistration
    {
        public HandlerRegistration(Type handlerType)
        {
            Type = handlerType;
            MethodInfo = handlerType.GetMethod("HandleAsync");
        }

        public readonly Type Type;
        public readonly MethodInfo MethodInfo;

        public override bool Equals(object obj)
        {
            return Equals(obj as HandlerRegistration);
        }

        public bool Equals(HandlerRegistration other)
        {
            return other != null && Type.Equals(other.Type);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
    }
}

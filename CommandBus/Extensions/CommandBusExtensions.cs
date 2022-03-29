using System.Reflection;

namespace CommandBus
{
    public static class CommandBusExtensions
    {
        public static void RegisterHandlers(this ICommandBus commandBus, Assembly handlerAssembley)
        {
            var handlerInterface = typeof(ICommandHandler);
            var handlerTypes = handlerAssembley.GetTypes().Where(e => handlerInterface.IsAssignableFrom(e) && !e.IsAbstract && !e.IsInterface);

            foreach (var handlerType in handlerTypes)
            {
                var genericHandlerType = handlerType.GetInterfaces().Where(e => handlerInterface.IsAssignableFrom(e) && e.IsGenericType).Single();
                var commandType = genericHandlerType.GetGenericArguments().Single();

                var method = typeof(CommandBus).GetMethods().Where(m => m.Name == nameof(CommandBus.RegisterHandler)).First();
                var genericMethod = method.MakeGenericMethod(commandType, handlerType);
                genericMethod.Invoke(commandBus, new object[] { });
            }
        }
    }
}

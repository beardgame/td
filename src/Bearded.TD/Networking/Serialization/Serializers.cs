using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Bearded.TD.Utilities;

namespace Bearded.TD.Networking.Serialization
{
    sealed class Serializers<TContext, TSender>
    {
        public static Serializers<TContext, TSender> Instance { get; } = new Serializers<TContext, TSender>();

        private readonly Dictionary<Type, int> requestIds;
        private readonly Dictionary<Type, int> commandIds;
        private readonly Dictionary<int, Func<IRequestSerializer<TContext, TSender>>> requestSerializers;
        private readonly Dictionary<int, Func<ICommandSerializer<TContext>>> commandSerializers;
        private readonly int firstCommandId;
        private readonly int maxId;

        public int RequestId(IRequestSerializer<TContext, TSender> serializer) => requestIds[serializer.GetType()];
        public int CommandId(ICommandSerializer<TContext> serializer) => commandIds[serializer.GetType()];
        public IRequestSerializer<TContext, TSender> RequestSerializer(int id) => requestSerializers[id]();
        public ICommandSerializer<TContext> CommandSerializer(int id) => commandSerializers[id]();
        public bool IsRequestSerializer(int id) => id >= 0 && id < firstCommandId;
        public bool IsCommandSerializer(int id) => id >= firstCommandId && id < maxId;
        public bool IsValidId(int id) => id >= 0 && id < maxId;

        private Serializers()
        {
            var timer = Stopwatch.StartNew();
            init(
                out requestIds,
                out commandIds,
                out requestSerializers,
                out commandSerializers,
                out firstCommandId,
                out maxId
            );
#if DEBUG
            Console.WriteLine($"Initialised {maxId} serializers in {timer.Elapsed.TotalMilliseconds:0.00}ms.");
#endif
        }

        private static void init(
            out Dictionary<Type, int> rIds,
            out Dictionary<Type, int> cIds,
            out Dictionary<int, Func<IRequestSerializer<TContext, TSender>>> rSerializers,
            out Dictionary<int, Func<ICommandSerializer<TContext>>> cSerializers,
            out int firstCId,
            out int maxId)
        {
            var (requests, commands) = getSerializers();

            rIds = getIds(requests);
            firstCId = rIds.Count;
            cIds = getIds(commands, firstCId);
            maxId = firstCId + cIds.Count;

            rSerializers = createConstructors<IRequestSerializer<TContext, TSender>>(requests, rIds);
            cSerializers = createConstructors<ICommandSerializer<TContext>>(commands, cIds);
        }

        private static (List<Type>, List<Type>) getSerializers()
        {
            var requests = new List<Type>();
            var commands = new List<Type>();

            var assembly = typeof(Serializers<TContext, TSender>).Assembly;
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract)
                    continue;

                var isRequestSerializer = type.Implements<IRequestSerializer<TContext, TSender>>();
                var isCommandSerializer = type.Implements<ICommandSerializer<TContext>>();

                if (!(isRequestSerializer || isCommandSerializer))
                    continue;

                if (!type.HasDefaultConstructor())
                {
#if DEBUG
                    writeWarning($"Found serializer without default constructor: {type.FullName}. "
                                 + "Please add one to allow for serialisation.");
#endif
                    continue;
                }

                if (isRequestSerializer)
                    requests.Add(type);

                if (isCommandSerializer)
                    commands.Add(type);
            }

            return (requests, commands);
        }

        private static Dictionary<Type, int> getIds(List<Type> types, int offset = 0)
            => types
                .OrderBy(t => t.FullName)
                .ToDictionary(t => t, t => offset++);

        private static Dictionary<int, Func<T>> createConstructors<T>(List<Type> types, Dictionary<Type, int> ids)
            => types.ToDictionary(t => ids[t], constructor<T>);

        private static Func<T> constructor<T>(Type type)
        {
            var body = Expression.New(type);
            return Expression.Lambda<Func<T>>(body).Compile();
        }

        private static void writeWarning(string text)
        {
            var rgb = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(text);
            Console.ForegroundColor = rgb;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Networking.Serialization
{
    sealed class Serializers
    {
        public static Serializers Instance { get; } = new Serializers();

        private readonly Dictionary<Type, int> requestIds;
        private readonly Dictionary<Type, int> commandIds;
        private readonly Dictionary<int, Func<IRequestSerializer>> requestSerializers;
        private readonly Dictionary<int, Func<ICommandSerializer>> commandSerializers;
        private readonly int firstCommandId;
        private readonly int maxId;

        public int RequestId(IRequestSerializer serializer) => requestIds[serializer.GetType()];
        public int CommandId(ICommandSerializer serializer) => commandIds[serializer.GetType()];
        public IRequestSerializer RequestSerializer(int id) => requestSerializers[id]();
        public ICommandSerializer CommandSerializer(int id) => commandSerializers[id]();
        public bool IsRequestSerializer(int id) => id >= 0 && id < firstCommandId;
        public bool IsCommandSerializer(int id) => id >= firstCommandId && id < maxId;
        public bool IsValidId(int id) => id >= 0 && id < maxId;

        private Serializers()
        {
            init(
                out requestIds,
                out commandIds,
                out requestSerializers,
                out commandSerializers,
                out firstCommandId,
                out maxId
            );
        }

        private static void init(
            out Dictionary<Type, int> rIds,
            out Dictionary<Type, int> cIds,
            out Dictionary<int, Func<IRequestSerializer>> rSerializers,
            out Dictionary<int, Func<ICommandSerializer>> cSerializers,
            out int firstCId,
            out int maxId)
        {
            var (requests, commands) = getSerializers();

            rIds = getIds(requests);
            firstCId = rIds.Count;
            cIds = getIds(commands, firstCId);
            maxId = firstCId + cIds.Count;

            rSerializers = createConstructors<IRequestSerializer>(requests, rIds);
            cSerializers = createConstructors<ICommandSerializer>(commands, cIds);
        }

        private static (List<Type>, List<Type>) getSerializers()
        {
            var requests = new List<Type>();
            var commands = new List<Type>();

            var assembly = typeof(Serializers).Assembly;
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract)
                    continue;

                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    #if DEBUG
                    if (type.Implements<ICommandSerializer>() || type.Implements<IRequestSerializer>())
                    {
                        Console.WriteLine($"Found potential serializer {type.FullName} without default constructor.");
                    }
                    #endif
                    continue;
                }

                if (type.Implements<IRequestSerializer>())
                    requests.Add(type);

                if (type.Implements<ICommandSerializer>())
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
            if (!type.HasDefaultConstructor())
                return () => (T)FormatterServices.GetUninitializedObject(type);

            var body = Expression.New(type);
            return Expression.Lambda<Func<T>>(body).Compile();
        }
    }
}
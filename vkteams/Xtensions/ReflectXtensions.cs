using LiteDB;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Buratino.Xtensions
{
    public static class ReflectXtensions
    {
        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x.Assembly == y.Assembly &&
                    x.Namespace == y.Namespace &&
                    x.Name == y.Name;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod(int index = 1)
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(index);
            return sf.GetMethod().Name;
        }

        /// <summary>
        /// Возвращает свойства, имеющие атрибут
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetPropsByAttribute<T>(this object obj) where T : Attribute
        {
            List<PropertyInfo> properties = new();
            foreach (var item in obj.GetType().GetProperties())
            {
                var valueAttributes = item.GetCustomAttributes<T>(false);
                if (valueAttributes is null)
                    continue;
                if (valueAttributes.Count() == 0)
                    continue;
                properties.Add(item);
            }
            return properties;
        }

        /// <summary>
        /// Возвращает методы, имеющие атрибут
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethodsByAttribute<T>(this object obj) where T : Attribute
        {
            List<MethodInfo> methods = new();
            foreach (var item in obj.GetType().GetMethods())
            {
                var valueAttributes = item.GetCustomAttributes<T>(false);
                if (valueAttributes is null)
                    continue;
                if (valueAttributes.Count() == 0)
                    continue;
                methods.Add(item);
            }
            return methods;
        }

        /// <summary>
        /// Возвращает свойства, имеющие атрибут
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<MethodInfo, T>> GetMethodsWithAttribute<T>(this object obj) where T : Attribute
        {
            List<KeyValuePair<MethodInfo, T>> keyValuePairs = new();
            foreach (var item in obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
            {
                var valueAttributes = item.GetCustomAttributes<T>(false);
                if (valueAttributes is null)
                    continue;
                if (valueAttributes.Count() == 0)
                    continue;
                keyValuePairs.Add(new KeyValuePair<MethodInfo, T>(item, valueAttributes.First()));
            }
            return keyValuePairs;
        }

        /// <summary>
        /// Возвращает атрибут свойства
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this PropertyInfo property) where T : Attribute
        {
            var valueAttributes = property.GetCustomAttributes<T>(false);
            if (valueAttributes is null)
                return null;
            if (valueAttributes.Count() == 0)
                return null;
            T description = valueAttributes.First();
            return description;
        }

        /// <summary>
        /// Возвращает атрибут перечисления
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attribute = memInfo[0].GetCustomAttributes(typeof(T), false).FirstOrDefault();
            return attribute != null ? (T)attribute : null;
        }

        /// <summary>
        /// Вызывает метод у объекта
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <param name="genericTypes"></param>
        /// <returns></returns>
        public static object InvokeMethod(this object obj, string methodName, object[] args = null, Type[] genericTypes = null)
        {
            Type[] methodTypes = args == null ? new Type[0] : args.Select(x => x.GetType()).ToArray();
            int genericCount = genericTypes == null ? 0 : genericTypes.Length;
            MethodInfo method = null;
            if (obj is Type type)
            {
                var allM = type.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x => x.Name == "AsQueryable" && x.IsGenericMethod);
                var param = allM.GetParameters();
                method = obj.GetType().GetMethod(methodName, genericCount, methodTypes);
            }
            else
            {
                type = null;
                if (genericCount > 0)
                {
                    method = obj.GetType().GetMethod(methodName);
                    method = method.MakeGenericMethod(genericTypes);
                }
                else
                {
                    method = obj.GetType().GetMethod(methodName);
                }
            }

            object result = null;
            if (type != null)
            {
                result = method.Invoke(null, args);
            }
            else
            {
                result = method.Invoke(obj, args);
            }
            return result;
        }

        /// <summary>
        /// Возвращает обобщенный метод
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] parameterTypes)
        {
            var methods = type.GetMethods();
            foreach (var method in methods.Where(m => m.Name == name))
            {
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
                {
                    return method;
                }
            }

            return null;
        }

        /// <summary>
        /// Динамичиский каст к нужному типу
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static dynamic CastDynamic(this object source, Type dest)
        {
            return Convert.ChangeType(source, dest);
        }

        /// <summary>
        /// Проверяет, является ли тип имплементацией базового типа
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool IsImplementationOfClass(this Type type, Type baseType)
        {
            if (type == null)
                return false;
            if (baseType.IsInterface)
            {
                return type.GetInterfaces().Any(x => x.FullName == baseType.FullName);
            }
            if (type.Name == baseType.Name)
                return true;
            return type.BaseType.IsImplementationOfClass(baseType);
        }

        /// <summary>
        /// Возвращает имплементации базового типа
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type[] GetImplementations(this Type type)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<Type> types = assemblies.SelectMany(s => s.GetTypes()).OrderBy(x => x.Name).ToArray();

            var entityMappers = types
                .Where(p =>
                p.IsClass
                && !p.IsAbstract
                && p.IsImplementationOfClass(type))
                .OrderBy(x => x.Name)
                .ToArray();

            return entityMappers;
        }
    }
}
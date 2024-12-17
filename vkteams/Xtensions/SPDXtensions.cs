using Buratino.Helpers;

using LiteDB;

using System.Reflection;

using vkteams.Entities;

namespace Buratino.Xtensions
{
    public static class SPDXtensions
    {
        public static object StringValueCast(this string str, Type target)
        {
            if (str is null || str.ToString() == "")
            {
                return null;
            }

            if (target.Name == "Nullable`1")
            {
                target = target.GenericTypeArguments.FirstOrDefault();
                if (str == null)
                    return null;
            }

            if (target == typeof(string))
            {
                return str;
            }
            else if (target.IsAssignableTo(typeof(EntityBase)))
            {
                var entity = Activator.CreateInstance(target) as EntityBase;
                if (Guid.TryParse(str, out Guid entityId))
                {
                    entity.Id = entityId;
                }
                else
                {
                    return null;
                }
                return entity;
            }
            else if (target.IsEnum)
            {
                if (Enum.TryParse(target, str, out object res))
                {
                    return res;
                }
                else
                {
                    return Activator.CreateInstance(target);
                }
            }
            else
            {
                var enumerable = target.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "TryParse").ToArray();
                var method = enumerable.First();

                if (target == typeof(decimal) || target == typeof(float) || target == typeof(double))
                {
                    str = str.Replace(".", ",");
                }

                var data = new object[] { str, null };
                var res = (bool)method.Invoke(null, data);
                if (res)
                {
                    return data[1];
                }
                else
                {
                    throw new Exception($"Не удалось преобразовать {str} в {target}");
                }
            }
        }

        public static object Cast(this object obj, Type target)
        {
            if (obj == null)
            {
                return null;
            }

            Type source = obj.GetType();
            if (source == target)
            {
                return obj;
            }
            else if (target == typeof(object))
            {
                return obj;
            }
            else if (target.IsAssignableFrom(source))
            {
                return obj;
            }
            else if (target == typeof(string))
            {
                return obj.ToString();
            }
            else if (target.IsEnum)
            {
                if (int.TryParse(obj.ToString(), out int valInt))
                {
                    foreach (var item in Enum.GetValues(target))
                    {
                        if ((int)item == valInt)
                        {
                            return item;
                        }
                    }
                    throw new Exception("Не получилось преобразовать числовой ключ в Enum");
                }
                else
                {
                    if (Enum.TryParse(target, obj.ToString(), out object res))
                    {
                        return res;
                    }
                    else
                    {
                        throw new Exception("Не получилось преобразовать текстовый ключ в Enum");
                    }
                }
            }
            else if (source == typeof(string))
            {
                return obj.ToString().StringValueCast(target);
            }
            else
            {
                var converter = Convertation.Convertations.FirstOrDefault(x => x.A == source && x.B == target);
                if (converter == null)
                    throw new Exception($"Нет преобразования для {source} в {target} ({obj?.ToString() ?? "|e|"})");
                else
                {
                    return converter.GetResult(obj);
                }
            }
        }

        public static bool Between(this int val, int min, int max) =>
             val >= min && val <= max;
    }
}
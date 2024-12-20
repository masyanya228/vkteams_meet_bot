using Buratino.Xtensions;

using vkteams.Entities;

namespace vkteams.Helpers
{
    public static class EntityFetchHelper
    {
        public static T Fetch<T>(T entity)
        {
            var type = entity.GetType();
            foreach (var property in type.GetProperties())
            {
                Type propType = property.PropertyType;
                if (propType.IsImplementationOfClass(typeof(EntityBase)))
                {
                    var propVal = property.GetValue(entity);
                    if (propVal is null)
                        continue;

                    var propId = (Guid)propType.GetProperty("Id").GetValue(propVal);

                    //todo - сделать через рефлексию MakeGenericMethod
                    object fetchedItem;
                    if (propType == typeof(Form))
                        fetchedItem = DBContext.DB.GetCollection<Form>().FindById(propId);
                    else if (propType == typeof(Person))
                        fetchedItem = DBContext.DB.GetCollection<Person>().FindById(propId);
                    else
                        throw new NotImplementedException();
                    property.SetValue(entity, fetchedItem);
                }
            }
            return entity;
        }
    }
}

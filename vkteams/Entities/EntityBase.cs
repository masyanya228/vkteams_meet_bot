using LiteDB;

namespace vkteams.Entities
{
    public class EntityBase
    {
        [BsonId]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public static bool operator == (EntityBase f1, EntityBase f2)
        {
            if (f1 is null && f2 is null)
            {
                return true;
            }

            if (f1 is null)
            {
                return false;
            }

            return f1.Equals(f2);
        }

        public static bool operator != (EntityBase f1, EntityBase f2)
        {
            if (f1 is null && f2 is null)
            {
                return false;
            }

            if (f1 is null)
            {
                return true;
            }

            return !f1.Equals(f2);
        }

        public override bool Equals(object obj)
        {
            if (obj is EntityBase entity)
            {
                return Id == entity.Id;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}

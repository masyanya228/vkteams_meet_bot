namespace Buratino.Helpers
{
    public class Convertation
    {
        public static Type[] availableTypes = new Type[] { typeof(int), typeof(double), typeof(decimal), typeof(float), typeof(bool), typeof(string), typeof(DateTime) };
        public Type A { get; set; }
        public Type B { get; set; }
        public Func<object, object> Expression { get; set; }
        public static List<Convertation> Convertations = new List<Convertation>();
        public Convertation(Type a, Type b, Func<object, object> myProperty)
        {
            A = a;
            B = b;
            Expression = myProperty;
        }
        public object GetResult(object source)
        {
            return Expression(source);
        }
        public static bool Init()
        {
            Convertations.Add(new Convertation(typeof(int), typeof(double), x => (double)(int)x));
            Convertations.Add(new Convertation(typeof(decimal), typeof(double), x => (double)(decimal)x));
            Convertations.Add(new Convertation(typeof(float), typeof(double), x => (double)(float)x));
            Convertations.Add(new Convertation(typeof(bool), typeof(double), x => true == (bool)x ? 1.0 : 0.0));
            Convertations.Add(new Convertation(typeof(DateTime), typeof(double), x => ((DateTime)x).ToOADate()));

            Convertations.Add(new Convertation(typeof(double), typeof(int), x => (int)(double)x));
            Convertations.Add(new Convertation(typeof(decimal), typeof(int), x => (int)(decimal)x));
            Convertations.Add(new Convertation(typeof(float), typeof(int), x => (int)(float)x));
            Convertations.Add(new Convertation(typeof(bool), typeof(int), x => true == (bool)x ? 1 : 0));
            Convertations.Add(new Convertation(typeof(DateTime), typeof(int), x => ((DateTime)x).CompareTo(DateTime.Now)));

            Convertations.Add(new Convertation(typeof(double), typeof(decimal), x => (decimal)(double)x));
            Convertations.Add(new Convertation(typeof(int), typeof(decimal), x => (decimal)(int)x));
            Convertations.Add(new Convertation(typeof(float), typeof(decimal), x => (decimal)(float)x));
            Convertations.Add(new Convertation(typeof(bool), typeof(decimal), x => true == (bool)x ? 1m : 0m));
            Convertations.Add(new Convertation(typeof(DateTime), typeof(decimal), x => ((DateTime)x).ToOADate()));

            Convertations.Add(new Convertation(typeof(double), typeof(float), x => (float)(double)x));
            Convertations.Add(new Convertation(typeof(decimal), typeof(float), x => (float)(decimal)x));
            Convertations.Add(new Convertation(typeof(int), typeof(float), x => (float)(int)x));
            Convertations.Add(new Convertation(typeof(bool), typeof(float), x => true == (bool)x ? 1f : 0f));
            Convertations.Add(new Convertation(typeof(DateTime), typeof(float), x => ((DateTime)x).ToOADate()));

            Convertations.Add(new Convertation(typeof(double), typeof(bool), x => (double)x == 1.0 ? 1.0 : 0.0));
            Convertations.Add(new Convertation(typeof(decimal), typeof(bool), x => (decimal)x == 1.0m ? 1.0m : 0.0m));
            Convertations.Add(new Convertation(typeof(int), typeof(bool), x => (int)x == 1 ? 1 : 0));
            Convertations.Add(new Convertation(typeof(float), typeof(bool), x => (float)x == 1f ? 1f : 0f));
            Convertations.Add(new Convertation(typeof(DateTime), typeof(bool), x =>
            {
                var dow = ((DateTime)x).DayOfWeek;
                return dow == DayOfWeek.Sunday || dow == DayOfWeek.Saturday;
            }));

            return true;
        }
    }
}
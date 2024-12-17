namespace Buratino.Attributes
{
    public class DisplayTextAttribute : Attribute
    {
        public string Name { get; set; }
        public string NameGenitive { get; set; }

        public DisplayTextAttribute(string name, string nameGenitive)
        {
            Name = name;
            NameGenitive = nameGenitive;
        }
    }
}
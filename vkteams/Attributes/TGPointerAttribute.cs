namespace Buratino.Attributes
{
    public class TGPointerAttribute : Attribute
    {
        public string[] Pointers { get => _pointers; set => _pointers = value; }
        private string[] _pointers;

        public TGPointerAttribute(params string[] pointers)
        {
            _pointers = pointers;
        }
    }
}
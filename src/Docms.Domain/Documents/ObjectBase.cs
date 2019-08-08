namespace Docms.Domain.Documents
{
    public abstract class ObjectBase
    {
        public ObjectBase(Hash hash)
        {
            Hash = hash;
        }

        public Hash Hash { get; }
    }
}

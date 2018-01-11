namespace Learn.CSharp.Attributes.Return.Entities
{
    public class User
    {
        public long Id { get; set; }

        public override string ToString() => Id.ToString();
    }
}
namespace Learn.CSharp.Attributes.Return.Entities
{
    public class UserProfile : User
    {
        public string Json { get; set; }


        public override string ToString() => $"{base.ToString()}: [{Json}]";
    }
}
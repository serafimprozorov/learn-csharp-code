namespace Learn.CSharp.Attributes.Return.Transformers
{
    using Entities;    

    
    public class IncrementGetUserById
    {
        public User Execute(User target)
        {
            return new User {Id = target.Id + 1};
        }        
    }
}
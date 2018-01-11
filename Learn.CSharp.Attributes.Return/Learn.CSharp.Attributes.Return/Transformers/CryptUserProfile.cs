namespace Learn.CSharp.Attributes.Return.Transformers
{
    using System.Linq;    
    using Entities;    
    
    
    public class CryptUserProfile
    {
        public UserProfile Execute(UserProfile target)
        {
            return new UserProfile
            {
                Id = target.Id,
                Json = new string(target.Json.Select(c => (char) (c + 1)).ToArray())
            };
        }
    }
}
namespace Learn.CSharp.Attributes.Return.Services
{
    using Framework;    
    using Transformers;
    
    
    public class UserProfileService : ServiceBase<long, Entities.UserProfile>
    {
        [return: TransformResult(typeof(CryptUserProfile))]
        public sealed override Entities.UserProfile Get(long id) => new Entities.UserProfile {Id = id, Json = $"'Profile of {id}'"};
    }
}
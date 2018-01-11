namespace Learn.CSharp.Attributes.Return.Services
{
    using Framework;
    using Transformers;
    
    
    public class UserService : ServiceBase<long, Entities.User>
    {
        // Идея в том, что переопределять надо только методы, которые должны быть реализованы.
        // Остальные обработаются инвокером, как нереализованные (в MVC у меня возвращался MethodNotAllowed)
        // Этот атрибут не применишь ни к чему, кроме возвращаемого значения, опять же -- гут.
         [return: TransformResult(typeof(IncrementGetUserById))]
        public override Entities.User Get(long id) => new Entities.User {Id = id};
    }
}
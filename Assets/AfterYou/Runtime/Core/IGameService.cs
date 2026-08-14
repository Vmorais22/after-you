namespace AfterYou.Core
{
    public interface IGameService
    {
        void Initialize(ServiceRegistry services);
        void Shutdown();
    }
}

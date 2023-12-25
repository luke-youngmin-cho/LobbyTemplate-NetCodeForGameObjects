using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T.Singleton;

namespace T.UnityServices
{
    public class ServiceManager : SingletonMonoBase<ServiceManager>
    {
        public ServiceContainer container;

        protected override void Init()
        {
            base.Init();
            container = new ServiceContainer();
            container.Register(new LobbyServiceFacade(new LobbyAPIInterface()));
            DontDestroyOnLoad(gameObject);
        }
    }
}

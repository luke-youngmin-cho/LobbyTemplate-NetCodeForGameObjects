using System;
using System.Collections.Generic;

public class ServiceContainer
{
    private readonly Dictionary<Type, object> s_services = new Dictionary<Type, object>();


    public void Register<TService>(TService service)
    {
        s_services.Add(typeof(TService), service);
    }

    public TService Resolve<TService>()
    {
        return (TService)s_services[typeof(TService)];
    }
}

using System;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Abstractions
{
    public interface IServerListUpdater
    {
        void Start(Func<Task> updateAction);

        void Stop();
    }
}
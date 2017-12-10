using IoT.CM.Models;
using System;

namespace IoT.CM.Clients
{
    public interface IClient
    {
        void Publish(string chanel, byte[] data);
    }
}

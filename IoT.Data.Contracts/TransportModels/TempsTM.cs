using System;
using System.Collections.Generic;
using System.Text;

namespace IoT.Data.Contracts.TransportModels
{
    public class TempsTM : BaseTM
    {
        public IEnumerable<TempTM> Temps { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces
{
    public interface IAmAModule
    {
        void Register(RconClient rconClient);
    }
}

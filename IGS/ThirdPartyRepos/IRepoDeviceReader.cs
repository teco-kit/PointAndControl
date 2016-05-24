using PointAndControl.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointAndControl.ThirdPartyRepos
{
    public interface IRepoDeviceReader
    {
        List<Device> read();
    }
}

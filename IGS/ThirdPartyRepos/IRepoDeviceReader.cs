using PointAndControl.Devices;
using System.Collections.Generic;

namespace PointAndControl.ThirdPartyRepos
{
    public interface IRepoDeviceReader
    {
        List<ExternalDevice> read();
    }
}

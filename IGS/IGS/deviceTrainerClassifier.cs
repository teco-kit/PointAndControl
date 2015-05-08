using IGS.Helperclasses;
using IGS.KNN;
using IGS.Server.Devices;
using IGS.Server.IGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace IGS.IGS
{
    class DeviceTrainerClassifier : DeviceTrainer
    {

        private ClassificationHandler handler { get; set; }

        public DeviceTrainerClassifier(ClassificationHandler classHandler)
        {
            this.handler = classHandler;
        }

        public override void train(List<Vector3D[]> vectorList, Device dev, String value)
        {

            handler.calculateWallProjectionSampleAndLearn(vectorList, dev.Name);

        }

    }
}

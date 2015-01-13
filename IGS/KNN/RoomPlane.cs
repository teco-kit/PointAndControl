using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Drawing.Design;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace IGS.KNN
{
    class RoomPlane
    {
        public String name { get; set; }
        public Plane3D plane { get; set; }

        public float width { get; set; }
        public float heigth { get; set; }
        public float depth { get; set; }

        public Bitmap deviceAreas { get; set; }
        
       
        
        //public struct labelArea 
        //{
        //    public String label;
        //    public List<Point3D> points;
        //    public kNNLabelHitSquare hitsquare;

        //}

        public RoomPlane(String name, Vector3D planeNormal, float width, float heigth, float depth)
        {
            this.name = name;
            this.width = width;
            this.heigth = heigth;
            this.depth = depth;
            
            plane = new Plane3D(new Point3D(width, heigth, depth), planeNormal);
            
        }


    }
}

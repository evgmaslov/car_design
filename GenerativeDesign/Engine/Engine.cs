using Leap71.ShapeKernel;
using PicoGK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PicoGKExamples.Sandbox
{
    public class Engine
    {
        public Engine()
        {

        }

        public Voxels voxConstruct()
        {
            //Set inductor's spline points
            List<Vector3> basePoints = new List<Vector3>()
            {
                new Vector3(-40, 0, 0),
                new Vector3(-37, 5.5f, 0),
                new Vector3(-30, 6f, 0),
                new Vector3(-25, 1.7f, 0),
                new Vector3(-20, 1.5f, 0),
            };
            ControlPointSpline spline = new ControlPointSpline(basePoints);
            List<Vector3> newPoints = spline.aGetPoints(100);

            //Set inductors' count
            int nSym = 1;
            List<Voxels> inductors = new List<Voxels>();
            for (int i = 0; i < nSym; i++)
            {
                //Create inductor and set surface modulation
                //LocalFrame set direction of cylinder's section z-axis
                BaseCylinder inductor = new BaseCylinder(
                    new Frames(newPoints, new LocalFrame(new Vector3(0, 0, 0), new Vector3(1, 0, 0))));
                inductor.SetRadius(new SurfaceModulation(InductorRadiusModulation));

                //Rotate inductor
                //curPhi is a global variable used by InductorTransformation method through voxelizing
                curPhi = (float)(2 * Math.PI / nSym * i);
                inductor.SetTransformation(InductorTransformation);

                //Transform to voxels
                Voxels voxInductor = inductor.voxConstruct();
                inductors.Add(voxInductor);
            }
            //Combine all inductors
            Voxels voxInductors = Sh.voxUnion(inductors);

            Voxels voxEngine = Sh.voxUnion(new List<Voxels>() { voxInductors });
            return voxEngine;
        }

        private float InductorRadiusModulation(float angle, float length)
        {
            //Set radius reduction along length
            float radiusDecrement = (10 - 5 * length);
            //Make cylinder surface more rectangular
            float rectangularizing = (float)Math.Pow(radiusDecrement / 10 * Math.Abs(Math.Sin(angle * 2)), 4);
            //Set surface's waving
            float nets = (float)(1*Math.Pow(Math.Abs(Math.Sin(length*20)), 2));
            return radiusDecrement + rectangularizing + nets;
        }

        private Vector3 InductorTransformation(Vector3 vec)
        {
            Vector3 newVec = VecOperations.vecRotateAroundZ(vec, curPhi);
            return newVec;
        }

        float curPhi = 0;

        private float InnerWallModulation1(float length)
        {
            return 1;
        }

        private float InnerWallModulation2(float length)
        {
            return -1;
        }
    }
}

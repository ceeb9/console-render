using System;
using ConsoleRender;
using System.Numerics;
using System.Drawing;
using System.Collections.Generic;

namespace Boids
{
    class BoidLogic
    {

        public Canvas cv;
        public List<Boid> boids;
        public BoidLogic(int count, Canvas _cv)
        {
            cv = _cv;
            boids = new List<Boid>();
            for (int i = 0; i < count; i++)
            {
                boids.Add(new Boid());
            }
        }
        public void boidTick()
        {
            for (int i = 0; i < boids.Count; i++)
            {
                boids[i] = updateInfo(boids[i]);

                //add the edge avoidance factor
                if(boids[i].pos.X < 20 || boids[i].pos.X > cv.Width-20 || boids[i].pos.Y < 20 || boids[i].pos.Y > cv.Height-20)
                {
                    Vector2 edge = Vector2.Zero;
                    edge.X = (avoidEdgeVect(boids[i]).X - boids[i].vel.X);
                    edge.Y = (avoidEdgeVect(boids[i]).Y - boids[i].vel.Y);
                    boids[i].vel.X += edge.X;
                    boids[i].vel.Y += edge.Y;
                }

                

                //apply boids rules and sum vectors
                if (boids[i].nearbyBoids.Count > 1)
                {
                    
                    boids[i].vel.X = alignVect(boids[i]).X*0.7f + cohereVect(boids[i]).X + separateVect(boids[i]).X;
                    boids[i].vel.Y = alignVect(boids[i]).Y*0.7f + cohereVect(boids[i]).Y + separateVect(boids[i]).Y;
                }
                
                //normalize vector
                boids[i].vel = Vector2.Normalize(boids[i].vel);

                boids[i].pos.X += boids[i].vel.X;
                boids[i].pos.Y += boids[i].vel.Y;
            }
        }

        public Vector2 avoidEdgeVect(Boid boid)
        {
            //float distFromCentre = Math.Abs(Vector2.Distance(new Vector2(cv.Width/2, cv.Height/2), new Vector2(boid.pos.X, boid.pos.Y)));

            //Vector2 avoidEdgeVect = new Vector2();
            //avoidEdgeVect = Vector2.Normalize(new Vector2((cv.Width/2 - boid.pos.X), (cv.Height/2 - boid.pos.Y)));

            Vector2 output = Vector2.Zero;
            output.X = (float)Math.Cos(Math.Atan2(((cv.Height / 2) - boid.pos.Y), ((cv.Width / 2) - boid.pos.X)));
            output.Y = (float)Math.Sin(Math.Atan2(((cv.Height / 2) - boid.pos.Y), ((cv.Width / 2) - boid.pos.X)));
            return output;
        }

        public Vector2 separateVect(Boid boid)
        {
            Vector2 sepDist = new Vector2();
            for (var i = 0; i < boid.nearbyBoids.Count; i++)
            {
                sepDist.X += boid.nearbyBoids[i].pos.X - boid.pos.X;
                sepDist.Y += boid.nearbyBoids[i].pos.Y - boid.pos.Y;
            }
            sepDist.X = sepDist.X / boid.nearbyBoids.Count;
            sepDist.Y = sepDist.Y / boid.nearbyBoids.Count;

            sepDist = new Vector2(sepDist.X * -1, sepDist.Y * -1);

            return Vector2.Normalize(sepDist);
        }

        public Vector2 cohereVect(Boid boid)
        {
            Vector2 avgPos = new Vector2();
            for (var i = 0; i < boid.nearbyBoids.Count; i++)
            {
                avgPos.X += boid.nearbyBoids[i].pos.X;
                avgPos.Y += boid.nearbyBoids[i].pos.Y;
            }

            avgPos.X /= boid.nearbyBoids.Count;
            avgPos.Y /= boid.nearbyBoids.Count;

            avgPos = new Vector2(avgPos.X - boid.pos.X, avgPos.Y - boid.pos.X);

            return Vector2.Normalize(avgPos);
        }

        public Vector2 alignVect(Boid boid)
        {
            Vector2 avgVector = new Vector2();
            for (var i = 0; i < boid.nearbyBoids.Count; i++)
            {
                avgVector.X += boid.nearbyBoids[i].vel.X;
                avgVector.Y += boid.nearbyBoids[i].vel.Y;
            }
            avgVector.X = avgVector.X / boid.nearbyBoids.Count;
            avgVector.Y = avgVector.Y / boid.nearbyBoids.Count;

            avgVector = Vector2.Normalize(avgVector);
            return avgVector;
        }

        public Boid updateInfo(Boid boid)
        {
            //find all boids in range
            boid.nearbyBoids.Clear();
            for (var i = 0; i < boids.Count; i++)
            {
                if (Math.Sqrt(Math.Abs(Math.Pow(boids[i].pos.X - boid.pos.X, 2) + Math.Pow(boids[i].pos.Y - boid.pos.Y, 2))) < boid.detectRange)
                {
                    boid.nearbyBoids.Add(boids[i]);
                }
            }
            return boid;
        }
    }
    class Boid
    {
        public PointF pos;
        public Vector2 vel;
        public int detectRange;
        public bool tooClose;
        public List<Boid> nearbyBoids;
        public Boid()
        {
            tooClose = false;
            pos.X = 32;
            pos.Y = 32;

            vel.X = 1;
            vel.Y = 1;

            detectRange = 8;
            nearbyBoids = new List<Boid>();
        }
    }


}
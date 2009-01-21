using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Emgu;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Emgu.CV
{
   /// <summary>
   /// A collection of points
   /// </summary>
   public static class PointCollection
   {
      /// <summary>
      /// A comparator which compares only the X value of the point
      /// </summary>
      private class XValueOfPointComparator : IComparer<PointF>
      {
         public int Compare(PointF p1, PointF p2)
         {
            return p1.X.CompareTo(p2.X);
         }
      }

      /// <summary>
      /// Perform a first degree interpolation to lookup the y coordinate given the x coordinate
      /// </summary>
      /// <param name="points">The collection of points. Must be sorted by the x value.</param>
      /// <param name="index">the x coordinate</param>
      /// <returns>the y coordinate as the result of the first degree interpolation</returns>
      public static float FirstDegreeInterpolate(PointF[] points, float index)
      {
         XValueOfPointComparator comparator = new XValueOfPointComparator();
         int idx = System.Array.BinarySearch<PointF>(points, new PointF(index, 0.0f), comparator);
         if (idx >= 0)
         {   // an exact index is matched
            return points[idx].Y;
         }
         else
         {   // the index fall into a range, in this case we do interpolation
            idx = -idx;
            if (idx == 1)
            {   // the specific index is smaller than all indexes
               idx = 0;
            }
            else if (idx == points.Length + 1)
            {   // the specific index is larger than all indexes
               idx = points.Length - 2;
            }
            else
            {
               idx -= 2;
            }

            LineSegment2DF line = new LineSegment2DF(points[idx], points[idx + 1]);
            return line.YByX(index);
         }
      }

      /// <summary>
      /// Perform a first degree interpolation to lookup the y coordinates given the x coordinates
      /// </summary>
      /// <param name="points">The collection of points, Must be sorted by x value</param>
      /// <param name="indexes">the x coordinates</param>
      /// <returns>The y coordinates as the result of the first degree interpolation</returns>
      public static float[] FirstDegreeInterpolate(PointF[] points, float[] indexes)
      {
         return System.Array.ConvertAll<float, float>(
             indexes,
             delegate(float d) { return FirstDegreeInterpolate(points, d); });
      }

      /*
      /// <summary>
      /// Convert the structures to a sequence
      /// </summary>
      /// <param name="stor">The sotrage</param>
      /// <param name="points">The structure to be converted to sequence</param>
      /// <returns>A pointer to the sequence</returns>
      public static Seq<PointF> To2D32fSequence(MemStorage stor, PointF[] points)
      {
         Seq<PointF> seq = new Seq<System.Drawing.PointF>(
             CvInvoke.CV_MAKETYPE((int)CvEnum.MAT_DEPTH.CV_32F, 2),
             stor);
         seq.Push(points, CvEnum.BACK_OR_FRONT.FRONT);

         return seq;
      }*/

      /// <summary>
      /// Fit a line to the points collection
      /// </summary>
      /// <param name="points">The points to be fitted</param>
      /// <param name="type">The type of the fitting</param>
      /// <param name="normalizedDirection">The normalized direction of the fitted line</param>
      /// <param name="aPointOnLine">A point on the fitted line</param>
      public static void Line2DFitting(PointF[] points, CvEnum.DIST_TYPE type, out PointF normalizedDirection, out PointF aPointOnLine)
      {
         float[] data = new float[6];
         IntPtr seq = Marshal.AllocHGlobal(StructSize.MCvSeq);
         IntPtr block = Marshal.AllocHGlobal(StructSize.MCvSeqBlock);
         GCHandle handle = GCHandle.Alloc(points, GCHandleType.Pinned);

         CvInvoke.cvMakeSeqHeaderForArray(
            CvInvoke.CV_MAKETYPE((int)CvEnum.MAT_DEPTH.CV_32F, 2),
            StructSize.MCvSeq,
            StructSize.PointF,
            handle.AddrOfPinnedObject(),
            points.Length,
            seq,
            block); 

         CvInvoke.cvFitLine(seq, type, 0.0, 0.01, 0.01, data);

         handle.Free();
         Marshal.FreeHGlobal(seq);
         Marshal.FreeHGlobal(block);
         normalizedDirection = new PointF(data[0], data[1]);
         aPointOnLine = new PointF(data[2], data[3]);
      }

      /// <summary>
      /// Fit an ellipse to the points collection
      /// </summary>
      /// <param name="points">The points to be fitted</param>
      /// <returns>An ellipse</returns>
      public static Ellipse EllipseLeastSquareFitting(PointF[] points)
      {
         IntPtr seq = Marshal.AllocHGlobal(StructSize.MCvSeq);
         IntPtr block = Marshal.AllocHGlobal(StructSize.MCvSeqBlock);
         GCHandle handle = GCHandle.Alloc(points, GCHandleType.Pinned);
         CvInvoke.cvMakeSeqHeaderForArray(
            CvInvoke.CV_MAKETYPE((int)CvEnum.MAT_DEPTH.CV_32F, 2),
            StructSize.MCvSeq,
            StructSize.PointF,
            handle.AddrOfPinnedObject(),
            points.Length,
            seq,
            block);
         Ellipse e = new Ellipse(CvInvoke.cvFitEllipse2(seq));
         handle.Free();
         Marshal.FreeHGlobal(seq);
         Marshal.FreeHGlobal(block);
         return e;
      }

      /// <summary>
      /// convert a series of points to LineSegment2D
      /// </summary>
      /// <param name="points">the array of points</param>
      /// <param name="closed">if true, the last line segment is defined by the last point of the array and the first point of the array</param>
      /// <returns>array of LineSegment2D</returns>
      public static LineSegment2DF[] PolyLine(PointF[] points, bool closed)
      {
         LineSegment2DF[] res;
         int length = points.Length;
         if (closed)
         {
            res = new LineSegment2DF[length];
            PointF lastPoint = points[length - 1];
            for (int i = 0; i < res.Length; i++)
            {
               res[i] = new LineSegment2DF(lastPoint, points[i]);
               lastPoint = points[i];
            }
         }
         else
         {
            res = new LineSegment2DF[length - 1];
            PointF lastPoint = points[0];
            for (int i = 1; i < res.Length; i++)
            {
               res[i] = new LineSegment2DF(lastPoint, points[i]);
               lastPoint = points[i];
            }
         }

         return res;
      }


      /// <summary>
      /// convert a series of System.Drawing.Point to LineSegment2D
      /// </summary>
      /// <param name="points">the array of points</param>
      /// <param name="closed">if true, the last line segment is defined by the last point of the array and the first point of the array</param>
      /// <returns>array of LineSegment2D</returns>
      public static LineSegment2D[] PolyLine(System.Drawing.Point[] points, bool closed)
      {
         LineSegment2D[] res;
         int length = points.Length;
         if (closed)
         {
            res = new LineSegment2D[length];
            for (int i = 0; i < res.Length; i++)
               res[i] = new LineSegment2D(points[i], points[(i + 1) % length]);
         }
         else
         {
            res = new LineSegment2D[length - 1];
            for (int i = 0; i < res.Length; i++)
               res[i] = new LineSegment2D(points[i], points[(i + 1)]);
         }
         return res;
      }

      /// <summary>
      /// Finds convex hull of 2D point set using Sklansky's algorithm
      /// </summary>
      /// <param name="points">The points to find convex hull from</param>
      /// <param name="storage">the storage used by the resulting sequence</param>
      /// <param name="orientation">The orientation of the convex hull</param>
      /// <returns>The convex hull of the points</returns>
      public static Seq<PointF> ConvexHull(PointF[] points, MemStorage storage, CvEnum.ORIENTATION orientation)
      {
         IntPtr seq = Marshal.AllocHGlobal(StructSize.MCvSeq);
         IntPtr block = Marshal.AllocHGlobal(StructSize.MCvSeqBlock);
         GCHandle handle = GCHandle.Alloc(points, GCHandleType.Pinned);
         CvInvoke.cvMakeSeqHeaderForArray(
            CvInvoke.CV_MAKETYPE((int)CvEnum.MAT_DEPTH.CV_32F, 2),
            StructSize.MCvSeq,
            StructSize.PointF,
            handle.AddrOfPinnedObject(),
            points.Length,
            seq,
            block);

         Seq<PointF> convexHull = new Seq<PointF>(CvInvoke.cvConvexHull2(seq, storage.Ptr, orientation, 1), storage);
         handle.Free();
         Marshal.FreeHGlobal(seq);
         Marshal.FreeHGlobal(block);
         return convexHull;
      }

      /// <summary>
      /// Find the bounding rectangle for the specific array of points
      /// </summary>
      /// <param name="points">The collection of points</param>
      /// <returns>The bounding rectangle for the array of points</returns>
      public static System.Drawing.Rectangle BoundingRectangle(PointF[] points)
      {
         IntPtr seq = Marshal.AllocHGlobal(StructSize.MCvContour);
         IntPtr block = Marshal.AllocHGlobal(StructSize.MCvSeqBlock);
         GCHandle handle = GCHandle.Alloc(points, GCHandleType.Pinned);
         CvInvoke.cvMakeSeqHeaderForArray(
            CvInvoke.CV_MAKETYPE((int)CvEnum.MAT_DEPTH.CV_32F, 2),
            StructSize.MCvSeq,
            StructSize.PointF,
            handle.AddrOfPinnedObject(),
            points.Length,
            seq,
            block);
         System.Drawing.Rectangle rect = CvInvoke.cvBoundingRect(seq, true);
         handle.Free();
         Marshal.FreeHGlobal(seq);
         Marshal.FreeHGlobal(block);
         return rect;
          
      }
   }
}

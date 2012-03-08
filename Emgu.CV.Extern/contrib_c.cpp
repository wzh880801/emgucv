//----------------------------------------------------------------------------
//
//  Copyright (C) 2004-2012 by EMGU. All rights reserved.
//
//----------------------------------------------------------------------------

#include "contrib_c.h"

//Octtree
cv::Octree* CvOctreeCreate() { return new cv::Octree(); }
void CvOctreeBuildTree(cv::Octree* tree, cv::Point3f* points, int numberOfPoints, int maxLevels, int minPoints ) 
{ 
   std::vector<cv::Point3f> pts = std::vector<cv::Point3f>(numberOfPoints); 
   memcpy(&pts[0], points, numberOfPoints * sizeof(cv::Point3f));  
   tree->buildTree(pts, maxLevels, minPoints); 
}
void CvOctreeGetPointsWithinSphere(cv::Octree* tree, cv::Point3f* center, float radius, CvSeq* pointSeq )
{
   std::vector<cv::Point3f> points; 
   tree->getPointsWithinSphere(*center, radius, points);
   cvClearSeq(pointSeq);
   if (!points.empty())
      cvSeqPushMulti(pointSeq, &points.front(), (int)points.size());
}
void CvOctreeRelease(cv::Octree* tree) { delete tree; } 

//CvAdaptiveSkinDetector
CvAdaptiveSkinDetector* CvAdaptiveSkinDetectorCreate(int samplingDivider, int morphingMethod) { return new CvAdaptiveSkinDetector(samplingDivider, morphingMethod); }
void CvAdaptiveSkinDetectorRelease(CvAdaptiveSkinDetector* detector) { delete detector; }
void CvAdaptiveSkinDetectorProcess(CvAdaptiveSkinDetector* detector, IplImage *inputBGRImage, IplImage *outputHueMask) { detector->process(inputBGRImage, outputHueMask); }

//Retina
cv::Retina* CvRetinaCreate(CvSize inputSize, const bool colorMode, int colorSamplingMethod, const bool useRetinaLogSampling, const double reductionFactor, const double samplingStrength)
{
   return new cv::Retina(inputSize, colorMode, (cv::RETINA_COLORSAMPLINGMETHOD)colorSamplingMethod, useRetinaLogSampling, reductionFactor, samplingStrength);
}
void CvRetinaRelease(cv::Retina** retina)
{
   delete *retina;
   *retina = 0;
}
void CvRetinaRun(cv::Retina* retina, IplImage* image)
{
   cv::Mat m = cv::cvarrToMat(image);
   retina->run(m);
}
void CvRetinaGetParvo(cv::Retina* retina, IplImage* parvo)
{
   cv::Mat m = cv::cvarrToMat(parvo);
   retina->getParvo(m);
}
void CvRetinaGetMagno(cv::Retina* retina, IplImage* magno)
{
   cv::Mat m = cv::cvarrToMat(magno);
   retina->getMagno(m);
}
void CvRetinaClearBuffers(cv::Retina* retina)
{
   retina->clearBuffers();
}
void CvRetinaGetParameters(cv::Retina* retina, cv::Retina::RetinaParameters* p)
{
   cv::Retina::RetinaParameters result = retina->getParameters();
   memcpy(p, &result, sizeof(cv::Retina::RetinaParameters));
}
void CvRetinaSetParameters(cv::Retina* retina, cv::Retina::RetinaParameters* p)
{
   retina->setup(*p);
}
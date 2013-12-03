#ifndef Parameter_h
#define Parameter_h 1

#include "ipdefine.h"


typedef struct{
  int type;
  double indexUp, indexLo, index;
  Vector2 angleRangeMin, angleRangeMax, angleStep;
  double cutAngle;
  Vector2 ccdReso, ccdResoSq, edge, angleDiff;
  Vector2 center;
  Vector2 pixelStep;
}TrackSearchParameter;

#ifdef __cplusplus
extern "C"{
#endif
extern TrackSearchParameter gTSP;
#ifdef __cplusplus
}
#endif

void SetEmulsionIndex( double indexUp, double indexDown );
void SetEmulsionIndexUp  ( double index );
void SetEmulsionIndexDown( double index );

double GetEmulsionIndexUp  ( void );
double GetEmulsionIndexDown( void );

#endif

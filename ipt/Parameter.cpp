#include "Parameter.h"

TrackSearchParameter gTSP;

void SetEmulsionIndex( double indexUp, double indexDown )
{
  gTSP.indexUp = indexUp;
  gTSP.indexLo = indexDown;
}
void SetEmulsionIndexUp( double index )
{
  gTSP.indexUp = index;
}
void SetEmulsionIndexDown( double index )
{
  gTSP.indexLo = index;
}

double GetEmulsionIndexUp( void )
{
  return gTSP.indexUp;
}
double GetEmulsionIndexDown( void )
{
  return gTSP.indexLo;
}

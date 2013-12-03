#include "ipdefine.h"



#include "tracking.h"
DLLEXPORT void __stdcall IP_CCDreso( double x, double y )
{
  SetCcdResolution( x, y );
}
#if 0
#include "imaging.h"
DLLEXPORT int __stdcall IP_AllocateImageMemory( int id, int nImage )
{
  return AllocateImageMemory( id, nImage );
}
DLLEXPORT void __stdcall IP_ReleaseImageMemory( int ud )
{
  ReleaseImageMemory( ud );
}
DLLEXPORT void __stdcall IP_DumpImageForBT( int ud, int n )
{
  DumpImageDataColl( ud );
}

#include "trackAreaCalculation.h"
DLLEXPORT double __stdcall IP_CalcTrackArea( int ix_ini, int iy_ini, double z_ini,  
					    int ix_fin, int iy_fin, double z_fin,
					    int id, int nImage )
{
  double thr = 80.;
  TrackData trackData = 
    CalcTrackArea( ix_ini, iy_ini, z_ini, ix_fin, iy_fin, z_fin,
		   id, nImage, thr );
  return trackData.area;
}
#endif
#include "Parameter.h"
DLLEXPORT void __stdcall IP_SetEmulsionIndex( double indexUp, double indexDown )
{
  SetEmulsionIndex( indexUp, indexDown );
}
DLLEXPORT void __stdcall IP_SetEmulsionIndexUp( double index )
{
  SetEmulsionIndexUp( index );
}
DLLEXPORT void __stdcall IP_SetEmulsionIndexDown( double index )
{
  SetEmulsionIndexDown( index );
}

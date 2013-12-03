#ifndef tracking_h
#define tracking_h

#include "ipdefine.h"

extern void ReadImage( char *fileName_ );

extern unsigned char* GetImageRaw( int iplane );
extern int* GetImageHalf( int iplane );
extern void SetImageRaw( int iplane, unsigned char *br );

extern void SetCcdResolution( double x, double y );

#endif

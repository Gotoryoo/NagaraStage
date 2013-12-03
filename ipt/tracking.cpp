#ifdef _WIN32
# include <windows.h>
# include <dos.h>
# include <conio.h>
# include <malloc.h>
# include <memory.h>
#endif

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <time.h>

//#include "ipxdef.h"
//#include "ipxsys.h"
//#include "ipxprot.h"

//	VP-910SDK関係の定義ファイル
#include "vpxdef.h"
#include "vpxsys.h"
#include "vpxfnc.h"

#include "clustering.h"
#include "Debug.h"
#include "imaging.h"
#include "ipdefine.h"
#include "drawing.h"
#include "dst4.h"
#include "ipproto.h"
#include "Parameter.h"
#include "tracking.h"
#include "tracklib.h"
#include "Global.h"

typedef struct {
  int    flag;
  Vector2 posStage, angStage;
  Vector2 posGrid;
  double constX, slopeX;
  double constY, slopeY;
  double tantheta, theta;
  double sigma;
  int    numhit, br, iplane;
  double x1, y1;
  double x2, y2;
  int	 viewID;
  double tmpdxdz, tmpdydz;
  PlaneData planeData;
  ONE_CLUSTER *clusterData;
} RawTrack;
typedef struct{
  int n;
  RawTrack* (*data);
} TrackCollection;
// for thin type: 0-upper gel, 1-lower gel
// for thick type: 0:upside in upper gel,  1: lower side in upper gel
//		   2:upside in lower gel,  3: lower side in lower gel
static TrackCollection gTrack[6] =
  { { 0, 0 },
    { 0, 0 },
    { 0, 0 },
    { 0, 0 },
    { 0, 0 },
    { 0, 0 } };    
static TrackSearchParameter gTSP =
  { EMThinType,
    2.0, 2.0, 2.0, // index
    { -0.08, -0.08 }, // angmeRangeMin
    {  0.09,  0.09 }, // angmeRangeMax
    {  0.04,  0.04 }, // angmeStep
    0.04, // cutAngle
    { 0.03, 0.03 }, // ccdReso
    { 0.03*0.03, 0.03*0.03 }, // ccdResoSq
    { 0., 0. }, // edge
    { 0., 0. }, // angleDiff
    { 0., 0. }, // center
    { 0., 0. } // pixelStep
  };

typedef struct{
  Vector2 position, angle;
  TrackCollection trackColl;
} ViewProfile;
typedef struct{
  int n;
  ViewProfile* (*data);
} ViewProfileCollection;
static ViewProfileCollection gViewAtLowerGel = { 0, 0 };

static void ViewCheck( RawTrack *track );

static void TrackSearch( TrackCollection *trackColl, double *z,
			 double dxdz0, double dydz0, int ud );
static void Cleaner( TrackCollection *trkColl );
void PutRawTrack2EMtrack( int plate, RawTrack track );
/*static const int thrGoodTrkBr = 1500;*/
/* static const int thrGoodTrkBr = 3000; */
static const int thrGoodTrkBr = 3500;
/* static const int thrGoodTrkBr = 5000; */
/* static const int thrGoodTrkBr = 10000; */

static int CheckReality( RawTrack *track1, RawTrack *track2,
			 double MSdxdz, double MSdydz );
static void ClearRawTrack( RawTrack *track );
static void ClearTrack( TrackCollection *trackColl );

ImageData *imageData;

#ifdef _WIN32
extern HWND hWnd;
extern HDC  hDC;
extern LONG DeviceID;
#else
HWND hWnd;
HDC  hDC;
LONG DeviceID;
#endif

extern int  PlateNo;
int Hi;
static unsigned char TrackImage[IP_DRAW_HEIGHT][IP_DRAW_WIDTH];

DLLEXPORT int __stdcall
IP_InitIchiLib( int emtype, int numstep0, double emIndexUp, double emIndexLo,
		HWND hWnd0, HDC hDC0, int Hi0 )
{
  int iid;

  hWnd = hWnd0;
  hDC  = hDC0;
  Hi   = Hi0;

  gTSP.indexUp = emIndexUp;
  gTSP.indexLo = emIndexLo;

  imageData = (ImageData*) malloc( sizeof(ImageData) * NumOfStep );
  for( iid=0; iid<NumOfStep; iid++ ){
    ImageData *img = &(imageData[iid]);

    img->raw = (unsigned char*) malloc( sizeof(unsigned char) * IP_DRAW_SIZE );
    if( img->raw == NULL ) return IP_ERROR;

    img->half = (int*) malloc( sizeof(int) * IP_HALF_SIZE );
/*     img->half = (unsigned char*) malloc( sizeof(unsigned char) * IP_HALF_SIZE ); */
    if( img->half == NULL ) return IP_ERROR;

    img->quarter = (int*) malloc( sizeof(int) * IP_QUAT_SIZE );
/*     img->quarter = (unsigned char*) malloc( sizeof(unsigned char) * IP_QUAT_SIZE ); */
    if( img->quarter == NULL ) return IP_ERROR;
  }

  if( AllocateImages4Track(NumOfStep) == IP_ERROR)
    return IP_ERROR;

  memset( TrackImage, 0x00, IP_DRAW_SIZE );

  SetAllWindow( DeviceID, 0, 0, IP_DRAW_WIDTH - 1, IP_DRAW_HEIGHT - 1 );

  return IP_NORMAL;
}
#if 0
DLLEXPORT void __stdcall IP_Last()
{
  extern FILE *emio_log;
  time_t ltime;

  if (emio_log == NULL) return;

  time( &ltime );
  fprintf( emio_log, "\t%s", ctime(&ltime) );
  fprintf( emio_log, "NumOfStep : %d\n", NumOfStep );
  fprintf( emio_log, "emulsion  index : %f %f\n",
	   gTSP.indexUp, gTSP.indexLo );
  fclose( emio_log );
}
#endif
DLLEXPORT void __stdcall IP_EMtype( short emtype, double min, double max,
				    double step )
{
  gTSP.type   = emtype;
  gTSP.angleRangeMin.x  = gTSP.angleRangeMin.y = min;
  gTSP.angleRangeMax.x  = gTSP.angleRangeMax.y = max;
  gTSP.angleStep    .x  = gTSP.angleStep    .y = step;

  switch( gTSP.type ){
  case EMThinType:
    gTSP.cutAngle = 0.04;
    break;
  case EMThickType:
    gTSP.cutAngle = 0.03;
    break;
  }
}

void SetCcdResolution( double x, double y )
{
  const double margin = 0.025; // 25 micro meters

  gTSP.ccdReso.x = x;
  gTSP.ccdReso.y = y;

  gTSP.ccdResoSq.x = gTSP.ccdReso.x * gTSP.ccdReso.x;
  gTSP.ccdResoSq.y = gTSP.ccdReso.y * gTSP.ccdReso.y;
  gTSP.edge.x = (IP_DRAW_WIDTH /2.) * gTSP.ccdReso.x - margin;
  gTSP.edge.y = (IP_DRAW_HEIGHT/2.) * gTSP.ccdReso.y - margin;

  gTSP.angleDiff.x = 0.05;
  gTSP.angleDiff.y = 0.05;
}

emscan EMscanData;
static int EMcand;

extern double GetMeanValue( void );
#if 0
DLLEXPORT void __stdcall IP_DumpImage( char *fileName_ )
{
  FILE *fp;
  int iid;
  const int MAXLEN = 144;
  char* fileName = (char*) malloc( sizeof(char) * MAXLEN );

  if( strcmp(fileName_,"") == 0 )
    { strcpy( fileName, "image.dat" ); }
  else
    { strcpy( fileName, fileName_ ); }

  fp = fopen( fileName, "wb" );
  for( iid=0; iid<NumOfStep; iid++ )
    fwrite( imageData[iid].raw, 1, IP_DRAW_SIZE, fp );

  fclose(fp);
  free( fileName );
}
void ReadImage( char *fileName_ )
{
  FILE *fp = fopen( fileName_, "rb" );
  int iid;
  for( iid=0; iid<NumOfStep; iid++ ){
    fread( imageData[iid].raw, 1, IP_DRAW_SIZE, fp );
  }
  fclose( fp );
}

unsigned char* GetImageRaw( int iplane )
{
  return imageData[iplane].raw;
}
int* GetImageHalf( int iplane )
{
  return imageData[iplane].half;
}

void SetImageRaw( int iplane, unsigned char* br )
{
  memcpy( imageData[iplane].raw, br, IP_DRAW_SIZE );
}

static void PutInTrackImage( int color, int times, int mbr, RawTrack *track )
{
  int ihit, ix, iy, ipl, br;
  int maximum, times2, offset;
  unsigned char *p;

  switch (color) {
  case 1:
    maximum = 255;
    offset = 128;
    times2 = times;
    break;
  case 2:
    maximum = 127;
    offset = 0;
    times2 = times;
    break;
  default:
    maximum = 255;
    offset = 0;
    times2 = times * 2;
    break;
  }

  for( ipl=0; ipl<NumOfStep; ipl++ ){
    for( ihit=0; ihit<track->planeData.nPixel[ipl]; ihit++ ){
      ix = track->planeData.data[ipl].hit[ihit].x;
      iy = track->planeData.data[ipl].hit[ihit].y;
      br = track->planeData.data[ipl].hit[ihit].br;
      br = br > mbr ? maximum : (br * times2) / 2 + offset;
/*       p = &TrackImage[iy][ix]; */
/*       if( br > *p ) */
/* 	*p = br; */
      if( br > TrackImage[2*iy][2*ix] ){
	TrackImage[2*iy  ][2*ix  ] = br;
	TrackImage[2*iy  ][2*ix+1] = br;
	TrackImage[2*iy+1][2*ix  ] = br;
	TrackImage[2*iy+1][2*ix+1] = br;
      }
    }
  }
}

DLLEXPORT void __stdcall
IP_ExtractTrack( int ud, int itrack, double *x, double *y,
		 double *dxdz, double *dydz, int *br )
{
  extern LPBITMAPINFO bmgRB;
  int i, j, times, mbr;
  RawTrack *track;

  switch (ud) {
  case 1:
    times = 4;
    mbr = 63;
    break;
  case 2:
  case 3:
    times = 5;
    mbr = 51;
    break;
  case 4:
    times = 6;
    mbr = 42;
    break;
  case 5:
    times = 7;
    mbr = 36;
    break;
  default:
    times = 3;
    mbr = 85;
  }

  //if (ud == 0) 
  memset(TrackImage, 0x00, IP_DRAW_SIZE);
  if( itrack == -1 ){
    for( i = 0; i < gTrack[ud].n; i++ )
      PutInTrackImage( 0, 3, 85, gTrack[ud].data[i] );
    return;
  }

  NewImage( hWnd, hDC, Hi, NULL, (unsigned char *) TrackImage );
  if( itrack >= gTrack[ud].n ) return;

  j = gTrack[ud].n - itrack - 1;
  if( j > 0 ){
    for( i = itrack; i < gTrack[ud].n; i++ ){
      if( i == itrack )	PutInTrackImage( 1, times, mbr, gTrack[ud].data[i] );
      else              PutInTrackImage( 2, times, mbr, gTrack[ud].data[i] );
    }
  }
  else{
    PutInTrackImage( 0, times, mbr, gTrack[ud].data[itrack] );
  }

  track = gTrack[ud].data[itrack];
  *x    = track->posStage.x;
  *y    = track->posStage.y;
  *dxdz = track->angStage.x;
  *dydz = track->angStage.y;
  *br   = track->br;

  for( i = ud + 1; i < 4; i++ ){
    EMscanData.EMtrackInfo[PlateNo][i].cand = 0;
    EMscanData.EMtrackInfo[PlateNo][i].comment[0] = '\0';
  }
  EMscanData.categ = EMscanData.TrackType = EMscanData.Nf = EMscanData.Nb = 0;

  if( j > 0 ) NewImage( hWnd, hDC, Hi, bmgRB, (unsigned char *) TrackImage );
  else        NewImage( hWnd, hDC, Hi, NULL,  (unsigned char *) TrackImage );
}


DLLEXPORT void __stdcall
IP_PutTrackInfo( int ud, double MSxin, double MSyin,
		 double MSxout, double MSyout, double MSdxdz, double MSdydz )
{
  /* This function should not be called for plate#1 */
  emtrackinfo *Info;
  double gx, gy, g2x, g2y, emdxdz, emdydz, l;

  EMscanData.LastPlate = PlateNo;
  if( ud >= 0 ){
    switch( ud % 2 ){
    case 0:
      Info = &(EMscanData.EMtrackInfo[PlateNo][ud]);
      Info->cand = 1;
      Info->comment[0] = '\0';
      IP_MtoG(1, MSxin, MSyin, &(Info->gx), &(Info->gy));
      IP_GtoG2(1, PlateNo, Info->gx, Info->gy, &g2x, &g2y);
      IP_G2toEM(1, g2x, g2y, EMscanData.LR, &(Info->emx), &(Info->emy));
      IP_MtoG(0, MSdxdz, MSdydz, &(Info->gdxdz), &(Info->gdydz));
      IP_GtoG2(0, PlateNo, Info->gdxdz, Info->gdydz, &g2x, &g2y);
      IP_G2toEMangle(g2x, g2y, &(Info->emdxdz), &(Info->emdydz));
      Info->br = 0;
      ud ++;	// breakしない
    case 1:
      EMscanData.LastUD = ud;
      Info = &(EMscanData.EMtrackInfo[PlateNo][ud]);
      Info->cand = 1;
      Info->comment[0] = '\0';
      IP_MtoG(1, MSxout, MSyout, &(Info->gx), &(Info->gy));
      IP_GtoG2(1, PlateNo, Info->gx, Info->gy, &g2x, &g2y);
      IP_G2toEM(1, g2x, g2y, EMscanData.LR, &(Info->emx), &(Info->emy));
      IP_MtoG(0, MSdxdz, MSdydz, &(Info->gdxdz), &(Info->gdydz));
      IP_GtoG2(0, PlateNo, Info->gdxdz, Info->gdydz, &g2x, &g2y);
      IP_G2toEMangle(g2x, g2y, &(Info->emdxdz), &(Info->emdydz));
      Info->br = 0;
      break;
    }
  }
  else{
    IP_MtoG(0, MSdxdz, MSdydz, &gx, &gy);
    IP_GtoG2(0, PlateNo, gx, gy, &g2x, &g2y);
    IP_G2toEMangle(g2x, g2y, &emdxdz, &emdydz);
    l = sqrt(emdxdz * emdxdz + emdydz * emdydz + 1);
    EMscanData.Dcay_vx = emdxdz / l;
    EMscanData.Dcay_vy = emdydz / l;
    EMscanData.Dcay_vz = 1 / l;
    if( MSxin < 0 ){
      EMscanData.Dcay_vx *= -1;
      EMscanData.Dcay_vy *= -1;
      EMscanData.Dcay_vz *= -1;
    }
  }
}

DLLEXPORT void __stdcall
IP_PutTrackInfo0( int ud, double MSx, double MSy,
		  double MSdxdz, double MSdydz, int br )
{
  emtrackinfo *Info;
  double g2x, g2y;

  EMscanData.LastPlate = PlateNo;
  EMscanData.LastUD = ud;
  Info = &(EMscanData.EMtrackInfo[PlateNo][ud]);
  Info->cand++;
  Info->comment[0] = '\0';
  IP_MtoG(1, MSx, MSy, &(Info->gx), &(Info->gy));
  if( PlateNo == 1 ){
    /* for plate#1, IP_G2toEM() includes the transfer from G to G2 */
    g2x = Info->gx;
    g2y = Info->gy;
  }
  else{
    IP_GtoG2( 1, PlateNo, Info->gx, Info->gy, &g2x, &g2y );
  }

  IP_G2toEM( 1, g2x, g2y, EMscanData.LR, &(Info->emx), &(Info->emy) );

  IP_MtoG( 0, MSdxdz, MSdydz, &(Info->gdxdz), &(Info->gdydz) );
  if( PlateNo == 1 ){
    /* for plate#1, IP_G2toEMangle() includes the transfer from G to G2 */
    g2x = Info->gdxdz;
    g2y = Info->gdydz;
  }
  else{
    IP_GtoG2( 0, PlateNo, Info->gdxdz, Info->gdydz, &g2x, &g2y );
  }
  IP_G2toEMangle(g2x, g2y, &(Info->emdxdz), &(Info->emdydz));
  Info->br = br;
}

DLLEXPORT void __stdcall IP_PutTrackInfoC( int ud, char *text )
{
  emtrackinfo *Info;

  Info = &(EMscanData.EMtrackInfo[PlateNo][ud]);
  strcpy(Info->comment, text);
}

DLLEXPORT void __stdcall
IP_PutTrackCateg( int categ, short TrackType, short Nf, short Nb )
{
  EMscanData.categ     = categ;
  EMscanData.TrackType = TrackType;
  EMscanData.Nf        = Nf;
  EMscanData.Nb        = Nb;
}

DLLEXPORT int __stdcall IP_TrackSearchUpperGel
( int ud, double *z,
  double limitPara, double limitPerp,
  double dxdz0, double dydz0,
  double X00, double Y00,	// center position of images
  double x0, double y0,
  double index )
{
  int nTrack0, iTrack;
  double dx, dy, para, perp;
  RawTrack *trk;

  if( !fpDebug ){
    fpDebug = fopen( "debug.log", "a" );
  }

  gTSP.index = index;
  gTSP.center.x = X00;
  gTSP.center.y = Y00;

  if( (limitPara == 0.0) && (limitPerp == 0.0) )
    limitPara = limitPerp = 1000000.0;

  nTrack0 = gTrack[0].n;
  TrackSearch( &(gTrack[0]), z, dxdz0, dydz0, 0 );

  for( iTrack = nTrack0; iTrack < gTrack[0].n; iTrack++ ){
    trk = gTrack[0].data[iTrack];
    dx = trk->posStage.x - x0;
    dy = trk->posStage.y - y0;
    para = fabs( (dx * dxdz0 + dy * dydz0)/sqrt( dxdz0*dxdz0 + dydz0*dydz0 ));
    perp = sqrt( dx*dx + dy*dy - para*para );

    if( !( (para < limitPara) && (perp < limitPerp) ) ){
      trk->flag = 0;
    }
  }

  return gTrack[0].n - nTrack0;
}

DLLEXPORT int __stdcall IP_CleanupTrackUpperGel()
{
  RawTrack *trk1, *trk2;
  int iTrack1, iTrack2;
  double dx, dy;
  double limitxy2;
  const double deltaXY  = 0.003;
  const double deltaXYa = 0.015;

  for( iTrack1 = 0; iTrack1 < gTrack[0].n; iTrack1++ ){
    trk1 = gTrack[0].data[iTrack1];
    if( trk1->flag == 0 ) continue;
    if( trk1->br < thrGoodTrkBr ){
      trk1->flag = 0;
      continue;
    }

    for( iTrack2 = iTrack1 + 1; iTrack2 < gTrack[0].n; iTrack2++ ){
      trk2 = gTrack[0].data[iTrack2];
      if( trk2->flag == 0 ) continue;
      if( trk2->br < thrGoodTrkBr ){
	trk2->flag = 0;
	continue;
      }

      dx = trk1->posStage.x - trk2->posStage.x;
      dy = trk1->posStage.y - trk2->posStage.y;
      limitxy2 = deltaXY + (deltaXYa * trk2->tantheta);
      limitxy2 = limitxy2 * limitxy2;
      if( (dx * dx + dy * dy) < limitxy2 ){
	if( trk1->br < trk2->br ){
	  trk1->flag = 0;
	}
	else{
	  trk2->flag = 0;
	}
      }
    }
  }

  Cleaner( &(gTrack[0]) );

  return gTrack[0].n;
}

static void Cleaner( TrackCollection *trkColl )
{
  int iTrack, nTrack=0;
  for( iTrack = 0; iTrack < trkColl->n; iTrack++ ){
    if( trkColl->data[iTrack]->flag ){
      trkColl->data[nTrack] = trkColl->data[iTrack];
      nTrack++;
    }
    else{
      ClearRawTrack( trkColl->data[iTrack] );
    }
  }
  trkColl->data = realloc( trkColl->data, sizeof( RawTrack* ) * nTrack );
  trkColl->n = nTrack;
}

DLLEXPORT void __stdcall IP_TrackEnd()
{
  IP_ClearData();

  memset( TrackImage, 0x00, IP_DRAW_SIZE );
}

DLLEXPORT int __stdcall
IP_MakeViewOfLowerGel( double deltaZ1, double deltaZ2 )
// deltaZ1: length from upper side of base (track position of upper track) to first image
// deltaZ2: length between first and last image (calculated from parameter, not real length)
{
  int itrack;
  RawTrack *track;

  for( itrack = 0; itrack < gTrack[0].n; itrack++ ){
    track = gTrack[0].data[itrack];
    track->x1 = track->posStage.x - track->angStage.x * deltaZ1;
    track->y1 = track->posStage.y - track->angStage.y * deltaZ1;
    track->x2 = track->x1  - track->angStage.x * deltaZ2;
    track->y2 = track->y1  - track->angStage.y * deltaZ2;

    ViewCheck( track );
  }

  return gViewAtLowerGel.n;
}

DLLEXPORT int __stdcall
IP_GetLowerViewProfile( int i, double *x, double *y, double *dxdz, double *dydz )
{
  static ViewProfile *view;
  if( i<0 || i>=gViewAtLowerGel.n ) return IP_ERROR;
  view = gViewAtLowerGel.data[i];;

  *x    = view->position.x;
  *y    = view->position.y;
  *dxdz = view->angle.x;
  *dydz = view->angle.y;

  return IP_NORMAL;
}

static void ViewCheck( RawTrack *track )
{
  int i;
  ViewProfileCollection *viewColl = &(gViewAtLowerGel);
  TrackCollection *trkColl;
  ViewProfile *view, *viewNew=0;

  for( i=0; i<viewColl->n; i++ ){
    view = viewColl->data[i];

    if( fabs( view->position.x - track->x1 ) < gTSP.edge.x &&
	fabs( view->position.x - track->x2 ) < gTSP.edge.x &&
	fabs( view->position.y - track->y1 ) < gTSP.edge.y &&
	fabs( view->position.y - track->y2 ) < gTSP.edge.y &&
	fabs( view->angle.x - track->angStage.x ) < gTSP.angleDiff.x &&
	fabs( view->angle.y - track->angStage.y ) < gTSP.angleDiff.y ){

      track->viewID = i;

      trkColl = &(view->trackColl);
      trkColl->data = (RawTrack**)realloc( trkColl->data, sizeof(RawTrack*) * ( trkColl->n + 1 ) );
      trkColl->data[ trkColl->n ] = track;
      (trkColl->n)++;
      return;
    }
  }

  track->viewID = viewColl->n;

  viewNew = (ViewProfile*) malloc( sizeof( ViewProfile) );
  viewNew->position.x = track->x1;
  viewNew->position.y = track->y1;
  viewNew->angle.x = track->angStage.x;
  viewNew->angle.y = track->angStage.y;
  viewNew->trackColl.data = (RawTrack**) malloc( sizeof(RawTrack*) );
  viewNew->trackColl.data[0] = track;
  viewNew->trackColl.n = 1;

  viewColl->data = (ViewProfile**) realloc( viewColl->data, sizeof(ViewProfile*) * (viewColl->n+1) );
  viewColl->data[viewColl->n] = viewNew;
  viewColl->n++;
}

double CalcPhi( double dxdz, double dydz )
{
  double phi;

  if( dxdz >= dydz )
    phi = atan(dydz / dxdz);
  else
    phi = 1.570796327 - atan(dxdz / dydz); // pi/2

  return phi;
}

static int CheckReality( RawTrack *track1, RawTrack *track2,
			 double MSdxdz, double MSdydz )
{
  double para01, para02, perp01, perp02, perp12;
  double normXY0, normXY0_2, normXYZ1_2, normXYZ2_2; // a_2 denote a*a

  normXY0_2  = MSdxdz * MSdxdz + MSdydz * MSdydz;
  normXY0    = sqrt(normXY0_2);
  normXYZ1_2 = track1->angStage.x * track1->angStage.x + track1->angStage.y * track1->angStage.y + 1;
  normXYZ2_2 = track2->angStage.x * track2->angStage.x + track2->angStage.y * track2->angStage.y + 1;

  para01 = (MSdxdz * track1->angStage.x + MSdydz * track1->angStage.y) / normXY0_2;
  para02 = (MSdxdz * track2->angStage.x + MSdydz * track2->angStage.y) / normXY0_2;

  perp12 = (track1->angStage.x * track2->angStage.x + track1->angStage.y * track2->angStage.y + 1);
  perp12 = perp12 * perp12 / ( normXYZ1_2 * normXYZ2_2 );
  perp12 = sqrt(1 - perp12);

  perp01 = (MSdxdz * track1->angStage.y - MSdydz * track1->angStage.x) / normXY0;
  perp02 = (MSdxdz * track2->angStage.y - MSdydz * track2->angStage.x) / normXY0;

  if( fabs(para01 - 1.0) < 0.42 &&
      fabs(para02 - 1.0) < 0.42 &&
      fabs(perp01) < 0.08 &&
      fabs(perp02) < 0.08 &&
      fabs(perp12) < 0.2 ){
    return IP_NORMAL;
  }
  else{
    return IP_ERROR;
  }
}

DLLEXPORT int __stdcall IP_TrackSearchLowerGel
( double *z, double centerX, double centerY, double index, double baseThickness, int viewID )
{
  emtrackinfo *Info;
  int iTrackU, iTrackL;
  ViewProfile *view;
  TrackCollection *trkColl;
  RawTrack *trkU, *trkL;
  Vector2 angleAtBase;
  const Vector2 angleRangeLowerGelMin = { -0.05,      -0.05 };
  const Vector2 angleRangeLowerGelMax = {  0.05+0.01,  0.05+0.01 };
  Vector2 angleMinTmp, angleMaxTmp;

  gTSP.index = index;
  gTSP.center.x = centerX;
  gTSP.center.y = centerY;
  angleMinTmp = gTSP.angleRangeMin;
  angleMaxTmp = gTSP.angleRangeMax;
  gTSP.angleRangeMin = angleRangeLowerGelMin;
  gTSP.angleRangeMax = angleRangeLowerGelMax;

  view = gViewAtLowerGel.data[viewID];
  trkColl = &(view->trackColl);
  for( iTrackU=0; iTrackU<trkColl->n; iTrackU++ ){
    trkU = trkColl->data[iTrackU];

    ClearTrack( &(gTrack[1]) );
    TrackSearch( &(gTrack[1]), z, trkU->angStage.x, trkU->angStage.y, 1 );

    for( iTrackL=0; iTrackL < gTrack[1].n; iTrackL++ ){
      trkL = gTrack[1].data[iTrackL];
      if( trkL->br < thrGoodTrkBr ) continue;

      angleAtBase.x = (trkU->posStage.x - trkL->posStage.x) / baseThickness;
      angleAtBase.y = (trkU->posStage.y - trkL->posStage.y) / baseThickness;

      if( CheckReality( trkU, trkL, angleAtBase.x, angleAtBase.y ) == IP_ERROR ){
	continue;
      }

      EMscanData.LastPlate = PlateNo;
      EMcand++;

      Info = &(EMscanData.EMtrackInfo[PlateNo][0]); // down side of upper gel
      Info->cand = EMcand;
      IP_MtoG( 1, trkU->posStage.x, trkU->posStage.y, &Info->gx, &Info->gy );
      IP_G2toEM( 1, Info->gx, Info->gy,EMscanData.LR, &Info->emx, &Info->emy );
      IP_MtoG( 0, trkU->angStage.x, trkU->angStage.y, &Info->gdxdz, &Info->gdydz );
      IP_G2toEMangle( Info->gdxdz, Info->gdydz, &Info->emdxdz, &Info->emdydz );
      Info->br = trkU->br;

      Info = &(EMscanData.EMtrackInfo[PlateNo][1]); // base
      Info->cand = EMcand;
      IP_MtoG( 0, angleAtBase.x, angleAtBase.y, &Info->gdxdz, &Info->gdydz );
      IP_G2toEMangle( Info->gdxdz, Info->gdydz, &Info->emdxdz, &Info->emdydz );
      Info->br = 0;

      Info = &(EMscanData.EMtrackInfo[PlateNo][2]); // up side of lower gel
      Info->cand = EMcand;
      IP_MtoG( 1, trkL->posStage.x, trkL->posStage.y, &Info->gx, &Info->gy );
      IP_G2toEM( 1, Info->gx, Info->gy,EMscanData.LR, &Info->emx, &Info->emy );
      IP_MtoG( 0, trkL->angStage.x, trkL->angStage.y, &Info->gdxdz, &Info->gdydz );
      IP_G2toEMangle( Info->gdxdz, Info->gdydz, &Info->emdxdz, &Info->emdydz );
      Info->br = trkL->br;

      emio_append_scan_data( &EMscanData );
      PutInTrackImage( 0, 3, 85, trkU );
      PutInTrackImage( 0, 3, 85, trkL );
    }
  }

  gTSP.angleRangeMin = angleMinTmp;
  gTSP.angleRangeMax = angleMaxTmp;
  return 0;
}

DLLEXPORT int __stdcall IP_AfterTrack( int newflag, double *Xout, double *Yout)
{
  int itrack;
  RawTrack *track;

  if( newflag ) memset( TrackImage, 0x00, IP_DRAW_SIZE );
  for( itrack = 0; itrack < gTrack[0].n; itrack++ ){
    track = gTrack[0].data[itrack];
    Xout[itrack] = track->posStage.x;
    Yout[itrack] = track->posStage.y;
    PutInTrackImage( 0, 3, 85, gTrack[0].data[itrack] );
  }

  free( gTrack[0].data );
  gTrack[0].data = NULL;
  gTrack[0].n = 0;
  NewImage( hWnd, hDC, Hi, NULL, (unsigned char *) TrackImage );

  return IP_NORMAL;
}

#include "tracklib.h"
extern int MakeShrinkData();

static int PreMoveAndOver( int thr, double dxPix, double dyPix, double *z, int* sumBr )
{
  double dxSegment, dySegment; // differnce of segment in quarter image / mm
  int *deltaX, *deltaY;
  double *deltaZ; 
  int ip, ix, iy;
  int ixp, iyp;
  int br, nhit;
  int nSegment;

  dxSegment = dxPix / 4;
  dySegment = dyPix / 4;

  deltaX = (int*) malloc( sizeof(int) * NumOfStep );
  deltaY = (int*) malloc( sizeof(int) * NumOfStep );
  deltaZ = (double*) malloc( sizeof(double) * NumOfStep );
  for( ip=0; ip<NumOfStep; ip++ ){
    deltaZ[ip] = z[ip] - z[0];

    deltaX[ip] = (int)(dxSegment * deltaZ[ip]);
    deltaY[ip] = (int)(dySegment * deltaZ[ip]);
  }

  memset( sumBr, 0x00, sizeof(int) * IP_QUAT_SIZE );
  nSegment = 0;
  for( ix=0; ix<IP_QUAT_WIDTH; ix++ ){
    for( iy=0; iy<IP_QUAT_HEIGHT; iy++ ){
      br = nhit = 0;

      for( ip=0; ip<NumOfStep; ip++ ){
	ixp = ix + deltaX[ip];
	if( ixp<0 || IP_QUAT_WIDTH<=ixp ) break;
	iyp = iy + deltaY[ip];
	if( iyp<0 || IP_QUAT_HEIGHT<=iyp ) break;

	if( imageData[ip].quarter[ ixp + iyp*IP_QUAT_WIDTH ] ){
	  br += imageData[ip].quarter[ ixp + iyp*IP_QUAT_WIDTH ];
	  nhit++;
	}
      }

      if( nhit>=thr ){
	sumBr[ ix + iy*IP_QUAT_WIDTH ] = br;
	nSegment++;
      }
    }
  }

  free( deltaX );
  free( deltaY );
  free( deltaZ );
  return nSegment;
}

void MakePlaneData( int thr, ONE_CLUSTER *cluster, PlaneData *planeData, int *deltaX, int *deltaY, double *z )
{
  int nhit;
  int ipix, ipl, ixy;
  int ix, ix0, x;
  int iy, iy0, y;
  HIT *hit;

  if( planeData->flag == 0 ){
    for( ipl=0; ipl<NumOfStep; ipl++ ){
      free( planeData->data[ipl].hit );
   }
  }
  memset( &(planeData->x), 0x00, sizeof( PlaneData ) );
  memcpy( &(planeData->z), z, sizeof(double)*NumOfStep );

  hit = cluster->hits;
  for( ipix=0; ipix<cluster->npix; ipix++ ){
    ix0 = hit[ipix].x * 2;
    iy0 = hit[ipix].y * 2;

    for( ix=ix0; ix<ix0+2; ix++ ){
      for( iy=iy0; iy<iy0+2; iy++ ){
	nhit = 0;

	for( ipl=0; ipl<NumOfStep; ipl++ ){
	  x = ix + deltaX[ipl];
	  if( x<0 || IP_HALF_WIDTH<=x )
	    break;
	  y = iy + deltaY[ipl];
	  if( y<0 || IP_HALF_HEIGHT<=y )
	    break;

	  ixy = x + IP_HALF_WIDTH*y;
	  if( imageData[ipl].half[ ixy ] > 0 ){
	    nhit++;
	  }
	}

	if( nhit >= thr ){
	  for( ipl=0; ipl<NumOfStep; ipl++ ){
	    x = ix + deltaX[ipl];
	    if( x<0 || IP_HALF_WIDTH<=x )
	      break;
	    y = iy + deltaY[ipl];
	    if( y<0 || IP_HALF_HEIGHT<=y )
	      break;

	    ixy = x + IP_HALF_WIDTH*y;
	    planeData->x [ipl] += x * imageData[ipl].half[ ixy ];
	    planeData->y [ipl] += y * imageData[ipl].half[ ixy ];
	    planeData->br[ipl] +=     imageData[ipl].half[ ixy ];
	    planeData->data[ipl].hit =
	      (HIT*)realloc( planeData->data[ipl].hit, sizeof( HIT ) * ( planeData->nPixel[ipl] + 1 ) );
	    planeData->data[ipl].hit[planeData->nPixel[ipl]].x  = x;
	    planeData->data[ipl].hit[planeData->nPixel[ipl]].y  = y;
	    planeData->data[ipl].hit[planeData->nPixel[ipl]].br = imageData[ipl].half[ixy];
	    planeData->nPixel[ipl]++;
	  }
	}
      } // iy
    } // ix
  } // ipix

  for( ipl=0; ipl<NumOfStep; ipl++ ){
    if( planeData->br[ipl] > 0 ){
      planeData->x[ipl] /= planeData->br[ipl];
      planeData->y[ipl] /= planeData->br[ipl];
      planeData->brSum  += planeData->br[ipl];

      planeData->x[ipl] *= 2.;
      planeData->y[ipl] *= 2.;

      planeData->nPlane ++;
    }
  }
}

TrackCollection MoveAndOver( int thr, double dxPixel0, double dyPixel0, double *z,
			     ClusterCollection *cluster )
{
  static int deltaX[NumOfStep], deltaY[NumOfStep];
  static double deltaZ[NumOfStep];
  double dxPixel, dxPixelStep;
  double dyPixel, dyPixelStep;
  int i, ipl;
  int iCluster, nSearch;

  ONE_CLUSTER *aCluster;
  HIT *hit;
  double constX, slopeX;
  double constY, slopeY;
  static PlaneData planeData[16]; // track search is carried out for nxbin=4, nybin=4
  static PlaneData *goodPlaneData;
  RawTrack *track=0;
  TrackCollection trkColl = { 0, 0 };

  for( ipl=0; ipl<NumOfStep; ipl++ ){
    deltaZ[ipl] = z[ipl] - z[0];
  }

  dxPixelStep = ( gTSP.pixelStep.x ) / 4.;
  dyPixelStep = ( gTSP.pixelStep.y ) / 4.;

  for( iCluster=0; iCluster<cluster->n; iCluster++ ){
    aCluster = cluster->data[iCluster];
    hit  = aCluster->hits;
    nSearch = 0;

    if( aCluster->npix > 100 ) continue;

    for( dxPixel  = dxPixel0 - 1.5*dxPixelStep;
	 dxPixel  < dxPixel0 + 2.0*dxPixelStep;
	 dxPixel += dxPixelStep ){
      for( ipl=0; ipl<NumOfStep; ipl++ ){
	deltaX[ipl] = (int)((dxPixel * deltaZ[ipl])/2.);
      }

      for( dyPixel  = dyPixel0 - 1.5*dyPixelStep;
	   dyPixel  < dyPixel0 + 2.0*dyPixelStep;
	   dyPixel += dyPixelStep ){
	for( ipl=0; ipl<NumOfStep; ipl++ ){
	  deltaY[ipl] = (int)((dyPixel * deltaZ[ipl])/2.);
	}

	MakePlaneData( thr, aCluster, &(planeData[nSearch]), deltaX, deltaY, z );
	planeData[nSearch].dxPixelSearch = dxPixel;
	planeData[nSearch].dyPixelSearch = dyPixel;
	nSearch++;
      } // dyPixel
    } // dxPixel

    goodPlaneData = &( planeData[0] );
    for( i=1; i<16; i++ ){
      if( goodPlaneData->brSum < planeData[i].brSum ){
	goodPlaneData = &( planeData[i] );
      }
    }

    if( goodPlaneData->brSum > 0 ){
      static int status;
      static const double limitAdjustFactor=2.0;
      status = StraightFit( NumOfStep, goodPlaneData->z, goodPlaneData->x, goodPlaneData->br, &constX, &slopeX );
      if( status == IP_ERROR ) break;
      status = StraightFit( NumOfStep, goodPlaneData->z, goodPlaneData->y, goodPlaneData->br, &constY, &slopeY );
      if( status == IP_ERROR ) break;

      if( ( fabs(slopeX-goodPlaneData->dxPixelSearch) > (gTSP.pixelStep.x*limitAdjustFactor) ) ||
	  ( fabs(slopeY-goodPlaneData->dyPixelSearch) > (gTSP.pixelStep.y*limitAdjustFactor) ) ){
	break;
      }

      track = (RawTrack*) malloc( sizeof(RawTrack) );
      track->iplane = goodPlaneData->nPlane;
      track->sigma = 
	sqrt( calc_sigma2( NumOfStep, goodPlaneData->z, goodPlaneData->x, goodPlaneData->br, constX, slopeX ) +
	      calc_sigma2( NumOfStep, goodPlaneData->z, goodPlaneData->y, goodPlaneData->br, constY, slopeY ) );
      track->flag = 1;
      for( ipl=0; ipl<NumOfStep; ipl++ ){
	track->numhit += goodPlaneData->nPixel[ipl];
      }

      track->constX = constX;
      track->slopeX = slopeX;
      track->posStage.x = gTSP.center.x - (constX - IP_DRAW_WIDTH/2.  ) * gTSP.ccdReso.x;
      track->angStage.x = -slopeX * gTSP.ccdReso.x / gTSP.index;
      track->constY = constY;
      track->slopeY = slopeY;
      track->posStage.y = gTSP.center.y + (constY - IP_DRAW_HEIGHT/2. ) * gTSP.ccdReso.y;
      track->angStage.y = slopeY * gTSP.ccdReso.y / gTSP.index;

      track->br = goodPlaneData->brSum;
      track->tantheta =
	sqrt( track->angStage.x * track->angStage.x + track->angStage.y * track->angStage.y );

      track->planeData = *goodPlaneData;
      goodPlaneData->flag = 1;
      track->clusterData = aCluster;
      aCluster->flag = 1;

      trkColl.data = realloc( trkColl.data, sizeof( RawTrack* ) * ( trkColl.n + 1 ) );
      trkColl.data[ trkColl.n ] = track;
      trkColl.n++;
    }
  } // iCluster

  return trkColl;
}

static void TrackSearch( TrackCollection *trkColl, double *z,
			 double dxdz0, double dydz0, int ud )
{
  double dxdz, dxdzGel, dxPix;
  double dydz, dydzGel, dyPix;
  int sumBr[IP_QUAT_SIZE];
  int nSegment;
  int iTrack, nTrack;
  ClusterCollection *cluster;
  TrackCollection trkCollLocal = { 0, 0 };
  int thrNumImage;
  double angle;

  MakeShrinkData();

  gTSP.pixelStep.x = gTSP.angleStep.x * gTSP.index / gTSP.ccdReso.x;
  gTSP.pixelStep.y = gTSP.angleStep.y * gTSP.index / gTSP.ccdReso.y;

  nTrack = 0;
  for( dxdz = dxdz0 + gTSP.angleRangeMin.x; dxdz < dxdz0 + gTSP.angleRangeMax.x; dxdz += gTSP.angleStep.x ){
    dxdzGel = -dxdz * gTSP.index;
    dxPix = dxdzGel / gTSP.ccdReso.x;

    for( dydz = dydz0 + gTSP.angleRangeMin.y; dydz < dydz0 + gTSP.angleRangeMax.y; dydz += gTSP.angleStep.y ){
      dydzGel = dydz * gTSP.index;
      dyPix = dydzGel / gTSP.ccdReso.y;

      angle = sqrt( dxdz*dxdz + dydz*dydz );
      if( angle < gTSP.cutAngle ) continue;
      if( angle < 3 * gTSP.cutAngle )
	thrNumImage = 7;
      else
	thrNumImage = 6;

      nSegment = PreMoveAndOver( thrNumImage, dxPix, dyPix, z, sumBr );
      if( nSegment == 0 ) continue;

      cluster = emclustering( 0, sumBr );
      trkCollLocal = MoveAndOver( thrNumImage, dxPix, dyPix, z, cluster );

      trkColl->data = realloc( trkColl->data, sizeof( RawTrack* ) * ( trkColl->n + trkCollLocal.n ) );
      for( iTrack = 0; iTrack < trkCollLocal.n; iTrack++ ){
	trkColl->data[ trkColl->n + iTrack ] = trkCollLocal.data[ iTrack ];
      }
      trkColl->n += trkCollLocal.n;
      free( trkCollLocal.data );

      ClearClusterData( cluster );
    }
  }
}

DLLEXPORT void __stdcall
IP_GetIndex( double *indexUp, double *indexLo )
{
  *indexUp = gTSP.indexUp;
  *indexLo = gTSP.indexLo;
}

static void ClearRawTrack( RawTrack *track )
{
  int ipl;

  if( !track ) return;

  for( ipl=0; ipl<NumOfStep; ipl++ ){
    free( track->planeData.data[ipl].hit );
  }
  ClearOneClusterData( track->clusterData );
  free( track );
}
static void ClearTrack( TrackCollection *trackColl )
{
  int iTrk;

  for( iTrk=0; iTrk<trackColl->n; iTrk++ ){
    ClearRawTrack( trackColl->data[iTrk] );
  }
  if( trackColl->data )
    free( trackColl->data );

  memset( &(trackColl->n), 0x00, sizeof( TrackCollection ) );
}
DLLEXPORT void __stdcall IP_ClearData( void )
{
  int i;

  for( i=0; i<6; i++ ){
    ClearTrack( &( gTrack[i] ) );
  }

  for( i=0; i<gViewAtLowerGel.n; i++ ){
    free( gViewAtLowerGel.data[i] );
  }
  if( gViewAtLowerGel.data )
    free( gViewAtLowerGel.data );
  memset( &(gViewAtLowerGel.n), 0x00, sizeof( ViewProfileCollection ) );

  return;
}

DLLEXPORT void __stdcall IP_ClearCandidateID( void )
{
  EMcand = 0;
}

DLLEXPORT void __stdcall IP_BeamSearch( int ud, double *z, int nImage )
{
  FILE *fp = fopen( "tmp.log", "w" );
  int i;
  for( i=0; i<nImage; i++ ){
    fprintf( fp, "%2d %lf\n", i, z[i] );
  }
  fclose( fp );
}


static void CleanupTrackThick( TrackCollection *trkColl )
{
  // temporaly function
  // The same with IP_CleanupTrackUpperGel except input value and brightness threshold.
  
  RawTrack *trk1, *trk2;
  int iTrack1, iTrack2;
  double dx, dy;
  double limitxy2;
  const double deltaXY  = 0.003;
  const double deltaXYa = 0.015;
  const int thrBr = 3000;

  for( iTrack1 = 0; iTrack1 < trkColl->n; iTrack1++ ){
    trk1 = trkColl->data[iTrack1];
    if( trk1->flag == 0 ) continue;
    if( trk1->br < thrBr ){
      trk1->flag = 0;
      continue;
    }

    for( iTrack2 = iTrack1 + 1; iTrack2 < trkColl->n; iTrack2++ ){
      trk2 = trkColl->data[iTrack2];
      if( trk2->flag == 0 ) continue;
      if( trk2->br < thrBr ){
	trk2->flag = 0;
	continue;
      }

      dx = trk1->posStage.x - trk2->posStage.x;
      dy = trk1->posStage.y - trk2->posStage.y;
      limitxy2 = deltaXY + (deltaXYa * trk2->tantheta);
      limitxy2 = limitxy2 * limitxy2;
      if( (dx * dx + dy * dy) < limitxy2 ){
	if( trk1->br < trk2->br ){
	  trk1->flag = 0;
	}
	else{
	  trk2->flag = 0;
	}
      }
    }
  }

  Cleaner( trkColl );
}
DLLEXPORT int __stdcall
IP_TrackSearchThick( int layer, double Zstep0, double dZsurf0,
		     double LimitPara, double LimitPerp,
		     double dxdz0, double dydz0,
		     double X00, double Y00, // center of image
		     double x0, double y0,
		     double index )
{
  double z[NumOfStep];
  int i;

  for( i=0; i<NumOfStep; i++ ){
    z[i] = dZsurf0 + i * Zstep0;
  }

  gTSP.index = index;
  gTSP.center.x = X00;
  gTSP.center.y = Y00;

  memset( &(gTrack[layer].n), 0x00, sizeof(TrackCollection) );
  TrackSearch( &(gTrack[layer]), z, dxdz0, dydz0, 0 );
  CleanupTrackThick( &(gTrack[layer]) );

  return gTrack[layer].n;
}
#endif
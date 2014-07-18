/*
 *  $Id: coordinate.c,v 1.8 2002/07/06 15:28:27 e373 Exp $
 */

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

#include "ipdefine.h"
#include "ipproto.h"
#include "dst4.h"

//#define NumPlateP 13    //  1-12で12枚
#define XRAYDIST 100.0

int PlateNo;
//Beam Coordinate by Xray Mark
static double XrayMarkX[3][5], XrayMarkY[3][5];
static double EMovMagnit = 1.0, EMovSita, EMovCos = 1.0, EMovSin;
static double EMorgX, EMorgY;
static double EMgataX[2];

//Global Grid Coordinate。 
	//Plate#2のgrid座標に対する各プレートのgrid座標の位置関係
static double GGsita[NUMPLATEp], GGmag[NUMPLATEp];
static double GGdx[NUMPLATEp], GGdy[NUMPLATEp];
static double GGcos[NUMPLATEp], GGsin[NUMPLATEp];

//Local Grid Coordinate
static double GridMagnit = 1.0, GridSita, GridCos = 1.0, GridSin;

//Local Grid Coordinate Calibration with 11*11 Grid Mark
#define NFineGrX 10		//0から始まるので、実際に使うマークの数は
#define NFineGrY 10		//(NFineGrX+1)*(NFineGrY+1)
#define FineGrArea 100.0
#define FineGrStep 20.0
static char GrOfsFile[80];
static double GrOfsFineX[NFineGrX + 1][NFineGrY + 1],
	GrOfsFineY[NFineGrX + 1][NFineGrY + 1];

// Hyper Fine Correction
static double hfdx, hfdy;

extern FILE *emio_log;


void LogPar(FILE * emio_log)
{
  time_t ltime;

  fprintf(emio_log, "Plate %d\n", PlateNo);
  time(&ltime);
  fprintf(emio_log, "\t\t\t%s", ctime(&ltime));

  fprintf(emio_log, "\nEmulsion Mover Coordinate Parameters\n");
  fprintf(emio_log, "Magni : %7.5f     Sita  : %7.5f\n", EMovMagnit, EMovSita);
  fprintf(emio_log, "Origin : (%7.3f,%7.3f)    gata : %6.3f\n",
	  EMorgX, EMorgY, EMgataX[0]);
  if (PlateNo < 3)
	fprintf(emio_log,
		"XrayMark  %8.3f   %8.3f   %8.3f   %8.3f   %8.3f   %8.3f   %8.3f   %8.3f\n",
		XrayMarkX[PlateNo][1], XrayMarkY[PlateNo][1],
		XrayMarkX[PlateNo][2], XrayMarkY[PlateNo][2],
		XrayMarkX[PlateNo][3], XrayMarkY[PlateNo][3],
		XrayMarkX[PlateNo][4], XrayMarkY[PlateNo][4]);

  fprintf(emio_log, "\nGlobal Grid Coordinate Parameters\n");
  fprintf(emio_log, "GGoffset (%6.3f,%6.3f)\n", GGdx[PlateNo], GGdy[PlateNo]);
  fprintf(emio_log, "Magni : %7.5f     Sita  : %7.5f\n",
	  GGmag[PlateNo], GGsita[PlateNo]);

  fprintf(emio_log, "\nLocal Grid Coordinate Parameters\n");
  fprintf(emio_log, "Magni : %7.5f     Sita  : %7.5f\n", GridMagnit, GridSita);
  fprintf(emio_log, "Offset Filename : %s\n\n", GrOfsFile);
  fflush(emio_log);
}

DLLEXPORT void __stdcall IP_InitCo(int mode, short nplate)
{
  int iplate;

  if (mode) {
	for (iplate = 1; iplate < NUMPLATEp; iplate++)
	  GGmag[iplate] = 1.0;
  }
  for (iplate = 1; iplate < NUMPLATEp; iplate++) {
	GGcos[iplate] = cos(GGsita[iplate]);
	GGsin[iplate] = sin(GGsita[iplate]);
  }
  PlateNo = nplate;
  return;
}

static void WriteGrFine(char *filename, double EMthick)
{
  FILE *fp;
  int ix, iy, ithick;

  fp = fopen(filename, "w");
  fprintf(fp, "#plate-%d\n", PlateNo);
  ithick = (int) (EMthick * 1000.0);
  fprintf(fp, "thickness %d\n", ithick);
  for (iy = 0; iy <= NFineGrY; iy++)
	for (ix = 0; ix <= NFineGrX; ix++)
	  fprintf(fp, "  %2d  %2d  %7.4f  %7.4f\n",ix, iy, GrOfsFineX[ix][iy], GrOfsFineY[ix][iy]);
  fclose(fp);
}

static int ReadGrFine(char *filename, double *EMthick)
{
	FILE *fp;
	int ix, iy, iix, iiy, iplate, ithick;
	char dummy[256];

	fp = fopen(filename, "r");
	fgets(dummy, 256, fp);
	if (sscanf(dummy, "#plate-%d", &iplate) != 1)
		return IP_ERROR;
	if (iplate != PlateNo)
		return IP_ERROR;
	fgets(dummy, 256, fp);
	if (sscanf(dummy, "thickness %d", &ithick) != 1)
		return IP_ERROR;
	*EMthick = ithick / 1000.0;
	if (iplate != PlateNo)
		return -iplate;
	for (iy = 0; iy <= NFineGrY; iy++)
		for (ix = 0; ix <= NFineGrX; ix++) {
			fscanf(fp, "  %d  %d  %lf  %lf",
				&iix, &iiy, &(GrOfsFineX[ix][iy]), &(GrOfsFineY[ix][iy]));
			if ((ix != iix) || (iy != iiy))
				return IP_ERROR;
		}
		fclose(fp);
		strcpy(GrOfsFile, filename);
		return IP_NORMAL;
}

DLLEXPORT int __stdcall IP_RWGrFine(char *mode, char *filename, double *EMthick)
{
  switch (mode[0]) {
	  case 'r':
		return ReadGrFine(filename, EMthick);
	  case 'w':
		WriteGrFine(filename, *EMthick);
		return IP_NORMAL;
  }

  return IP_NORMAL;
}
#if 0
DLLEXPORT void __stdcall IP_GelThickness(double msx, double msy, double t1, double t2)
{
  if (emio_log == NULL)
	return;
  fprintf(emio_log, "Gel Thickness 1 : %6.4f, 2 : %6.4f at (%8.3f,%8.3f)\n", t1, t2, msx, msy);
  return;
}
#endif
DLLEXPORT void __stdcall IP_SetGridLocal(short iplate, double magnit, double sita, double EMindex1, double EMindex2)
{
  time_t ltime;
  extern emscan EMscanData;

  PlateNo = iplate;
  GridMagnit = magnit;
  GridSita = sita;
  GridCos = cos(sita);
  GridSin = sin(sita);
  hfdx = hfdy = 0.0;

  if (emio_log != NULL) {
	time(&ltime);
	fprintf(emio_log, "\n%s", ctime(&ltime));
	fprintf(emio_log, "current trackid is %d %d\n", EMscanData.iTrack, EMscanData.sub);
	fprintf(emio_log, "Magni : %7.5f     Sita  : %7.5f\n", GridMagnit, GridSita);
	fprintf(emio_log, "EMindex  %5.3f    %5.3f\n", EMindex1, EMindex2);
	fflush(emio_log);
  }
}
#if 0
DLLEXPORT void __stdcall IP_SetFineGr(short ix, short iy, double dx, double dy)
{
  GrOfsFineX[ix][iy] = dx;
  GrOfsFineY[ix][iy] = dy;
}

DLLEXPORT void __stdcall IP_SetHyperFineXY(double dx, double dy)
{
  hfdx = dx;
  hfdy = dy;
}
#endif
static point GOffsetFine(double gx, double gy)
{
  point delta;
  int ix, iy;
  double t, u;

  ix = (int) ((gx + FineGrArea) / FineGrStep);
  iy = (int) ((gy + FineGrArea) / FineGrStep);
  if ((ix >= 0) && (ix < NFineGrX) && (iy >= 0) && (iy < NFineGrY)) {
	t = (gx - (ix * FineGrStep - FineGrArea)) / FineGrStep;
	t = t < 0 ? 0.0 : t;
	t = t > 1 ? 1.0 : t;
	u = (gy - (iy * FineGrStep - FineGrArea)) / FineGrStep;
	u = u < 0 ? 0.0 : u;
	u = u > 1 ? 1.0 : u;
	delta.x =
	  (1 - t) * (1 - u) * GrOfsFineX[ix][iy]
	  + (1 - t) * u * GrOfsFineX[ix][iy + 1]
	  + t * (1 - u) * GrOfsFineX[ix + 1][iy]
	  + t * u * GrOfsFineX[ix + 1][iy + 1];
	delta.y =
	  (1 - t) * (1 - u) * GrOfsFineY[ix][iy]
	  + (1 - t) * u * GrOfsFineY[ix][iy + 1]
	  + t * (1 - u) * GrOfsFineY[ix + 1][iy]
	  + t * u * GrOfsFineY[ix + 1][iy + 1];
  }
  else {
	if ((iy >= 0) && (iy < NFineGrY)) {
	  if (ix < 0)
		  ix = 0;
	  else if (ix > NFineGrX)
		  ix = NFineGrX;
	  u = (gy - (iy * FineGrStep - FineGrArea)) / FineGrStep;
	  u = u < 0 ? 0.0 : u;
	  u = u > 1 ? 1.0 : u;
	  delta.x = (1 - u) * GrOfsFineX[ix][iy] + u * GrOfsFineX[ix][iy + 1];
	  delta.y = (1 - u) * GrOfsFineY[ix][iy] + u * GrOfsFineY[ix][iy + 1];
	} else if ((ix >= 0) && (ix < NFineGrX)) {
	  if (iy < 0)
		  iy = 0;
	  else if (iy > NFineGrY)
		  iy = NFineGrY;
	  t = (gx - (ix * FineGrStep - FineGrArea)) / FineGrStep;
	  t = t < 0 ? 0.0 : t;
	  t = t > 1 ? 1.0 : t;
	  delta.x = (1 - t) * GrOfsFineX[ix][iy] + t * GrOfsFineX[ix + 1][iy];
	  delta.y = (1 - t) * GrOfsFineY[ix][iy] + t * GrOfsFineY[ix + 1][iy];
	} else {
	  if (ix < 0)
		  ix = 0;
	  else if (ix > NFineGrX)
		  ix = NFineGrX;
	  if (iy < 0)
		  iy = 0;
	  else if (iy > NFineGrY)
		  iy = NFineGrY;
	  delta.x = GrOfsFineX[ix][iy];
	  delta.y = GrOfsFineY[ix][iy];
	}
  }
  return delta;
}

DLLEXPORT void __stdcall IP_GtoM(char *mode, double gx, double gy, double *msx, double *msy)
{
  point delta;
  double dx, dy;

  if ((mode[0] == 'p') || (mode[0] == 'P')) {
	delta = GOffsetFine(gx, gy);
	dx = delta.x + hfdx;
	dy = delta.y + hfdy;
  }
  else {
	dx = dy = 0.0;
  }

  *msx = (gx * GridCos - gy * GridSin + dx) * GridMagnit;
  *msy = (gx * GridSin + gy * GridCos + dy) * GridMagnit;
}

DLLEXPORT void __stdcall IP_MtoG(int mode, double msx, double msy, double *gx, double *gy)
{
  point delta;

  *gx = (msx * GridCos + msy * GridSin) / GridMagnit;
  *gy = (-msx * GridSin + msy * GridCos) / GridMagnit;
  if (mode) {
	delta = GOffsetFine(*gx, *gy);
	*gx -= delta.x + hfdx;
	*gy -= delta.y + hfdy;
  }
}

DLLEXPORT void __stdcall IP_EMtoG2(int mode, double emx, double emy, int yflag, double *gx, double *gy)
{
  emx -= EMgataX[yflag];
  *gx = (-emx * EMovCos - emy * EMovSin) * EMovMagnit;
  *gy = (-emx * EMovSin + emy * EMovCos) * EMovMagnit;
  if (mode) {
	*gx += EMorgX;
	*gy += EMorgY;
  }
}

DLLEXPORT void __stdcall IP_G2toEM(int mode, double gx, double gy, int yflag, double *emx, double *emy)
{
  if (mode) {
	gx -= EMorgX;
	gy -= EMorgY;
  }
  *emx = (-gx * EMovCos - gy * EMovSin) / EMovMagnit;
  *emy = (-gx * EMovSin + gy * EMovCos) / EMovMagnit;
  *emx += EMgataX[yflag];
}

DLLEXPORT void __stdcall IP_EMtoG2angle(double emx, double emy, double *gx, double *gy)
{
  /*
   * EmulsionMover座標と顕微鏡座標では、z軸の向きが違うので、
   * IP_EMtoGとは別に関数を作った。
   */
  *gx = -(-emx * EMovCos - emy * EMovSin) * EMovMagnit;
  *gy = -(-emx * EMovSin + emy * EMovCos) * EMovMagnit;
}
#if 0
DLLEXPORT void __stdcall IP_G2toEMangle(double gx, double gy, double *emx, double *emy)
{
  /*
   * EmulsionMover座標と顕微鏡座標では、z軸の向きが違うので、
   * IP_GtoEMとは別に関数を作った。
   */
  *emx = -(-gx * EMovCos - gy * EMovSin) / EMovMagnit;
  *emy = -(-gx * EMovSin + gy * EMovCos) / EMovMagnit;
}
#endif
DLLEXPORT void __stdcall IP_G2toG(int mode, double g2x, double g2y, double *gx, double *gy)
{
  if (mode) {
	g2x -= GGdx[PlateNo];
	g2y -= GGdy[PlateNo];
  }
  *gx = (g2x * GGcos[PlateNo] + g2y * GGsin[PlateNo]) * GGmag[PlateNo];
  *gy = (-g2x * GGsin[PlateNo] + g2y * GGcos[PlateNo]) * GGmag[PlateNo];
}

DLLEXPORT void __stdcall IP_GtoG2(int mode, int nplate, double gx, double gy, double *g2x, double *g2y)
{
  *g2x = (gx * GGcos[nplate] - gy * GGsin[nplate]) / GGmag[nplate];
  *g2y = (gx * GGsin[nplate] + gy * GGcos[nplate]) / GGmag[nplate];
  if (mode) {
	*g2x += GGdx[nplate];
	*g2y += GGdy[nplate];
  }
}
#if 0
DLLEXPORT void __stdcall IP_CorrectGrOfsFine()
{
  int ix, iy, npoint;
  double dx, dy;

  for (iy = 0; iy <= NFineGrY; iy++)
	for (ix = 0; ix <= NFineGrX; ix++)
	  if ((GrOfsFineX[ix][iy] == 0) && (GrOfsFineY[ix][iy] == 0)) {
	//何らかの理由で値が決まらなかった。
	dx = dy = 0.0;
	npoint = 0;
	if (ix > 0) {
	  dx += GrOfsFineX[ix - 1][iy];
	  dy += GrOfsFineY[ix - 1][iy];
	  npoint++;
	}
	if (ix < NFineGrX) {
	  dx += GrOfsFineX[ix + 1][iy];
	  dy += GrOfsFineY[ix + 1][iy];
	  npoint++;
	}
	if (iy > 0) {
	  dx += GrOfsFineX[ix][iy - 1];
	  dy += GrOfsFineY[ix][iy - 1];
	  npoint++;
	}
	if (iy < NFineGrY) {
	  dx += GrOfsFineX[ix][iy + 1];
	  dy += GrOfsFineY[ix][iy + 1];
	  npoint++;
	}
	GrOfsFineX[ix][iy] = dx / npoint;
	GrOfsFineY[ix][iy] = dy / npoint;
	  }
}

DLLEXPORT void __stdcall IP_DecideGG(short iplate, double GGsita0, double GGdx0, double GGdy0, double GGmag0) {
  PlateNo = iplate;
  GGsita[PlateNo] = GGsita0;
  GGcos[PlateNo] = cos(GGsita0);
  GGsin[PlateNo] = sin(GGsita0);
  GGdx[PlateNo] = GGdx0;
  GGdy[PlateNo] = GGdy0;
  GGmag[PlateNo] = GGmag0;
}
#endif
static int ReadGridData(char *filename)
{
  FILE *fp;
  char dummy[256];
  int i, iplate, stat;

  stat = -1;
  if ((fp = fopen(filename, "r")) == NULL)
	return -1;

  i = 1;
  while (1) {
	if (fgets(dummy, 256, fp) == NULL) {
	  stat = -111;
	  goto error;
	}
	if (dummy[0] == '#' || dummy[0] == '\n' || dummy[0] == '\0')
	  continue;
	if (sscanf(dummy, "%d %lf %lf %lf %lf %lf %lf %lf %lf", &iplate,
		   &(XrayMarkX[i][1]), &(XrayMarkY[i][1]), &(XrayMarkX[i][2]),
		   &(XrayMarkY[i][2]), &(XrayMarkX[i][3]), &(XrayMarkY[i][3]),
		   &(XrayMarkX[i][4]), &(XrayMarkY[i][4])) != 9) {
	  stat = -2;
	  goto error;
	}
	if (iplate != i) {
	  stat = -3;
	  goto error;
	}
	i++;
	if (i > 2)
	  break;
  }
  stat = -10;
  i = 3;
  while (fgets(dummy, 256, fp) != NULL) {
	if (dummy[0] == '#' || dummy[0] == '\n' || dummy[0] == '\0')
	  continue;
	if (sscanf(dummy, "%d %lf %lf %lf %lf",
		   &iplate, &GGdx[i], &GGdy[i], &GGsita[i], &GGmag[i]) != 5)
	  goto error;
	GGcos[i] = cos(GGsita[i]);
	GGsin[i] = sin(GGsita[i]);
	if (iplate != i)
	  goto error;
	i++;
  }
  if (i != NUMPLATEp)
	goto error;
  stat = IP_NORMAL;
error:
  fclose(fp);
  return stat;
}

static void WriteGridData(char *filename)
{
  FILE *fp;
  int iplate;

  fp = fopen(filename, "w");
  fprintf(fp,
	  "#  plate  XrayX1    XrayY1    XrayX2    XrayY2    XrayX3    XrayY3    XrayX4    XrayY4\n");
  for (iplate = 1; iplate <= 2; iplate++)
	fprintf(fp,
		"   %2d   %8.3f   %8.3f   %8.3f   %8.3f   %8.3f   %8.3f   %8.3f   %8.3f\n",
		iplate, XrayMarkX[iplate][1], XrayMarkY[iplate][1],
		XrayMarkX[iplate][2], XrayMarkY[iplate][2],
		XrayMarkX[iplate][3], XrayMarkY[iplate][3],
		XrayMarkX[iplate][4], XrayMarkY[iplate][4]);
  fprintf(fp, "#plate delatx  deltay    sita   magni\n");
  for (iplate = 3; iplate < NUMPLATEp; iplate++)
	fprintf(fp, " %2d   %6.3f  %6.3f %8.5f %7.5f\n",
		iplate, GGdx[iplate], GGdy[iplate], GGsita[iplate],
		GGmag[iplate]);
  fclose(fp);
  return;
}

DLLEXPORT int __stdcall IP_RWGridData(char *mode, char *filename)
{
  switch (mode[0]) {
  case 'r':
	return ReadGridData(filename);
  case 'w':
	WriteGridData(filename);
	return IP_NORMAL;
  }
  return IP_ERROR;
}
#if 0
DLLEXPORT void __stdcall IP_PutXmarkPos(short imark, double gx, double gy)
{
  XrayMarkX[PlateNo][imark] = gx;
  XrayMarkY[PlateNo][imark] = gy;
}
#endif
static void coord_make_beam_coord(int iplate, double *orgx, double *orgy,
				  double *theta, double *magni, double *gata)
{
  int imark;
  double UL[2]={0.,0.}, UR[2]={0.,0.}, DL[2]={0.,0.}, DR[2]={0.,0.};
  double x, y, dist, mx, my;
  double sin_theta, cos_theta;

  for (imark = 1; imark <= 4; imark++) {
	x = XrayMarkX[iplate][imark];
	y = XrayMarkY[iplate][imark];
	if (x > 0) {
	  if (y > 0) {
	UR[0] = x;
	UR[1] = y;
	  } else {
	DR[0] = x;
	DR[1] = y;
	  }
	} else {
	  if (y > 0) {
	UL[0] = x;
	UL[1] = y;
	  } else {
	DL[0] = x;
	DL[1] = y;
	  }
	}
  }
  *theta = (atan((UR[1] - UL[1]) / (UR[0] - UL[0]))
		- atan((UL[0] - DL[0]) / (UL[1] - DL[1]))
		+ atan((DR[1] - DL[1]) / (DR[0] - DL[0]))) / 3;
  cos_theta = cos(*theta);
  sin_theta = sin(*theta);

  dist = sqrt((UR[0] - UL[0]) * (UR[0] - UL[0]) +
		  (UR[1] - UL[1]) * (UR[1] - UL[1]))
	+ sqrt((UL[0] - DL[0]) * (UL[0] - DL[0]) +
	   (UL[1] - DL[1]) * (UL[1] - DL[1]));
  *magni = dist / 2.0 / (XRAYDIST * 2.0);

  mx = -XRAYDIST;
  my = XRAYDIST;		// 右上 EM座標で
  *orgx = UR[0] + ( mx * cos_theta + my * sin_theta) * (*magni);
  *orgy = UR[1] - (-mx * sin_theta + my * cos_theta) * (*magni);
  mx = XRAYDIST;		//左上
  *orgx += UL[0] + ( mx * cos_theta + my * sin_theta) * (*magni);
  *orgy += UL[1] - (-mx * sin_theta + my * cos_theta) * (*magni);
  my = -XRAYDIST;		//左下
  *orgx += DL[0] + ( mx * cos_theta + my * sin_theta) * (*magni);
  *orgy += DL[1] - (-mx * sin_theta + my * cos_theta) * (*magni);
  *orgx /= 3.0;
  *orgy /= 3.0;
  mx = -XRAYDIST;		//右下
  *gata = mx
	+ ((DR[0] - *orgx) * cos_theta + (DR[1] - *orgy) * sin_theta) / *magni;
}

static void coord_make_global_grid_par_from_xray(int plate)
{
  double orgx[2], orgy[2], theta[2], magni[2], gata[2];

  coord_make_beam_coord(1, &orgx[0], &orgy[0], &theta[0], &magni[0], &gata[0]);
  coord_make_beam_coord(2, &orgx[1], &orgy[1], &theta[1], &magni[1], &gata[1]);

  /* make global-grid-coordinate parameters for plate#1 */
  GGdx[1] = orgx[1] - orgx[0];
  GGdy[1] = orgy[1] - orgy[0];
  GGsita[1] = theta[1] - theta[0];
  GGmag[1] = magni[1] / magni[0];
  GGcos[1] = cos(GGsita[1]);
  GGsin[1] = sin(GGsita[1]);

  /* make emulsion-mover-coordinate parameters for specified plate */
  EMorgX = orgx[plate];
  EMorgY = orgy[plate];
  EMovSita = theta[plate];
  EMovMagnit = magni[plate];
  EMovCos = cos(EMovSita);
  EMovSin = sin(EMovSita);
  /* mover-GATA is made from X-ray marks on plate#1 */
  EMgataX[0] = gata[0];
  EMgataX[1] = 0.0;
}

DLLEXPORT void __stdcall IP_MakeBeamCo(double *sita, double *magnit,
					   double *x0, double *y0, double *dx)
{
  if (PlateNo == 1)
	coord_make_global_grid_par_from_xray(0);
  else
	coord_make_global_grid_par_from_xray(1);

  if (PlateNo < 3) {
	*sita = EMovSita;
	*magnit = EMovMagnit;
	*x0 = EMorgX;
	*y0 = EMorgY;
  } else {
	*sita = GGsita[PlateNo];
	*magnit = GGmag[PlateNo];
	*x0 = GGdx[PlateNo];
	*y0 = GGdy[PlateNo];
  }
  *dx = EMgataX[0];
}
#if 0
typedef struct{
  short year;
  short month;
  short dayOfWeek;
  short day;
  short hour;
  short min;
  short sec;
  short msec;
} WinTime;
DLLEXPORT void __stdcall IP_WritePosition( int n, WinTime *t, double loD, double loU, double upD, double upU )
{
  FILE *fp = fopen( "position.txt", "a" );
  fprintf( fp, "%6d %4d %2d %2d %2d %2d %2d %3d %9.4lf %9.4lf %9.4lf %9.4lf\n",
	   n,
	   t->year,
	   t->month,
	   t->day,
	   t->hour,
	   t->min,
	   t->sec,
	   t->msec,
	   loD, loU, upD, upU );
  fclose(fp);
}
#endif
#ifdef _WIN32
# include <windows.h>
# include <conio.h>
# include <malloc.h>
# include <memory.h>
#endif

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>

#include "ipdefine.h"
#include "dst4.h"
#include "ipproto.h"
#include "Parameter.h"

#define EMIO_IN_MODE_PRED	0
#define EMIO_IN_MODE_SCAN	1
#define EMIO_IN_MODE_VTX	2
#define EMIO_OUT_MODE_SCAN	0
#define EMIO_OUT_MODE_VTX	1

FILE *emio_log = NULL;
static EmScanFile *emio_scanfp = NULL;
static int emio_in_mode, emio_out_mode;
static char emio_in_file_name  [256];
static char emio_scan_file_name[256];
static char emio_err_file_name [256];


extern emscan EMscanData;

extern int PlateNo;
#if 0
DLLEXPORT int __stdcall IP_InFileReopen()
{
  if (emio_scanfp == NULL) return IP_ERROR;
  if (dst4_close(emio_scanfp) != 0) return IP_ERROR;
  if ((emio_scanfp = dst4_open(emio_in_file_name, "r")) == NULL) return IP_ERROR;

  return IP_NORMAL;
}
#endif
DLLEXPORT int __stdcall IP_OpenFile(int mode, char *predfile, char *filename, char *wamode)
{
  EmScanFile *scanfp;
  char	*dummy;
  char	logfile[256];

  switch (mode) {
  case 0:
    emio_in_mode = EMIO_IN_MODE_PRED;
    emio_out_mode = EMIO_OUT_MODE_SCAN;
    break;
  case 1:
    emio_in_mode = EMIO_IN_MODE_SCAN;
    emio_out_mode = EMIO_OUT_MODE_SCAN;
    break;
  case 2:
    emio_in_mode = EMIO_IN_MODE_SCAN;
    emio_out_mode = EMIO_OUT_MODE_VTX;
    break;
  case 3:
    emio_in_mode = EMIO_IN_MODE_VTX;
    emio_out_mode = EMIO_OUT_MODE_VTX;
  }

  if (emio_scanfp) {
    if (dst4_close(emio_scanfp) != 0) return IP_ERROR;
  }

  if ((emio_scanfp = dst4_open(predfile, "r")) == NULL) return IP_ERROR;
  strcpy(emio_in_file_name, predfile);
  strcpy(emio_scan_file_name, filename);

  if (strlen(filename) != 0) {
    if ((scanfp = dst4_open(filename, wamode)) == NULL) return IP_ERROR;
    fprintf(scanfp->fp, "#%s\n", predfile);
    dst4_write_version(scanfp);
    dst4_close(scanfp);

    dummy = strtok(filename, ".");
    sprintf(logfile, "%s.log", dummy);
    if (emio_log) fclose(emio_log);
    if ((emio_log = fopen(logfile, wamode)) == NULL) return IP_ERROR;
    fprintf(emio_log, "%s\n", predfile);
    LogPar(emio_log);

    if (emio_out_mode == EMIO_OUT_MODE_SCAN) {
      sprintf(emio_err_file_name, "%s.err", dummy);
      if ((scanfp = dst4_open(emio_err_file_name, wamode)) == NULL) return IP_ERROR;
      dst4_write_version(scanfp);
      dst4_close(scanfp);
    }
  }

  if (emio_in_mode == EMIO_IN_MODE_VTX) {
    if (dst4_read_scan(emio_scanfp, &EMscanData, NULL) != 1) return IP_ERROR;
    emio_append_scan_data(&EMscanData);
  }

  return IP_NORMAL;
}
#if 0
DLLEXPORT void __stdcall IP_PrintVertex(short trackid, double pos,double msx, double msy, double msz)
{
  EmScanFile *scanfp;
  double gx, gy, g2x, g2y;

  if ((scanfp = dst4_open(emio_scan_file_name, "a")) == NULL) return;
  if (trackid == -1) {
    fprintf(scanfp->fp, "\n");
  }
  else {
    vtrackinfo vtInfo;
    IP_MtoG(1, msx, msy, &gx, &gy);
    IP_GtoG2(1, PlateNo, gx, gy, &g2x, &g2y);
    vtInfo.iplate = PlateNo;
    vtInfo.ud = pos;
    vtInfo.gx = g2x;
    vtInfo.gy = g2y;
    vtInfo.msz = msz;
    if( pos<=1.0 ) vtInfo.index = GetEmulsionIndexUp();
    else           vtInfo.index = GetEmulsionIndexDown();
    dst4_write_vtrackinfo( scanfp, trackid, &vtInfo );
  }

  dst4_close(scanfp);
}

DLLEXPORT void __stdcall IP_PrintVertexRange(short trackid)
{
  int stat;
  EmScanFile *scanfp;
  emscan scan_data;
  vtrack track = { 0, 0, 0 };

  if ((scanfp = dst4_open(emio_scan_file_name, "r")) == NULL) return;
  if (dst4_read_scan(scanfp, &scan_data, NULL) != 1) return;
  while ((stat = dst4_read_vtrack(scanfp, &track)) == 1) {
    if (track.itrack == trackid) break;
  }
  dst4_close(scanfp);
  if (stat != 1) return;

  if ((scanfp = dst4_open(emio_scan_file_name, "a")) == NULL) return;
  dst4_write_vtrack_range(scanfp, &track);
  dst4_close(scanfp);

  free(track.vt);
}
#endif
// Extract next track data.
static int emio_read_pred(short *TrackID, short *direction, double *gx, double *gdxdz, double *gy, double *gdydz)
{
  int stat;
  short track_id;

  while ((stat = dst4_read_pred(emio_scanfp, &EMscanData)) == 1) {
    track_id = EMscanData.iTrack * 100 + EMscanData.sub;
    if (track_id > TrackID[0]) {
      TrackID[0] = track_id;
      *direction = EMscanData.LR;
      IP_EMtoG2(1, EMscanData.x0, EMscanData.y0, EMscanData.LR, gx, gy);
      IP_EMtoG2angle(EMscanData.dxdz0, EMscanData.dydz0, gdxdz, gdydz);
      break;
    }
  }

  return stat;
}

// Extract assinged track data.
static int emio_read_pred2(short *TrackID, short *direction, double *gx, double *gdxdz, double *gy, double *gdydz)
{
  int stat;
  short track_id;

  if ((emio_scanfp != 0) && (emio_scanfp->fp != 0)) {
    fseek(emio_scanfp->fp,0,SEEK_SET);
  }
  else {
    return 0;
  }

  while ((stat = dst4_read_pred(emio_scanfp, &EMscanData)) == 1) {
    track_id = EMscanData.iTrack * 100 + EMscanData.sub;
    if (track_id == TrackID[0]) {
      TrackID[0] = track_id;
      *direction = EMscanData.LR;
      IP_EMtoG2(1, EMscanData.x0, EMscanData.y0, EMscanData.LR, gx, gy);
      IP_EMtoG2angle(EMscanData.dxdz0, EMscanData.dydz0, gdxdz, gdydz);
      break;
    }
  }

  return stat;
}

static int emio_is_to_jump(short *TrackID)
{
  int iplate;
  int TrackNum;

  TrackNum = EMscanData.iTrack * 100 + EMscanData.sub;
  if (TrackNum < TrackID[0]) return 1;			//jump
  if (TrackNum > TrackID[0]) return 0;
  for (iplate = 1; iplate <= NUMPLATE; iplate++) {
    if (EMscanData.EMtrackInfo[iplate][0].cand < TrackID[iplate]) return 1;
    if (EMscanData.EMtrackInfo[iplate][0].cand > TrackID[iplate]) return 0;
  }

  return 0;
}

static int emio_read_scan(double deltaz, short *TrackID, short *direction,
			  double *gx, double *gdxdz, double *gy, double *gdydz,
			  char *comment)
{
  int stat, i;
  emtrackinfo *TrackInfo;
  emtrackinfo *trackPl1Base, *trackPl1LowerGel;
  double g2x, g2y;
  static int LastTrackNum, LastCand1, LastCand2;

  LastCand2 = EMscanData.EMtrackInfo[2][0].cand;
  while ((stat = dst4_read_scan(emio_scanfp, &EMscanData, comment)) == 1) {
    if (emio_is_to_jump(TrackID)) continue;

    if( PlateNo == 1 ){
      *gx = EMscanData.EMtrackInfo[1][0].gx;
      *gy = EMscanData.EMtrackInfo[1][0].gy;
      *gdxdz = EMscanData.EMtrackInfo[1][1].gdxdz;
      *gdydz = EMscanData.EMtrackInfo[1][1].gdydz;
    }
    if( EMscanData.LastPlate >= PlateNo ){
      TrackInfo = &EMscanData.EMtrackInfo[PlateNo][0];
      *gx = TrackInfo->gx;
      *gy = TrackInfo->gy;
      *gdxdz = TrackInfo->gdxdz;
      *gdydz = TrackInfo->gdydz;
    }
    else if (EMscanData.LastPlate == 1 && PlateNo == 2) {
      trackPl1Base     = &EMscanData.EMtrackInfo[1][1] ;
      trackPl1LowerGel = &EMscanData.EMtrackInfo[1][2] ;
      IP_EMtoG2(1, trackPl1LowerGel->emx, trackPl1LowerGel->emy, EMscanData.LR, gx, gy);
      IP_EMtoG2angle( trackPl1Base->emdxdz, trackPl1Base->emdydz, gdxdz, gdydz);
      *gx += *gdxdz * deltaz;
      *gy += *gdydz * deltaz;
    }
    else if (EMscanData.LastPlate == PlateNo - 1 && EMscanData.LastUD == 3) {
      TrackInfo = &EMscanData.EMtrackInfo[EMscanData.LastPlate][3];
      IP_GtoG2(1, EMscanData.LastPlate, TrackInfo->gx, TrackInfo->gy, &g2x, &g2y);
      IP_G2toG(1, g2x, g2y, gx, gy);
      IP_GtoG2(0, EMscanData.LastPlate, TrackInfo->gdxdz, TrackInfo->gdydz, &g2x, &g2y);
      IP_G2toG(0, g2x, g2y, gdxdz, gdydz);
    }
    else {
      emio_append_scan_data(&EMscanData); // Don't write when 'jump'.
      continue;
    }

    TrackID[0] = EMscanData.iTrack * 100 + EMscanData.sub;
    for (i = 1; i <= EMscanData.LastPlate; i++) {
      TrackID[i] = EMscanData.EMtrackInfo[i][0].cand;
    }
    *direction = EMscanData.LR;
    if (EMscanData.LastPlate == 2 && PlateNo == 2) {
      // 'cand' is counted from 0 when manual scan of plate#2
      if (TrackID[0] != LastTrackNum || TrackID[1] != LastCand1) {
	EMscanData.EMtrackInfo[2][0].cand = 0;
      }
      else {
	EMscanData.EMtrackInfo[2][0].cand = LastCand2;
      }

      LastTrackNum = TrackID[0];
      LastCand1 = TrackID[1];
    }
    break;
  }

  return stat;
}

static int emio_read_vtrack(short *TrackID, short *direction,
			    double *gx, double *gdxdz,
			    double *gy, double *gdydz, char *comment)
{
  int stat;
  double gxp, gyp;
  EmScanFile *outfp;
  static vtrack Vtrack = { 0, 0, 0 };
  vtrackinfo *vtInfo1, *vtInfo2;

  /* read track data (and write it to output file) */
  if ((outfp = dst4_open(emio_scan_file_name, "a")) == NULL) return IP_ERROR;

  while ((stat = dst4_read_vtrack(emio_scanfp, &Vtrack)) == 1) {
    if (Vtrack.itrack == 0) continue;
    dst4_write_vtrack(outfp, &Vtrack);
    if (Vtrack.NumPosition < 2) continue;
    if (Vtrack.vt[Vtrack.NumPosition - 1].iplate == PlateNo - 1 &&
	Vtrack.vt[Vtrack.NumPosition - 1].ud == 3.0) {
      *direction = 1;
      break;
    }
    else if (Vtrack.vt[Vtrack.NumPosition - 1].iplate == PlateNo + 1 &&
	     Vtrack.vt[Vtrack.NumPosition - 1].ud == 0.0) {
      *direction = -1;
      break;
    }
    dst4_write_vtrack_range(outfp, &Vtrack);
  }
  dst4_close(outfp);

  if (stat != 1) return stat;

  /* calculate values to return */
  TrackID[0] = Vtrack.itrack;
  sprintf(comment, EMscanData.EMtrackInfo[EMscanData.LastPlate][EMscanData.LastUD].comment);
  vtInfo1 = &(Vtrack.vt[Vtrack.NumPosition - 1]);
  vtInfo2 = &(Vtrack.vt[Vtrack.NumPosition - 2]);
  IP_G2toG(1, vtInfo1->gx, vtInfo1->gy, gx, gy);
  IP_G2toG(1, vtInfo2->gx, vtInfo2->gy, &gxp, &gyp);
  *gdxdz = (*gx - gxp) / ( (vtInfo1->msz - vtInfo2->msz) * vtInfo1->index );
  *gdydz = (*gy - gyp) / ( (vtInfo1->msz - vtInfo2->msz) * vtInfo1->index );

  return stat;
}

static const int getTrackMode_NextTrack = 0;
static const int getTrackMode_TrackOfID = 1;
DLLEXPORT int __stdcall
IP_ReadInFile(double deltaz, short *TrackID, short *direction,
	      double *gx, double *gdxdz, double *gy, double *gdydz, char *comment,
	      int getTrackMode )
{
  int stat = IP_ERROR;

  switch (emio_in_mode) {
  case EMIO_IN_MODE_PRED:
    if( getTrackMode == getTrackMode_NextTrack )
      stat = emio_read_pred(TrackID, direction, gx, gdxdz, gy, gdydz);
    else if( getTrackMode == getTrackMode_TrackOfID )
      stat = emio_read_pred2(TrackID, direction, gx, gdxdz, gy, gdydz);
    break;
  case EMIO_IN_MODE_SCAN:
    stat = emio_read_scan(deltaz, TrackID, direction, gx, gdxdz, gy, gdydz, comment);
    break;
  case EMIO_IN_MODE_VTX:
    stat = emio_read_vtrack(TrackID, direction, gx, gdxdz, gy, gdydz, comment);
    break;
  }

  if (stat == 0) {  /* EOF */
    dst4_close(emio_scanfp);
    emio_scanfp = NULL;
  }

  return stat;
}

DLLEXPORT int __stdcall
IP_GetNextTrack(double deltaz, short *TrackID, short *direction,
		double *gx, double *gdxdz, double *gy, double *gdydz, char *comment)
{
  return IP_ReadInFile( deltaz, TrackID, direction, gx, gdxdz, gy, gdydz, comment, getTrackMode_NextTrack );
}

DLLEXPORT int __stdcall
IP_GetTrackOfID(double deltaz, short *TrackID, short *direction,
		double *gx, double *gdxdz, double *gy, double *gdydz, char *comment)
{
  return IP_ReadInFile( deltaz, TrackID, direction, gx, gdxdz, gy, gdydz, comment, getTrackMode_TrackOfID );
}

#if 0
DLLEXPORT void __stdcall IP_GetTrackPositionUpperGel( double *gx, double *gy )
{
  *gx = EMscanData.EMtrackInfo[1][0].gx;
  *gy = EMscanData.EMtrackInfo[1][0].gy;
}
DLLEXPORT void __stdcall IP_GetTrackPositionLowerGel( double *gx, double *gy )
{
  if( emio_scanfp->major <= 3 ){
    emtrackinfo *data = &(EMscanData.EMtrackInfo[1][0]);
    *gx = data->gx - 0.2 * data->gdxdz;
    *gy = data->gy - 0.2 * data->gdydz;
  }
  else{
    *gx = EMscanData.EMtrackInfo[1][2].gx;
    *gy = EMscanData.EMtrackInfo[1][2].gy;
  }
}

DLLEXPORT int __stdcall IP_GetLastID(short *TrackID)
{
  int iplate;
  EmScanFile *scanfp;
  int stat;

  if ((scanfp = dst4_open(emio_scan_file_name, "r")) == NULL) {
    fprintf(emio_log, "GetLastID error : cannot open %s\n", emio_scan_file_name);
    fflush(emio_log);
    return IP_ERROR;
  }
  stat = dst4_get_last_scan(scanfp, &EMscanData);
  dst4_close(scanfp);
  if (stat == -1) {
    fprintf(emio_log, "GetLastID error : after %d %d %d\n", EMscanData.iTrack, EMscanData.sub, EMscanData.EMtrackInfo[1][0].cand);
    fflush(emio_log);
    return IP_ERROR;
  }

  TrackID[0] = EMscanData.iTrack * 100 + EMscanData.sub;
  for (iplate = 1; iplate <= EMscanData.LastPlate; iplate ++) {
    TrackID[iplate] = EMscanData.EMtrackInfo[iplate][0].cand;
  }

  for (iplate = EMscanData.LastPlate + 1; iplate <= NUMPLATE; iplate ++) {
    TrackID[iplate] = 0;
  }

  return IP_NORMAL;
}
#endif
int emio_append_scan_data(emscan *EMscan)
{
  // In order to edit scan0 file in scanning, file is closed for each track.
  EmScanFile *outfp = dst4_open(emio_scan_file_name, "a");
  if (outfp == NULL) return IP_ERROR;

  dst4_write_scan(outfp, EMscan);
  dst4_close(outfp);

  return IP_NORMAL;
}
#if 0
DLLEXPORT void __stdcall IP_PrintOut(short flag)
{
  if (flag) {
    EMscanData.LastPlate = PlateNo > 0 ? PlateNo - 1 : 0;
    if (EMscanData.LastPlate <= 1)
      EMscanData.LastUD = 0;
    else
      EMscanData.LastUD = 3;
  }
  emio_append_scan_data(&EMscanData);
}

#endif
DLLEXPORT void __stdcall IP_PrintErr()
{
  EmScanFile *outfp = dst4_open(emio_err_file_name, "a");
  if (outfp == NULL) return;
  if (emio_in_mode == EMIO_IN_MODE_PRED)
    dst4_write_pred(outfp, &EMscanData);
  else
    dst4_write_scan(outfp, &EMscanData);

  dst4_close(outfp);
}
#if 0
DLLEXPORT int __stdcall IP_MakeCommentFile(char *infile, char *outfile)
{
  EmScanFile *scanfp;
  FILE *ofp;
  emscan scan_data;
  char comment[256];

  if ((scanfp = dst4_open(infile, "r")) == NULL) return IP_ERROR;
  if ((ofp = fopen(outfile, "w")) == NULL) {
    dst4_close(scanfp);
    return IP_ERROR;
  }
  while (dst4_read_scan(scanfp, &scan_data, comment) == 1) {
    if (strlen(comment)) fprintf(ofp, "%s\n", comment);
  }

  fclose(ofp);
  dst4_close(scanfp);

  return IP_NORMAL;
}
#endif
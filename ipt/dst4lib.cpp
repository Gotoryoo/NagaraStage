/*
 *  $Id: dst4lib.c,v 1.9 2002/07/11 14:15:54 thitoshi Exp $
 */

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <stdarg.h>
#include <math.h>
#ifdef _WIN32
# include <malloc.h>
#endif

#include "dst4.h"
#include "ipdefine.h"


EmScanFile *dst4_new(FILE *fp)
{
  EmScanFile *scanfp;
  scanfp = (EmScanFile *) malloc(sizeof(EmScanFile));
  scanfp->fp = fp;
  scanfp->major = DST4_MAJOR_VERSION;
  scanfp->minor = DST4_MINOR_VERSION;
  return scanfp;
}

void dst4_delete(EmScanFile *scanfp)
{
  free(scanfp);
}

EmScanFile *dst4_open(const char *filename, const char *mode)
{
  FILE *fp = fopen(filename, mode);
  if (fp == NULL)
    return NULL;
  return dst4_new(fp);
}

int dst4_close(EmScanFile *scanfp)
{
  int stat;

  if (scanfp == NULL)
    return EOF;
  stat = fclose(scanfp->fp);
  dst4_delete(scanfp);
  return stat;
}

void dst4_write_version(EmScanFile *scanfp)
{
  if (scanfp == NULL || scanfp->fp == NULL) return;
  fprintf(scanfp->fp, "#ver%s\n", DST4_VERSION_STRING);
  fflush(scanfp->fp);
  fflush(scanfp->fp);
}

void dst4_write_pred(EmScanFile *scanfp, emscan *EMscan)
{
  if (scanfp == NULL || scanfp->fp == NULL) return;

  fprintf(scanfp->fp, " %3d %2d %3d %4d %2d %5.3f %5.3f %5.3f %8.4f %8.4f",
	  EMscan->iTrack, EMscan->sub,
	  EMscan->run, EMscan->spill, EMscan->event,
	  EMscan->Kmom, EMscan->misma, EMscan->mismo,
	  EMscan->misdxdz, EMscan->misdydz);
  fprintf(scanfp->fp, " %2d %7d%2d %7d %8.3f %8.4f %8.3f %8.4f %4d %3d\n",
	  EMscan->L, EMscan->Lbr, EMscan->R, EMscan->Rbr,
	  EMscan->x0, EMscan->dxdz0, EMscan->y0, EMscan->dydz0,
	  EMscan->info, EMscan->LR);
}

void dst4_write_scan(EmScanFile *scanfp, emscan *EMscan)
{
  int iplate, ud;
  emtrackinfo *TrackInfo;
  char *p;

  if (scanfp == NULL || scanfp->fp == NULL) return;

  fprintf(scanfp->fp, "a %2d %d %3d %2d %3d %4d %2d %5.3f %5.3f %5.3f",
	  EMscan->LastPlate, EMscan->LastUD, EMscan->iTrack, EMscan->sub,
	  EMscan->run, EMscan->spill, EMscan->event,
	  EMscan->Kmom, EMscan->misma, EMscan->mismo);
  fprintf(scanfp->fp, " %2d %7d %2d %7d %8.3f %8.4f %8.3f %8.4f %4d %3d\n",
	  EMscan->L, EMscan->Lbr, EMscan->R, EMscan->Rbr,
	  EMscan->x0, EMscan->dxdz0, EMscan->y0, EMscan->dydz0,
	  EMscan->info, EMscan->LR);
  fflush(scanfp->fp);
  if (EMscan->LastPlate == 0)
    goto flush;

  for (ud = 0; ud < 3; ud++) { // for Plate#1
    TrackInfo = &(EMscan->EMtrackInfo[1][ud]);
    fprintf(scanfp->fp,
	    "  1 %d %3d %8.3f %7.4f %8.3f %7.4f %8.3f %7.4f %8.3f %7.4f %d",
	    ud, TrackInfo->cand,
	    TrackInfo->emx, TrackInfo->emdxdz,
	    TrackInfo->emy, TrackInfo->emdydz,
	    TrackInfo->gx, TrackInfo->gdxdz,
	    TrackInfo->gy, TrackInfo->gdydz,
	    TrackInfo->br );
    if (strlen(TrackInfo->comment)) {
      p = TrackInfo->comment;
      while ((*p == ' ') && (*p != '\0'))
	p++;
      if (*p != '\0')
	fprintf(scanfp->fp, " %s", p);
    }
    fprintf(scanfp->fp, "\n");
  }

  if (EMscan->LastPlate == 1) goto flush;

  for (iplate = 2; iplate < EMscan->LastPlate; iplate++)
    for (ud = 0; ud < 4; ud++) {
      TrackInfo = &(EMscan->EMtrackInfo[iplate][ud]);
      fprintf(scanfp->fp,
	      " %2d %1d %3d %8.3f %6.4f %8.3f %6.4f %8.3f %6.4f %8.3f %6.4f %d",
	      iplate, ud, TrackInfo->cand, TrackInfo->emx,
	      TrackInfo->emdxdz, TrackInfo->emy, TrackInfo->emdydz,
	      TrackInfo->gx, TrackInfo->gdxdz, TrackInfo->gy,
	      TrackInfo->gdydz, TrackInfo->br);
      if (strlen(TrackInfo->comment)) {
	p = TrackInfo->comment;
	while (*p == ' ') p++;
	if (*p != '\0')
	  fprintf(scanfp->fp, " %s", p);
      }
      fprintf(scanfp->fp, "\n");
    }
  for (ud = 0; ud <= EMscan->LastUD; ud++) {
    TrackInfo = &(EMscan->EMtrackInfo[iplate][ud]);
    fprintf(scanfp->fp,
	    " %2d %1d %3d %8.3f %6.4f %8.3f %6.4f %8.3f %6.4f %8.3f %6.4f %d",
	    iplate, ud, TrackInfo->cand, TrackInfo->emx, TrackInfo->emdxdz,
	    TrackInfo->emy, TrackInfo->emdydz, TrackInfo->gx,
	    TrackInfo->gdxdz, TrackInfo->gy, TrackInfo->gdydz,
	    TrackInfo->br);
    if (strlen(TrackInfo->comment)) {
      p = TrackInfo->comment;
      while (*p == ' ') p++;
      if (*p != '\0')
	fprintf(scanfp->fp, " %s", p);
    }
    fprintf(scanfp->fp, "\n");
  }
  fprintf(scanfp->fp, "s %10d %2d %2d %2d %6.3f %6.3f %6.3f\n",
	  EMscan->categ, EMscan->TrackType, EMscan->Nf, EMscan->Nb,
	  EMscan->Dcay_vx, EMscan->Dcay_vy, EMscan->Dcay_vz);
 flush:
  fflush(scanfp->fp);
}


static int sscaninfo(char *dummy, EmScanFile *scanfp, int *iiplate, int *iud, emtrackinfo *TrackInfo)
{
  char *p;
  int iw, nd;

  TrackInfo->comment[0] = '\0';
  if ((scanfp->major == 1 && scanfp->minor > 0) || scanfp->major > 1) {
    nd = 12;
    if (sscanf(dummy, " %d %d %d %lf %lf %lf %lf %lf %lf %lf %lf %d",
	       iiplate, iud, &(TrackInfo->cand),
	       &(TrackInfo->emx), &(TrackInfo->emdxdz),
	       &(TrackInfo->emy), &(TrackInfo->emdydz),
	       &(TrackInfo->gx), &(TrackInfo->gdxdz),
	       &(TrackInfo->gy), &(TrackInfo->gdydz),
	       &(TrackInfo->br)) != nd)
      return -1;
  } else {
    nd = 11;
    if (sscanf(dummy, " %d %d %d %lf %lf %lf %lf %lf %lf %lf %lf",
	       iiplate, iud, &(TrackInfo->cand),
	       &(TrackInfo->emx), &(TrackInfo->emdxdz),
	       &(TrackInfo->emy), &(TrackInfo->emdydz),
	       &(TrackInfo->gx), &(TrackInfo->gdxdz),
	       &(TrackInfo->gy), &(TrackInfo->gdydz)) != nd)
      return -1;
  }
  p = dummy;
  for (iw = 0; iw < nd; iw++) {
    while (*p == ' ') p++;
    while ((*p != ' ') && (*p != '\n')) p++;
  }
  iw = 0;
  while (*p == ' ') p++;
  while ((*p != '\n') && (*p != '\0' && (*p != 13))) {
    TrackInfo->comment[iw] = *p;
    p++;
    iw++;
  }
  TrackInfo->comment[iw] = '\0';
  return 0;
}


int dst4_read_pred(EmScanFile *scanfp, emscan *EMscan)
{
  char dummy[256];

  if (scanfp == NULL || scanfp->fp == NULL) return -1;

  while (fgets(dummy, sizeof(dummy), scanfp->fp) != NULL) {
    if ((dummy[0] == '#') || (dummy[0] == '\n')) continue;
    if (sscanf(dummy,
	       " %d %d %d %d %d %lf %lf %lf %lf %lf %d %d %d %d %lf %lf %lf %lf %d %d",
	       &(EMscan->iTrack), &(EMscan->sub),
	       &(EMscan->run), &(EMscan->spill), &(EMscan->event),
	       &(EMscan->Kmom), &(EMscan->misma), &(EMscan->mismo),
	       &(EMscan->misdxdz), &(EMscan->misdydz),
	       &(EMscan->L), &(EMscan->Lbr), &(EMscan->R), &(EMscan->Rbr),
	       &(EMscan->x0), &(EMscan->dxdz0),
	       &(EMscan->y0), &(EMscan->dydz0),
	       &(EMscan->info), &(EMscan->LR)) != 20)
      return -1;
    EMscan->LastPlate = EMscan->LastUD = 0;
    return 1;
  }
  return 0;
}


int dst4_read_scan(EmScanFile *scanfp, emscan *EMscan, char *comment)
{
  char dummy[256], *p;
  int iplate, ud;
  int iiplate, iud, j;
  int nLayerPlate1;
  emtrackinfo *TrackInfo;

  if( scanfp == NULL || scanfp->fp == NULL ){
    return IP_ERROR;
  }

  if( comment != NULL ) comment[0] = '\0';
  EMscan->categ = EMscan->TrackType = EMscan->Nf = EMscan->Nb = 0;

  while( fgets(dummy, sizeof(dummy), scanfp->fp) != NULL ){
    if( dummy[0] == '\n' ) continue;
    if( dummy[0] == '#' ){
      if( (p = strstr(dummy, "ver")) != NULL ){
	if( sscanf(p, "ver%d.%d", &scanfp->major, &scanfp->minor) != 2 ){
	  scanfp->minor = 0;
	}
      }
      else if( (p = strstr(dummy, "Ver")) != NULL ){
	if( sscanf(p, "Ver%d.%d", &scanfp->major, &scanfp->minor) != 2 ){
	  scanfp->minor = 0;
	}
      }
      continue;
    }

    for( iplate = 1; iplate < NUMPLATEp; iplate++ ){
      for( ud = 0; ud < 4; ud++ ){
	TrackInfo = &(EMscan->EMtrackInfo[iplate][ud]);
	TrackInfo->cand = TrackInfo->br = 0;
	TrackInfo->comment[0] = '\0';
	TrackInfo->emx = TrackInfo->emy = 0.0;
	TrackInfo->emdxdz = TrackInfo->emdydz = 0.0;
	TrackInfo->gx = TrackInfo->gy = 0.0;
	TrackInfo->gdxdz = TrackInfo->gdydz = 0.0;
      }
    }
    if( sscanf(dummy,
	       "a %d %d %d %d %d %d %d %lf %lf %lf %d %d %d %d %lf %lf %lf %lf %d %d",
	       &(EMscan->LastPlate), &(EMscan->LastUD),
	       &(EMscan->iTrack), &(EMscan->sub),
	       &(EMscan->run), &(EMscan->spill), &(EMscan->event),
	       &(EMscan->Kmom), &(EMscan->misma), &(EMscan->mismo),
	       &(EMscan->L), &(EMscan->Lbr), &(EMscan->R), &(EMscan->Rbr),
	       &(EMscan->x0), &(EMscan->dxdz0), &(EMscan->y0),
	       &(EMscan->dydz0), &(EMscan->info), &(EMscan->LR)) != 20 ){
      return IP_ERROR;
    }
    
    if( EMscan->LastPlate == 0 ){
      return 1;
    }

/*     read scan data of plate#1 */
    if( scanfp->major >= 4 ){
      nLayerPlate1 = 3;
    }
    else{
      nLayerPlate1 = 1;
    }
    for( ud=0; ud<nLayerPlate1; ud++ ){
      if( fgets(dummy, sizeof(dummy), scanfp->fp) == NULL ){
	return IP_ERROR;
      }
      if( sscaninfo(dummy, scanfp, &iiplate, &iud, &EMscan->EMtrackInfo[1][ud]) == -1 ){
	return IP_ERROR;
      }
    }

/*     read scan data after plate#2 */
    for( iplate = 2; iplate < EMscan->LastPlate; iplate++ ){
      for( ud = 0; ud < 4; ud++ ){
	if( fgets(dummy, sizeof(dummy), scanfp->fp) == NULL ){
	  return IP_ERROR;
	}
	if( sscaninfo(dummy, scanfp, &iiplate, &iud, &EMscan->EMtrackInfo[iplate][ud]) == -1 ){
	  return IP_ERROR;
	}
	if( (iiplate != iplate) || (iud != ud) ){
	  return IP_ERROR;
	}
      }
    }

    if( EMscan->LastPlate > 1 ){
      for( ud = 0; ud <= EMscan->LastUD; ud++ ){
	if( fgets(dummy, sizeof(dummy), scanfp->fp) == NULL ){
	  return IP_ERROR;
	}
	if( sscaninfo(dummy, scanfp, &iiplate, &iud, &EMscan->EMtrackInfo[iplate][ud]) == -1 ){
	  return IP_ERROR;
	}
	if( (iiplate != iplate) || (iud != ud) ){
	  return IP_ERROR;
	}
      }
      if( scanfp->major < 2 ){
	EMscan->categ = EMscan->TrackType = EMscan->Nf = EMscan->Nb = 0;
	EMscan->Dcay_vx = EMscan->Dcay_vy = EMscan->Dcay_vz = 0.0;
      }
      else{
	if( fgets(dummy, sizeof(dummy), scanfp->fp) == NULL ){
	  return IP_ERROR;
	}

	switch( scanfp->major ){
	case 2:
	  if( scanfp->minor == 0 ){
	    if( sscanf(dummy, "s %d %d %d", &EMscan->categ, &EMscan->TrackType, &EMscan->Nb) != 3 ){
	      return IP_ERROR;
	    }
	  }
	  else{
	    if( sscanf(dummy, "s %d %d %d %d", &(EMscan->categ), &(EMscan->TrackType), &(EMscan->Nf), &(EMscan->Nb)) != 4 ){
	      return IP_ERROR;
	    }
	  }
	  EMscan->Dcay_vx = EMscan->Dcay_vy = EMscan->Dcay_vz = 0.0;
	  break;
	case 3:
	  if( scanfp->minor == 0 ){
	    EMscan->Dcay_vx = EMscan->Dcay_vy = EMscan->Dcay_vz = 0.0;
	    if( sscanf(dummy, "s %d %d %d %d",
		       &(EMscan->categ), &(EMscan->TrackType),
		       &(EMscan->Nf), &(EMscan->Nb)) != 4 ){
	      return IP_ERROR;
	    }
	  }
	  else{
	    if( sscanf(dummy, "s %d %d %d %d %lf %lf %lf",
		       &(EMscan->categ), &(EMscan->TrackType),
		       &(EMscan->Nf), &(EMscan->Nb), &(EMscan->Dcay_vx),
		       &(EMscan->Dcay_vy), &(EMscan->Dcay_vz)) != 7 ){
	      return IP_ERROR;
	    }
	  }
	case 4:
	  if( sscanf(dummy, "s %d %d %d %d %lf %lf %lf",
		     &(EMscan->categ), &(EMscan->TrackType),
		     &(EMscan->Nf), &(EMscan->Nb), &(EMscan->Dcay_vx),
		     &(EMscan->Dcay_vy), &(EMscan->Dcay_vz)) != 7 ){
	    return IP_ERROR;
	  }
	}
      }
    }

    if( comment == NULL ){
      return 1;
    }

    j = sprintf(comment, " %d", EMscan->iTrack * 100 + EMscan->sub);
    j += sprintf(comment + j, "-%d", EMscan->EMtrackInfo[1][0].cand);
    if( strlen(EMscan->EMtrackInfo[1][0].comment) != 0 ){
      j += sprintf(comment + j, "(%s)", EMscan->EMtrackInfo[1][0].comment);
    }
    for( iplate = 2; iplate <= EMscan->LastPlate; iplate++ ){
      j += sprintf(comment + j, "-%d",
		   EMscan->EMtrackInfo[iplate][0].cand);
      for( iud = 0; iud < 4; iud++ ){
	if( strlen(EMscan->EMtrackInfo[iplate][iud].comment) != 0 ){
	  j += sprintf(comment + j, "(%d:%s)", iplate,
		       EMscan->EMtrackInfo[iplate][iud].comment);
	}
      }
    }

    return 1;
  }

  return 0;
}

int dst4_write_vtrack(EmScanFile *scanfp, vtrack *Vtrack)
{
  int i;

  if (scanfp == NULL || scanfp->fp == NULL) return -1;

  fprintf(scanfp->fp, "\n");
  for (i = 0; i < Vtrack->NumPosition; i++) {
    dst4_write_vtrackinfo( scanfp, Vtrack->itrack, &(Vtrack->vt[i]) );
  }

  return 0;
}

int dst4_write_vtrackinfo( EmScanFile *scanfp, int trkId, vtrackinfo *vtInfo)
{
  fprintf( scanfp->fp, "%d %2d %3.1f %10.5f %10.5f %9.5f %6.4lf\n",
	   trkId, vtInfo->iplate, vtInfo->ud,
	   vtInfo->gx, vtInfo->gy, vtInfo->msz,
	   vtInfo->index );
  return 0;
}

int dst4_write_vtrack_range(EmScanFile *scanfp, vtrack *Vtrack)
{
  int i;
  vtrackinfo *vt, *prevt;
  double range = 0;
  double dx, dy, dz;

  if (scanfp == NULL || scanfp->fp == NULL) return -1;

  prevt = &(Vtrack->vt[0]);
  for( i = 1; i < Vtrack->NumPosition; i++ ){
    vt = &Vtrack->vt[i];
    if( vt->iplate == prevt->iplate ){
      dx = vt->gx  - prevt->gx;
      dy = vt->gy  - prevt->gy;
      dz = vt->msz - prevt->msz;

      if( (vt->ud == 1.0 && prevt->ud == 2.0) ||
	  (vt->ud == 2.0 && prevt->ud == 1.0) ){
	// base
	range += sqrt( dx*dx + dy*dy + dz*dz ) / 3.6;
	/* divided by 3.6 for conversion from base to emulsion */
      }
      else{
	dz *= vt->index;
	range += sqrt( dx*dx + dy*dy + dz*dz );
      }
    }
    prevt = vt;
  }
  fprintf(scanfp->fp, "# approximate range = %.4f mm\n", range);

  return 0;
}

int dst4_read_vtrack(EmScanFile *scanfp, vtrack *Vtrack)
{
  char dummy[256];
  int stat = 0;
  int nval;

  if (scanfp == NULL || scanfp->fp == NULL) return -1;

  Vtrack->itrack = 0;
  Vtrack->NumPosition = 0;
  while (fgets(dummy, sizeof(dummy), scanfp->fp) != NULL) {
    if (dummy[0] == '\0' || dummy[0] == ' ' || dummy[0] == '\n') return 1;  /* next event */
    if (dummy[0] == '#') continue;  /* comment */
    stat = 1;
    Vtrack->vt = (vtrackinfo *)realloc(Vtrack->vt,sizeof(vtrackinfo) * (Vtrack->NumPosition + 1));
    if( ( nval = sscanf( dummy, "%d %d %lf %lf %lf %lf %lf",
			 &(Vtrack->itrack),
			 &(Vtrack->vt[Vtrack->NumPosition].iplate),
			 &(Vtrack->vt[Vtrack->NumPosition].ud),
			 &(Vtrack->vt[Vtrack->NumPosition].gx),
			 &(Vtrack->vt[Vtrack->NumPosition].gy),
			 &(Vtrack->vt[Vtrack->NumPosition].msz),
			 &(Vtrack->vt[Vtrack->NumPosition].index) ) )
	< 6 ){
      return -1;
    }
    if( nval==6 ) Vtrack->vt[Vtrack->NumPosition].index = 2.0; // typical value
      
    Vtrack->NumPosition ++;
  }

  return stat;
}

#if 0
int dst4_get_last_scan(EmScanFile *scanfp, emscan *EMscan)
{
  int stat;

  if (dst4_read_scan(scanfp, EMscan, NULL) == 0) {	// empty file
    EMscan->iTrack = EMscan->sub = EMscan->LastPlate = 0;
    return 0;
  }

  while ((stat = dst4_read_scan(scanfp, EMscan, NULL)) == 1);

  return stat;
}
#endif
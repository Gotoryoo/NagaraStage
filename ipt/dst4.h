#ifndef DST4_H_INCLUDE
#define DST4_H_INCLUDE

#define DST4_VERSION_STRING "4.0"
#define DST4_MAJOR_VERSION 4
#define DST4_MINOR_VERSION 0

#define EMSCANTYPE 20
#define EMTRACKTYPE 21

#define NUMPLATE 12
#define NUMPLATEp 13

typedef struct {
  int ix, iy;
  double z;
  int br;
} emhit;

typedef struct {
  int plate;			/* 1 */
  int view;			/* 2  */
  double X0, Y0, Z0;		/* 4*3=12 *//* 15 */
  double x, dxdz;		/* 4+4=8 *//* 23 */
  double y, dydz;		/* 4+4=8 *//* 31 */
  int numhit, br, numplane;	/* 2+4+2=8 *//* 39 */
  double sigma;			/* 4 *//* 43 */
  double sizeD, sizeL;		/* 8 *//* 51 */
  int *ix, *iy;			/* 4+4=8 */
  double *z;			/* 4 */
  int *ibr;			/* 4 */
} emtrack;

#define EMTRACKSIZE 51

typedef struct {
  int cand;
  double emx, emdxdz, emy, emdydz;
  double gx, gdxdz, gy, gdydz;
  int br;
  char comment[256];
} emtrackinfo;

typedef struct {
  int iTrack, sub;
  int run, spill, event;
  int LastPlate, LastUD;
  double Kmom, misma, mismo;
  double misdxdz, misdydz;
  int LR, L, Lbr, R, Rbr, info;
  double x0, dxdz0;
  double y0, dydz0;
  emtrackinfo EMtrackInfo[NUMPLATEp][4];
  int categ, TrackType, Nf, Nb;
  double Dcay_vx, Dcay_vy, Dcay_vz;
} emscan;

typedef struct {
  int iplate;
  double ud;
  double gx, gy, msz;
  double index;
} vtrackinfo;

typedef struct {
  int itrack;
  int NumPosition;
  vtrackinfo *vt;
} vtrack;

#define EMSCANSIZE 217

typedef struct {
  FILE *fp;
  int major, minor;
} EmScanFile;


extern EmScanFile *dst4_new(FILE *fp);
extern void dst4_delete(EmScanFile *scanfp);
extern EmScanFile *dst4_open(const char *filename, const char *mode);
extern int dst4_close(EmScanFile *scanfp);
extern void dst4_write_version(EmScanFile *scanfp);
extern void dst4_write_scan(EmScanFile *scanfp, emscan *EMscan);
extern void dst4_write_pred(EmScanFile *scanfp, emscan *EMscan);
extern int dst4_write_vtrack(EmScanFile *scanfp, vtrack *Vtrack);
extern int dst4_write_vtrackinfo(EmScanFile *scanfp, int trkId, vtrackinfo *vtInfo);
extern int dst4_write_vtrack_range(EmScanFile *scanfp, vtrack *Vtrack);
extern int dst4_read_scan(EmScanFile *scanfp, emscan *EMscan, char *comment);
extern int dst4_read_pred(EmScanFile *scanfp, emscan *EMscan);
extern int dst4_read_vtrack(EmScanFile *scanfp, vtrack *Vtrack);
extern int dst4_get_last_scan(EmScanFile *scanfp, emscan *EMscan);

#endif

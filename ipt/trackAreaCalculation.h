#ifndef trackAreaCalc_h
#define trackAreaCalc_h

typedef struct{
  double area;
  double volume;
} TrackData;

extern TrackData CalcTrackArea( int ix_ini, int iy_ini, double z_ini,
				int ix_fin, int iy_fin, double z_fin,
				int id, int nImage, double thr );

#endif

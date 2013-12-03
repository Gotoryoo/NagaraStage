#ifndef clustering_h
#define clustering_h 1

#include "ipdefine.h"

typedef struct {
  int npix;
  int br, brmax;
  double sigma;
  double centerx, centery;
  HIT *hits;
  int flag;
} ONE_CLUSTER;
typedef struct{
  int n;
  ONE_CLUSTER* (*data);
} ClusterCollection;

extern void CleanDust(unsigned char *Src);
extern ClusterCollection* emclustering(int thr, int *br );
extern void ClearOneClusterData( ONE_CLUSTER *cluster );
extern void ClearClusterData( ClusterCollection *clusColl );

#endif

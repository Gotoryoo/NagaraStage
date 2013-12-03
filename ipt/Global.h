#ifndef Global_h
#define Global_h 1

typedef struct{
  double x, y, z;
} Point;

typedef struct{
  int n;
  Point *coll;
} PointColl;

typedef unsigned char PixData;

#endif

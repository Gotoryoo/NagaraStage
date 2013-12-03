#ifndef __Vector_h__
#define __Vector_h__

typedef struct{
  int x;
  int y;
} Vector2I;
typedef struct{
  double x;
  double y;
} Vector2D;

typedef struct{
  double x;
  double y;
  double z;
} Vector3D;

double Norm2D( Vector2D vec );
double Dist2D( Vector2D vec1, Vector2D vec2 );

#endif

/* make gaus value of size of "2*size+1" (-size - +size) */

#include <math.h>
#include <stdlib.h>

#ifdef _WIN32
const double M_PI = 3.141592;
#endif

static double **value;
static int size, sizeFull;

static double Gaus( int x, int y, double sigma )
{
  return exp(-(x*x+y*y)/(2*sigma*sigma)) / (2*M_PI*sigma*sigma);
}
static void ClearData( void )
{
  int i;
  for( i=0; i<sizeFull; i++ ){
    free( value[i] );
  }
  free( value );
  size = sizeFull = 0;
}

void MakeGausValue( int size_, double sigma )
{
  int i, ix, iy;
  double total, factor;

  if( value ){ ClearData(); }

  size = size_;
  sizeFull = 2*size + 1;
  value = (double**)malloc( sizeof(double*) * sizeFull );
  for( i=0; i<sizeFull; i++ ){
    value[i] = (double*)malloc( sizeof(double) * sizeFull );
  }

  total = 0;
  for( ix=-size; ix<=size; ix++ ){
    for( iy=-size; iy<=size; iy++ ){
      value[iy+size][ix+size] = Gaus( ix, iy, sigma );
      total += value[iy+size][ix+size];
    }
  }
  factor = 1./total;
  for( ix=-size; ix<=size; ix++ ){
    for( iy=-size; iy<=size; iy++ ){
      value[iy+size][ix+size] *= factor;
    }
  }
}

double GetGausValue( int idX, int idY )
{
  return value[idY+size][idX+size];
}

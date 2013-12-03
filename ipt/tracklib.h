/*
 *  $Id: tracklib.h,v 1.2 2001/11/22 04:31:56 thitoshi Exp $
 */

#ifndef _E373_AUTOSTAGE_LIB_TRACKLIB_H
#define _E373_AUTOSTAGE_LIB_TRACKLIB_H

extern void FreeFitData( int *X, int *Y, double *Z, int *Br );
extern int StraightFit( int ndata, double *z, double *x, int *br,
			double *Const, double *Slope );
extern int iStraightFit( int ndata, double *x, int *y, int *br,
			 double *Const, double *Slope);
extern double calc_sigma2( int ndata, double *x, double *y, int *br,
			   double Const, double Slope );
extern double xyStraightFit( int ndata, int wsum, int *x, int *y, int *w,
			     double *Const, double *Slope );
extern double xyzStraightFit( int ndata, int wsum, double *x, int *y, int *w,
			      double *Const, double *Slope );
extern int CoreBright( double Const, double Slope, int ndata,
		       int *x, int *y, int *br );

#endif  /* !defined(_E373_AUTOSTAGE_LIB_TRACKLIB_H) */

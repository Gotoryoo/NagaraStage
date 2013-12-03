/*                                                                                        */
/*      +------------------------------------------------------------------------+        */
/*      + Copyright (c) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.     +        */
/*      +------------------------------------------------------------------------+        */
/*      Title : VP-900 Serise SH and PC Function Convert Define                           */
/*                                                                                        */
/*                                                                                        */


#ifndef		__vpxcnv_h__
#define		__vpxcnv_h__

#include 	<math.h>

#define		frexpf(a,b)				(float)frexp(a,b)
#define		ldexpf(a,b)				(float)ldexp(a,b)
#define		modff(a,b)				(float)modf(a,b)
#define		ceilf(d)				(float)ceil(d)
#define		fabsf(d)				(float)fabs(d)
#define		floorf(d)				(float)floof(d)
#define		fmodf(d)				(float)fmod(d)
#define		asinf(d)				(float)asin(d)
#define		acosf(d)				(float)acos(d)
#define		atanf(d)				(float)atan(d)
#define		atan2f(a,b)				(float)atan2(a,b)
#define		sinf(d)					(float)sin(d)
#define		cosf(d)					(float)cos(d)
#define		tanf(d)					(float)tan(d)
#define		sinhf(d)				(float)sinh(d)
#define		coshf(d)				(float)cosh(d)
#define		tanhf(d)				(float)tanh(d)
#define		expf(s)					(float)exp(d)
#define		logf(d)					(float)log(d)
#define		log10f(d)				(float)log10(d)
#define		powf(a,b)				(float)pow(a,b)
#define		sqrtf(d)				(float)sqrt(d)


#define		scanh					scanf
#define		scanfloat				scanf
#define		charget					getchar
#define		charput					putchar

/*                                                                         */
/*                                                                         */
/*     この後にユーザーのオリジナルコードを追加して下さい                  */
/*                                                                         */
/*                                                                         */









/*                                                                         */
/*                                                                         */
/*     ユーザーのオリジナルコードはここまでです                            */
/*                                                                         */
/*                                                                         */

#endif /* !__vpxcnv_h__ */


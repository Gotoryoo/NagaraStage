#ifndef __GAUSDIST_H__
#define __GAUSDIST_H__
#ifdef __cplusplus
extern "C"{
#endif

void MakeGausValue( int size, double sigma );
double GetGausValue( int idx, int idy );

#ifdef __cplusplus
}
#endif


#endif /* __GAUSDIST_H__ */
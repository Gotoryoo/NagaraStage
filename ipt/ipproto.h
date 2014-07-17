/**
* DLLエクスポートする関数を始め、すべての関数を宣言します.
* @file ipproto.h
* @authro Tomonaga Wada, Hirokazu Yokoyama
* @date 2012-07-11
*/

#ifndef __IPPROTO_H__
#define __IPPROTO_H__

#include "dst4.h"

extern "C" {
    /* main.cpp */
    DLLEXPORT int __stdcall IP_Init(int BorderNumber, long *RetCode);

    DLLEXPORT int __stdcall IP_CapMain( HWND, HDC, unsigned char *, int flagDrawOnPC );
    DLLEXPORT void __stdcall IP_DataPrint(HWND, HDC, short, unsigned char *);
    DLLEXPORT void __stdcall IP_NichiPrint(HWND, HDC, int, int, unsigned char *, unsigned char *, int emType);
    DLLEXPORT int __stdcall IP_BmpSave(char *, unsigned char *);
    DLLEXPORT  int __stdcall IP_BmpLoad(char *, char *, char *, char *, char *, unsigned char *);

    DLLEXPORT int __stdcall IP_GetDeviceID(void);
    DLLEXPORT int IP_GetCamera(int,int);
    DLLEXPORT void __stdcall IP_SetCameraType(int);

    void NewImage(HWND hWnd, HDC hDC, int Hi, LPBITMAPINFO bmi, unsigned char *Src);
    int BmpSave(char *FileName, RGBQUAD * rgb, int Width, int Height, unsigned char *Buff);

    /* mysocklib.cpp */
    DLLEXPORT int __stdcall IP_ReadSocketIniFile();
    DLLEXPORT int __stdcall IP_InitSocket(char *hostname, int port);
    DLLEXPORT int __stdcall IP_SendMessage(const char *mes);

    /* tracking.c */
    DLLEXPORT int __stdcall IP_InitIchiLib(int emtype, int numstep0, double EMindex10, double EMindex20, HWND hWnd0, HDC hDC0, int Hi0);
    DLLEXPORT void __stdcall IP_EMtype(short emtype, double min, double max, double step);
    DLLEXPORT void __stdcall IP_CCDreso( double x, double y );
    //DLLEXPORT int __stdcall IP_TrackSearchUpperGel
    //    ( int ud, double *z,
    //    double LimitPara, double LimitPerp,
    //    double dxdz0, double dydz0, double X00, double Y00,
    //    double x0, double y0, double index );
    //DLLEXPORT int __stdcall IP_CleanupTrackUpperGel();
    //DLLEXPORT void __stdcall IP_ExtractTrack(int ud, int itrack, double *x, double *y,double *dxdz, double *dydz, int *br);
    //DLLEXPORT int __stdcall IP_MakeViewOfLowerGel( double deltaZ1, double deltaZ2 );
    //DLLEXPORT int __stdcall IP_TrackSearchLowerGel
    //    ( double *z, double centerX, double centerY, double index, double baseThickness, int viewID );
    //DLLEXPORT int __stdcall IP_AfterTrack(int newflag, double *Xout, double *Yout);
    //DLLEXPORT void __stdcall IP_TrackEnd();
    //DLLEXPORT void __stdcall IP_Last();
    //DLLEXPORT void __stdcall IP_PutTrackInfo0(int ud, double MSx, double MSy,
    //    double MSdxdz, double MSdydz, int br);
    //DLLEXPORT void __stdcall IP_PutTrackInfo(int ud, double MSxin, double MSyin,
    //    double MSxout, double MSyout,
    //    double MSdxdz, double MSdydz);
    //DLLEXPORT void __stdcall IP_PutTrackInfoC(int ud, char *text);
    //DLLEXPORT void __stdcall IP_PutTrackCateg(int categ, short TrackType, short Nf, short Nb);
    //DLLEXPORT void __stdcall IP_DumpImage( char *fileName );
    //DLLEXPORT void __stdcall IP_ClearData( void );

    /* imageing.cpp */
    DLLEXPORT void __stdcall IP_SetThreshold(int thr_hit_num, int thr_br_dust,int thr_dust1, int thr_dust2);
    DLLEXPORT int  __stdcall IP_getHitNum();
    //DLLEXPORT int  __stdcall IP_getHitNum0(short thr, short *hit1, short *hit2, short *hit3, short *hit4,double *x, double *y);
    //DLLEXPORT void __stdcall IP_GetPhoto(int beginend, char *filename);
    //DLLEXPORT int  __stdcall IP_GetImageContly(short thr_surface, short thr_track);
    //DLLEXPORT int  __stdcall IP_ContImgCheck(short mode, short emtype, short iid, short *thr, short *throffset);
    //DLLEXPORT void __stdcall IP_SeqPhoto();
    //DLLEXPORT void __stdcall IP_SeqPhotoLast( char *filename );
    DLLEXPORT int  __stdcall IP_MarkCenter(double *sx, double *sy, short size);
    DLLEXPORT int  __stdcall IP_getAver();
    //DLLEXPORT void __stdcall IP_ReadDumpImage();
    //DLLEXPORT int  __stdcall IP_ImageAutoRange( short thr_surface, int emType );
    //DLLEXPORT void __stdcall IP_MakePictureForConnection( char *filename );
    //DLLEXPORT int  __stdcall IP_PreImageProcessing( short thr_surface, short thr_track, int emType );

    int AllocateImages();
    int AllocateImages4Track(int step);
    //int MakeSegData2();
    //void DrawSegImage(int (*segimage)[SEGROW]);
    
    //DLLEXPORT int  __stdcall IP_AllocateImageMemory( int id, int nImage );
    //DLLEXPORT void __stdcall IP_ReleaseImageMemory( int ud );
    //DLLEXPORT void __stdcall IP_DumpImageForBT( int ud, int n );

    /* coordinate.cpp */
    DLLEXPORT void __stdcall IP_InitCo(int mode, short nplate);
    DLLEXPORT void __stdcall IP_SetGridLocal(short iplate, double magnit, double sita, double index1, double index2);
    //DLLEXPORT void __stdcall IP_DecideGG(short iplate, double GGsita0,double GGdx0, double GGdy0, double GGmag0);
    DLLEXPORT int __stdcall IP_RWGridData(char *mode, char *filename);
    //DLLEXPORT void __stdcall IP_PutXmarkPos(short imark, double gx, double gy);
    DLLEXPORT void __stdcall IP_MakeBeamCo(double *sita, double *magnit, double *x0, double *y0, double *dx);
    DLLEXPORT void __stdcall IP_G2toG(int mode, double g2x, double g2y, double *gx, double *gy);
    DLLEXPORT void __stdcall IP_GtoG2(int mode, int nplate, double gx, double gy,double *g2x, double *g2y);
    DLLEXPORT void __stdcall IP_GtoM(char *mode, double gx, double gy, double *msx, double *msy);
    DLLEXPORT void __stdcall IP_MtoG(int mode, double msx, double msy, double *gx, double *gy);
    //DLLEXPORT void __stdcall IP_CorrectGrOfsFine();
    //DLLEXPORT void __stdcall IP_SetFineGr(short ix, short iy, double dx, double dy);
    DLLEXPORT int __stdcall IP_RWGrFine(char *mode, char *filename, double *EMthick);
    DLLEXPORT void __stdcall IP_EMtoG2(int mode, double emx, double emy, int yflag, double *gx, double *gy);
    DLLEXPORT void __stdcall IP_G2toEM(int mode, double gx, double gy, int yflag, double *emx, double *emy);
    DLLEXPORT void __stdcall IP_EMtoG2angle(double emx, double emy, double *gx, double *gy);
    //DLLEXPORT void __stdcall IP_G2toEMangle(double gx, double gy, double *emx, double *emy);
    //DLLEXPORT void __stdcall IP_GelThickness(double msx, double msy, double t1, double t2);
    DLLEXPORT void __stdcall IP_SetHyperFineXY(double dx, double dy);

    void LogPar(FILE *logfp);

    /*** emio.c ***/

    DLLEXPORT int __stdcall IP_OpenFile(int mode, char *predfile, char *filename, char *wamode);
    //DLLEXPORT int __stdcall IP_InFileReopen();
    //DLLEXPORT int __stdcall IP_ReadInFile
    //    ( double deltaz, short *TrackNum, short *LR,
    //    double *gx, double *gdxdz, double *gy, double *gdydz, char *comment, int getTrackMode );
    //DLLEXPORT int __stdcall IP_GetLastID(short *TrackID);
    DLLEXPORT void __stdcall IP_PrintErr();
    //DLLEXPORT void __stdcall IP_PrintOut(short flag);
    //DLLEXPORT int __stdcall IP_MakeCommentFile(char *infile, char *outfile);
    //DLLEXPORT void __stdcall IP_PrintVertex(short trackid, double pos, double msx, double msy, double msz);

    int emio_append_scan_data(emscan *EMscan);

    /*** Parameter.c ***/
    DLLEXPORT void __stdcall IP_SetEmulsionIndex
        ( double indexUp, double indexDown );
    DLLEXPORT void __stdcall IP_SetEmulsionIndexUp  ( double index );
    DLLEXPORT void __stdcall IP_SetEmulsionIndexDown( double index );

}



#endif /* __IPPROTO_H__ */
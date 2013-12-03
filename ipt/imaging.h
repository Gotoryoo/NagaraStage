#ifndef __IMAGING_H__
#define __IMAGING_H__


#ifdef __cplusplus
extern "C"{
#endif
void GetCameraImage( unsigned char *data );
void ImageAutoRange( unsigned char *dest, unsigned char *src, int emType );
#ifdef __cplusplus
}
#endif


#ifdef __cplusplus
extern "C"{
#endif
int MakeShrinkData( void );
#ifdef __cplusplus
}
#endif

int PreImageProceccing( int emType );

// Following function is made for test of beam tracking at SearchSurface.
void CopyCameraImage( unsigned char *dest, int imageBuffer );

typedef struct{
  unsigned char* raw;
  int* half;
  int* quarter;
  double z;
} ImageData;

typedef struct{
  int n;
  ImageData **data;
} ImageDataColl;

#ifdef __cplusplus
extern "C"{
#endif
int AllocateImageMemory( int id, int nImage );
#ifdef __cplusplus
}
#endif

#ifdef __cplusplus
extern "C"{
#endif
void ReleaseImageMemory( int ud );
#ifdef __cplusplus
}
#endif


#ifdef __cplusplus
extern "C"{
#endif
ImageDataColl* GetImageDataColl( int id );
#ifdef __cplusplus
}
#endif

#ifdef __cplusplus
extern "C"{
#endif
void DumpImageDataColl( int id );
#ifdef __cplusplus
}
#endif


void DumpImageDataCollTo( int id, int nImage, char *fnameI, char *fnameZ );

int  ReadRawData( ImageDataColl *imgColl, char* fileNameImage, char* fileNamePos );

#endif /* IMAGING_H__ */

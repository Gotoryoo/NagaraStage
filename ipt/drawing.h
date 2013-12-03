#ifndef drawing_h
#define drawing_h

#if 0
#ifdef _WIN32
extern void NewImage(HWND hWnd, HDC hDC, int Hi, LPBITMAPINFO bmi, unsigned char *Src);
#else
void NewImage(HWND hWnd, HDC hDC, int Hi, LPBITMAPINFO bmi, unsigned char *Src){}
LPBITMAPINFO bmMono;
LPBITMAPINFO bmRB;
LPBITMAPINFO bmgRB;
#endif
#endif

#endif

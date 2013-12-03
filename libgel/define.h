/**
* プリプロセッサによる定義を行うヘッダファイルです．
* このプロジェクト内において，原則#defineはこのファイルでのみでしか使用してはいけません．
* @authro Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
* @date 2012-09-10
*/

#ifndef __DEFINE_H__
#define __DEFINE_H__

#if _WIN32
#define DLLEXPORT __declspec(dllexport)
#define DLLIMPORT __declspec(dllimport)
#else
#define DLLEXPORT
#define DLLIMPORT
#define __stdcall
#endif

#endif /* __DEFINE_H__ */
/*
 *  $Id: mysocklib.cpp,v 1.3 2001/11/21 13:12:26 e373 Exp $
 */

#include <stdio.h>
#include <windows.h>
#include <winsock.h>
#include <iostream>
#include "ipdefine.h"
#include "ipproto.h"



#define SOCK_INI_FILE "StageSocket.ini"

static int SocketStatus = 0;
static SOCKADDR_IN svAddress;
static SOCKET svSocket;

DLLEXPORT int __stdcall IP_ReadSocketIniFile()
{
  FILE *fp;
  char buf[1024];
  char hostname[1024];
  int port;


  if ((fp = fopen(SOCK_INI_FILE, "r")) == NULL)
    return -1;
  while (fgets(buf, sizeof(buf), fp)) {
    if (sscanf(buf, "%1000s %d", hostname, &port) == 2) {
      fclose(fp);
      return IP_InitSocket(hostname, port);
    }
  }
  // Cannot find a hostname and a port number
  fclose(fp);
  return -1;
}

DLLEXPORT int __stdcall IP_InitSocket(char *hostname, int port)
{
  WSADATA Data;
  int result = WSAStartup(MAKEWORD(1, 1), &Data);

  if (WSAStartup(MAKEWORD(1, 1), &Data) != 0) return -1;

  svAddress.sin_family = AF_INET;
  svAddress.sin_port = htons(port);
  u_long svaddr = inet_addr(hostname);
  memcpy(&svAddress.sin_addr, &svaddr, sizeof(u_long));

  SocketStatus = 1;
  return 0;
}

DLLEXPORT int __stdcall IP_ConnectServer(int mestype, int data1, int data2)
{
  char mes[512];

  if (SocketStatus != 1) return SocketStatus;

  svSocket = socket(AF_INET, SOCK_STREAM, 0);
  if (svSocket == INVALID_SOCKET) return -1;

  if (connect(svSocket, (const struct sockaddr *) &svAddress,sizeof(svAddress)) == SOCKET_ERROR) return -1;

  switch (mestype) {
	  case 0:
		sprintf(mes, "Whole scanning End !!\n\0");
		break;
	  case 1:
		sprintf(mes, "Start the scanning for track %d\n\0", data1);
		break;
	  case 2:
		sprintf(mes,
			"End the scanning for this track. %d tracks were found.\n\0",
			data1);
		break;
	  case 3:
		sprintf(mes, "Fault! Identification of the surface.\n\0");
		break;
	  case 4:
		sprintf(mes, "\t Track Found. Cand=%d\n", data1);
		break;
	  case 5:
		sprintf(mes, "Fault! Image is too dark %d!\n\0", data1);
  }

  if (send(svSocket, mes, strlen(mes) + 1, 0) != (signed) strlen(mes) + 1) return -1;

  closesocket(svSocket);

  return 0;
}

DLLEXPORT int __stdcall IP_SendMessage(const char *mes)
{
  SOCKET sock;
  int length = strlen(mes) + 1;

  if (SocketStatus != 1) return SocketStatus;
  sock = socket(AF_INET, SOCK_STREAM, 0);
  if (sock == INVALID_SOCKET) return -1;
  if (connect(sock, (const struct sockaddr *) &svAddress, sizeof(svAddress)) == SOCKET_ERROR) return -1;
  if (send(sock, mes, length, 0) != length) return -1;
  closesocket(sock);

  return 0;
}

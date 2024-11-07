// MrSidLib.cpp : Definiert den Einstiegspunkt für die DLL-Anwendung.
//

//#include "support.h"
#include "stdafx.h"
#include "MrSidLib.h"

// lib/base
#include "lti_pixel.h"
#include "lti_scene.h"
#include "lti_sceneBuffer.h"
#include "lti_navigator.h"
#include "lti_imagestage.h"
#include "lti_utils.h"

// lib/support
#include "lt_base.h"
#include "lt_fileSpec.h" 
#include "lt_status.h"

// lib/MrSIDreaders
#include "MrSIDImageReader.h"
#include "J2KImageReader.h"

// lib/filters
#include "lti_colorTransformer.h"
#include "lti_multiresFilter.h"
#include "lti_viewerImageFilter.h"

#include "lt_ioMemStream.h"
#include "lt_ioSubStream.h"

// MACROS
#define SAFE_DELETE(b) { if(b) { delete (b);  (b)=NULL; } }
#define SAFE_DELETE_ARRAY(b) { if(b) { delete[] (b);  (b)=NULL; } }
#define SAFE_DELETE_OBJECT(b) {if(b) { DeleteObject((b)); (b)=NULL; } }
#define FREE_READER(r) { if(r) { r->release(); (r)=NULL; } }
#define RELEASEALL(r) { if(r) { r->releaseAll(); (r)=NULL; } }

#define TEST_SUCCESS(expression) \
   do { \
      if(!LT_SUCCESS(sts = (expression))) \
         return NULL; \
   } while(0)

#define TEST_FAILURE(expression) \
   do { \
      if(!LT_FAILURE(sts = (expression))) \
         return NULL; \
   } while(0)

#define TEST_BOOL(expression) \
   do { \
      if(!(expression)) \
         return NULL; \
   } while(0)

LT_USE_NAMESPACE(LizardTech);	

#ifdef _MANAGED
#pragma managed(push, off)
#endif

unsigned char* CreateDIB(int numBands,HBITMAP& g_hbmp,int width,int height);

class myBufferData 
{
public:
	/** constructor */
	myBufferData(LTISceneBuffer *bd,void *b) 
	{
		bufferData=bd;
		memBuffer=b;
	}
	/** destructor */
	~myBufferData() {
		this->release();
	};

	void release() {
		SAFE_DELETE(this->memBuffer);
		SAFE_DELETE_OBJECT(this->bufferData);
	}

	LTISceneBuffer *bufferData;
	void *memBuffer;
};

class filterPointer
{
public:
	LTIImageStage* readerInstance = NULL;
	LTIColorTransformer* clrtrans = NULL;
	LTIMultiResFilter* resFilter = NULL;
	LTIViewerImageFilter* viewerFilter = NULL;
	LTIImageStage* reader = NULL;

	~filterPointer() {
		this->releaseAll();
	};

	void releaseAll() {
		FREE_READER(viewerFilter);
		FREE_READER(resFilter);
		FREE_READER(clrtrans);
		FREE_READER(readerInstance);/**/
		
		reader = nullptr;
	}
};

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
    return TRUE;
}

extern "C" __declspec(dllexport) filterPointer *LoadMrSIDReader(char *szFileName,MrSidGeoCoord *mrSidGeoCoord)
{
	LT_STATUS sts = LT_STS_Uninit;
	//LTFileSpec *fileSpec;
	//fileSpec = new LTFileSpec(szFileName);
	const LTFileSpec fileSpec(szFileName);

	// create the image reader
	//LTIImageStage *reader = new MrSIDImageReader(*fileSpec);	
	//sts = reader->initialize();
	//if (!LT_SUCCESS(sts)) return NULL;

	filterPointer* ptr = new filterPointer();

	LTIImageStage* reader = MrSIDImageReader::create();
	TEST_BOOL(reader != nullptr);

	ptr->readerInstance = reader;
	ptr->reader = reader;

	sts = ((MrSIDImageReader*)reader)->initialize(fileSpec, false);
	if (!LT_SUCCESS(sts)) {
		RELEASEALL(ptr);
		return NULL;
	}

	if(mrSidGeoCoord!=NULL) {
		LTIGeoCoord geoCoord=reader->getGeoCoord();
		mrSidGeoCoord->iWidth=reader->getWidth();
		mrSidGeoCoord->iHeight=reader->getHeight();
		mrSidGeoCoord->MinMagnification=reader->getMinMagnification();
		mrSidGeoCoord->MaxMagnification=reader->getMaxMagnification();

		bool found;
		geoCoord.readWorldFile(fileSpec,true,found);
		geoCoord.get(mrSidGeoCoord->X,mrSidGeoCoord->Y,mrSidGeoCoord->xRes,mrSidGeoCoord->yRes,mrSidGeoCoord->xRot,mrSidGeoCoord->yRot);
	}
	//delete fileSpec;

	// convert colorspace to RGB (if possible)
   if( LTI_COLORSPACE_RGB != ptr->readerInstance->getColorSpace())
   {      
	   const LTIPixel ltiPixel(LTI_COLORSPACE_RGB, 3, LTI_DATATYPE_UINT8);
	   ptr->clrtrans = LTIColorTransformer::create();
	   sts = ptr->clrtrans->initialize(ptr->reader, ltiPixel);
	   if (!LT_SUCCESS(sts)) {
		   //RELEASEALL(ptr);
		   //return NULL;
		   FREE_READER(ptr->clrtrans);
	   }
	   else {
		   ptr->reader = ptr->clrtrans;
	   }
   }
     
   // It's possible that the minimum magnification on some images
   // isn't small enough to fit into our client area.
   // for this, we use LTIMultiResFilter to force a smaller magnification
   {
	   ptr->resFilter = LTIMultiResFilter::create();
	   sts = ptr->resFilter->initialize(ptr->reader, 0);

	   if (!LT_SUCCESS(sts)) {
		   RELEASEALL(ptr);
		   return NULL;
	   }
	   ptr->reader = ptr->resFilter;
   }

   // add a viewer filter to the pipeline.  This will do some handy conversions to our images
   // to ensure that we can display them.  (ie. swap the blue and red bands -- RGB to BGR)
   {
	   ptr->viewerFilter = LTIViewerImageFilter::create();
	   sts = ptr->viewerFilter->initialize(ptr->reader, true, true/*, true*/);
	   if (!LT_SUCCESS(sts)) {
		   RELEASEALL(ptr);
		   return NULL;
	   }
	   ptr->reader = ptr->viewerFilter;
   }

	return ptr;
}

//extern "C" __declspec(dllexport) LTIImageStage *LoadMrSIDMemReader(void *data,lt_uint32 data_size,MrSidGeoCoord *mrSidGeoCoord)
//{
//	LT_STATUS sts = LT_STS_Uninit;
//	
//	// create MemStream
//	LTIOMemStream *stream=new LTIOMemStream();
//	stream->initialize(data,data_size);
//	
//	// create the image reader
//	//LTIImageStage *reader = new MrSIDImageReader(stream);	
//	//sts = reader->initialize();
//	//if(!LT_SUCCESS(sts)) return NULL;
//	
//	LTIImageStage *reader = MrSIDImageReader::create();	
//	TEST_BOOL(reader != nullptr);
//	TEST_SUCCESS(reader->initialize(stream));
//
//	if(mrSidGeoCoord!=NULL) {
//		LTIGeoCoord geoCoord=reader->getGeoCoord();
//		mrSidGeoCoord->iWidth=reader->getWidth();
//		mrSidGeoCoord->iHeight=reader->getHeight();
//		mrSidGeoCoord->MinMagnification=reader->getMinMagnification();
//		mrSidGeoCoord->MaxMagnification=reader->getMaxMagnification();
//	
//
//		bool found;
//		//geoCoord.readWorldFile(*fileSpec,true,found);
//		geoCoord.get(mrSidGeoCoord->X,mrSidGeoCoord->Y,mrSidGeoCoord->xRes,mrSidGeoCoord->yRes,mrSidGeoCoord->xRot,mrSidGeoCoord->yRot);
//	}
//	
//	// convert colorspace to RGB
//   if( LTI_COLORSPACE_RGB != reader->getColorSpace())
//   {      
//      LTIColorTransformer *clrtrans = 
//         new LTIColorTransformer( reader, LTI_COLORSPACE_RGB, 3, true );
//      sts = clrtrans->initialize();
//	  if(!LT_SUCCESS(sts)) {
//		  SAFE_DELETE(reader);
//		  return NULL;
//	  }
//      reader = clrtrans;
//   }
//     
//   // It's possible that the minimum magnification on some images
//   // isn't small enough to fit into our client area.
//   // for this, we use LTIMultiResFilter to force a smaller magnification
//   {
//      LTIMultiResFilter *resFilter = new LTIMultiResFilter( reader, true );
//      sts = resFilter->initialize();
//      if(!LT_SUCCESS(sts)) {
//		  SAFE_DELETE(reader);
//		  return NULL;
//	  }
//      reader = resFilter; 
//   }
//
//   // add a viewer filter to the pipeline.  This will do some handy conversions to our images
//   // to ensure that we can display them.  (ie. swap the blue and red bands -- RGB to BGR)
//   {
//      LTIViewerImageFilter *viewerFilter = new LTIViewerImageFilter(reader, true, true, true);
//      sts = viewerFilter->initialize();
//      if(!LT_SUCCESS(sts)) {
//		  SAFE_DELETE(reader);
//		  return NULL;
//	  }
//      reader = viewerFilter;
//   }
//
//	return reader;
//}

extern "C" __declspec(dllexport) filterPointer *LoadJP2Reader(char *szFileName,MrSidGeoCoord *mrSidGeoCoord)
{
	LT_STATUS sts = LT_STS_Uninit;
	//LTFileSpec *fileSpec;
	//fileSpec = new LTFileSpec(szFileName);
	const LTFileSpec fileSpec(szFileName);

	// create the image reader
	//LTIImageStage *reader = new J2KImageReader(*fileSpec, false);	
	//sts = reader->initialize();
	//if(!LT_SUCCESS(sts)) return NULL;

	filterPointer* ptr = new filterPointer();

	LTIImageStage* reader = J2KImageReader::create();
	TEST_BOOL(reader != nullptr);

	ptr->readerInstance = reader;
	ptr->reader = reader;

	sts = ((J2KImageReader*)reader)->initialize(fileSpec, false);
	if (!LT_SUCCESS(sts)) {
		RELEASEALL(ptr);
		return NULL;
	}

	if(mrSidGeoCoord!=NULL) {
		LTIGeoCoord geoCoord=reader->getGeoCoord();
		mrSidGeoCoord->iWidth=reader->getWidth();
		mrSidGeoCoord->iHeight=reader->getHeight();
		mrSidGeoCoord->MinMagnification=reader->getMinMagnification();
		mrSidGeoCoord->MaxMagnification=reader->getMaxMagnification();
	
		bool found;
		geoCoord.readWorldFile(fileSpec,true,found);
		geoCoord.get(mrSidGeoCoord->X,mrSidGeoCoord->Y,mrSidGeoCoord->xRes,mrSidGeoCoord->yRes,mrSidGeoCoord->xRot,mrSidGeoCoord->yRot);
	}
	//delete fileSpec;

	// convert colorspace to RGB (if possible)
    if( LTI_COLORSPACE_RGB != ptr->readerInstance->getColorSpace())
    {      
		const LTIPixel ltiPixel(LTI_COLORSPACE_RGB, 3, LTI_DATATYPE_UINT8);
		ptr->clrtrans = LTIColorTransformer::create();
		sts = ptr->clrtrans->initialize(ptr->reader, ltiPixel);
		if (!LT_SUCCESS(sts)) {
			//RELEASEALL(ptr);
			//return NULL;
			FREE_READER(ptr->clrtrans);
		}
		else {
			ptr->reader = ptr->clrtrans;
		}
    }
     
   // It's possible that the minimum magnification on some images
   // isn't small enough to fit into our client area.
   // for this, we use LTIMultiResFilter to force a smaller magnification
   {
      //LTIMultiResFilter *resFilter = new LTIMultiResFilter( reader, true );
      //sts = resFilter->initialize();

	  ptr->resFilter = LTIMultiResFilter::create();
	  sts = ptr->resFilter->initialize(ptr->reader, 0);

      if(!LT_SUCCESS(sts)) {
		  RELEASEALL(ptr);
		  return NULL;
	  }
      ptr->reader = ptr->resFilter; 
   }

   // add a viewer filter to the pipeline.  This will do some handy conversions to our images
   // to ensure that we can display them.  (ie. swap the blue and red bands -- RGB to BGR)
   {
      ptr->viewerFilter = LTIViewerImageFilter::create();
      sts = ptr->viewerFilter->initialize(ptr->reader, true, true/*, true*/);
      if(!LT_SUCCESS(sts)) {
		  RELEASEALL(ptr);
		  return NULL;
	  }
      ptr->reader = ptr->viewerFilter;
   }

	return ptr;
}

//extern "C" __declspec(dllexport) LTIImageStage *LoadJP2MemReader(void *data,lt_uint32 datasize,MrSidGeoCoord *mrSidGeoCoord) 
//{
//	LT_STATUS sts = LT_STS_Uninit;
//	//LTFileSpec *fileSpec;
//	//fileSpec = new LTFileSpec(szFileName);
//   
//	LTIOMemStream stream;
//	stream.initialize(data, datasize);
//
//	// create the image reader
//	//LTIImageStage *reader = new J2KImageReader(stream, false);	
//	//sts = reader->initialize();
//	//if(!LT_SUCCESS(sts)) return NULL;
//
//	LTIImageStage* reader = J2KImageReader::create();
//	TEST_BOOL(reader != nullptr);
//	TEST_SUCCESS(reader->initialize(stream, false));
//
//	if(mrSidGeoCoord!=NULL) {
//		LTIGeoCoord geoCoord=reader->getGeoCoord();
//		mrSidGeoCoord->iWidth=reader->getWidth();
//		mrSidGeoCoord->iHeight=reader->getHeight();
//		mrSidGeoCoord->MinMagnification=reader->getMinMagnification();
//		mrSidGeoCoord->MaxMagnification=reader->getMaxMagnification();
//	
//		bool found;
//		//geoCoord.readWorldFile(*fileSpec,true,found);
//		geoCoord.get(mrSidGeoCoord->X,mrSidGeoCoord->Y,mrSidGeoCoord->xRes,mrSidGeoCoord->yRes,mrSidGeoCoord->xRot,mrSidGeoCoord->yRot);
//	}
//	//delete fileSpec;
//
//	// convert colorspace to RGB
//    if( LTI_COLORSPACE_RGB != reader->getColorSpace())
//    {      
//      LTIColorTransformer *clrtrans = 
//         new LTIColorTransformer( reader, LTI_COLORSPACE_RGB, 3, true );
//      sts = clrtrans->initialize();
//	  if(!LT_SUCCESS(sts)) {
//		  SAFE_DELETE(reader);
//		  return NULL;
//	  }
//      reader = clrtrans;
//    }
//     
//   // It's possible that the minimum magnification on some images
//   // isn't small enough to fit into our client area.
//   // for this, we use LTIMultiResFilter to force a smaller magnification
//   {
//      LTIMultiResFilter *resFilter = new LTIMultiResFilter( reader, true );
//      sts = resFilter->initialize();
//      if(!LT_SUCCESS(sts)) {
//		  SAFE_DELETE(reader);
//		  return NULL;
//	  }
//      reader = resFilter; 
//   }
//
//   // add a viewer filter to the pipeline.  This will do some handy conversions to our images
//   // to ensure that we can display them.  (ie. swap the blue and red bands -- RGB to BGR)
//   {
//      LTIViewerImageFilter *viewerFilter = new LTIViewerImageFilter(reader, true, true, true);
//      sts = viewerFilter->initialize();
//      if(!LT_SUCCESS(sts)) {
//		  SAFE_DELETE(reader);
//		  return NULL;
//	  }
//      reader = viewerFilter;
//   }
//
//	return reader;
//}

extern "C"  __declspec(dllexport) void FreeReader(filterPointer* ptr)
{
	RELEASEALL(ptr);
}

extern "C" __declspec(dllexport) HBITMAP ReadHBitmap(filterPointer * ptr,int x,int y,int width,int height,double mag)
{
	if(ptr==NULL || ptr->reader==NULL) return NULL;
	LTIImageStage* reader = ptr->reader;

	LTINavigator navy(*reader);	
	navy.setSceneAsULLR(x,y,x+width,y+height,mag);
	
	LTIScene scene=navy.getScene();
	//LTIScene scene(X1,Y1,X2*mag,Y2*mag,mag);

	LT_STATUS sts = LT_STS_Uninit; 

	// immer ein bisserl mehr Speicher reservieren als notwendig... 
	// es kann sonst zu einen schreiben von daten in den geschützten Speicher kommen...
	const lt_uint32 siz = scene.getNumCols() * scene.getNumRows() * 1 + /* mehr zur sicherheit */
		                  (scene.getNumCols() + scene.getNumRows()) * 16;

    lt_uint8* membuf = new lt_uint8[siz*3];
    void* bufs[3] = { membuf+siz*0, membuf+siz*1, membuf+siz*2 };

	LTISceneBuffer bufferData(reader->getPixelProps(),
                              scene.getNumCols(),
                              scene.getNumRows(),
                              bufs);

	sts = reader->read(scene, bufferData);
	if(!LT_SUCCESS(sts)) {
		SAFE_DELETE(membuf);
		return NULL;
	}
	HBITMAP hbmp;
	unsigned char* bufAddr = CreateDIB(reader->getNumBands(),
		                               hbmp,
									   bufferData.getTotalNumCols(),
									   bufferData.getTotalNumRows());
									   //scene.getNumCols(),
									   //scene.getNumRows());
	if(bufAddr==NULL) {
		SAFE_DELETE(membuf);
		return NULL;
	}
	const lt_uint32 pixelBytes = reader->getPixelProps().getNumBytes();
    const lt_uint32 rowBytesBase = /*scene.getNumCols()*/ bufferData.getTotalNumCols() * pixelBytes;
    const lt_uint32 rowBytesAligned = LTISceneBuffer::addAlignment(rowBytesBase, 4);

	sts = bufferData.exportData(bufAddr,
                                pixelBytes,
                                rowBytesAligned,
                                1);

	SAFE_DELETE(membuf);
	if(!LT_SUCCESS(sts)) return NULL;
	
	return hbmp;
}

extern "C" __declspec(dllexport) myBufferData* Read(filterPointer *ptr,int x,int y,int width,int height,double mag) 
{
	if (ptr == NULL || ptr->reader == NULL) return NULL;
	LTIImageStage* reader = ptr->reader;

	LTINavigator navy(*reader);	
	navy.setSceneAsULLR(x,y,x+width,y+height,mag);
	
	LTIScene scene=navy.getScene();

	LT_STATUS sts = LT_STS_Uninit; 

	// immer ein bisserl mehr Speicher reservieren als notwendig... 
	// es kann sonst zu einen schreiben von daten in den geschützten Speicher kommen...
	const lt_uint32 siz = scene.getNumCols() * scene.getNumRows() * 1 + /* mehr zur sicherheit */
		                  (scene.getNumCols() + scene.getNumRows())*2;

    lt_uint8* membuf = new lt_uint8[siz*3];
    void* bufs[3] = { membuf+siz*0, membuf+siz*1, membuf+siz*2 };

	LTISceneBuffer *bufferData=new LTISceneBuffer(reader->getPixelProps(),
												  scene.getNumCols(),
												  scene.getNumRows(),
											      bufs);

	sts = reader->read(scene, *bufferData);
	if (!LT_SUCCESS(sts)) {
		SAFE_DELETE(membuf);
		SAFE_DELETE_OBJECT(bufferData);

		return NULL;
	}

	return new myBufferData(bufferData,(void *)membuf);
}

extern "C" __declspec(dllexport) lt_int16 GetNumBand(myBufferData *bufferData)
{
	if(bufferData==NULL) return -1;
	return bufferData->bufferData->getNumBands();
}
extern "C" __declspec(dllexport) void *GetBandData(myBufferData *bufferData, lt_int16 band) 
{
	if(bufferData==NULL) return NULL;
	if(bufferData->bufferData->getNumBands()>= band || band<0) return NULL;

	return bufferData->bufferData->getWindowBandData(band);
}

extern "C" __declspec(dllexport) lt_int32 GetTotalRows(myBufferData *bufferData) 
{
	if(bufferData==NULL) return 0;
	return bufferData->bufferData->getTotalNumRows();
}

extern "C" __declspec(dllexport) lt_int32 GetTotalCols(myBufferData *bufferData) 
{
	if(bufferData==NULL) return 0;
	return bufferData->bufferData->getTotalNumCols();
}

extern "C" __declspec(dllexport) void ReadBandData(myBufferData *bufferData, void *data, lt_uint32 pixelBytes, lt_uint32 rowBytes) 
{
	if(bufferData==NULL || bufferData->bufferData==NULL) return;

	bufferData->bufferData->exportData(data,
		pixelBytes,
		rowBytes,
		1);
}

extern "C" __declspec(dllexport) void ReleaseBandData(myBufferData *bufferData) 
{
	if(bufferData==NULL) return;

	bufferData->release();
	SAFE_DELETE_OBJECT(bufferData);
}
extern "C" __declspec(dllexport) void ReleaseHBitmap(HBITMAP hbmp) 
{
	SAFE_DELETE_OBJECT(hbmp);
}

unsigned char* CreateDIB(int numBands,HBITMAP& g_hbmp,int width,int height)
{
   unsigned char* bufAddr = NULL;

	if (numBands == 3)
	{
		BITMAPINFO *g_bmpInfo = new BITMAPINFO;

		//	color image - we set the fields of the m_colorBmpInfo
		//	which describes our bitmap
		g_bmpInfo->bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
		g_bmpInfo->bmiHeader.biWidth = width;/*g_rect.right;*/
		g_bmpInfo->bmiHeader.biHeight = height;/*-g_rect.bottom;*/	//	 negative indicates a top-down image
		g_bmpInfo->bmiHeader.biPlanes = 1;
		g_bmpInfo->bmiHeader.biBitCount = 24;
		g_bmpInfo->bmiHeader.biCompression = BI_RGB;
		g_bmpInfo->bmiHeader.biSizeImage = 0;
		g_bmpInfo->bmiHeader.biXPelsPerMeter = 0;
		g_bmpInfo->bmiHeader.biYPelsPerMeter = 0;
		g_bmpInfo->bmiHeader.biClrUsed = 0;
		g_bmpInfo->bmiHeader.biClrImportant = 0;

		//	create the bitmap - this allocates memory for the pixels, etc
		g_hbmp = CreateDIBSection(	NULL,
									      g_bmpInfo,
									      DIB_RGB_COLORS,
									      (void**)(&bufAddr),
									      0,
									      0L );
	}
   else
   {
      // shouldn't happen, should have gone to an RGB converter
      return NULL;
   }
   return bufAddr;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif



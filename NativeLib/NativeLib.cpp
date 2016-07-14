// NativeLib.cpp : Defines the exported functions for the DLL application.
//


#include "stdafx.h"

#include <stdio.h>



//#include "hdf5_hl.h"

#include "caffe\caffe.hpp"

#ifdef TEST_EXE
#include <Windows.h>
#endif

using namespace caffe;

Net<float>* classNet;

extern "C" __declspec(dllexport) int test_native(int i)
{
	Caffe::set_mode(Caffe::CPU);

    classNet = new Net<float>("vae.prototxt", TRAIN);
	int shape = classNet->blob_by_name("mu")->shape()[0];


	//printf("Hello native: %s!\r\n", classNet->blob_names()[0]);
	printf("Hello native!\r\n");

	return shape;
}

#ifdef TEST_EXE
int main()
{
	int k = test_native(42);

	printf("Hello native %d!\r\n", k);	
	getchar();
}
#endif



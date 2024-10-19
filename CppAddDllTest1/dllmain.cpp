// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include <vector>
#include <string>
#include <fstream>
#include <iostream>
#include <wchar.h>

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}


extern "C" __declspec(dllexport) int Add(int a, int b)
{
    return a + b;
}

// 存储图片路径的全局变量
std::string g_imagePath;

struct ProcessResult
{
    bool success; // 处理结果
    int length; //图像的大小
    unsigned char* imageData; // 存储图像数据的指针
    char* imageFormat;// 图像格式的指针（例如："PNG", "JPEG"）
    unsigned char* metadata; // 其他相关元数据的指针
};

// 回调函数类型定义
typedef void (*ImageDataCallback)(const ProcessResult* result);

// 接受图片路径
extern "C" __declspec(dllexport) void SetImagePath(const char* imagePath)
{
    g_imagePath = std::string(imagePath);  // 保存图片路径
}

// 从文件中读取图像数据，并通过回调传递给 C#
extern "C" __declspec(dllexport) void ProcessImageFromMemory(ImageDataCallback callback)
{
    // 假设在这里处理图片，我们将其读取为二进制数据
    std::ifstream file(g_imagePath, std::ios::binary | std::ios::ate);
    ProcessResult result;
    
    if (file)
    {
        std::streamsize fileSize = file.tellg();
        file.seekg(0, std::ios::beg);

        result.imageData = new unsigned char[fileSize];
        std::vector<unsigned char> imageDataVector(fileSize);
        //if (file.read((char*)imageData.data(), fileSize))
        //{
        //    // 处理完图片后通过回调函数将数据传递回 C#
        //    callback(imageData.data(), static_cast<int>(imageData.size()));
        //}

        if (file.read((char*)result.imageData, fileSize))
        {
            result.success = true; // 图片成功读取
            result.length = static_cast<int>(imageDataVector.size());//图片的大小
                        // 分配并设置图像格式
            const char* format = "PNG"; // 假设格式
            //const wchar_t* format = L"PNG";
            //result.imageFormat = new wchar_t[wcslen(format) + 1];
            result.imageFormat = new char[strlen(format) + 1];
            //result.imageFormat = L"PNG";
           
            strcpy_s(result.imageFormat, strlen(format) + 1, format);
            //wcsncpy_s(result.imageFormat, sizeof(result.imageFormat)/2, format, _TRUNCATE);



            // 分配并设置其他元数据（如果有的话）
            // 这里可以根据实际需求进行分配
            result.metadata = nullptr; // 或者其他元数据

            callback(&result); // 通过回调函数传递结果
        }
        else
        {
            result.success = false; // 读取失败
            result.imageData = nullptr;
            //result.imageFormat = nullptr; // 读取失败时，设置为 nullptr
            callback(&result);
        }

        file.close();
        delete[] result.imageData;     // 释放图像数据的内存
        //delete[] result.imageFormat;    // 释放图像格式的内存
        // 释放其他元数据的内存
        if (result.metadata)
        {
            delete[] result.metadata;
        }

    }
    else
    {
        std::cerr << "Error: Unable to open image file at " << g_imagePath << std::endl;
        result.success = false; // 文件打开失败
        result.imageData = nullptr;
        //result.imageFormat = nullptr; // 失败时设置为 nullptr
        callback(&result); // 传递失败结果
    }
}


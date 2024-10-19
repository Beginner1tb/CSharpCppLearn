// CppConsoleCallBackTest1.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include <functional>

//Modern Usage
void doTask(std::function<void(int)> callback) {
    std::cout << "Task is running..." << std::endl;
    callback(42);
}


int main()
{
    auto myCallback = [](int result) {
        std::cout << "Lambda callback executed with result: " << result << std::endl;
    };

    doTask(myCallback);
    return 0;
    std::cout << "Hello World!\n";
}


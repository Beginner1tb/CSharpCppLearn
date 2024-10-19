using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace WpfCppTest
{

    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessResult
    {
        
        public bool success;      // 处理结果
        public int length;         // 图像的大小
        public IntPtr imageData;  // 存储图像数据的指针
        
        public IntPtr imageFormat; // 图像格式的指针
        public IntPtr metadata;    // 其他相关元数据的指针
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ImageDataCallback(ref ProcessResult result);

        [DllImport("CppAddDllTest1.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Add(int a, int b);

        // DLL 导入函数，第一步：将图片路径传递给 DLL，DLL 进行图片处理
        [DllImport("CppAddDllTest1.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetImagePath([MarshalAs(UnmanagedType.LPStr)] string imagePath);

        // DLL 导入函数，第二步：处理图片并将处理后的图像通过回调函数传回 C#
        [DllImport("CppAddDllTest1.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ProcessImageFromMemory(ImageDataCallback callback);
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Add(1, 2).ToString());
        }

        private void Btn_LoadImg_Click(object sender, RoutedEventArgs e)
        {
            // 假设这里通过 WPF TextBox 或 OpenFileDialog 获得了图片路径
            string imagePath = TB1.Text;  // 实际中应根据 UI 选择

            // 第一步：将图片路径传递给 C++ DLL
            SetImagePath(imagePath);

            // 第二步：处理图片并通过回调接收内存中的二进制图像数据
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ProcessImageFromMemory(OnImageDataReceived);
            stopwatch.Stop();
            Debug.WriteLine("Cpp Elapsed Time: {0} ms", stopwatch.ElapsedMilliseconds);

            stopwatch.Start();
            ShowImageByCSharp(imagePath);
            stopwatch.Stop();
            Debug.WriteLine("CSharp Elapsed Time: {0} ms", stopwatch.ElapsedMilliseconds);

        }

        private void ShowImageByCSharp(string filePath)
        {
            Img.Dispatcher.Invoke(() =>
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.EndInit();
                Img.Source = bitmap;
            });
            
        }

        private void OnImageDataReceived(ref ProcessResult result)
        {

            if (!result.success)
            {
                Console.WriteLine("Image processing failed.");
                return; // 处理失败，直接返回
            }

            byte[] imageData = new byte[result.length];
            Marshal.Copy(result.imageData, imageData, 0, result.length);

            string imageFormat = Marshal.PtrToStringAnsi(result.imageFormat);
            //string imageFormat = result.imageFormat;

            // 将二进制数据转化为图片并显示
            Img.Dispatcher.Invoke(() =>
            {
                using (var stream = new MemoryStream(imageData))
                {
                    stream.Position = 0; // 重置流的位置
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // 加载时缓存
                    bitmap.EndInit();
                    bitmap.Freeze(); // 冻结以提高性能

                    // 显示在 Image 控件中
                    Img.Source = bitmap;
                }
            });
            MessageBox.Show(imageFormat.ToString());

            // 处理其他元数据，如果有的话
            if (result.metadata != IntPtr.Zero)
            {
                // 根据需要处理元数据
            }
        }
    }
}

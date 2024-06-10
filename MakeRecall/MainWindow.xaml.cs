using ABI.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MakeRecall
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_CLOSE = 0x0010;

        private const int SW_MAXIMIZE = 3;

        List<string> urls = new List<string>();

        private int current_url_index = 0;

        private Process run_process = null;

        private bool running_edge = false;



        DispatcherTimer timer;
        public MainWindow()
        {
            urls.Add(@"https://www.bbc.com");
            urls.Add(@"https://www.cnn.com");

            this.InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            current_url_index = 0;
            this.AppWindow.Hide();
            Thread.Sleep(1000);
            RunProcesswithUrl(urls[current_url_index]);
            timer = new DispatcherTimer();
            timer.Interval = System.TimeSpan.FromSeconds(30);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            current_url_index++;
            if (run_process != null)
            {
                //if (running_edge)
                //{
                //    KillEdge();
                //    running_edge = false;
                //}
                //else
                //{
                //    KillProcessAndChildren(run_process.Id);
                //    Thread.Sleep(1000);
                //}
                KillTopWindow();
                Thread.Sleep(1000);

            }
            if (current_url_index > urls.Count)
            {
                timer.Stop();
                this.AppWindow.Show();
                return;
            }
            else if (current_url_index == urls.Count)
            {
                RunPaint("1.jpg");
                return;
            }
            RunProcesswithUrl(urls[current_url_index]);
        }

        private void RunProcesswithUrl(string url)
        {
            // Start the browser process independently
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe", // 원하는 브라우저의 실행 파일 이름
                Arguments = url,
                UseShellExecute = true
            };

            run_process = Process.Start(psi);

            // Wait for the process to be ready
            Thread.Sleep(5000); // 웹 브라우저가 완전히 로드될 때까지 대기
            running_edge = true;

            if (run_process != null)
            {
                IntPtr hWnd = run_process.MainWindowHandle;

                // Retry to get the MainWindowHandle if it's not available yet
                for (int i = 0; i < 10 && hWnd == IntPtr.Zero; i++)
                {
                    Thread.Sleep(500); // 잠시 대기
                    run_process.Refresh(); // 프로세스 정보를 새로 고침
                    hWnd = run_process.MainWindowHandle;
                }

                if (hWnd != IntPtr.Zero)
                {
                    // Maximize the window
                    ShowWindowAsync(hWnd, SW_MAXIMIZE);

                    // Set the window as foreground
                    SetForegroundWindow(hWnd);
                }
            }

        }

        private void RunPaint(string img)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "mspaint.exe", // 원하는 실행 파일 이름
                //Arguments = "E:\\images\\1.jpg",
                UseShellExecute = true
            };

            run_process = Process.Start(psi);

            // Wait for the process to be ready
            Thread.Sleep(5000); // 웹 브라우저가 완전히 로드될 때까지 대기

            if (run_process != null)
            {
                IntPtr hWnd = run_process.MainWindowHandle;

                // Retry to get the MainWindowHandle if it's not available yet
                for (int i = 0; i < 10 && hWnd == IntPtr.Zero; i++)
                {
                    Thread.Sleep(500); // 잠시 대기
                    run_process.Refresh(); // 프로세스 정보를 새로 고침
                    hWnd = run_process.MainWindowHandle;
                }

                if (hWnd != IntPtr.Zero)
                {
                    // Maximize the window
                    ShowWindowAsync(hWnd, SW_MAXIMIZE);

                    // Set the window as foreground
                    SetForegroundWindow(hWnd);
                }
            }
        }

        private static void KillProcessAndChildren(int pid)
        {
            // Create a ProcessStartInfo for 'taskkill' command
            var psi = new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/PID {pid} /T /F",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            var process = new Process
            {
                StartInfo = psi
            };

            process.Start();
            process.WaitForExit();
        }

        private static void KillEdge()
        {
            var processes = Process.GetProcessesByName("msedge");
            //var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                process.Kill();
            }
        }

        private void KillTopWindow()
        {
            // 현재 최상위 창 핸들 가져오기
            IntPtr hWnd = GetForegroundWindow();

            if (hWnd == IntPtr.Zero)
            {
                Debug.WriteLine("No foreground window found.");
                return;
            }

            // 창 제목 가져오기 (선택 사항, 디버깅 목적으로)
            const int nChars = 256;
            System.Text.StringBuilder Buff = new System.Text.StringBuilder(nChars);
            if (GetWindowText(hWnd, Buff, nChars) > 0)
            {
                Debug.WriteLine("Foreground window title: " + Buff.ToString());
            }

            // 창 닫기
            PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            Debug.WriteLine("Foreground window has been closed.");
        }
    }
}

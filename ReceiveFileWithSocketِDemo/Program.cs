// ایجاد سوکت
using System.Net.Sockets;
using System.Net;
using System.Text;

Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

// تعریف آدرس سرور
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
Console.WriteLine(IPAddress.Any);
int port = 8888;
IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

// برقراری اتصال
serverSocket.Bind(localEndPoint);
serverSocket.Listen(1);

while (true)
{
    // در انتظار برقراری اتصال
    Console.WriteLine("Waiting for a connection...");
    Socket clientSocket = serverSocket.Accept();

    try
    {
        // ارتباط برقرار شده و اطلاعات برقرار کننده در دسترس است
        Console.WriteLine("Connection from " + clientSocket.RemoteEndPoint);

        // دریافت نام فایل
        byte[] fileNameLengthBytes = new byte[4];
        clientSocket.Receive(fileNameLengthBytes);
        int fileNameLength = BitConverter.ToInt32(fileNameLengthBytes, 0);

        byte[] fileNameBytes = new byte[fileNameLength];
        clientSocket.Receive(fileNameBytes);
        string fileName = Encoding.UTF8.GetString(fileNameBytes);

        // ایجاد فایل برای تزریق بایت ها
        FileStream fileStream = new FileStream(fileName, FileMode.Create);

        int i = 1;
        // دریافت فایل در پکت های 1 مگابایتی
        byte[] buffer = new byte[1048576];
        int bytesRead;
        while ((bytesRead = clientSocket.Receive(buffer)) > 0)
        {
            fileStream.Write(buffer, 0, bytesRead);
            Console.WriteLine("Received Part " + i);
            i++;
        }

        // پایان دریافت فایل - بستن کانکشن و فایل
        fileStream.Close();
        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
        Console.WriteLine("File received and connection closed.");
    }
    catch (Exception ex)
    {
        // بستن کانکشن در صورت بروز خطا
        clientSocket.Close();
        Console.WriteLine("Error receiving file: " + ex.Message);
    }
}
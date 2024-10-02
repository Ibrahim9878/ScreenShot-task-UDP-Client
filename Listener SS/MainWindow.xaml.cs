using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Listener_SS;

public partial class MainWindow : Window
{
    string path = "image.png";
    UdpClient client = new(27001);
    IPEndPoint iPEndPoint = new(IPAddress.Any, 0);
    private string imagePath;
    public string ImagePath { get => imagePath; set { imagePath = value; OnPropertyChanged(); } }
    DispatcherTimer dispatcher;
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public MainWindow()
    {

        InitializeComponent();
        DataContext = this;
        var ByteArray = new List<byte>();
        dispatcher = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(0.5)
        };
        dispatcher.Tick += ChangeImage;
        dispatcher.Start();
        var Any = Task.Run(() =>
        {
            byte end = 0x00;
            try
            {
                path = $"{Guid.NewGuid()}.jpeg";
                while (true)
                {
                    var bytes = client.Receive(ref iPEndPoint);
                    if (bytes.Length == 1 && bytes[0] == end)
                    {
                        using var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                        fileStream.Write(ByteArray.ToArray(), 0, ByteArray.Count);
                        ByteArray.Clear();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            img.Source = new BitmapImage(new Uri(Path.GetFullPath(path), UriKind.RelativeOrAbsolute));
                        });
                    }
                    else
                        ByteArray.AddRange(bytes);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        });

    }
    public BitmapImage ByteArrayToImageSource(byte[] byteArray)
    {
        using (var stream = new MemoryStream(byteArray))
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
    private void ChangeImage(object? sender, EventArgs e)
    {
    }

    

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var client = new UdpClient();
            var EndPoint = new IPEndPoint(IPAddress.Loopback, 27000);
            var bytes = Encoding.UTF8.GetBytes("Start");
            client.Send(bytes, bytes.Length, EndPoint);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

}
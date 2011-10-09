using System.ServiceModel;
using System.Windows;
using Reverse;

namespace ClientGUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        //"net.tcp://accountname.cloudapp.net:PORT/SERVICENAME";
        //se l'applicazione è sulla cloud bisogna utilizzare la stringa di sopra
        private const string endpointIP = "127.0.0.1";
        private const string endpointPort = "12000";
        private const string serviceName = "Reverse";
        private const string connectionString = "net.tcp://" + endpointIP + ":" + endpointPort + "/" + serviceName;
        private readonly ChannelFactory<IReverse> cfactory;
        private readonly IReverse serviceProvider;

        public MainWindow() {
            InitializeComponent();

            cfactory = new ChannelFactory<IReverse>(new NetTcpBinding(SecurityMode.None), connectionString);
            serviceProvider = cfactory.CreateChannel();
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            if (inputString.Text != "")
                outputString.Text = serviceProvider.ReverseString(inputString.Text);
        }

        private void button2_Click(object sender, RoutedEventArgs e) {
            ((IClientChannel) serviceProvider).Close();
            Application.Current.Shutdown();
        }

        private void button3_Click(object sender, RoutedEventArgs e) {
            inputString.Text = outputString.Text = "";
        }
    }
}

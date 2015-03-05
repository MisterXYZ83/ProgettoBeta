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
using System.IO;
using System.Data.SqlClient;


namespace EstrattoContoOCR
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        String mFilePath;
        OCRWindow mOcrWindow;

        public MainWindow()
        {
            InitializeComponent();

            mStatusBarMessage.Text = "Nessun file selezionato";

            mOcrWindow = new OCRWindow();

            this.Closing += MainWindow_Closing;

            mOcrWindow.Hide();
        }

        private void FileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|PDF Files (*.pdf)|*.pdf";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document 
                mFilePath = dlg.FileName;

                mOcrWindow.SetImagePath(mFilePath);

                mStatusBarMessage.Text = mFilePath + " selezionato";
            }
        }

        private void ScannerMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProvaMenuItem_Click(object sender, RoutedEventArgs e)
        {
            
        }


        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }

        private void ElaboraMenuItem_Click(object sender, RoutedEventArgs e)
        {
        
            mOcrWindow.Show();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show("Uscire?", "Conferma", MessageBoxButton.YesNo);
            
            if (dialogResult == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}

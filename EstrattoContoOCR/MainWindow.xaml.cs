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

        private System.Threading.CancellationTokenSource cancSource;

        public MainWindow()
        {
            InitializeComponent();

            mStatusBarMessage.Text = "Nessun file selezionato";

            cancSource = new System.Threading.CancellationTokenSource();
           
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

                bool ret = mOcrWindow.SetImagePath(mFilePath);
               
                if ( ret )
                {
                    mStatusBarMessage.Text = mFilePath + " selezionato";
                }
                else
                {
                    mFilePath = null;
                }
                
            }
        }


        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }

        private void ElaboraMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ( mOcrWindow.OCRWindowReady )
            {
                mOcrWindow.Show();
            }
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

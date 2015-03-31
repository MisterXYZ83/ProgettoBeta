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
using System.Windows.Shapes;

namespace EstrattoContoOCR
{
    /// <summary>
    /// Interaction logic for PDFPageSelector.xaml
    /// </summary>
    public partial class PDFPageSelector : Window
    {
        OCRWindow mManager;
        int mNumPages;
        int mSelectedPage;

        public PDFPageSelector()
        {
            InitializeComponent();

            mSelectedPage = 0;
            mNumPages = 0;
        }

        public OCRWindow PageManager
        {
            set { mManager = value; }
        }

        public int NumPages
        {
            set 
            {
                if ( value > 0 )
                {
                    mNumPages = value;

                    //aggiorno
                    for ( int k = 0 ; k < mNumPages ; k++ )
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = "Pagina " + (k+1);
                        item.Tag = k;
                        cbPage.Items.Add(item);
                    }
                }
            }
        }

        public int SelectedPage
        {
            get
            {
                return mSelectedPage;
            }
        }

        private void Page_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ( cbPage.SelectedIndex != -1 )
            {
                ComboBoxItem item = (ComboBoxItem)cbPage.SelectedItem;

                if (item == null) return;
                
                int index = (int)item.Tag;

                if (mManager != null)
                {
                    mSelectedPage = index+1;
                }
            }
        }

        public void Confirm_Click(object sender, RoutedEventArgs e)
        {
            //notifico ed esco
            Close();
        }
    }
}

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
    /// Interaction logic for EditRecognizedAreaDialogBox.xaml
    /// </summary>
    public partial class EditRecognizedAreaDialogBox : Window
    {

        RecognizedArea mParentArea;

        public EditRecognizedAreaDialogBox(RecognizedArea area)
        {
            InitializeComponent();

            mParentArea = area;

        }

        public void SetOldText ( string old )
        {
            tbOldText.Text = String.Copy(old);
        }

        public void Confirm_Click(object sender, RoutedEventArgs e)
        {
            //salvo il valore
            string old_val = mParentArea.RecognizedData;
            string new_val = tbNewText.Text;

            string show_val = (new_val.Trim().Equals(string.Empty)) ? "(vuoto)" : new_val;

            MessageBoxResult res = MessageBox.Show("Confermi la modifica da \"" + old_val + "\" in \"" + show_val + "\" ?", "Conferma?", MessageBoxButton.YesNo);

            if ( res == MessageBoxResult.Yes )
            {
                mParentArea.RecognizedData = new_val;

                tbOldText.Text = new_val;
            }
        }
    }
}

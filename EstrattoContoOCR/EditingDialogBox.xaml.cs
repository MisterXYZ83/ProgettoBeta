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
    /// Interaction logic for EditingDialogBox.xaml
    /// </summary>
    /// 

    public enum EditingToolType
    {
        Pencil = 0,
        Rubber = 1,
    };
    
    public enum EditingToolThickness
    {
        Small = 5,
        Medium = 10,
        Big = 20
    };


    public partial class EditingDialogBox : Window
    {
        private EditingToolThickness mActualThickness;
        private EditingToolType mActualToolType;

        private IEditingToolDialogDelegate mEditingDelegate;

        private bool mActive;

        private SolidColorBrush mPencilColor;
        private SolidColorBrush mRubberColor;

        private int mCorrectionCounter;

        public EditingDialogBox(IEditingToolDialogDelegate del)
        {
            InitializeComponent();

            mEditingDelegate = del;

            this.Closing += EditingDialogBox_Closing;

            //aggiorno

            mActualToolType = EditingToolType.Rubber;
            mActualThickness = EditingToolThickness.Medium;

            cbBrush.SelectedIndex = 1;
            cbTick.SelectedIndex = 1;

            btToggle.Content = "Attiva";

            mActive = false;

            mPencilColor = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));           //nero
            mRubberColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));     // bianco

            mCorrectionCounter = 0;
        }

        public Brush PencilBrush
        {
            get
            {
                return mPencilColor;
            }
        }

        public Brush RubberBrush
        {
            get
            {
                return mRubberColor;
            }
        }

        public Brush CurrentBrush
        {
            get
            {
                if (mActualToolType == EditingToolType.Pencil) return mPencilColor;
                else return mRubberColor;
            }
        }

        void EditingDialogBox_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            this.Hide();
        }


        public EditingToolThickness ToolThickness
        {
            get
            {
                return mActualThickness;
            }
        }

        public EditingToolType ToolType
        {
            get
            {
                return mActualToolType;
            }
        }

        public bool EditingActive
        {
            get
            {
                return mActive;
            }
        }

        public void DrawToggle_Click(object sender, RoutedEventArgs e)
        {
            if ( mActive )
            {
                //tool attivo
                //segnalo la disattivazione
                mActive = false;

                if (mEditingDelegate != null) mEditingDelegate.EditingToggle(mActive);

                btToggle.Content = "Attiva";

                cbBrush.IsEnabled = true;
                cbCorrection.IsEnabled = true;
                cbTick.IsEnabled = true;
                btRemoveAll.IsEnabled = true;
                btUndo.IsEnabled = true;
                btSave.IsEnabled = true;
            }
            else
            {
                //tool disattivato, attivo

                if (mEditingDelegate != null)
                {
                    mActive = true;
                    mEditingDelegate.EditingToggle(mActive);

                    btToggle.Content = "Disattiva";

                    cbBrush.IsEnabled = false;
                    cbCorrection.IsEnabled = false;
                    cbTick.IsEnabled = false;
                    btRemoveAll.IsEnabled = false;
                    btUndo.IsEnabled = false;
                    btSave.IsEnabled = false;
                }
            }
        }

        private void Pencil_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = sender as ComboBox;

            int sel_item = cbox.SelectedIndex;

            if ( sel_item >= 0 )
            {
                if (0 == sel_item)
                {
                    mActualToolType = EditingToolType.Rubber;
                }

                if (1 == sel_item)
                {
                    mActualToolType = EditingToolType.Pencil;
                }
            }

            //notifico 
            if ( mEditingDelegate != null )
            {
                mEditingDelegate.EditingChangeToolType(mActualToolType);
            }
        }

        private void Thick_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = sender as ComboBox;

            int sel_item = cbox.SelectedIndex;
            
            if (sel_item >= 0)
            {
                if (0 == sel_item) mActualThickness = EditingToolThickness.Small;
                if (1 == sel_item) mActualThickness = EditingToolThickness.Medium;
                if (2 == sel_item) mActualThickness = EditingToolThickness.Big;
            }

            //notifico 
            if (mEditingDelegate != null)
            {
                mEditingDelegate.EditingChangeToolTickness(mActualThickness);
            }
        }

        public void DrawSave_Click(object sender, RoutedEventArgs e)
        {
            //salvo su file tutte le modifiche
        }

        public void DrawUndo_Click(object sender, RoutedEventArgs e)
        {
            //annullo la selezionata
            if ( cbCorrection.SelectedIndex != -1 )
            {
                ComboBoxItem item = (ComboBoxItem)cbCorrection.SelectedItem;

                int index = (int)item.Tag;

                MessageBoxResult result = MessageBox.Show("Elimini la correzione [" + index + "] ?", "Conferma", MessageBoxButton.YesNo);

                if ( result == MessageBoxResult.Yes)
                {
                    mEditingDelegate.EditingUndoCorrection(index);
                }
            }
        }

        public void DrawUndoAll_Click(object sender, RoutedEventArgs e)
        {
            //annullo tutto
        }

        private void Correction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //seleziono un gruppo di disegno
            ComboBoxItem item = (ComboBoxItem)cbCorrection.SelectedItem;

            if (item == null) return; //cancellazione di un elemento
            int index = (int)item.Tag;

            if ( mEditingDelegate != null)
            {
                mEditingDelegate.EditingSelectCorrection(index);
            }

        }

        public int AddCorrection (int num_op)
        {
            ComboBoxItem item = new ComboBoxItem();
            item.Content = "Correzione[" + mCorrectionCounter + "] (" + num_op.ToString() + ")";

            int ret = mCorrectionCounter;

            item.Tag = mCorrectionCounter++;

            cbCorrection.Items.Add(item);

            return ret;
        }

        public void RemoveCorrection ( int idx )
        {
            
            //rimuovo dalla lista
            ComboBoxItem to_be_removed = null;
            int item_idx = -1;

            foreach ( ComboBoxItem item in cbCorrection.Items )
            {
                item_idx = (int)item.Tag;

                if (item_idx == idx)
                {
                    to_be_removed = item;
                    break;
                }
            }

            if ( to_be_removed != null)
            {
                cbCorrection.Items.Remove(to_be_removed); 
            }
        }
    }
}

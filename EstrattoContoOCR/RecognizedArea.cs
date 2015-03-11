using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

using Tesseract;

namespace EstrattoContoOCR
{

    public class RecognizedArea
    {

        private Tesseract.Rect mAreaRect;
        private string mText;

        private Rectangle mShapeArea;

        private Canvas mParentCanvas;

        private SelectionArea mParentArea;

        private ContextMenu mOptionsMenu;

        private SolidColorBrush mSelectedColorFill;
        private SolidColorBrush mUnselectedColorFill;

        private Boolean mActive;

        public Boolean Active
        {
            get
            {
                return mActive;
            }

            set
            {
                mActive = value;
            }
        }

        public Tesseract.Rect AreaRect
        {
            get { return mAreaRect; }
        }


        public string RecognizedData
        {
            get { return string.Copy(mText); }

            set
            { 
                mText = null;
                mText = String.Copy(value);
            }
        }

        public RecognizedArea ( SelectionArea parea, Tesseract.Rect rct, string text, float conf )
        {
            mParentArea = parea;

            mShapeArea = new Rectangle();

            SolidColorBrush lineColor = new SolidColorBrush();
            lineColor.Color = Color.FromArgb(255, 0, 0, 0);

            mSelectedColorFill = new SolidColorBrush();
            mSelectedColorFill.Color = Color.FromArgb(80, 255, 0, 0);

            mUnselectedColorFill = new SolidColorBrush();
            mUnselectedColorFill.Color = Color.FromArgb(80, 255, 255, 255);

            mShapeArea.Stroke = lineColor;
            mShapeArea.Fill = mUnselectedColorFill;
            mShapeArea.StrokeThickness = 2;

            mAreaRect = new Tesseract.Rect(rct.X1, rct.Y1, rct.Width, rct.Height);
            mText = String.Copy(text);

            mOptionsMenu = new ContextMenu();

            mShapeArea.ContextMenu = mOptionsMenu;
            mShapeArea.ContextMenuOpening += OptionsMenuOpening;
            mShapeArea.ContextMenuClosing += OptionsMenuClosing;

            mActive = true;

        }

        private void OptionsMenuOpening (object sender, ContextMenuEventArgs e)
        {

            SelectStateArea(true);

            mOptionsMenu.Items.Clear();

            MenuItem title = new MenuItem();

            title.Header = "Info OCR";

            mOptionsMenu.Items.Add(title);

            Separator separator = new Separator();

            mOptionsMenu.Items.Add(separator);

            MenuItem name = new MenuItem();
            name.Header = "Testo Riconosciuto: \"" + mText.TrimEnd() + "\"";

            mOptionsMenu.Items.Add(name);

            MenuItem removeArea = new MenuItem();
            removeArea.Header = "Modifica Risultato";
            removeArea.Click += modifyArea_Click;

            mOptionsMenu.Items.Add(removeArea);

            MenuItem modifyArea = new MenuItem();
            modifyArea.Header = "Elimina Area";
            modifyArea.Click += removeArea_Click;

            mOptionsMenu.Items.Add(modifyArea);

        }

     
        void removeArea_Click(object sender, RoutedEventArgs e)
        {
            //elimino
            if ( mParentArea != null)
            {
                //mParentArea.RemoveArea(this);

                //RemoveAreaFromCanvas();

                mActive = false;

                mParentArea.SetAreaAsInactive(this);

            }
        }

        void modifyArea_Click(object sender, RoutedEventArgs e)
        {
            //apro dialog di modifica
            EditRecognizedAreaDialogBox dialog = new EditRecognizedAreaDialogBox(this);

            dialog.SetOldText(mText);

            dialog.ShowDialog();

            dialog.Close();
        }

        public void SelectStateArea (bool val)
        {
            if ( !val )
            {
                mShapeArea.Fill = mUnselectedColorFill;
            }
            else
            {
                mShapeArea.Fill = mSelectedColorFill;
            }
        }

        private void OptionsMenuClosing(object sender, ContextMenuEventArgs e)
        {
            SelectStateArea(false);
        }

        public void AddAreaInCanvas ( Canvas canvas )
        {
            //disegno l'area nel canvas
            if ( canvas != null )
            {
                mParentCanvas = canvas;

                mParentCanvas.Children.Add(mShapeArea);

                mShapeArea.SetValue(Canvas.LeftProperty, Convert.ToDouble(mAreaRect.X1));
                mShapeArea.SetValue(Canvas.TopProperty, Convert.ToDouble(mAreaRect.Y1));
                mShapeArea.SetValue(Canvas.WidthProperty, Convert.ToDouble(mAreaRect.Width));
                mShapeArea.SetValue(Canvas.HeightProperty, Convert.ToDouble(mAreaRect.Height));
            }

        }

        public void RemoveAreaFromCanvas ( )
        {
            if ( mParentCanvas != null )
            {
                mParentCanvas.Children.Remove(mShapeArea);
            }
        }
     
    }
}

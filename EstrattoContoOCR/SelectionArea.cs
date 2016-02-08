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

    public enum SelectionAreaType
    {
        DataOperazioneArea = 0,
        DataValutaArea = 1,
        DareArea = 2,
        AvereArea = 3,
        DescrizioneArea = 4
    };


    public class SelectionAreaPositionComparer : IComparer<SelectionArea>
    {
        public int Compare(SelectionArea x, SelectionArea y)
        {
            //la verifica consiste nel controllare l'ordinata delle aree riconosciute

            if (x == null && y != null) return -1;
            if (x == null && y == null) return 0;
            if (x != null && y == null) return 1;

            List<RecognizedArea> x_data = x.GetAreas();
            List<RecognizedArea> y_data = y.GetAreas();

            if (x_data.Count == 0 && y_data.Count > 0) return -1;
            if (x_data.Count == 0 && x_data.Count == 0) return 0;
            if (x_data.Count > 0 && y_data.Count == 0) return 1;

            RecognizedArea x0 = x_data[0];
            RecognizedArea y0 = y_data[0];

            if (x0.AreaRect.Y1 < y0.AreaRect.Y1) return -1;
            else if (x0.AreaRect.Y1 == y0.AreaRect.Y1) return 0;
            else return 1; //(x0.AreaRect.Y1 > y0.AreaRect.Y1) return 1;

        }
    };

    public class SelectionArea
    {
        public Rectangle t, tl, tr, l, c, r, bl, b, br;

        ISelectionAreaDelegate managerWindow;

        private SelectionAreaType mAreaType;

        public static double BORDER_SIZE = 5;

        private TextBlock mAreaInfos;
        private bool mAreaVisible;
        private bool mIsMoving;
        private bool mIsStreching;

        private Point mPointOffset;

        private double mInitTop, mInitLeft, mInitW, mInitH;

        private List<RecognizedArea> mRecognizedAreas;

        private List<RecognizedArea> mDeletedAreas;

        private bool mRecognizedAreasVisible;

        private Canvas mParentCanvas;

        private ContextMenu mContextMenu;

        private SolidColorBrush mCenterColorNormal;
        private SolidColorBrush mCenterColorSelected;
        private SolidColorBrush mCenterColorHasAreas;

        public SelectionArea(double top, double left, double w, double h, SelectionAreaType type, ISelectionAreaDelegate manager)
        {

            mAreaType = type;

            managerWindow = manager;

            mPointOffset = new Point();

            tr = new Rectangle();
            t = new Rectangle();
            tl = new Rectangle();

            l = new Rectangle();
            c = new Rectangle();
            r = new Rectangle();

            bl = new Rectangle();
            b = new Rectangle();
            br = new Rectangle();

            SolidColorBrush backTextColor = new SolidColorBrush();
            backTextColor.Color = Color.FromArgb(200, 255, 255, 255);

            SolidColorBrush angleColor = new SolidColorBrush();
            angleColor.Color = Color.FromArgb(255, 255, 255, 0);

            SolidColorBrush borderColor = new SolidColorBrush();
            borderColor.Color = Color.FromArgb(255, 0, 0, 0);

            mCenterColorNormal = new SolidColorBrush();
            mCenterColorNormal.Color = Color.FromArgb(100, 255, 255, 255);

            mCenterColorSelected = new SolidColorBrush();
            mCenterColorSelected.Color = Color.FromArgb(120, 208, 175, 174);

            mCenterColorHasAreas = new SolidColorBrush();
            mCenterColorHasAreas.Color = Color.FromArgb(100, 255, 255, 102);

            mAreaInfos = new TextBlock();
            mAreaInfos.FontSize = 14;
            mAreaInfos.FontWeight = System.Windows.FontWeights.Bold;
            mAreaInfos.Foreground = borderColor;
            mAreaInfos.Background = backTextColor;

            bl.Fill = angleColor;
            br.Fill = angleColor;
            tl.Fill = angleColor;
            tr.Fill = angleColor;

            t.Fill = borderColor;
            l.Fill = borderColor;
            r.Fill = borderColor;
            b.Fill = borderColor;
            c.Fill = mCenterColorNormal;

            t.StrokeThickness = 0;
            tl.StrokeThickness = 0;
            tr.StrokeThickness = 0;

            l.StrokeThickness = 0;
            r.StrokeThickness = 0;
            c.StrokeThickness = 0;

            bl.StrokeThickness = 0;
            br.StrokeThickness = 0;
            b.StrokeThickness = 0;

            RefreshArea(top, left, w, h);

            
            l.Tag = this;
            r.Tag = this;
            c.Tag = this;
            tl.Tag = this;
            tr.Tag = this;
            t.Tag = this;
            bl.Tag = this;
            br.Tag = this;
            b.Tag = this;

         


            mContextMenu = new ContextMenu();

            c.ContextMenu = mContextMenu;

            c.ContextMenuOpening += c_ContextMenuOpening;
            c.ContextMenuClosing += c_ContextMenuClosing;
            mRecognizedAreas = new List<RecognizedArea>();
            mDeletedAreas = new List<RecognizedArea>();

            HideArea();

        }

        void c_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            SetAreaColor(false);
        }

        private void SetAreaColor ( bool selected )
        {
            if ( selected )
            {
                //normale 
                c.Fill = mCenterColorSelected;
            }
            
            else 
            {
                if ( HasRecognizedData )
                {
                    c.Fill = mCenterColorHasAreas;
                }
                else
                {
                    c.Fill = mCenterColorNormal;
                }
            }
        }

        void c_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            SetAreaColor(true);
            //creo menu contestuale per l'area
            mContextMenu.Items.Clear();

            MenuItem deleteArea = new MenuItem();
            MenuItem analizeArea = new MenuItem();

            deleteArea.Header = "Elimina Area";
            deleteArea.Tag = this;
            deleteArea.Click += deleteArea_Click;

            analizeArea = new MenuItem();
            analizeArea.Header = "Analizza Area con OCR";
            analizeArea.Tag = this;
            analizeArea.Click += analizeArea_Click;

            mContextMenu.Items.Add(deleteArea);
            mContextMenu.Items.Add(analizeArea);

            mContextMenu.Items.Add(new Separator());

            if ( this.HasRecognizedData )
            {
                MenuItem mShowReconAreas = new MenuItem();

                if ( this.RecognizedAreaVisible )
                {
                    mShowReconAreas.Header = "Nascondi Risultati OCR";
                }
                else
                {
                    mShowReconAreas.Header = "Mostra Risultati OCR";
                }

                mShowReconAreas.Tag = this;
                mShowReconAreas.Click += mShowReconAreas_Click;

                mContextMenu.Items.Add(mShowReconAreas);

                MenuItem copyItem = new MenuItem();

                copyItem.Header = "Copia risultati";
                copyItem.Tag = this;
                copyItem.Click += copyItem_Click;

                mContextMenu.Items.Add(copyItem);
            }

            //mostro l'annulla rimozione

            if ( mDeletedAreas != null && mDeletedAreas.Count() > 0 )
            {
                MenuItem undoArea = new MenuItem();

                undoArea.Header = "Ripristina ultima area";
                undoArea.Click += undoArea_Click;

                mContextMenu.Items.Add(new Separator());
                mContextMenu.Items.Add(undoArea);
            }
           
        }

        void copyItem_Click(object sender, RoutedEventArgs e)
        {
            string copy_text = "";

            SelectionArea area = (sender as MenuItem).Tag as SelectionArea;

            for ( int k = 0 ; k < area.mRecognizedAreas.Count ; k++ )
            {
                copy_text += mRecognizedAreas[k].RecognizedData + "\r\n";
            }

            Clipboard.SetText(copy_text);
        }

        void undoArea_Click(object sender, RoutedEventArgs e)
        {
            //estraggo l'ultima dalla lista e riattivo
            if ( mDeletedAreas != null && mDeletedAreas.Count() > 0 )
            {
                int n = mDeletedAreas.Count();
                
                RecognizedArea last = mDeletedAreas[n - 1];

                last.Active = true;

                last.SelectStateArea(false);

                if (mRecognizedAreasVisible) last.AddAreaInCanvas(mParentCanvas);

                mDeletedAreas.Remove(last);

                SetAreaColor(false);
            }
        }

        void mShowReconAreas_Click(object sender, RoutedEventArgs e)
        {
            if (mRecognizedAreasVisible)
            {
                HideRecognizedAreas();
            }
            else
            {
                ShowRecognizedAreas();
            }
        }

        void RemoveSelectionFromCanvas()
        {
            mParentCanvas.Children.Remove(tl);
            mParentCanvas.Children.Remove(tr);
            
            mParentCanvas.Children.Remove(t);
            mParentCanvas.Children.Remove(r);
            mParentCanvas.Children.Remove(l);
            mParentCanvas.Children.Remove(c);
            mParentCanvas.Children.Remove(b);

            mParentCanvas.Children.Remove(bl);
            mParentCanvas.Children.Remove(br);

            mParentCanvas.Children.Remove(mAreaInfos);

        }

        void analizeArea_Click(object sender, RoutedEventArgs e)
        {
            if ( managerWindow != null )
            {
                managerWindow.OCRAreaAnalysis(this);
            }
        }

        void deleteArea_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show("Eliminare l'area con tutti i risultati?", "Conferma", MessageBoxButton.YesNo);
           
            if ( dialogResult == MessageBoxResult.Yes )
            {
                ClearRecognizedArea();

                HideArea();

                RemoveSelectionFromCanvas();

                managerWindow.RequireRemoveArea(this);
            }
         
        }

        public void SetAreaAsInactive(RecognizedArea area)
        {
            if ( area != null )
            {
                //aggiungo tra i cancellati
                mDeletedAreas.Add(area);
                //rendo invisibiles
                area.RemoveAreaFromCanvas();
            }
        }

        public void HideArea ()
        {
            tl.Visibility = System.Windows.Visibility.Hidden;
            t.Visibility = System.Windows.Visibility.Hidden;
            tr.Visibility = System.Windows.Visibility.Hidden;
            l.Visibility = System.Windows.Visibility.Hidden;
            c.Visibility = System.Windows.Visibility.Hidden;
            r.Visibility = System.Windows.Visibility.Hidden;
            bl.Visibility = System.Windows.Visibility.Hidden;
            b.Visibility = System.Windows.Visibility.Hidden;
            br.Visibility = System.Windows.Visibility.Hidden;
            mAreaInfos.Visibility = System.Windows.Visibility.Hidden;

            //mRecognizedAreasVisible = false;

            HideRecognizedAreas();

            SetAreaColor(false);

            mAreaVisible = false;
        }

        public void ShowArea()
        {
            tl.Visibility = System.Windows.Visibility.Visible;
            t.Visibility = System.Windows.Visibility.Visible;
            tr.Visibility = System.Windows.Visibility.Visible;
            l.Visibility = System.Windows.Visibility.Visible;
            c.Visibility = System.Windows.Visibility.Visible;
            r.Visibility = System.Windows.Visibility.Visible;
            bl.Visibility = System.Windows.Visibility.Visible;
            b.Visibility = System.Windows.Visibility.Visible;
            br.Visibility = System.Windows.Visibility.Visible;
            mAreaInfos.Visibility = System.Windows.Visibility.Visible;

            mAreaVisible = true;

            SetAreaColor(false);
        }

        public bool AreaVisibility ()
        {
            return mAreaVisible;
        }


        public void ClearRecognizedArea()
        {
            if ( mRecognizedAreas != null )
            {
                HideRecognizedAreas();
                mRecognizedAreas.Clear();
            }
        }

        public void MoveArea ( Point pt )
        {
            //muovo l'area calcolando l'offset da pt

            double _w = Convert.ToDouble(c.GetValue(Canvas.WidthProperty)) + 2 * BORDER_SIZE;
            double _h = Convert.ToDouble(c.GetValue(Canvas.HeightProperty)) + 2 * BORDER_SIZE;

            RefreshArea(pt.X - mPointOffset.X, pt.Y - mPointOffset.Y, _w, _h);

        }

        public void StretchArea ( Rectangle rt, Point pt )
        {
            double top = 0, left = 0, w = 0, h = 0;
            double dx = 0, dy = 0;

            int px = 0;
            int py = 0;
            int vx = 0;
            int vy = 0;

            dx = pt.X - mPointOffset.X;
            dy = pt.Y - mPointOffset.Y;

            if ( rt == t )
            {
                px = 0; py = 1;
                vx = 0; vy = -1;
            }
            else if ( rt == tl )
            {
                px = 1; py = 1;
                vx = -1; vy = -1;
            }
            else if ( rt == tr )
            {
                px = 0; py = 1;
                vx = 1; vy = -1;
            }
            else if ( rt == l )
            {
                px = 1; py = 0;
                vx = -1; vy = 0;
            }
            else if ( rt == r )
            {
                px = 0; py = 0;
                vx = 1; vy = 0;
            }
            else if ( rt == bl )
            {
                px = 1; py = 0;
                vx = -1; vy = 1;
            }
            else if ( rt == b )
            {
                px = 0; py = 0;
                vx = 0; vy = 1;
            }
            else if ( rt == br )
            {
                px = 0; py = 0;
                vx = 1; vy = 1;
            }
            else
            {
                
            }

            top = mInitTop + py * dy;
            left = mInitLeft + px * dx;
            w = mInitW + vx * dx;
            h = mInitH + vy * dy;

            

            RefreshArea(left, top, w, h);
        }

        public void RefreshArea(double left, double top, double w, double h)
        {

            if (h < 2 * BORDER_SIZE) return;
            if (w < 2 * BORDER_SIZE) return;

            tl.SetValue(Canvas.LeftProperty, left);
            tl.SetValue(Canvas.TopProperty, top);
            tl.Width = BORDER_SIZE;
            tl.Height = BORDER_SIZE;

            t.SetValue(Canvas.LeftProperty, left + BORDER_SIZE);
            t.SetValue(Canvas.TopProperty, top);
            t.Width = w - 2 * BORDER_SIZE;
            t.Height = BORDER_SIZE;

            tr.SetValue(Canvas.LeftProperty, left + w - BORDER_SIZE);
            tr.SetValue(Canvas.TopProperty, top);
            tr.Width = BORDER_SIZE;
            tr.Height = BORDER_SIZE;

            l.SetValue(Canvas.LeftProperty, left);
            l.SetValue(Canvas.TopProperty, top + BORDER_SIZE);
            l.Width = BORDER_SIZE;
            l.Height = h - 2 * BORDER_SIZE;

            c.SetValue(Canvas.LeftProperty, left + BORDER_SIZE);
            c.SetValue(Canvas.TopProperty, top + BORDER_SIZE);
            c.Width = w - 2 * BORDER_SIZE;
            c.Height = h - 2 * BORDER_SIZE;

            r.SetValue(Canvas.LeftProperty, left + w - BORDER_SIZE);
            r.SetValue(Canvas.TopProperty, top + BORDER_SIZE);
            r.Width = BORDER_SIZE;
            r.Height = h - 2 * BORDER_SIZE;

            bl.SetValue(Canvas.LeftProperty, left);
            bl.SetValue(Canvas.TopProperty, top + h - BORDER_SIZE);
            bl.Width = BORDER_SIZE;
            bl.Height = BORDER_SIZE;

            b.SetValue(Canvas.LeftProperty, left + BORDER_SIZE);
            b.SetValue(Canvas.TopProperty, top + h - BORDER_SIZE);
            b.Width = w - 2 * BORDER_SIZE;
            b.Height = BORDER_SIZE;

            br.SetValue(Canvas.LeftProperty, left + w - BORDER_SIZE);
            br.SetValue(Canvas.TopProperty, top + h - BORDER_SIZE);
            br.Width = BORDER_SIZE;
            br.Height = BORDER_SIZE;

            mAreaInfos.SetValue(Canvas.LeftProperty, left);
            mAreaInfos.SetValue(Canvas.TopProperty, top - 25);

            RefreshAreaInfos();

        }

        public RecognizedArea AddRecognizedArea ( Tesseract.Rect rct, string data_rec, float conf )
        {
            RecognizedArea ret = null;

            if ( rct != null && data_rec != null )
            {
                string data_to_add = String.Copy(data_rec);

                if (mAreaType == SelectionAreaType.DataOperazioneArea || mAreaType == SelectionAreaType.DataValutaArea )
                {
                    //verifica se manca anno
                    DateTime dt;
                    DateTime.TryParse(data_rec, out dt);

                    data_to_add = dt.ToShortDateString();

                }

                ret = new RecognizedArea(this, rct, data_to_add, conf);

                mRecognizedAreas.Add(ret);

                //imposto l'area come riconosciuta
                SetAreaColor(false);
            }

            return ret;
        }

        public Tesseract.Rect GetOCRArea ()
        {

            int top = Convert.ToInt32(tl.GetValue(Canvas.TopProperty)) + (int)BORDER_SIZE;
            int left = Convert.ToInt32(tl.GetValue(Canvas.LeftProperty)) + (int)BORDER_SIZE;

            int w = Convert.ToInt32(c.GetValue(Canvas.WidthProperty));
            int h = Convert.ToInt32(c.GetValue(Canvas.HeightProperty));

            Tesseract.Rect rct = new Tesseract.Rect(left, top, w, h);

            return rct;

        }

        public bool StretchRectangle(Rectangle r)
        {
            return r != c;
        }

        public bool GetMoving()
        {
            return mIsMoving;
        }

        public bool GetStretching ()
        {
            return mIsStreching;
        }

        public void SetMoving ( bool val, Point startPoint )
        {
            mIsMoving = val;

            if (val)
            {
                double _l = Convert.ToDouble(tl.GetValue(Canvas.LeftProperty));
                double _t = Convert.ToDouble(tl.GetValue(Canvas.TopProperty));

                mPointOffset.X = startPoint.X - _l;
                mPointOffset.Y = startPoint.Y - _t;
            }
            
        }

        public void SetStretching ( bool val, Point startPoint )
        {
            mIsStreching = val;

            if (val)
            {
                mInitTop = Convert.ToDouble(tl.GetValue(Canvas.TopProperty));
                mInitLeft = Convert.ToDouble(tl.GetValue(Canvas.LeftProperty));
                mInitW = Convert.ToDouble(c.GetValue(Canvas.WidthProperty)) + 2 * BORDER_SIZE;
                mInitH = Convert.ToDouble(c.GetValue(Canvas.HeightProperty)) + 2 * BORDER_SIZE;

                mPointOffset.X = startPoint.X;
                mPointOffset.Y = startPoint.Y;
            }
            
        }

        public void ResetTransformPoint()
        {
            mPointOffset.X = 0;
            mPointOffset.Y = 0;
        }

        void RefreshAreaInfos()
        {
            object[] parms = new object[5];
        
            double _l = Convert.ToDouble(tl.GetValue(Canvas.LeftProperty));
            double _t = Convert.ToDouble(tl.GetValue(Canvas.TopProperty));
            double _w = Convert.ToDouble(c.GetValue(Canvas.WidthProperty)) + 2 * BORDER_SIZE;
            double _h = Convert.ToDouble(c.GetValue(Canvas.HeightProperty)) + 2 * BORDER_SIZE;

            parms[1] = _l;
            parms[2] = _t;
            parms[3] = _w;
            parms[4] = _h;

            switch ( mAreaType )
            {
                case SelectionAreaType.DareArea:
                {
                    parms[0] = "DARE";
                }
                break;

                case SelectionAreaType.AvereArea:
                {
                    parms[0] = "AVERE";
                }
                break;

                case SelectionAreaType.DataOperazioneArea:
                {
                    parms[0] = "D.OPERAZ.";
                }
                break;

                case SelectionAreaType.DataValutaArea:
                {
                    parms[0] = "D.VAL.";
                }
                break;

                case SelectionAreaType.DescrizioneArea:
                {
                    parms[0] = "DESCR";
                }
                break;
            }

            mAreaInfos.Text = String.Format("Area: {0} - L:{1:0} T:{2:0} | W:{3:0} H:{4:0}", parms);
            
        }


        public void AddToCanvas(Canvas cv)
        {
            cv.Children.Add(tl);
            cv.Children.Add(t);
            cv.Children.Add(tr);
            cv.Children.Add(l);
            cv.Children.Add(c);
            cv.Children.Add(r);
            cv.Children.Add(bl);
            cv.Children.Add(b);
            cv.Children.Add(br);
            cv.Children.Add(mAreaInfos);
            
            mParentCanvas = cv;

            tl.SetValue(Canvas.ZIndexProperty, 2);
            t.SetValue(Canvas.ZIndexProperty, 2);
            tr.SetValue(Canvas.ZIndexProperty, 2);
            l.SetValue(Canvas.ZIndexProperty, 2);
            c.SetValue(Canvas.ZIndexProperty, 2);
            r.SetValue(Canvas.ZIndexProperty, 2);
            bl.SetValue(Canvas.ZIndexProperty, 2);
            b.SetValue(Canvas.ZIndexProperty, 2);
            br.SetValue(Canvas.ZIndexProperty, 2);
            mAreaInfos.SetValue(Canvas.ZIndexProperty, 2);

        }

        public void RemoveFromCanvas()
        {
            RemoveSelectionFromCanvas();
        }

        public void ShowRecognizedAreas()
        {
            if ( mRecognizedAreas != null && mAreaVisible && !mRecognizedAreasVisible )
            {
                if ( mParentCanvas != null )
                {
                    int n_elem = mRecognizedAreas.Count;

                    for ( int k = 0 ; k < n_elem ; k++ )
                    {
                        RecognizedArea ra = mRecognizedAreas.ElementAt(k);

                        if ( ra.Active ) ra.AddAreaInCanvas(mParentCanvas);
                    }

                    mRecognizedAreasVisible = true;
                }
            }
        }

        public void HideRecognizedAreas()
        {
            if (mRecognizedAreas != null && mRecognizedAreasVisible)
            {
                //int n_elem = mRecognizedAreas.Count;

                /*for (int k = 0; k < n_elem; k++)
                {
                    RecognizedArea ra = mRecognizedAreas.ElementAt(k);

                    ra.RemoveAreaFromCanvas();
                }*/

                foreach ( RecognizedArea ra in mRecognizedAreas )
                {
                    ra.RemoveAreaFromCanvas();
                }

                mRecognizedAreasVisible = false;
            }
        }

        public SelectionAreaType AreaType
        {
            get
            {
                return mAreaType;
            }
        }

        public bool HasRecognizedData
        {
            get
            {
                return (mRecognizedAreas != null && mRecognizedAreas.Count > 0);
                
            }
        }

        public bool RecognizedAreaVisible
        {
            get { return mRecognizedAreasVisible; }
        }

        public void SetBorderCursor(Rectangle rr)
        {
            if (rr == tl || rr == br) rr.Cursor = Cursors.SizeNWSE;
            else if (rr == bl || rr == tr) rr.Cursor = Cursors.SizeNESW;
            else if (rr == l || rr == r) rr.Cursor = Cursors.SizeWE;
            else if (rr == t || rr == b) rr.Cursor = Cursors.SizeNS;
            else if (rr == c) rr.Cursor = Cursors.SizeAll;
            else rr.Cursor = Cursors.Arrow;

        }

        public List<string> GetResults()
        {
            List<string> results = null; 

            if ( mRecognizedAreas != null && mRecognizedAreas.Count() > 0 )
            {
                results = new List<String>();
                
                int num = mRecognizedAreas.Count();

                for ( int k = 0 ; k < num ; k++ )
                {
                    string str = string.Copy(mRecognizedAreas[k].RecognizedData);
                    results.Add(str);
                }
            }

            return results;
        }

        public List<RecognizedArea> GetAreas()
        {
            return mRecognizedAreas;
        }

        public int GetNumResults()
        {
            int res = 0;

            if (mRecognizedAreas != null) res = mRecognizedAreas.Count();

            return res;
        }

        public bool RemoveArea(RecognizedArea area)
        {
            bool ret = false;

            if ( mRecognizedAreas != null && mRecognizedAreas.Count > 0 )
            {
                ret = mRecognizedAreas.Remove(area);
            }

            return ret;
        }

        public void AddDummyAreas( RecognizedArea area, int num, int pos )
        {
            //0 prima di area, 1 dopo di area
        }

    }
}

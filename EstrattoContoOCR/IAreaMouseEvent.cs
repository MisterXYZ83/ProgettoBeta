using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace EstrattoContoOCR
{
    public interface ISelectionAreaDelegate
    {

        void OCRAreaAnalysis(SelectionArea area);

        //void SelectionArea_MouseEnter(object sender, MouseEventArgs e);
        //void SelectionArea_MouseLeave(object sender, MouseEventArgs e);
        //void SelectionArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e);
        //void SelectionArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e);
        //void SelectionArea_MouseMove(object sender, MouseEventArgs e);

    }

    public interface IEditingToolDialogDelegate
    {
        void EditingToggle(bool actualState);
        void EditingChangeToolType(EditingToolType newType);
        void EditingChangeToolTickness(EditingToolThickness newTick);
        void EditingUndoCorrection(int idx);
        void EditingSaveCorrections();
        void EditingSelectCorrection(int idx);
        bool EditingHasSavedImages();
        void EditingUndoLastSave();
    }
}

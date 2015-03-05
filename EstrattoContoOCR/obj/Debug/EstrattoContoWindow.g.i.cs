﻿#pragma checksum "..\..\EstrattoContoWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "E44A52374BC3419EE2EEDB44792C70CA"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.36213
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Windows.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace EstrattoContoOCR {
    
    
    /// <summary>
    /// EstrattoContoWindow
    /// </summary>
    public partial class EstrattoContoWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\EstrattoContoWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Menu mMainMenu;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\EstrattoContoWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.StatusBar mStatusBar;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\EstrattoContoWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView mUsersListView;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\EstrattoContoWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button mAddUserButton;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\EstrattoContoWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button mSelectUserButton;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\EstrattoContoWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button mDeleteUserButton;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\EstrattoContoWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView mEstrattoContoListView;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\EstrattoContoWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button mEstrattoContoAnalizeButton;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\EstrattoContoWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button mAnalizeHistoryButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/EstrattoContoOCR;component/estrattocontowindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\EstrattoContoWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.mMainMenu = ((System.Windows.Controls.Menu)(target));
            return;
            case 2:
            
            #line 8 "..\..\EstrattoContoWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.CreaUtenteItem_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 10 "..\..\EstrattoContoWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.ExitMenuItem_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.mStatusBar = ((System.Windows.Controls.Primitives.StatusBar)(target));
            return;
            case 5:
            this.mUsersListView = ((System.Windows.Controls.ListView)(target));
            
            #line 14 "..\..\EstrattoContoWindow.xaml"
            this.mUsersListView.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.mUsersListView_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.mAddUserButton = ((System.Windows.Controls.Button)(target));
            
            #line 22 "..\..\EstrattoContoWindow.xaml"
            this.mAddUserButton.Click += new System.Windows.RoutedEventHandler(this.mAddUser_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.mSelectUserButton = ((System.Windows.Controls.Button)(target));
            
            #line 23 "..\..\EstrattoContoWindow.xaml"
            this.mSelectUserButton.Click += new System.Windows.RoutedEventHandler(this.mSelectUser_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.mDeleteUserButton = ((System.Windows.Controls.Button)(target));
            
            #line 24 "..\..\EstrattoContoWindow.xaml"
            this.mDeleteUserButton.Click += new System.Windows.RoutedEventHandler(this.mDeleteUser_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.mEstrattoContoListView = ((System.Windows.Controls.ListView)(target));
            return;
            case 10:
            this.mEstrattoContoAnalizeButton = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\EstrattoContoWindow.xaml"
            this.mEstrattoContoAnalizeButton.Click += new System.Windows.RoutedEventHandler(this.mEstrattoContoAnalizeButton_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.mAnalizeHistoryButton = ((System.Windows.Controls.Button)(target));
            
            #line 34 "..\..\EstrattoContoWindow.xaml"
            this.mAnalizeHistoryButton.Click += new System.Windows.RoutedEventHandler(this.mAnalizeHistoryButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


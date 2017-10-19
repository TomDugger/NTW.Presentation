using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NTW.Presentation;
using ModelTest.Test;

namespace ModelTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ModelTest.Test.PresentationItem p;

        public MainWindow()
        {
            InitializeComponent();

            p = new Test.PresentationItem();

            this.DataContext = new Test.PresentationItem();
            //System.Windows.Controls.Primitives.Popup p = new System.Windows.Controls.Primitives.Popup();
            //p.Placement = System.Windows.Controls.Primitives.PlacementMode.Center
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            this.DataContext = p;
        }
    }
}

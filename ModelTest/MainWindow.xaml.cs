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
using System.Diagnostics;

namespace ModelTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //ModelTest.Test.Presentation p;
        Stopwatch sw = new Stopwatch();
        public MainWindow()
        {
            InitializeComponent();

            //p = new Test.Presentation();

            //this.DataContext = new Test.Presentation();
            //System.Windows.Controls.Primitives.Popup p = new System.Windows.Controls.Primitives.Popup();
            //p.Placement = System.Windows.Controls.Primitives.PlacementMode.Center
            sw.Start();
            NTW.Presentation.Presentation.Generation(t => t == typeof(ModelTest.Test.Presentation) || t == typeof(ModelTest.Test.ChildrenMyInterface) || t == typeof(ModelTest.Test.Children2MyInterface));
            this.DataContext = new Test.Presentation();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            //this.DataContext = p;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sw.Stop();
            this.Title = sw.Elapsed.ToString();
        }
    }
}

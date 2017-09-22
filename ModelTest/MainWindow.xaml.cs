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
        ModelTest.Test.Presentation p;

        public MainWindow()
        {
            InitializeComponent();

            PresentationList<PresentationItem> pl = new PresentationList<PresentationItem>();
            pl.Add(new PresentationItem());
            pl.Add(new PresentationItem());

            p = new Test.Presentation()
            {
                Enum = Aenum.a4,

                dictionary = new Dictionary<int, PresentationItem>(),
                dictionary2 = new Dictionary<string, PresentationItem>(),
                ArrayPresentationItems = new PresentationItem[] { 
                        new PresentationItem() { ItemName = "item1", ItemValue = 1 }, 
                        new PresentationItem() { ItemName = "item2", ItemValue = 2 },
                        new PresentationItem() { ItemName = "item3", ItemValue = 3 } 
                    },
                ArrayGeneric = new PresentationGeneric<int, int>[]
                    {
                        new PresentationGeneric<int, int>() { Value1 = 1, Value2 = 0 },
                        new PresentationGeneric<int, int>() { Value1 = 2, Value2 = 1 },
                        new PresentationGeneric<int, int>() { Value1 = 3, Value2 = 2 },
                    },
                Item = new PresentationItem(),
                GItem = new PresentationGeneric<int, string>() { Value1 = 123, Value2 = "123" },
                ListGItems = new List<PresentationGeneric<double, long>> { new PresentationGeneric<double, long>() { Value1 = 0.1, Value2 = 1234 }, new PresentationGeneric<double, long>() { Value1 = 0.2, Value2 = 4321 } },
                ListG2Items = new List<PresentationGeneric<int, int>> { new PresentationGeneric<int, int>() { Value1 = 12, Value2 = 14 } },
                ArrayGItems = new PresentationGeneric<string, double>[] { new PresentationGeneric<string, double>() { Value1 = "321", Value2 = 1.45 } },
                PList = pl,
                List = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                Collection = new System.Collections.ObjectModel.ObservableCollection<int>() { 1, 2, 3, 4, 5 },
                Items = new List<PresentationItem>(),
                Items1 = new PresentationItem[] { new PresentationItem() },
                Items2 = new int[] { 1, 2, 3, 4, 5, 6 } 
            };

            this.DataContext = p;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            this.DataContext = p;
        }
    }
}

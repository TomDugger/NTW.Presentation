using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using NTW.Commands;

namespace NTW.Presentation
{
    internal class DictionaryItemsControl<TKey, TValue> : BaseItemsControl
    {
        #region Private
        private Command addCommand;
        private Command removeCommand;

        private DictionatyNewItem<TKey, TValue> NewValue;
        #endregion

        public DictionaryItemsControl() {

            NewValue = new DictionatyNewItem<TKey, TValue>();

            AddTemplateFromDictionatyNewValue();

            ControlTemplate container = new ControlTemplate(typeof(DictionaryItemsControl<TKey, TValue>));
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));

            FrameworkElementFactory ContainerRowProperty = new FrameworkElementFactory(typeof(RowDefinition));
            ContainerRowProperty.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
            grid.AppendChild(ContainerRowProperty);

            FrameworkElementFactory ContainerRow1Property = new FrameworkElementFactory(typeof(RowDefinition));
            ContainerRow1Property.SetValue(RowDefinition.HeightProperty, new GridLength(40));
            grid.AppendChild(ContainerRow1Property);

            FrameworkElementFactory itemsPreseter = new FrameworkElementFactory(typeof(ItemsPresenter));
            grid.AppendChild(itemsPreseter);

            FrameworkElementFactory addButton = new FrameworkElementFactory(typeof(ToggleButton), "ButtonAdd");
            addButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsOpen") { Source = NewValue });
            addButton.SetValue(Button.ContentProperty, "Add...");
            addButton.SetValue(Grid.RowProperty, 1);
            grid.AppendChild(addButton);

            FrameworkElementFactory MenuAddElement = new FrameworkElementFactory(typeof(Popup));
            MenuAddElement.SetValue(Popup.PlacementProperty, System.Windows.Controls.Primitives.PlacementMode.Center);
            MenuAddElement.SetValue(Popup.StaysOpenProperty, false);
            MenuAddElement.SetBinding(Popup.IsOpenProperty, new Binding("IsOpen") { Source = NewValue });
            MenuAddElement.SetValue(Popup.MinHeightProperty, 150.0);
            MenuAddElement.SetValue(Popup.MinWidthProperty, 100.0);
            MenuAddElement.SetBinding(Popup.WidthProperty, new Binding("ActualWidth") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1) });
            MenuAddElement.SetBinding(Popup.HeightProperty, new Binding("ActualHeight") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1) });

            FrameworkElementFactory MenuGrid = new FrameworkElementFactory(typeof(Grid));
            MenuGrid.SetValue(Grid.BackgroundProperty, new SolidColorBrush(Colors.White));

            #region Rows
            FrameworkElementFactory MenuGridRowProperty = new FrameworkElementFactory(typeof(RowDefinition));
            MenuGridRowProperty.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
            MenuGrid.AppendChild(MenuGridRowProperty);

            FrameworkElementFactory MenuGridRow1Property = new FrameworkElementFactory(typeof(RowDefinition));
            MenuGridRow1Property.SetValue(RowDefinition.HeightProperty, new GridLength(40));
            MenuGrid.AppendChild(MenuGridRow1Property); 
            #endregion

            #region Columns
            FrameworkElementFactory MenuGridColumnProperty = new FrameworkElementFactory(typeof(ColumnDefinition));
            MenuGridColumnProperty.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            MenuGrid.AppendChild(MenuGridColumnProperty);

            FrameworkElementFactory MenuGridColumn1Property = new FrameworkElementFactory(typeof(ColumnDefinition));
            MenuGridColumn1Property.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            MenuGrid.AppendChild(MenuGridColumn1Property);
            #endregion

            #region Content
            FrameworkElementFactory Content = new FrameworkElementFactory(typeof(Label));
            Content.SetValue(Grid.ColumnSpanProperty, 2);
            Content.SetValue(Label.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
            Content.SetValue(Label.VerticalContentAlignmentProperty, System.Windows.VerticalAlignment.Stretch);
            Content.SetBinding(Label.ContentProperty, new Binding() { Source = NewValue });
            MenuGrid.AppendChild(Content);
            #endregion

            #region Add
            FrameworkElementFactory AddButton = new FrameworkElementFactory(typeof(Button));
            AddButton.SetValue(Grid.RowProperty, 1);
            AddButton.SetValue(Grid.ColumnProperty, 0);
            AddButton.SetValue(Button.ContentProperty, "Add");
            MenuGrid.AppendChild(AddButton); 
            #endregion

            #region Cancel
            FrameworkElementFactory CancelButton = new FrameworkElementFactory(typeof(ToggleButton));
            CancelButton.SetValue(Grid.RowProperty, 1);
            CancelButton.SetValue(Grid.ColumnProperty, 1);
            CancelButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsOpen") { Source = NewValue });
            CancelButton.SetValue(ToggleButton.ContentProperty, "Cancel");
            MenuGrid.AppendChild(CancelButton); 
            #endregion

            MenuAddElement.AppendChild(MenuGrid);

            grid.AppendChild(MenuAddElement);

            container.VisualTree = grid;

            this.Template = container;

            #region OverrideMetadata
            if (ContextProperty.GetMetadata(typeof(DictionaryItemsControl<TKey, TValue>)) as UIPropertyMetadata == null)
                ContextProperty.OverrideMetadata(typeof(DictionaryItemsControl<TKey, TValue>), new UIPropertyMetadata((o, e) => {
                    List<KeyValue<TKey, TValue>> temp = new List<KeyValue<TKey, TValue>>();
                    foreach (var i in (e.NewValue as IDictionary<TKey, TValue>)) {
                        KeyValue<TKey, TValue> item = new KeyValue<TKey, TValue>(i.Key, i.Value);
                        item.PropertyChanged += new PropertyChangedEventHandler(itemChanged);
                        temp.Add(item);
                    }

                    o.SetValue(DictionaryItemsControl<TKey, TValue>.ItemsSourceProperty, temp);
                })); 
            #endregion
        }

        private void itemChanged(object s, PropertyChangedEventArgs ea) {
            Context[(s as KeyValue<TKey, TValue>).Key] = (s as KeyValue<TKey, TValue>).Value;
        }

        #region Public
        public IDictionary<TKey, TValue> Context {
            get { return (IDictionary<TKey, TValue>)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        } 
        #endregion

        #region Commads
        public Command AddCommand {
            get {
                return addCommand ?? (addCommand = new Command(obj => {
                    
                }, obj => ItemsSource != null && obj != null));
            }
        }

        public override Command RemoveCommand {
            get {
                return removeCommand ?? (removeCommand = new Command(obj => {
                    if (Context != null) {
                        Context.Remove(((KeyValue<TKey, TValue>)obj).Key);
                        (ItemsSource as List<KeyValue<TKey, TValue>>).Remove((KeyValue<TKey, TValue>)obj);
                    }
                    CollectionViewSource.GetDefaultView(ItemsSource).Refresh();
                }, obj => obj != null));
            }
        }
        #endregion

        #region Help
        private void AddTemplateFromDictionatyNewValue() {
            if (Application.Current.TryFindResource(new DataTemplateKey(typeof(DictionatyNewItem<TKey, TValue>))) == null) {
                DataTemplate template = new DataTemplate(typeof(DictionatyNewItem<TKey, TValue>));

                FrameworkElementFactory Container = new FrameworkElementFactory(typeof(TabControl));

                FrameworkElementFactory KeyTab = new FrameworkElementFactory(typeof(TabItem));
                KeyTab.SetValue(TabItem.HeaderProperty, "Key");
                KeyTab.SetBinding(TabItem.ContentProperty, new Binding("Key"));
                Container.AppendChild(KeyTab);

                FrameworkElementFactory ValueTab = new FrameworkElementFactory(typeof(TabItem));
                ValueTab.SetValue(TabItem.HeaderProperty, "Value");
                Container.AppendChild(ValueTab);

                template.VisualTree = Container;

                Application.Current.Resources.Add(new DataTemplateKey(typeof(DictionatyNewItem<TKey, TValue>)), template);
            }
        }
        #endregion
    }
}

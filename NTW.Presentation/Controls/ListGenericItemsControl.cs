using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTW.Commands;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NTW.Presentation
{
    internal class ListGenericItemsControl<T> : BaseItemsControl
    {
        #region Private
        private Command addCommand;
        private Command removeCommand;
        #endregion

        public ListGenericItemsControl() {
            ControlTemplate container = new ControlTemplate(typeof(ListGenericItemsControl<T>));
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
            grid.SetValue(Grid.BackgroundProperty, new SolidColorBrush(Colors.Transparent));

            FrameworkElementFactory ContainerRowProperty = new FrameworkElementFactory(typeof(RowDefinition));
            ContainerRowProperty.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
            grid.AppendChild(ContainerRowProperty);

            FrameworkElementFactory ContainerRow1Property = new FrameworkElementFactory(typeof(RowDefinition));
            ContainerRow1Property.SetValue(RowDefinition.HeightProperty, new GridLength(40));
            grid.AppendChild(ContainerRow1Property);

            FrameworkElementFactory ScrollViewItemsPresenter = new FrameworkElementFactory(typeof(ScrollViewer));
            ScrollViewItemsPresenter.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            FrameworkElementFactory itemsPreseter = new FrameworkElementFactory(typeof(ItemsPresenter));
            ScrollViewItemsPresenter.AppendChild(itemsPreseter);
            grid.AppendChild(ScrollViewItemsPresenter);

            FrameworkElementFactory addButton = new FrameworkElementFactory(typeof(Button));
            addButton.SetValue(Button.ContentProperty, "Add");
            addButton.SetValue(Button.CommandProperty, this.AddCommand);
            addButton.SetValue(Grid.RowProperty, 1);
            grid.AppendChild(addButton);
            container.VisualTree = grid;

            this.Template = container;

            if (ContextProperty.GetMetadata(typeof(ListGenericItemsControl<T>)) as UIPropertyMetadata == null)
                ContextProperty.OverrideMetadata(typeof(ListGenericItemsControl<T>), new UIPropertyMetadata((o, e) => {
                    List<Item<T>> temp = new List<Item<T>>();
                    if (e.NewValue != null)
                        for (int i = 0; i < (e.NewValue as IList<T>).Count; i++)
                        {
                            Item<T> item = new Item<T>(i, (T)(e.NewValue as IList<T>)[i]);
                            item.PropertyChanged += new PropertyChangedEventHandler(itemChanged);
                            temp.Add(item);
                        }

                    o.SetValue(ListItemsControl<T>.ItemsSourceProperty, temp);
                }));
        }

        public IList<T> Context {
            get { return (IList<T>)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        void itemChanged(object s, PropertyChangedEventArgs ea) {
            Context[(ItemsSource as List<Item<T>>).IndexOf((Item<T>)s)] = (T)(s as Item<T>).Value;
        }

        #region Commads
        public Command AddCommand {
            get {
                return addCommand ?? (addCommand = new Command(obj => {
                    object value;

                    if (typeof(T) == typeof(string))
                        value = Activator.CreateInstance(typeof(T), new object[] { "value".ToCharArray() });
                    else if (typeof(T).GetInterface(typeof(ICommand).Name) != null)
                    {
                        Action<object> f = new Action<object>(x => { });
                        value = Activator.CreateInstance(typeof(T), new object[] { f, null });
                    }
                    else
                        value = Activator.CreateInstance(typeof(T));

                    if (Context != null) {
                        Context.Add((T)value);
                        Item<T> item = new Item<T>(Context.Count, (T)value);
                        item.PropertyChanged += new PropertyChangedEventHandler(itemChanged);
                        (ItemsSource as List<Item<T>>).Add(item);
                    }
                    else
                        (ItemsSource as IList<T>).Add((T)value);
                    CollectionViewSource.GetDefaultView(ItemsSource).Refresh();
                }, obj => ItemsSource != null));
            }
        }

        public override Command RemoveCommand {
            get {
                return removeCommand ?? (removeCommand = new Command(obj => {
                    if (Context != null) {
                        Context.RemoveAt((ItemsSource as List<Item<T>>).IndexOf((Item<T>)obj));
                        (ItemsSource as List<Item<T>>).Remove((Item<T>)obj);
                    }
                    else
                        (ItemsSource as IList<T>).Remove((T)obj);
                    CollectionViewSource.GetDefaultView(ItemsSource).Refresh();
                }, obj => obj != null));
            }
        }
        #endregion
    }
}

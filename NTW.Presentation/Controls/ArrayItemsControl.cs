using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace NTW.Presentation
{
    internal class ArrayItemsControl<T> : BaseItemsControl
    {
        public T[] Context {
            get { return (T[])GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public ArrayItemsControl() {
            if (ContextProperty.GetMetadata(typeof(ArrayItemsControl<T>)) as UIPropertyMetadata == null)
                ContextProperty.OverrideMetadata(typeof(ArrayItemsControl<T>), new UIPropertyMetadata((o, e) => {
                    List<Item<T>> temp = new List<Item<T>>();
                    for (int i = 0; i < (e.NewValue as T[]).Length; i++) {
                        Item<T> item = new Item<T>(i, (e.NewValue as T[])[i]);
                        item.PropertyChanged += new PropertyChangedEventHandler((s, ea) => {
                            (o as ArrayItemsControl<T>).Context[(s as Item<T>).Index] = (T)(s as Item<T>).Value;
                        });
                        temp.Add(item);
                    }

                    o.SetValue(ArrayItemsControl<T>.ItemsSourceProperty, temp);
                }));
        }
    }
}

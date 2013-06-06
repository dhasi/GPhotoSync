using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;

namespace GPhotoSync
{
    public static class DialogManagerExtension
    {
        public static object GetDialogManager(DependencyObject obj)
        {
            return (object)obj.GetValue(DialogManagerProperty);
        }

        public static void SetDialogManager(DependencyObject obj, object value)
        {
            obj.SetValue(DialogManagerProperty, value);
        }

        // Using a DependencyProperty as the backing store for DialogManager.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DialogManagerProperty =
            DependencyProperty.RegisterAttached("DialogManager", typeof(object), typeof(DialogManagerExtension),
            new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = d as Panel;
            if (panel == null)
                throw new NotSupportedException("Property only supported on panels.");

            var manager = e.NewValue as IDialogManager;
            if (manager != null)
                manager.Container = panel;
        }
    }

    public interface IDialogManager
    {
        Panel Container { get; set; }
    }

    public class DialogManager : IDialogManager
    {
        #region Fields
        private readonly IMessenger _messenger;
        private readonly List<object> _contentList = new List<object>();
        #endregion Fields

        #region Properties
        public Panel Container { get; set; }
        #endregion Properties

        #region Ctor
        public DialogManager(IMessenger messenger)
        {
            _messenger = messenger;
            _messenger.Register<ShowDialogMessage>(this, OnShowDialog);
            _messenger.Register<CloseDialogMessage>(this, OnCloseDialog);
        }
        #endregion Ctor

        #region Methods
        private void OnShowDialog(ShowDialogMessage msg)
        {
            if (msg == null) return;

            if (!Container.Children.OfType<ContentControl>().Any(x => x.Content == msg.Content))
            {
                _contentList.Add(msg.Content);
                Container.Children.Add(
                    new ContentControl
                    {
                        Content = msg.Content
                    });
            }
        }

        private void OnCloseDialog(CloseDialogMessage msg)
        {
            var control = FindControl(msg.Content);
            if (control != null)
            {
                _contentList.Remove(msg.Content);
                Container.Children.Remove(control);

                //var next = _contentList.FirstOrDefault();
                //if (next != null)
                //{
                //    control = FindControl(next);
                //    if(control != null)
                //        Container.Children.Add(control
                //}
            }
        }

        private UIElement FindControl(object content)
        {
            return Container.Children.OfType<ContentControl>().FirstOrDefault(x => x.Content == content);
        }
        #endregion Methods


    }
}

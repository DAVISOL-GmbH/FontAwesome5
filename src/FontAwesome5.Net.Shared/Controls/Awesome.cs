﻿using FontAwesome5.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FontAwesome5.Controls
{
    /// <summary>
    /// Provides attached properties to set FontAwesome icons on controls.
    /// </summary>
    public static class Awesome
    {
        /// <summary>
        /// Identifies the FontAwesome.WPF.Awesome.Content attached dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.RegisterAttached(
                "Content",
                typeof(EFontAwesomeIcon),
                typeof(Awesome),
                new PropertyMetadata(DEFAULT_CONTENT, ContentChanged));

        /// <summary>
        /// Gets the content of a ContentControl, expressed as a FontAwesome icon.
        /// </summary>
        /// <param name="target">The ContentControl subject of the query</param>
        /// <returns>FontAwesome icon found as content</returns>
        public static EFontAwesomeIcon GetContent(DependencyObject target)
        {
            return (EFontAwesomeIcon)target.GetValue(ContentProperty);
        }
        /// <summary>
        /// Sets the content of a ContentControl expressed as a FontAwesome icon. This will cause the content to be redrawn.
        /// </summary>
        /// <param name="target">The ContentControl where to set the content</param>
        /// <param name="value">FontAwesome icon to set as content</param>
        public static void SetContent(DependencyObject target, EFontAwesomeIcon value)
        {
            target.SetValue(ContentProperty, value);
        }

        private static void ContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs evt)
        {
            // If target is not a ContenControl just ignore: Awesome.Content property can only be set on a ContentControl element
            if (sender is not ContentControl)
            {
                return;
            }

            var target = (ContentControl)sender;

            // If value is not a FontAwesomeIcon just ignore: Awesome.Content property can only be set to a FontAwesomeIcon value
            if (evt.NewValue is not EFontAwesomeIcon)
            {
                return;
            }

            var symbolIcon = (EFontAwesomeIcon)evt.NewValue;
            
            target.FontFamily = symbolIcon.GetFontFamily();
            target.Content = symbolIcon.GetUnicode();
        }

        private const EFontAwesomeIcon DEFAULT_CONTENT = EFontAwesomeIcon.None;
    }
}

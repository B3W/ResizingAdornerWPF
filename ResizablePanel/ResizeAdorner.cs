using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AllowedDragDirections = ResizablePanel.ResizeThumb.AllowedDragDirections;
using ThumbPosition = ResizablePanel.ResizeThumb.ThumbPosition;

namespace ResizablePanel
{
   /// <summary>
   /// Configuration for the resizing adorner
   /// </summary>
   public class ResizeAdornerConfig
   {
      public int ThumbThickness { get; } = 10;

      public double ThumbOpacity { get; } = 1.0D;

      public Brush ThumbColor { get; } = new SolidColorBrush(Colors.Red);
   }


   /// <summary>
   /// Adorner that allows for resizing of the adorned element
   /// </summary>
   public class ResizeAdorner : Adorner
   {
      /// <summary>
      /// Enumeration defining where each thumb resides in the thumbs array
      /// </summary>
      private enum ThumbIndex
      {
         Top = 0,
         Bottom,
         Left,
         Right,
         TopLeft,
         TopRight,
         BottomLeft,
         BottomRight,
         NumThumbs
      }


      #region Fields

      /// <summary>
      /// Collection of all visual children managed by adorner
      /// </summary>
      private readonly VisualCollection _visualChildren;

      /// <summary>
      /// Collection of all thumbs that can be used to resize adorned element
      /// </summary>
      private readonly Thumb[] _resizeThumbs;

      /// <summary>
      /// Mapping of configuration of thumb position to allowed drag direction
      /// </summary>
      private readonly Dictionary<ThumbPosition, AllowedDragDirections> dragDirectionMap
         = new Dictionary<ThumbPosition, AllowedDragDirections>()
      {
         { ThumbPosition.Top,          AllowedDragDirections.Vertical },
         { ThumbPosition.Bottom,       AllowedDragDirections.Vertical },
         { ThumbPosition.Left,         AllowedDragDirections.Horizontal },
         { ThumbPosition.Right,        AllowedDragDirections.Horizontal },
         { ThumbPosition.TopLeft,      AllowedDragDirections.All },
         { ThumbPosition.TopRight,     AllowedDragDirections.All },
         { ThumbPosition.BottomLeft,   AllowedDragDirections.All },
         { ThumbPosition.BottomRight,  AllowedDragDirections.All },
      };

      /// <summary>
      /// Adorned element as a framework element. Gives access to some layout properties.
      /// </summary>
      private readonly FrameworkElement _adornedFrameworkElement;

      /// <summary>
      /// Configuration of adorner
      /// </summary>
      private readonly ResizeAdornerConfig _config;

      /// <summary>
      /// Minimium width of the adorned element
      /// </summary>
      private readonly double _minWidth;

      /// <summary>
      /// Minimum height of the adorned element
      /// </summary>
      private readonly double _minHeight;

      /// <summary>
      /// Cache that holds coordinates for resizing thumbs
      /// </summary>
      private readonly Rect[] _thumbCoordinates;

      #endregion // Fields


      #region Properties

      // Overriding FrameworkElement property as this adorner will contain more than one visual
      // Allows WPF framework to interface with this adorner's visual collection
      protected override int VisualChildrenCount => _visualChildren.Count;

      #endregion // Properties


      #region Methods

      /// <summary>
      /// Constructs a ResizeAdorner attached to the given UIElement
      /// </summary>
      /// <param name="adornedElement">UIElement to adorn. Must be child of Canvas.</param>
      /// <param name="adornerConfig">Configuration of adorner</param>
      /// <exception cref="ArgumentException"></exception>
      public ResizeAdorner(UIElement adornedElement, ResizeAdornerConfig adornerConfig) : base(adornedElement)
      {
         _adornedFrameworkElement = adornedElement as FrameworkElement;
         _config = adornerConfig;

         if (!(_adornedFrameworkElement.Parent is Canvas))
         {
            throw new ArgumentException("Adorned element must be the child of a Canvas", "adornedElement");
         }

         _visualChildren = new VisualCollection(adornedElement);
         _resizeThumbs = new Thumb[(int)ThumbIndex.NumThumbs];

         // Create all resize thumbs
         _resizeThumbs[(int)ThumbIndex.Top] = CreateResizeThumb(ThumbPosition.Top, Cursors.SizeNS);
         _resizeThumbs[(int)ThumbIndex.Bottom] = CreateResizeThumb(ThumbPosition.Bottom, Cursors.SizeNS);
         _resizeThumbs[(int)ThumbIndex.Left] = CreateResizeThumb(ThumbPosition.Left, Cursors.SizeWE);
         _resizeThumbs[(int)ThumbIndex.Right] = CreateResizeThumb(ThumbPosition.Right, Cursors.SizeWE);
         _resizeThumbs[(int)ThumbIndex.TopLeft] = CreateResizeThumb(ThumbPosition.TopLeft, Cursors.SizeNWSE);
         _resizeThumbs[(int)ThumbIndex.TopRight] = CreateResizeThumb(ThumbPosition.TopRight, Cursors.SizeNESW);
         _resizeThumbs[(int)ThumbIndex.BottomLeft] = CreateResizeThumb(ThumbPosition.BottomLeft, Cursors.SizeNESW);
         _resizeThumbs[(int)ThumbIndex.BottomRight] = CreateResizeThumb(ThumbPosition.BottomRight, Cursors.SizeNWSE);

         // Add thumbs to the visual children of the adorner
         foreach (Thumb thumb in _resizeThumbs)
         {
            _ = _visualChildren.Add(thumb);
         }

         // Create starting coordinates for all thumbs that will be updated in ArrangeOverride
         // Most thumbs only need one coordinate change during arrange pass since they are arranged relative to the adorned control
         _thumbCoordinates = new Rect[(int)ThumbIndex.NumThumbs];

         _thumbCoordinates[(int)ThumbIndex.Top]         = new Rect(                         0.0D, -(_config.ThumbThickness / 2), _config.ThumbThickness, _config.ThumbThickness);
         _thumbCoordinates[(int)ThumbIndex.Bottom]      = new Rect(                         0.0D,                          0.0D, _config.ThumbThickness, _config.ThumbThickness);
         _thumbCoordinates[(int)ThumbIndex.Left]        = new Rect(-(_config.ThumbThickness / 2),                          0.0D, _config.ThumbThickness, _config.ThumbThickness);
         _thumbCoordinates[(int)ThumbIndex.Right]       = new Rect(                         0.0D,                          0.0D, _config.ThumbThickness, _config.ThumbThickness);
         _thumbCoordinates[(int)ThumbIndex.TopLeft]     = new Rect(-(_config.ThumbThickness / 2), -(_config.ThumbThickness / 2), _config.ThumbThickness, _config.ThumbThickness);
         _thumbCoordinates[(int)ThumbIndex.TopRight]    = new Rect(                         0.0D, -(_config.ThumbThickness / 2), _config.ThumbThickness, _config.ThumbThickness);
         _thumbCoordinates[(int)ThumbIndex.BottomLeft]  = new Rect(-(_config.ThumbThickness / 2),                          0.0D, _config.ThumbThickness, _config.ThumbThickness);
         _thumbCoordinates[(int)ThumbIndex.BottomRight] = new Rect(                         0.0D,                          0.0D, _config.ThumbThickness, _config.ThumbThickness);

         // Set the minimum dimensions for the adorned element
         _minWidth = _adornedFrameworkElement.MinWidth.Equals(double.NaN) ? _config.ThumbThickness * 3
                                                                          : _adornedFrameworkElement.MinWidth;
         _minHeight = _adornedFrameworkElement.MinHeight.Equals(double.NaN) ? _config.ThumbThickness * 3
                                                                            : _adornedFrameworkElement.MinHeight;
         MinWidth = _minWidth;
         MinHeight = _minHeight;
      }


      // Overriding FrameworkElement method to place adorners in correct location
      protected override Size ArrangeOverride(Size finalSize)
      {
         // Calculate new coordinates for thumbs
         // NOTE: Use 'finalSize' instead of 'DesiredSize'. 'DesiredSize' does not behave well with zooming
         _thumbCoordinates[(int)ThumbIndex.Top].X = (finalSize.Width / 2.0D) - (_config.ThumbThickness / 2);
         _thumbCoordinates[(int)ThumbIndex.Bottom].X = (finalSize.Width / 2.0D) - (_config.ThumbThickness / 2);
         _thumbCoordinates[(int)ThumbIndex.Bottom].Y = finalSize.Height - (_config.ThumbThickness / 2);
         _thumbCoordinates[(int)ThumbIndex.Left].Y = (finalSize.Height / 2.0D) - (_config.ThumbThickness / 2);
         _thumbCoordinates[(int)ThumbIndex.Right].X = finalSize.Width - (_config.ThumbThickness / 2);
         _thumbCoordinates[(int)ThumbIndex.Right].Y = (finalSize.Height / 2.0D) - (_config.ThumbThickness / 2);
         //_thumbCoordinates[(int)ThumbIndex.TopLeft] -> No Change
         _thumbCoordinates[(int)ThumbIndex.TopRight].X = finalSize.Width - (_config.ThumbThickness / 2);
         _thumbCoordinates[(int)ThumbIndex.BottomLeft].Y = finalSize.Height - (_config.ThumbThickness / 2);
         _thumbCoordinates[(int)ThumbIndex.BottomRight].X = finalSize.Width - (_config.ThumbThickness / 2);
         _thumbCoordinates[(int)ThumbIndex.BottomRight].Y = finalSize.Height - (_config.ThumbThickness / 2);

         // Apply new coordinates to thumbs
         _resizeThumbs[(int)ThumbIndex.Top].Arrange(_thumbCoordinates[(int)ThumbIndex.Top]);
         _resizeThumbs[(int)ThumbIndex.Bottom].Arrange(_thumbCoordinates[(int)ThumbIndex.Bottom]);
         _resizeThumbs[(int)ThumbIndex.Left].Arrange(_thumbCoordinates[(int)ThumbIndex.Left]);
         _resizeThumbs[(int)ThumbIndex.Right].Arrange(_thumbCoordinates[(int)ThumbIndex.Right]);
         _resizeThumbs[(int)ThumbIndex.TopLeft].Arrange(_thumbCoordinates[(int)ThumbIndex.TopLeft]);
         _resizeThumbs[(int)ThumbIndex.TopRight].Arrange(_thumbCoordinates[(int)ThumbIndex.TopRight]);
         _resizeThumbs[(int)ThumbIndex.BottomLeft].Arrange(_thumbCoordinates[(int)ThumbIndex.BottomLeft]);
         _resizeThumbs[(int)ThumbIndex.BottomRight].Arrange(_thumbCoordinates[(int)ThumbIndex.BottomRight]);

         return finalSize;
      }


      // Overriding FrameworkElement method as this adorner will contain more than one visual
      // Allows WPF framework to interface with this adorner's visual collection
      protected override Visual GetVisualChild(int index)
      {
         return _visualChildren[index];
      }


      /// <summary>
      /// Creates a thumb that can be used to resize adorned element in specified direction
      /// </summary>
      /// <returns>Thumb that can be used to resize</returns>
      private ResizeThumb CreateResizeThumb(ThumbPosition position, Cursor cursor)
      {
         ResizeThumb resizeThumb = new ResizeThumb(position, dragDirectionMap[position])
         {
            Cursor = cursor,
            Background = _config.ThumbColor,
            Opacity = _config.ThumbOpacity,
         };

         resizeThumb.DragStarted += OnDragStarted;
         resizeThumb.DragDelta += OnDragDelta;
         resizeThumb.DragCompleted += OnDragCompleted;

         return resizeThumb;
      }


      /// <summary>
      /// Handler for Thumb DragStarted event
      /// </summary>
      /// <param name="sender">Thumb which started drag</param>
      /// <param name="e">Information on drag start</param>
      private void OnDragStarted(object sender, DragStartedEventArgs e)
      {
         // Implement as needed
      }


      /// <summary>
      /// Handler for Thumb DragDelta event
      /// </summary>
      /// <param name="sender">Thumb which was dragged</param>
      /// <param name="e">Information on drag</param>
      private void OnDragDelta(object sender, DragDeltaEventArgs e)
      {
         ResizeThumb resizeThumb = (ResizeThumb)sender;

         // Calculate the change in size
         double deltaX = 0.0D;
         double deltaY = 0.0D;

         switch (resizeThumb.DragDirections)
         {
            case AllowedDragDirections.Up:
               deltaY = Math.Max(0.0D, e.VerticalChange);
               break;

            case AllowedDragDirections.Down:
               deltaY = Math.Min(0.0D, e.VerticalChange);
               break;

            case AllowedDragDirections.Left:
               deltaX = Math.Min(0.0D, e.HorizontalChange);
               break;

            case AllowedDragDirections.Right:
               deltaX = Math.Max(0.0D, e.HorizontalChange);
               break;

            case AllowedDragDirections.Vertical:
               deltaY = e.VerticalChange;
               break;

            case AllowedDragDirections.Horizontal:
               deltaX = e.HorizontalChange;
               break;

            case AllowedDragDirections.All:
               deltaX = e.HorizontalChange;
               deltaY = e.VerticalChange;
               break;

            default:
               // Do nothing
               break;
         }

         ResizeElement(resizeThumb.Position, deltaX, deltaY);
      }


      /// <summary>
      /// Handler for Thumb DragCompleted event
      /// </summary>
      /// <param name="sender">Thumb which was dragged</param>
      /// <param name="e">Information on drag completion</param>
      private void OnDragCompleted(object sender, DragCompletedEventArgs e)
      {
         // Implement as needed
      }


      /// <summary>
      /// Calculates how the element should be resized based on thumb dragged and drag amount
      /// </summary>
      /// <param name="thumbPosition">Position of Thumb that was dragged</param>
      /// <param name="deltaX">Amount thumb was dragged in X direction</param>
      /// <param name="deltaY">Amount thumb was dragged in Y direction</param>
      private void ResizeElement(ThumbPosition thumbPosition, double deltaX, double deltaY)
      {
         // Resize vertically
         if ((thumbPosition & ThumbPosition.Top) == ThumbPosition.Top)
         {
            double newHeight = _adornedFrameworkElement.Height - deltaY;

            if (newHeight > _minHeight)
            {
               Canvas.SetTop(AdornedElement, Canvas.GetTop(AdornedElement) + deltaY);
               _adornedFrameworkElement.Height = newHeight;
            }
            else
            {
               _adornedFrameworkElement.Height = _minHeight;
            }
         }
         else if ((thumbPosition & ThumbPosition.Bottom) == ThumbPosition.Bottom)
         {
            _adornedFrameworkElement.Height = Math.Max(_adornedFrameworkElement.Height + deltaY, _minHeight);
         }

         // Resize horizontally
         if ((thumbPosition & ThumbPosition.Left) == ThumbPosition.Left)
         {
            double newWidth = _adornedFrameworkElement.Width - deltaX;

            if (newWidth > _minWidth)
            {
               Canvas.SetLeft(AdornedElement, Canvas.GetLeft(AdornedElement) + deltaX);
               _adornedFrameworkElement.Width = newWidth;
            }
            else
            {
               _adornedFrameworkElement.Width = _minWidth;
            }
         }
         else if ((thumbPosition & ThumbPosition.Right) == ThumbPosition.Right)
         {
            _adornedFrameworkElement.Width = Math.Max(_adornedFrameworkElement.Width + deltaX, _minWidth);
         }
      }

      #endregion // Methods
   }
}

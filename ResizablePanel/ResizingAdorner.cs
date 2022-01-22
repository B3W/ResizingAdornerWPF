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

   public class ResizingAdornerConfig
   {
      public int ThumbThickness { get; } = 10;

      public double ThumbOpacity { get; } = 1.0D;

      public Brush ThumbColor { get; } = new SolidColorBrush(Colors.Red);
   }



   public class ResizingAdorner : Adorner
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


      private readonly FrameworkElement _adornedFrameworkElement;


      private readonly ResizingAdornerConfig _config;


      private readonly double _minWidth;


      private readonly double _minHeight;

      #endregion // Fields


      #region Properties

      // Overriding FrameworkElement property as this adorner will contain more than one visual
      // Allows WPF framework to interface with this adorner's visual collection
      protected override int VisualChildrenCount => _visualChildren.Count;


      private Rect TopThumbRect => new Rect((DesiredSize.Width / 2.0D) - (_config.ThumbThickness / 2),
                                            -(_config.ThumbThickness / 2),
                                            _config.ThumbThickness,
                                            _config.ThumbThickness);

      private Rect BottomThumbRect => new Rect((DesiredSize.Width / 2.0D) - (_config.ThumbThickness / 2),
                                               DesiredSize.Height - (_config.ThumbThickness / 2),
                                               _config.ThumbThickness,
                                               _config.ThumbThickness);

      private Rect LeftThumbRect => new Rect(-(_config.ThumbThickness / 2),
                                             (DesiredSize.Height / 2.0D) - (_config.ThumbThickness / 2),
                                             _config.ThumbThickness,
                                             _config.ThumbThickness);

      private Rect RightThumbRect => new Rect(DesiredSize.Width - (_config.ThumbThickness / 2),
                                              (DesiredSize.Height / 2.0D) - (_config.ThumbThickness / 2),
                                              _config.ThumbThickness,
                                              _config.ThumbThickness);

      private Rect TopLeftThumbRect => new Rect(-(_config.ThumbThickness / 2),
                                                -(_config.ThumbThickness / 2),
                                                _config.ThumbThickness,
                                                _config.ThumbThickness);

      private Rect TopRightThumbRect => new Rect(DesiredSize.Width - (_config.ThumbThickness / 2),
                                                 -(_config.ThumbThickness / 2),
                                                 _config.ThumbThickness,
                                                 _config.ThumbThickness);

      private Rect BottomLeftThumbRect => new Rect(-(_config.ThumbThickness / 2),
                                                   DesiredSize.Height - (_config.ThumbThickness / 2),
                                                   _config.ThumbThickness,
                                                   _config.ThumbThickness);

      private Rect BottomRightThumbRect => new Rect(DesiredSize.Width - (_config.ThumbThickness / 2),
                                                    DesiredSize.Height - (_config.ThumbThickness / 2),
                                                    _config.ThumbThickness,
                                                    _config.ThumbThickness);

      #endregion // Properties


      #region Methods

      public ResizingAdorner(UIElement adornedElement, ResizingAdornerConfig adornerConfig) : base(adornedElement)
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

         // Set the minimum dimensions for the adorned element
         _minWidth = _adornedFrameworkElement.MinWidth.Equals(double.NaN) ? _config.ThumbThickness * 3
                                                                          : _adornedFrameworkElement.MinWidth;
         _minHeight = _adornedFrameworkElement.MinHeight.Equals(double.NaN) ? _config.ThumbThickness * 3
                                                                            : _adornedFrameworkElement.MinHeight;
         MinWidth = _minWidth;
         MinHeight = _minHeight;
      }



      protected override Size ArrangeOverride(Size finalSize)
      {
         _resizeThumbs[(int)ThumbIndex.Top].Arrange(TopThumbRect);
         _resizeThumbs[(int)ThumbIndex.Bottom].Arrange(BottomThumbRect);
         _resizeThumbs[(int)ThumbIndex.Left].Arrange(LeftThumbRect);
         _resizeThumbs[(int)ThumbIndex.Right].Arrange(RightThumbRect);
         _resizeThumbs[(int)ThumbIndex.TopLeft].Arrange(TopLeftThumbRect);
         _resizeThumbs[(int)ThumbIndex.TopRight].Arrange(TopRightThumbRect);
         _resizeThumbs[(int)ThumbIndex.BottomLeft].Arrange(BottomLeftThumbRect);
         _resizeThumbs[(int)ThumbIndex.BottomRight].Arrange(BottomRightThumbRect);

         return finalSize;
      }


      // Overriding FrameworkElement property as this adorner will contain more than one visual
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



      private void OnDragStarted(object sender, DragStartedEventArgs e)
      {
         // TODO
      }



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



      private void OnDragCompleted(object sender, DragCompletedEventArgs e)
      {
         // TODO
      }



      private void ResizeElement(ThumbPosition thumbPosition, double deltaX, double deltaY)
      {
         // Resize vertically
         if ((thumbPosition & ThumbPosition.Top) == ThumbPosition.Top)
         {
            Canvas.SetTop(AdornedElement, Canvas.GetTop(AdornedElement) + deltaY);
            _adornedFrameworkElement.Height -= deltaY;
         }
         else if ((thumbPosition & ThumbPosition.Bottom) == ThumbPosition.Bottom)
         {
            _adornedFrameworkElement.Height += deltaY;
         }

         // Resize horizontally
         if ((thumbPosition & ThumbPosition.Left) == ThumbPosition.Left)
         {
            Canvas.SetLeft(AdornedElement, Canvas.GetLeft(AdornedElement) + deltaX);
            _adornedFrameworkElement.Width -= deltaX;
         }
         else if ((thumbPosition & ThumbPosition.Right) == ThumbPosition.Right)
         {
            _adornedFrameworkElement.Width += deltaX;
         }
      }

      #endregion // Methods
   }
}

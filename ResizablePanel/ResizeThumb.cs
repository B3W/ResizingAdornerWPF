using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace ResizablePanel
{
   public class ResizeThumb : Thumb
   {
      [Flags]
      public enum ThumbPosition
      {
         Top = 0b0001,
         Bottom = 0b0010,
         Left = 0b0100,
         Right = 0b1000,
         TopLeft = Top | Left,
         TopRight = Top | Right,
         BottomLeft = Bottom | Left,
         BottomRight = Bottom | Right
      }


      [Flags]
      public enum AllowedDragDirections
      {
         Up = 0b0001,
         Down = 0b0010,
         Left = 0b0100,
         Right = 0b1000,
         Vertical = Up | Down,
         Horizontal = Left | Right,
         All = Up | Down | Left | Right
      }


      #region Fields
      #endregion // Fields


      #region Properties

      /// <summary>
      /// Position of the resize thumb in relation to the adorned element
      /// </summary>
      public ThumbPosition Position { get; }

      /// <summary>
      /// Bit field indicating the directions this thumb can be dragged
      /// </summary>
      public AllowedDragDirections DragDirections { get; }

      #endregion // Properties


      #region Methods


      public ResizeThumb(ThumbPosition thumbPosition, AllowedDragDirections allowedDragDirections)
      {
         if (!IsValidPosition(thumbPosition))
         {
            throw new ArgumentException($"0x{thumbPosition:X1} is an invalid thumb position", "thumbPosition");
         }

         Position = thumbPosition;
         DragDirections = allowedDragDirections;
      }



      private bool IsValidPosition(ThumbPosition thumbPosition)
      {
         // Invalid positions
         const ThumbPosition topBottom = ThumbPosition.Top | ThumbPosition.Bottom;
         const ThumbPosition leftRight = ThumbPosition.Left | ThumbPosition.Right;

         return ((thumbPosition & topBottom) != topBottom) && ((thumbPosition & leftRight) != leftRight);
      }

      #endregion // Methods
   }
}

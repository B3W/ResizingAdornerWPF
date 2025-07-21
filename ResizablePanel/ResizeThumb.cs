//
// Copyright (c) 2025 Weston Berg
//
// SPDX-License-Identifier: MIT
//
using System;
using System.Windows.Controls.Primitives;

namespace ResizablePanel
{
   /// <summary>
   /// Derived from the Thumb class. Implements thumbs that can be used for resizing an adorned element.
   /// </summary>
   public class ResizeThumb : Thumb
   {
      /// <summary>
      /// Bitfield describing where the thumb is positioned in relation to the adorned element
      /// </summary>
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

      /// <summary>
      /// Bitfield describing the directions the thumb can be resized in relation to the adorned element
      /// </summary>
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

      /// <summary>
      /// Constructs instance of ResizeThumb
      /// </summary>
      /// <param name="thumbPosition">Position for this thumb</param>
      /// <param name="allowedDragDirections">Directions this thumb can be resized</param>
      /// <exception cref="ArgumentException"></exception>
      public ResizeThumb(ThumbPosition thumbPosition, AllowedDragDirections allowedDragDirections)
      {
         if (!IsValidPosition(thumbPosition))
         {
            throw new ArgumentException($"'{thumbPosition}' is an invalid thumb position", "thumbPosition");
         }

         Position = thumbPosition;
         DragDirections = allowedDragDirections;
      }


      /// <summary>
      /// Helper method for checking if given thumb positions are valid
      /// </summary>
      /// <param name="thumbPosition">Thumb position to validate</param>
      /// <returns>true if valid, false otherwise</returns>
      private bool IsValidPosition(ThumbPosition thumbPosition)
      {
         // Invalid position definitions
         const ThumbPosition topBottom = ThumbPosition.Top | ThumbPosition.Bottom;
         const ThumbPosition leftRight = ThumbPosition.Left | ThumbPosition.Right;

         return ((thumbPosition & topBottom) != topBottom) && ((thumbPosition & leftRight) != leftRight);
      }

      #endregion // Methods
   }
}

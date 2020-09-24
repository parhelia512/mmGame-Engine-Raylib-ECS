﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Raylib_cs;

namespace mmGameEngine
{
    /*
     * UI element that is attached to an entity
     */
    public class Label : RenderComponent
    {

        public Color TextColor = Color.WHITE;
        public Color BackgroundColor = Color.GRAY;
        public Color BorderColor = Color.WHITE;
        public Vector2 Position;

        Color CurrentBackgroundColor;
        Color CurrentTextColor;
        public TextInfo TextData;

        //Vector2 textPosition;
        //int width;
        //int height;
        string content;
        
        public Label(Vector2 _position, string _content)
        {
            content = _content;
            TextData = new TextInfo(_content, TextFontTypes.Default, 25, Color.WHITE);
            CurrentBackgroundColor = BackgroundColor;
            CurrentTextColor = TextData.FontColor;
            CompPosition = _position;
        }
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            //
            // If component is attached to an Entity, the obey his position
            //
            if (CompEntity != null)
            {
                CompPosition = Transform.Position;
            }
        }
        public override void Render()
        {
            base.Render();
            //
            // Todo: use this to wrap words
            //
            //Raylib.DrawTextRec()
            //
            // Draw the text
            //
            Raylib.DrawTextEx(TextData.TextFont,
                  TextData.Content,
                  CompPosition,
                  (float)TextData.FontSize,
                  0,
                  CurrentTextColor);
        }
    }
}
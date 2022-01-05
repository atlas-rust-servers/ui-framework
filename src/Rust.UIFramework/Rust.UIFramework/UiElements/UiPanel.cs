﻿using Newtonsoft.Json;
using UI.Framework.Rust.Colors;
using UI.Framework.Rust.Components;
using UI.Framework.Rust.Json;
using UI.Framework.Rust.Positions;
using Pool = Facepunch.Pool;

namespace UI.Framework.Rust.UiElements
{
    public class UiPanel : BaseUiComponent
    {
        public bool NeedsCursor;
        public ImageComponent Image = Pool.Get<ImageComponent>();

        public void AddSprite(string sprite)
        {
            Image.Sprite = sprite;
        }

        public void AddMaterial(string material)
        {
            Image.Material = material;
        }

        public static UiPanel Create(UiPosition pos, UiOffset offset, UiColor color)
        {
            UiPanel panel = CreateBase<UiPanel>(pos, offset);
            panel.Image.Color = color;
            return panel;
        }

        public static UiPanel Create(Position pos, Offset? offset, UiColor color)
        {
            UiPanel panel = CreateBase<UiPanel>(pos, offset);
            panel.Image.Color = color;
            return panel;
        }

        public override void WriteComponents(JsonTextWriter writer)
        {
            JsonCreator.Add(writer, Image);
            base.WriteComponents(writer);

            if (NeedsCursor)
            {
                JsonCreator.AddCursor(writer);
            }
        }

        public override void EnterPool()
        {
            base.EnterPool();
            NeedsCursor = false;
            Pool.Free(ref Image);
        }

        public override void LeavePool()
        {
            base.LeavePool();
            Image = Pool.Get<ImageComponent>();
        }
    }
}
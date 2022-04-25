﻿using Newtonsoft.Json;
using Oxide.Ext.UiFramework.Json;
using Oxide.Ext.UiFramework.Offsets;
using Oxide.Ext.UiFramework.Positions;
using Pool = Facepunch.Pool;

namespace Oxide.Ext.UiFramework.UiElements
{
    public abstract class BaseUiComponent : Pool.IPooled
    {
        public string Name;
        public string Parent;
        public float FadeOut;
        public Position Position;
        public Offset? Offset;
        private bool _inPool = true;

        protected static T CreateBase<T>(UiPosition pos, UiOffset offset) where T : BaseUiComponent, new()
        {
            T component = Pool.Get<T>();
            component.Position = pos.ToPosition();
            component.Offset = offset?.ToOffset();
            if (component._inPool)
            {
                component.LeavePool();
            }
            return component;
        }

        protected static T CreateBase<T>(Position pos, Offset? offset) where T : BaseUiComponent, new()
        {
            T component = Pool.Get<T>();
            component.Position = pos;
            component.Offset = offset;
            if (component._inPool)
            {
                component.LeavePool();
            }
            return component;
        }

        protected static T CreateBase<T>(UiPosition pos) where T : BaseUiComponent, new()
        {
            T component = Pool.Get<T>();
            component.Position = pos.ToPosition();
            if (component._inPool)
            {
                component.LeavePool();
            }
            return component;
        }

        public void WriteRootComponent(JsonTextWriter writer, bool needsMouse, bool needsKeyboard)
        {
            writer.WriteStartObject();
            JsonCreator.AddFieldRaw(writer, JsonDefaults.ComponentName, Name);
            JsonCreator.AddFieldRaw(writer, JsonDefaults.ParentName, Parent);
            JsonCreator.AddField(writer, JsonDefaults.FadeOutName, FadeOut, JsonDefaults.FadeOutValue);

            writer.WritePropertyName("components");
            writer.WriteStartArray();
            WriteComponents(writer);

            if (needsMouse)
            {
                JsonCreator.AddMouse(writer);
            }

            if (needsKeyboard)
            {
                JsonCreator.AddKeyboard(writer);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public void WriteComponent(JsonTextWriter writer)
        {
            writer.WriteStartObject();
            JsonCreator.AddFieldRaw(writer, JsonDefaults.ComponentName, Name);
            JsonCreator.AddFieldRaw(writer, JsonDefaults.ParentName, Parent);
            JsonCreator.AddField(writer, JsonDefaults.FadeOutName, FadeOut, JsonDefaults.FadeOutValue);

            writer.WritePropertyName("components");
            writer.WriteStartArray();
            WriteComponents(writer);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        
        protected virtual void WriteComponents(JsonTextWriter writer)
        {
            writer.WriteStartObject();
            JsonCreator.AddFieldRaw(writer, JsonDefaults.ComponentTypeName, JsonDefaults.RectTransformName);
            JsonCreator.AddField(writer, JsonDefaults.AnchorMinName, Position.Min, JsonDefaults.AnchorMin, Position.MinString);
            JsonCreator.AddField(writer, JsonDefaults.AnchorMaxName, Position.Max, JsonDefaults.AnchorMax, Position.MaxString);

            if (Offset.HasValue)
            {
                Offset offset = Offset.Value;
                JsonCreator.AddField(writer, JsonDefaults.OffsetMinName, offset.Min, JsonDefaults.OffsetMin, offset.MinString);
                JsonCreator.AddField(writer, JsonDefaults.OffsetMaxName, offset.Max, JsonDefaults.OffsetMax, offset.MaxString);
            }
            else
            {
                //Fixes issue with UI going outside of bounds
                JsonCreator.AddFieldRaw(writer, JsonDefaults.OffsetMaxName, JsonDefaults.DefaultOffsetMax);
            }

            writer.WriteEndObject();
        }

        public void SetFadeOut(float duration)
        {
            FadeOut = duration;
        }

        public abstract void SetFadeIn(float duration);

        public virtual void EnterPool()
        {
            Name = null;
            Parent = null;
            FadeOut = 0;
            Position = default(Position);
            Offset = null;
            _inPool = true;
        }

        public virtual void LeavePool()
        {
            _inPool = false;
        }
    }
}
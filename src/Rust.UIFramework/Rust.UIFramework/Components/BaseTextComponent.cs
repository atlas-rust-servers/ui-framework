﻿using Newtonsoft.Json;
using Oxide.Ext.UiFramework.Json;
using UnityEngine;

namespace Oxide.Ext.UiFramework.Components
{
    public class BaseTextComponent : FadeInComponent
    {
        public int FontSize = JsonDefaults.BaseText.FontSize;
        public string Font;
        public TextAnchor Align;
        public string Text;

        public override void WriteComponent(JsonTextWriter writer)
        {
            JsonCreator.AddTextField(writer, JsonDefaults.BaseText.TextName, Text);
            JsonCreator.AddField(writer, JsonDefaults.BaseText.FontSizeName, FontSize, JsonDefaults.BaseText.FontSize);
            JsonCreator.AddField(writer, JsonDefaults.BaseText.FontName, Font, JsonDefaults.BaseText.FontValue);
            JsonCreator.AddField(writer, JsonDefaults.BaseText.AlignName, Align);
            base.WriteComponent(writer);
        }

        public override void EnterPool()
        {
            base.EnterPool();
            FontSize = JsonDefaults.BaseText.FontSize;
            Font = null;
            Align = TextAnchor.UpperLeft;
            Text = null;
        }
    }
}
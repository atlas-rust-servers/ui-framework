﻿namespace Oxide.Ext.UiFramework.Json
{
    public static class JsonDefaults
    {
        public static class Common
        {
            public const string ComponentTypeName = "type";
            public const string ComponentName = "name";
            public const string ParentName = "parent";
            public const string FadeInName = "fadeIn";
            public const string FadeOutName = "fadeOut";
            public const float FadeOut = 0;
            public const float FadeIn = 0;
            public const string RectTransformName = "RectTransform";
            public const string NullValue = null;
            public const string NeedsCursorValue = "NeedsCursor";
            public const string NeedsKeyboardValue = "NeedsKeyboard";
            public const string CommandName = "command";
        }
        
        public static class Position
        {
            public const string AnchorMinName = "anchormin";
            public const string AnchorMaxName = "anchormax";
        }

        public static class Offset
        {
            public const string OffsetMinName = "offsetmin";
            public const string OffsetMaxName = "offsetmax";
            public const string DefaultOffsetMax = "0 0";
        }

        public static class Color
        {
            public const string ColorName = "color";
            public const uint ColorValue = 0xFFFFFFFF;
        }

        public static class BaseImage
        {
            public const string SpriteName = "sprite";
            public const string MaterialName = "material";
            public const string Sprite = "Assets/Content/UI/UI.Background.Tile.psd";
            public const string Material = "Assets/Icons/IconMaterial.mat";
        }

        public static class RawImage
        {
            public const string TextureValue = "Assets/Icons/rust.png";
        }

        public static class BaseText
        {
            public const int FontSize = 14;
            public const string FontValue = "RobotoCondensed-Bold.ttf";
            public const string FontName = "font";
            public const string TextName = "text";
            public const string FontSizeName = "fontSize";
            public const string AlignName = "align";
        }

        public static class Outline
        {
            public const string DistanceName = "distance";
            public const string UseGraphicAlphaName = "useGraphicAlpha";
        }

        public static class Button
        {
            public const string CloseName = "close";
        }

        public static class Image
        {
            public const string PngName = "png";
            public const string UrlName = "url";
            public const string ImageType = "imagetype";
        }

        public static class ItemIcon
        {
            public const string ItemIdName = "itemid";
            public const string SkinIdName = "skinid";
        }

        public static class Input
        {
            public const string CharacterLimitName = "characterLimit";
            public const int CharacterLimitValue = 0;
            public const string PasswordName = "password";
            public const string ReadOnlyName = "readOnly";
            public const string LineTypeName = "lineType";
            public const string InputNeedsKeyboardName = "needsKeyboard";
            public const string AutoFocusName = "autofocus";
        }
        
        public static class Countdown
        {
            public const string StartTimeName = "startTime";
            public const int StartTimeValue = 0;
            public const string EndTimeName = "endTime";
            public const int EndTimeValue = 0;
            public const string StepName = "step";
            public const int StepValue = 1;
            public const string CountdownCommandName = "command";
        }
    }
}
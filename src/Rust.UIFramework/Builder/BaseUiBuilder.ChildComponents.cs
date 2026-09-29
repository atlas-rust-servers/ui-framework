using Oxide.Ext.UiFramework.Colors;
using Oxide.Ext.UiFramework.Components;
using Oxide.Ext.UiFramework.Json;
using Oxide.Ext.UiFramework.UiElements;

namespace Oxide.Ext.UiFramework.Builder;

public abstract partial class BaseUiBuilder
{
    #region ColorBlock
    public ColorBlockComponent ColorBlock(UiButton button, in UiColor? highlightColor = null, in UiColor? pressedColor = null, in UiColor? selectedColor = null, in float? colorMultiplier = null, in float? fadeDuration = null)
    {
        return ColorBlock(button, highlightColor, pressedColor, selectedColor, null, colorMultiplier, fadeDuration);
    }

    public ColorBlockComponent ColorBlock(UiButton button, in UiColor? highlightColor, in UiColor? pressedColor, in UiColor? selectedColor, in UiColor? disabledColor, in float? colorMultiplier, in float? fadeDuration)
    {
        return button.AddColorBlock(highlightColor, pressedColor, selectedColor, disabledColor, colorMultiplier, fadeDuration);
    }
    #endregion

    #region ScrollBar
    public (ScrollbarComponent horizontal, ScrollbarComponent vertical) AddScrollBars(UiScrollView view, bool invert, bool autoHide, string handleSprite, string trackSprite, float size,
        UiColor? handleColor, UiColor? highlightColor, UiColor? pressedColor, UiColor? trackColor)
    {
        return AddScrollBars(view, invert, autoHide, handleSprite, trackSprite, size, handleColor, highlightColor, pressedColor, trackColor, JsonDefaults.ScrollBar.FadeDuration);
    }

    public (ScrollbarComponent horizontal, ScrollbarComponent vertical) AddScrollBars(UiScrollView view, bool invert = false, bool autoHide = false, string handleSprite = null, string trackSprite = null, float size = JsonDefaults.ScrollBar.Size,
        UiColor? handleColor = null, UiColor? highlightColor = null, UiColor? pressedColor = null, UiColor? trackColor = null, float fadeDuration = JsonDefaults.ScrollBar.FadeDuration)
    {
        return view.AddScrollBars(invert, autoHide, handleSprite, trackSprite, size, handleColor, highlightColor, pressedColor, trackColor, fadeDuration);
    }
    
    public ScrollbarComponent AddHorizontalScrollBar(UiScrollView view, bool invert, bool autoHide, string handleSprite, string trackSprite, float size,
        UiColor? handleColor, UiColor? highlightColor, UiColor? pressedColor, UiColor? trackColor)
    {
        return AddHorizontalScrollBar(view, invert, autoHide, handleSprite, trackSprite, size, handleColor, highlightColor, pressedColor, trackColor, JsonDefaults.ScrollBar.FadeDuration);
    }

    public ScrollbarComponent AddHorizontalScrollBar(UiScrollView view, bool invert = false, bool autoHide = false, string handleSprite = null, string trackSprite = null, float size = JsonDefaults.ScrollBar.Size,
        UiColor? handleColor = null, UiColor? highlightColor = null, UiColor? pressedColor = null, UiColor? trackColor = null, float fadeDuration = JsonDefaults.ScrollBar.FadeDuration)
    {
        return view.AddHorizontalScrollBar(invert, autoHide, handleSprite, trackSprite, size, handleColor, highlightColor, pressedColor, trackColor, fadeDuration);
    }
    
    public ScrollbarComponent AddVerticalScrollBar(UiScrollView view, bool invert, bool autoHide, string handleSprite, string trackSprite, float size,
        UiColor? handleColor, UiColor? highlightColor, UiColor? pressedColor, UiColor? trackColor)
    {
        return AddVerticalScrollBar(view, invert, autoHide, handleSprite, trackSprite, size, handleColor, highlightColor, pressedColor, trackColor, JsonDefaults.ScrollBar.FadeDuration);
    }

    public ScrollbarComponent AddVerticalScrollBar(UiScrollView view, bool invert = false, bool autoHide = false, string handleSprite = null, string trackSprite = null, float size = JsonDefaults.ScrollBar.Size,
        UiColor? handleColor = null, UiColor? highlightColor = null, UiColor? pressedColor = null, UiColor? trackColor = null, float fadeDuration = JsonDefaults.ScrollBar.FadeDuration)
    {
        return view.AddVerticalScrollBar(invert, autoHide, handleSprite, trackSprite, size, handleColor, highlightColor, pressedColor, trackColor, fadeDuration);
    }
    #endregion
}

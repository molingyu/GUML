using System.Reflection;
using Godot;

namespace GUML;

public class TypeNotFoundException(string msg) : Exception(msg);

public class TypeErrorException(string msg) : Exception(msg);

public enum ThemeValueType
{
    Constant,
    Color,
    Font,
    FontSize,
    Icon,
    Style
}

public static class Guml
{
    
    public static readonly Dictionary<string, Dictionary<string, ThemeValueType>> ThemeOverrides = new()
    {
        {
            "BoxContainer", new Dictionary<string,  ThemeValueType>
            {
                {"separation", ThemeValueType.Constant}
            }
        },
        {
            "HBoxContainer", new Dictionary<string,  ThemeValueType>
            {
                {"separation", ThemeValueType.Constant}
            }
        },
        {
            "VBoxContainer", new Dictionary<string,  ThemeValueType>
            {
                {"separation", ThemeValueType.Constant}
            }
        },
        {
            "ColorPicker", new Dictionary<string,  ThemeValueType>
            {
                {"margin", ThemeValueType.Constant},
                {"sv_width", ThemeValueType.Constant},
                {"sv_height", ThemeValueType.Constant},
                {"h_width", ThemeValueType.Constant},
                {"label_width", ThemeValueType.Constant},
                {"center_slider_grabbers", ThemeValueType.Constant},
                {"folded_arrow", ThemeValueType.Icon},
                {"expanded_arrow", ThemeValueType.Icon},
                {"screen_picker", ThemeValueType.Icon},
                {"shape_circle", ThemeValueType.Icon},
                {"shape_rect", ThemeValueType.Icon},
                {"shape_rect_wheel", ThemeValueType.Icon},
                {"add_preset", ThemeValueType.Icon},
                {"sample_bg", ThemeValueType.Icon},
                {"overbright_indicator", ThemeValueType.Icon},
                {"bar_arrow", ThemeValueType.Icon},
                {"picker_cursor", ThemeValueType.Icon},
                {"color_hue", ThemeValueType.Icon},
                {"color_okhsl_hue", ThemeValueType.Icon},
            }
        },
        {
            "FlowContainer", new Dictionary<string,  ThemeValueType>
            {
                {"h_separation", ThemeValueType.Constant},
                {"v_separation", ThemeValueType.Constant}
            }
        },
        {
            "HFlowContainer", new Dictionary<string,  ThemeValueType>
            {
                {"h_separation", ThemeValueType.Constant},
                {"v_separation", ThemeValueType.Constant}
            }
        },
        {
            "VFlowContainer", new Dictionary<string,  ThemeValueType>
            {
                {"h_separation", ThemeValueType.Constant},
                {"v_separation", ThemeValueType.Constant}
            }
        },
        {
            "GraphNode", new Dictionary<string,  ThemeValueType>
            {
                {"resizer_color", ThemeValueType.Color},
                {"separation", ThemeValueType.Constant},
                {"port_h_offset", ThemeValueType.Constant},
                {"port", ThemeValueType.Icon},
                {"resizer", ThemeValueType.Icon},
                {"panel", ThemeValueType.Style},
                {"panel_selected", ThemeValueType.Style},
                {"titlebar", ThemeValueType.Style},
                {"titlebar_selected", ThemeValueType.Style},
                {"slot", ThemeValueType.Style},
            }
        },
        {
            "GridContainer", new Dictionary<string,  ThemeValueType>
            {
                {"h_separation", ThemeValueType.Constant},
                {"v_separation", ThemeValueType.Constant}
            }
        },
        {
            "SplitContainer", new Dictionary<string,  ThemeValueType>
            {
                {"separation", ThemeValueType.Constant},
                {"minimum_grab_thickness", ThemeValueType.Constant},
                {"autohide", ThemeValueType.Constant},
                {"h_grabber", ThemeValueType.Icon},
                {"v_grabber", ThemeValueType.Icon}
            }
        },
        {
            "HSplitContainer", new Dictionary<string,  ThemeValueType>
            {
                {"separation", ThemeValueType.Constant},
                {"minimum_grab_thickness", ThemeValueType.Constant},
                {"autohide", ThemeValueType.Constant},
                {"grabber", ThemeValueType.Icon}
            }
        },
        {
            "VSplitContainer", new Dictionary<string,  ThemeValueType>
            {
                {"separation", ThemeValueType.Constant},
                {"minimum_grab_thickness", ThemeValueType.Constant},
                {"autohide", ThemeValueType.Constant},
                {"grabber", ThemeValueType.Icon}
            }
        },
        {
            "MarginContainer", new Dictionary<string,  ThemeValueType>
            {
                {"margin_left", ThemeValueType.Constant},
                {"margin_top", ThemeValueType.Constant},
                {"margin_right", ThemeValueType.Constant},
                {"margin_bottom", ThemeValueType.Constant}
            }
        },
        {
            "PanelContainer", new Dictionary<string,  ThemeValueType>
            {
                {"panel", ThemeValueType.Style}
            }
        },
        {
            "ScrollContainer", new Dictionary<string,  ThemeValueType>
            {
                {"panel", ThemeValueType.Style}
            }
        },
        {
            "TabContainer", new Dictionary<string,  ThemeValueType>
            {
                {"font_selected_color", ThemeValueType.Color},
                {"font_hovered_color", ThemeValueType.Color},
                {"font_unselected_color", ThemeValueType.Color},
                {"font_disabled_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"drop_mark_color", ThemeValueType.Color},
                {"side_margin", ThemeValueType.Constant},
                {"icon_separation", ThemeValueType.Constant},
                {"icon_max_width", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"increment", ThemeValueType.Icon},
                {"increment_highlight", ThemeValueType.Icon},
                {"decrement", ThemeValueType.Icon},
                {"decrement_highlight", ThemeValueType.Icon},
                {"drop_mark", ThemeValueType.Icon},
                {"menu", ThemeValueType.Icon},
                {"menu_highlight", ThemeValueType.Icon},
                {"tab_selected", ThemeValueType.Style},
                {"tab_hovered", ThemeValueType.Style},
                {"tab_unselected", ThemeValueType.Style},
                {"tab_disabled", ThemeValueType.Style},
                {"tab_focus", ThemeValueType.Style},
                {"panel", ThemeValueType.Style},
                {"tabbar_background", ThemeValueType.Style}
            }
        },
        {
            "Button", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_pressed_color", ThemeValueType.Color},
                {"font_hover_color", ThemeValueType.Color},
                {"font_focus_color", ThemeValueType.Color},
                {"font_hover_pressed_color", ThemeValueType.Color},
                {"font_disabled_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"icon_normal_color", ThemeValueType.Color},
                {"icon_pressed_color", ThemeValueType.Color},
                {"icon_hover_color", ThemeValueType.Color},
                {"icon_hover_pressed_color", ThemeValueType.Color},
                {"icon_focus_color", ThemeValueType.Color},
                {"icon_disabled_color", ThemeValueType.Color},
                {"outline_size", ThemeValueType.Constant},
                {"h_separation", ThemeValueType.Constant},
                {"icon_max_width", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"normal", ThemeValueType.Style},
                {"hover", ThemeValueType.Style},
                {"pressed", ThemeValueType.Style},
                {"disabled", ThemeValueType.Style},
                {"focus", ThemeValueType.Style}
            }
        },
        {
            "CheckBox", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_pressed_color", ThemeValueType.Color},
                {"font_hover_color", ThemeValueType.Color},
                {"font_focus_color", ThemeValueType.Color},
                {"font_hover_pressed_color", ThemeValueType.Color},
                {"font_disabled_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"h_separation", ThemeValueType.Constant},
                {"check_v_offset", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"checked", ThemeValueType.Icon},
                {"checked_disabled", ThemeValueType.Icon},
                {"unchecked", ThemeValueType.Icon},
                {"unchecked_disabled", ThemeValueType.Icon},
                {"radio_checked", ThemeValueType.Icon},
                {"radio_checked_disabled", ThemeValueType.Icon},
                {"radio_unchecked", ThemeValueType.Icon},
                {"radio_unchecked_disabled", ThemeValueType.Icon},
                {"normal", ThemeValueType.Style},
                {"pressed", ThemeValueType.Style},
                {"disabled", ThemeValueType.Style},
                {"hover", ThemeValueType.Style},
                {"hover_pressed", ThemeValueType.Style},
                {"focus", ThemeValueType.Style}
            }
        },
        {
            "CheckButton", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_pressed_color", ThemeValueType.Color},
                {"font_hover_color", ThemeValueType.Color},
                {"font_focus_color", ThemeValueType.Color},
                {"font_hover_pressed_color", ThemeValueType.Color},
                {"font_disabled_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"h_separation", ThemeValueType.Constant},
                {"check_v_offset", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"checked", ThemeValueType.Icon},
                {"checked_disabled", ThemeValueType.Icon},
                {"unchecked", ThemeValueType.Icon},
                {"unchecked_disabled", ThemeValueType.Icon},
                {"radio_checked", ThemeValueType.Icon},
                {"radio_checked_disabled", ThemeValueType.Icon},
                {"radio_unchecked", ThemeValueType.Icon},
                {"radio_unchecked_disabled", ThemeValueType.Icon},
                {"normal", ThemeValueType.Style},
                {"pressed", ThemeValueType.Style},
                {"disabled", ThemeValueType.Style},
                {"hover", ThemeValueType.Style},
                {"hover_pressed", ThemeValueType.Style},
                {"focus", ThemeValueType.Style}
            }
        },
        {
            "ColorPickerButton", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_pressed_color", ThemeValueType.Color},
                {"font_hover_color", ThemeValueType.Color},
                {"font_focus_color", ThemeValueType.Color},
                {"font_disabled_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"h_separation", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"bg", ThemeValueType.Icon},
                {"normal", ThemeValueType.Style},
                {"pressed", ThemeValueType.Style},
                {"hover", ThemeValueType.Style},
                {"disabled", ThemeValueType.Style},
                {"focus", ThemeValueType.Style}
            }
        },
        {
            "MenuButton", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_pressed_color", ThemeValueType.Color},
                {"font_hover_color", ThemeValueType.Color},
                {"font_focus_color", ThemeValueType.Color},
                {"font_disabled_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"h_separation", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"normal", ThemeValueType.Style},
                {"pressed", ThemeValueType.Style},
                {"hover", ThemeValueType.Style},
                {"disabled", ThemeValueType.Style},
                {"focus", ThemeValueType.Style}
            }
        },
        {
            "OptionButton", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_pressed_color", ThemeValueType.Color},
                {"font_hover_color", ThemeValueType.Color},
                {"font_hover_pressed_color", ThemeValueType.Color},
                {"font_focus_color", ThemeValueType.Color},
                {"font_disabled_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"h_separation", ThemeValueType.Constant},
                {"arrow_margin", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"modulate_arrow", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"arrow", ThemeValueType.Icon},
                {"focus", ThemeValueType.Style},
                {"normal", ThemeValueType.Style},
                {"hover", ThemeValueType.Style},
                {"pressed", ThemeValueType.Style},
                {"disabled", ThemeValueType.Style},
                {"normal_mirrored", ThemeValueType.Style},
                {"hover_mirrored", ThemeValueType.Style},
                {"pressed_mirrored", ThemeValueType.Style},
                {"disabled_mirrored", ThemeValueType.Style}
            }
        },
        {
            "LinkButton", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_pressed_color", ThemeValueType.Color},
                {"font_hover_color", ThemeValueType.Color},
                {"font_focus_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"outline_size", ThemeValueType.Constant},
                {"underline_spacing", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"focus", ThemeValueType.Style}
            }
        },
        {
            "TextEdit", new Dictionary<string,  ThemeValueType>
            {
                {"background_color", ThemeValueType.Color},
                {"font_color", ThemeValueType.Color},
                {"font_selected_color", ThemeValueType.Color},
                {"font_readonly_color", ThemeValueType.Color},
                {"font_placeholder_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"selection_color", ThemeValueType.Color},
                {"current_line_color", ThemeValueType.Color},
                {"caret_color", ThemeValueType.Color},
                {"caret_background_color", ThemeValueType.Color},
                {"word_highlighted_color", ThemeValueType.Color},
                {"search_result_color", ThemeValueType.Color},
                {"search_result_border_color", ThemeValueType.Color},
                {"line_spacing", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"caret_width", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"tab", ThemeValueType.Icon},
                {"space", ThemeValueType.Icon},
                {"normal", ThemeValueType.Style},
                {"focus", ThemeValueType.Style},
                {"read_only", ThemeValueType.Style}
            }
        },
        {
            "GraphEdit", new Dictionary<string,  ThemeValueType>
            {
                {"grid_minor", ThemeValueType.Color},
                {"grid_major", ThemeValueType.Color},
                {"selection_fill", ThemeValueType.Color},
                {"selection_stroke", ThemeValueType.Color},
                {"activity", ThemeValueType.Color},
                {"port_hotzone_inner_extent", ThemeValueType.Constant},
                {"port_hotzone_outer_extent", ThemeValueType.Constant},
                {"zoom_out", ThemeValueType.Icon},
                {"zoom_in", ThemeValueType.Icon},
                {"zoom_reset", ThemeValueType.Icon},
                {"grid_toggle", ThemeValueType.Icon},
                {"minimap_toggle", ThemeValueType.Icon},
                {"snapping_toggle", ThemeValueType.Icon},
                {"layout", ThemeValueType.Icon},
                {"panel", ThemeValueType.Style},
                {"menu_panel", ThemeValueType.Style}
            }
        },
        {
            "HScrollBar", new Dictionary<string,  ThemeValueType>
            {
                {"increment", ThemeValueType.Icon},
                {"increment_highlight", ThemeValueType.Icon},
                {"increment_pressed", ThemeValueType.Icon},
                {"decrement", ThemeValueType.Icon},
                {"decrement_highlight", ThemeValueType.Icon},
                {"decrement_pressed", ThemeValueType.Icon},
                {"scroll", ThemeValueType.Style},
                {"scroll_focus", ThemeValueType.Style},
                {"grabber", ThemeValueType.Style},
                {"grabber_highlight", ThemeValueType.Style},
                {"grabber_pressed", ThemeValueType.Style}
            }
        },
        {
            "VScrollBar", new Dictionary<string,  ThemeValueType>
            {
                {"increment", ThemeValueType.Icon},
                {"increment_highlight", ThemeValueType.Icon},
                {"increment_pressed", ThemeValueType.Icon},
                {"decrement", ThemeValueType.Icon},
                {"decrement_highlight", ThemeValueType.Icon},
                {"decrement_pressed", ThemeValueType.Icon},
                {"scroll", ThemeValueType.Style},
                {"scroll_focus", ThemeValueType.Style},
                {"grabber", ThemeValueType.Style},
                {"grabber_highlight", ThemeValueType.Style},
                {"grabber_pressed", ThemeValueType.Style}
            }
        },
        {
            "HSlider", new Dictionary<string,  ThemeValueType>
            {
                {"center_grabber", ThemeValueType.Constant},
                {"grabber_offset", ThemeValueType.Constant},
                {"grabber", ThemeValueType.Icon},
                {"grabber_highlight", ThemeValueType.Icon},
                {"grabber_disabled", ThemeValueType.Icon},
                {"tick", ThemeValueType.Icon},
                {"slider", ThemeValueType.Style},
                {"grabber_area", ThemeValueType.Style},
                {"grabber_area_highlight", ThemeValueType.Style}
            }
        },
        {
            "VSlider", new Dictionary<string,  ThemeValueType>
            {
                {"center_grabber", ThemeValueType.Constant},
                {"grabber_offset", ThemeValueType.Constant},
                {"grabber", ThemeValueType.Icon},
                {"grabber_highlight", ThemeValueType.Icon},
                {"grabber_disabled", ThemeValueType.Icon},
                {"tick", ThemeValueType.Icon},
                {"slider", ThemeValueType.Style},
                {"grabber_area", ThemeValueType.Style},
                {"grabber_area_highlight", ThemeValueType.Style}
            }
        },
        {
            "ProgressBar", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"outline_size", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"background", ThemeValueType.Style},
                {"fill", ThemeValueType.Style}
            }
        },
        {
            "SpinBox", new Dictionary<string,  ThemeValueType>
            {
                {"updown", ThemeValueType.Icon}
            }
        },
        {
            "HSeparator", new Dictionary<string,  ThemeValueType>
            {
                {"separation", ThemeValueType.Constant},
                {"separator", ThemeValueType.Style}
            }
        },
        {
            "VSeparator", new Dictionary<string,  ThemeValueType>
            {
                {"separation", ThemeValueType.Constant},
                {"separator", ThemeValueType.Style}
            }
        },
        {
            "ItemList", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_hovered_color", ThemeValueType.Color},
                {"font_selected_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"guide_color", ThemeValueType.Color},
                {"h_separation", ThemeValueType.Constant},
                {"v_separation", ThemeValueType.Constant},
                {"icon_margin", ThemeValueType.Constant},
                {"line_separation", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"panel", ThemeValueType.Style},
                {"focus", ThemeValueType.Style},
                {"hovered", ThemeValueType.Style},
                {"selected", ThemeValueType.Style},
                {"selected_focus", ThemeValueType.Style},
                {"cursor", ThemeValueType.Style},
                {"cursor_unfocused", ThemeValueType.Style}
            }
        },
        {
            "Label", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_shadow_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"shadow_offset_x", ThemeValueType.Constant},
                {"shadow_offset_y", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"shadow_outline_size", ThemeValueType.Constant},
                {"line_spacing", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"normal", ThemeValueType.Style}
            }
        },
        {
            "LineEdit", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_selected_color", ThemeValueType.Color},
                {"font_uneditable_color", ThemeValueType.Color},
                {"font_placeholder_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"caret_color", ThemeValueType.Color},
                {"selection_color", ThemeValueType.Color},
                {"clear_button_color", ThemeValueType.Color},
                {"clear_button_color_pressed", ThemeValueType.Color},
                {"minimum_character_width", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"caret_width", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"clear", ThemeValueType.Icon},
                {"normal", ThemeValueType.Style},
                {"focus", ThemeValueType.Style},
                {"read_only", ThemeValueType.Style}
            }
        },
        {
            "MenuBar", new Dictionary<string,  ThemeValueType>
            {
                {"font_color", ThemeValueType.Color},
                {"font_pressed_color", ThemeValueType.Color},
                {"font_hover_color", ThemeValueType.Color},
                {"font_focus_color", ThemeValueType.Color},
                {"font_hover_pressed_color", ThemeValueType.Color},
                {"font_disabled_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"outline_size", ThemeValueType.Constant},
                {"h_separation", ThemeValueType.Constant},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"normal", ThemeValueType.Style},
                {"hover", ThemeValueType.Style},
                {"pressed", ThemeValueType.Style},
                {"disabled", ThemeValueType.Style}
            }
        },
        {
            "Panel", new Dictionary<string,  ThemeValueType>
            {
                {"panel", ThemeValueType.Style}
            }
        },
        {
            "RichTextLabel", new Dictionary<string,  ThemeValueType>
            {
                {"default_color", ThemeValueType.Color},
                {"font_selected_color", ThemeValueType.Color},
                {"selection_color", ThemeValueType.Color},
                {"font_shadow_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"table_odd_row_bg", ThemeValueType.Color},
                {"table_even_row_bg", ThemeValueType.Color},
                {"table_border", ThemeValueType.Color},
                {"shadow_offset_x", ThemeValueType.Constant},
                {"shadow_offset_y", ThemeValueType.Constant},
                {"shadow_outline_size", ThemeValueType.Constant},
                {"line_separation", ThemeValueType.Constant},
                {"table_h_separation", ThemeValueType.Constant},
                {"table_v_separation", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"text_highlight_h_padding", ThemeValueType.Constant},
                {"text_highlight_v_padding", ThemeValueType.Constant},
                {"normal_font", ThemeValueType.Font},
                {"bold_font", ThemeValueType.Font},
                {"italics_font", ThemeValueType.Font},
                {"bold_italics_font", ThemeValueType.Font},
                {"mono_font", ThemeValueType.Font},
                {"normal_font_size", ThemeValueType.FontSize},
                {"bold_font_size", ThemeValueType.FontSize},
                {"italics_font_size", ThemeValueType.FontSize},
                {"bold_italics_font_size", ThemeValueType.FontSize},
                {"mono_font_size", ThemeValueType.FontSize},
                {"focus", ThemeValueType.Style},
                {"normal", ThemeValueType.Style}
            }
        },
        {
            "TabBar", new Dictionary<string,  ThemeValueType>
            {
                {"font_selected_color", ThemeValueType.Color},
                {"font_hovered_color", ThemeValueType.Color},
                {"font_unselected_color", ThemeValueType.Color},
                {"font_disabled_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"drop_mark_color", ThemeValueType.Color},
                {"h_separation", ThemeValueType.Constant},
                {"icon_max_width", ThemeValueType.Constant},
                {"font", ThemeValueType.Constant},
                {"font_size", ThemeValueType.Font},
                {"increment", ThemeValueType.Icon},
                {"increment_highlight", ThemeValueType.Icon},
                {"decrement", ThemeValueType.Icon},
                {"decrement_highlight", ThemeValueType.Icon},
                {"drop_mark", ThemeValueType.Icon},
                {"close", ThemeValueType.Icon},
                {"tab_selected", ThemeValueType.Style},
                {"tab_hovered", ThemeValueType.Style},
                {"tab_unselected", ThemeValueType.Style},
                {"tab_disabled", ThemeValueType.Style},
                {"tab_focus", ThemeValueType.Style},
                {"button_pressed", ThemeValueType.Style},
                {"button_highlight", ThemeValueType.Style}
            }
        },
        {
            "Tree", new Dictionary<string,  ThemeValueType>
            {
                {"title_button_color", ThemeValueType.Color},
                {"font_color", ThemeValueType.Color},
                {"font_selected_color", ThemeValueType.Color},
                {"font_outline_color", ThemeValueType.Color},
                {"guide_color", ThemeValueType.Color},
                {"drop_position_color", ThemeValueType.Color},
                {"relationship_line_color", ThemeValueType.Color},
                {"parent_hl_line_color", ThemeValueType.Color},
                {"children_hl_line_color", ThemeValueType.Color},
                {"custom_button_font_highlight", ThemeValueType.Color},
                {"h_separation", ThemeValueType.Constant},
                {"v_separation", ThemeValueType.Constant},
                {"item_margin", ThemeValueType.Constant},
                {"inner_item_margin_bottom", ThemeValueType.Constant},
                {"inner_item_margin_left", ThemeValueType.Constant},
                {"inner_item_margin_right", ThemeValueType.Constant},
                {"inner_item_margin_top", ThemeValueType.Constant},
                {"button_margin", ThemeValueType.Constant},
                {"draw_relationship_lines", ThemeValueType.Constant},
                {"relationship_line_width", ThemeValueType.Constant},
                {"parent_hl_line_width", ThemeValueType.Constant},
                {"children_hl_line_width", ThemeValueType.Constant},
                {"parent_hl_line_margin", ThemeValueType.Constant},
                {"draw_guides", ThemeValueType.Constant},
                {"scroll_border", ThemeValueType.Constant},
                {"scroll_speed", ThemeValueType.Constant},
                {"outline_size", ThemeValueType.Constant},
                {"icon_max_width", ThemeValueType.Constant},
                {"scrollbar_margin_left", ThemeValueType.Constant},
                {"scrollbar_margin_top", ThemeValueType.Constant},
                {"scrollbar_margin_right", ThemeValueType.Constant},
                {"scrollbar_margin_bottom", ThemeValueType.Constant},
                {"scrollbar_h_separation", ThemeValueType.Constant},
                {"scrollbar_v_separation", ThemeValueType.Constant},
                {"title_button_font", ThemeValueType.Font},
                {"font", ThemeValueType.Font},
                {"font_size", ThemeValueType.FontSize},
                {"title_button_font_size", ThemeValueType.FontSize},
                {"checked", ThemeValueType.Icon},
                {"unchecked", ThemeValueType.Icon},
                {"indeterminate", ThemeValueType.Icon},
                {"updown", ThemeValueType.Icon},
                {"select_arrow", ThemeValueType.Icon},
                {"arrow", ThemeValueType.Icon},
                {"arrow_collapsed", ThemeValueType.Icon},
                {"arrow_collapsed_mirrored", ThemeValueType.Icon},
                {"panel", ThemeValueType.Style},
                {"focus", ThemeValueType.Style},
                {"selected", ThemeValueType.Style},
                {"selected_focus", ThemeValueType.Style},
                {"cursor", ThemeValueType.Style},
                {"cursor_unfocused", ThemeValueType.Style},
                {"button_pressed", ThemeValueType.Style},
                {"title_button_normal", ThemeValueType.Style},
                {"title_button_pressed", ThemeValueType.Style},
                {"title_button_hover", ThemeValueType.Style},
                {"custom_button", ThemeValueType.Style},
                {"custom_button_pressed", ThemeValueType.Style},
                {"custom_button_hover", ThemeValueType.Style}
            }
        }
    };
    
    /// <summary>
    /// Top Controller List.
    /// </summary>
    public static readonly Dictionary<string, GuiController> TopControllers = new ();
    /// <summary>
    /// Resource Loader 
    /// </summary>
    public static Func<string, object> ResourceLoader = null!;
    /// <summary>
    /// The default theme.
    /// </summary>
    public static Theme? DefaultTheme = null;
    /// <summary>
    /// GUML Controller namespace list.
    /// </summary>
    public static List<string> ControllerNamespaces = [""];
    /// <summary>
    /// GUML Controller Assembly list.
    /// </summary>
    public static readonly List<Assembly> Assemblies = [];
    /// <summary>
    /// GUML Global variable dictionary
    /// </summary>
    public static readonly Dictionary<string, object> GlobalRefs = new ();
    /// <summary>
    /// GUML Parser
    /// </summary>
    public static readonly GumlParser Parser = new ();

    /// <summary>
    /// Initializes the static members of Guml.
    /// </summary>
    public static void Init()
    {
        Parser.WithConverter(new KeyConverter());
        Assemblies.Add(typeof(Guml).Assembly);
        Assemblies.Add(typeof(Node).Assembly);
    }

    public static object GetResource(string path)
    {
        return ResourceLoader.Invoke(path);
    }

    public static GuiController LoadGuml(Node root, string path)
    {
        var importPath = Path.GetDirectoryName(Path.GetFullPath(path)) ?? throw new InvalidOperationException();
        var controllerName = $"{KeyConverter.ToPascalCase(Path.GetFileNameWithoutExtension(path))}Controller";
        var controller = Activator.CreateInstance(FindType(controllerName)) as GuiController ?? throw new InvalidOperationException();
        GumlRenderer.Render(Parser.Parse(File.ReadAllText(path)), controller, root, importPath);
        controller.Created();
        return controller;
    }

    public static Type FindType(string name)
    {
        foreach (var type in ControllerNamespaces.SelectMany(@namespace => 
                     Assemblies.Select(assembly => assembly.GetType($"{@namespace}.{name}")).OfType<Type>()))
        {
            return type;
        }


        throw new TypeNotFoundException(name);
    }

}

using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using FluentIcons.Common;
using FluentIcons.Common.Internals;

namespace FluentIcons.Avalonia;

[TypeConverter(typeof(SymbolIconConverter))]
public class SymbolIcon : IconElement
{
    private static readonly Typeface _system = new("avares://FluentIcons.Avalonia/Assets#Fluent System Icons");
    private static readonly Typeface _seagull = new("avares://FluentIcons.Avalonia/Assets#Seagull Fluent Icons");
    internal static bool UseSegoeMetricsDefaultValue = false;

    public static readonly StyledProperty<Symbol> SymbolProperty =
        AvaloniaProperty.Register<SymbolIcon, Symbol>(nameof(Symbol), Symbol.Home);
    public static readonly StyledProperty<bool> IsFilledProperty =
        AvaloniaProperty.Register<SymbolIcon, bool>(nameof(IsFilled));
    public static readonly StyledProperty<bool> UseSegoeMetricsProperty =
        AvaloniaProperty.Register<SymbolIcon, bool>(nameof(UseSegoeMetrics));
    public static new readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<SymbolIcon, double>(nameof(FontSize), 20d, false);

    private readonly Border _border;
    private readonly Core _core;

    public SymbolIcon()
    {
        UseSegoeMetrics = UseSegoeMetricsDefaultValue;

        _border = new();
        _border.Bind(BackgroundProperty, this.GetBindingObservable(BackgroundProperty));
        _border.Bind(BorderBrushProperty, this.GetBindingObservable(BorderBrushProperty));
        _border.Bind(BorderThicknessProperty, this.GetBindingObservable(BorderThicknessProperty));
        _border.Bind(CornerRadiusProperty, this.GetBindingObservable(CornerRadiusProperty));
        _border.Bind(PaddingProperty, this.GetBindingObservable(PaddingProperty));
        (_border as ISetLogicalParent).SetParent(this);
        VisualChildren.Add(_border);
        LogicalChildren.Add(_border);

        _core = new();
        (_core as ISetLogicalParent).SetParent(this);
        VisualChildren.Add(_core);
        LogicalChildren.Add(_core);
    }

    public Symbol Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    public bool IsFilled
    {
        get => GetValue(IsFilledProperty);
        set => SetValue(IsFilledProperty, value);
    }

    public bool UseSegoeMetrics
    {
        get => GetValue(UseSegoeMetricsProperty);
        set => SetValue(UseSegoeMetricsProperty, value);
    }

    public new double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        InvalidateText();
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _core.Clear();
        base.OnUnloaded(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == FontSizeProperty)
        {
            InvalidateMeasure();
            InvalidateText();
        }
        else if (change.Property == ForegroundProperty ||
            change.Property == SymbolProperty ||
            change.Property == IsFilledProperty ||
            change.Property == UseSegoeMetricsProperty ||
            change.Property == FlowDirectionProperty)
        {
            InvalidateText();
        }

        base.OnPropertyChanged(change);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double fs = FontSize;
        Size size = new Size(fs, fs).Inflate(Padding).Inflate(BorderThickness);
        return new Size(
            Width.Or(
                HorizontalAlignment == HorizontalAlignment.Stretch
                    ? availableSize.Width.Or(size.Width)
                    : Math.Min(availableSize.Width, size.Width)),
            Height.Or(
                VerticalAlignment == VerticalAlignment.Stretch
                    ? availableSize.Height.Or(size.Height)
                    : Math.Min(availableSize.Height, size.Height)));
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        double fs = FontSize;
        Size size = new Size(fs, fs).Inflate(Padding).Inflate(BorderThickness);
        Rect rect = new(
            HorizontalAlignment switch
            {
                HorizontalAlignment.Center => finalRect.Center.X - fs / 2,
                HorizontalAlignment.Right => finalRect.Right - fs,
                _ => finalRect.Left
            },
            VerticalAlignment switch
            {
                VerticalAlignment.Center => finalRect.Center.Y - fs / 2,
                VerticalAlignment.Bottom => finalRect.Bottom - fs,
                _ => finalRect.Top
            },
            HorizontalAlignment switch
            {
                HorizontalAlignment.Stretch => finalRect.Width,
                _ => size.Width,
            },
            VerticalAlignment switch
            {
                VerticalAlignment.Stretch => finalRect.Height,
                _ => size.Height,
            });
        _border.Arrange(rect);
        _core.Arrange(rect.Deflate(BorderThickness).Deflate(Padding));
        base.ArrangeCore(finalRect);
    }

    private void InvalidateText()
    {
        if (!IsLoaded)
            return;

        _core.InvalidateText(
            Symbol.ToString(IsFilled, FlowDirection == FlowDirection.RightToLeft),
            UseSegoeMetrics ? _seagull : _system,
            FontSize,
            Foreground);
    }

    private sealed class Core : Control
    {
        private double _size;
        private TextLayout? _textLayout;

        public override void Render(DrawingContext context)
        {
            if (_textLayout is null)
                return;

            Rect bounds = Bounds;
            using (context.PushClip(new Rect(bounds.Size)))
            {
                var origin = new Point(
                    (bounds.Width - _size) / 2,
                    (bounds.Height - _size) / 2);
                _textLayout.Draw(context, origin);
            }
        }

        public void InvalidateText(string text, Typeface typeface, double fontSize, IBrush? foreground)
        {
            _size = fontSize;

            _textLayout?.Dispose();
            _textLayout = new TextLayout(
                text,
                typeface,
                fontSize,
                foreground,
                TextAlignment.Center);

            InvalidateVisual();
        }

        public void Clear()
        {
            _textLayout?.Dispose();
            _textLayout = null;
        }
    }
}

public class SymbolIconConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string) || sourceType == typeof(Symbol))
        {
            return true;
        }
        return base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string val)
        {
            return new SymbolIcon { Symbol = (Symbol)Enum.Parse(typeof(Symbol), val) };
        }
        else if (value is Symbol symbol)
        {
            return new SymbolIcon { Symbol = symbol };
        }
        return base.ConvertFrom(context, culture, value);
    }
}

public class SymbolIconExtension : MarkupExtension
{
    public Symbol Symbol { get; set; } = Symbol.Home;
    public bool IsFilled { get; set; }
    public bool UseSegoeMetrics { get; set; } = SymbolIcon.UseSegoeMetricsDefaultValue;
    public double FontSize { get; set; } = 20d;
    public Brush? Foreground { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var icon = new SymbolIcon
        {
            Symbol = Symbol,
            IsFilled = IsFilled,
            UseSegoeMetrics = UseSegoeMetrics,
            FontSize = FontSize,
        };

        var service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        if (service?.TargetObject is Visual elem)
        {
            icon.FlowDirection = elem.FlowDirection;
        }

        if (Foreground is not null)
        {
            icon.Foreground = Foreground;
        }

        return icon;
    }
}
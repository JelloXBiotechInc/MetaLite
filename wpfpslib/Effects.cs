using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System.Text;
using System;

namespace wpfpslib
{
    /// <summary>
    /// Represents an effect, which renderes any input into its corresponding normal map
    /// </summary>
    public sealed class NormalMapEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Range"/> property
        /// </summary>
        public static readonly DependencyProperty RangeProperty = Register(nameof(Range), typeof(double), typeof(NormalMapEffect), 0d, 0);
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(NormalMapEffect), 0);


        /// <summary>
        /// The normal map edge detection range.
        /// <para/>
        /// Should be set to a value between [0..1]
        /// <para/>
        /// Should be set to `4 / Min(Width, Height)` for best results
        /// </summary>
        public double Range
        {
            get => ((double)(GetValue(RangeProperty)));
            set => SetValue(RangeProperty, value);
        }

        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => ((Brush)(GetValue(InputProperty)));
            set => SetValue(InputProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public NormalMapEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(RangeProperty);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class GlassTilesEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(GlassTilesEffect), 0);
        /// <summary>
        /// Identifies the <see cref="Tiles"/> property
        /// </summary>
        public static readonly DependencyProperty TilesProperty = Register(nameof(Tiles), typeof(double), typeof(GlassTilesEffect), 5d, 0);
        /// <summary>
        /// Identifies the <see cref="BevelWidth"/> property
        /// </summary>
        public static readonly DependencyProperty BevelWidthProperty = Register(nameof(BevelWidth), typeof(double), typeof(GlassTilesEffect), 1d, 1);
        /// <summary>
        /// Identifies the <see cref="Offset"/> property
        /// </summary>
        public static readonly DependencyProperty OffsetProperty = Register(nameof(Offset), typeof(double), typeof(GlassTilesEffect), 1d, 3);
        /// <summary>
        /// Identifies the <see cref="BackgroundColor"/> property
        /// </summary>
        public static readonly DependencyProperty BackgroundColorProperty = Register(nameof(BackgroundColor), typeof(Color), typeof(GlassTilesEffect), Color.FromArgb(255, 0, 0, 0), 2);


        public double Tiles
        {
            get => (double)GetValue(TilesProperty);
            set => SetValue(TilesProperty, value);
        }

        public double BevelWidth
        {
            get => (double)GetValue(BevelWidthProperty);
            set => SetValue(BevelWidthProperty, value);
        }

        public double Offset
        {
            get => (double)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }

        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public GlassTilesEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(TilesProperty);
            UpdateShaderValue(BevelWidthProperty);
            UpdateShaderValue(OffsetProperty);
            UpdateShaderValue(BackgroundColorProperty);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class CubicLensDistortionEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(CubicLensDistortionEffect), 0);
        /// <summary>
        /// Identifies the <see cref="DistortionCoefficient"/> property
        /// </summary>
        public static readonly DependencyProperty DistortionCoefficientProperty = DependencyProperty.Register(nameof(DistortionCoefficient), typeof(double), typeof(CubicLensDistortionEffect), new UIPropertyMetadata(-.15, PixelShaderConstantCallback(0)));
        /// <summary>
        /// Identifies the <see cref="CubicDistortion"/> property
        /// </summary>
        public static readonly DependencyProperty CubicDistortionProperty = DependencyProperty.Register(nameof(CubicDistortion), typeof(double), typeof(CubicLensDistortionEffect), new UIPropertyMetadata(.5, PixelShaderConstantCallback(1)));
        /// <summary>
        /// Identifies the <see cref="Zoom"/> property
        /// </summary>
        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(CubicLensDistortionEffect), new UIPropertyMetadata(1d, PixelShaderConstantCallback(2)));


        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        public double DistortionCoefficient
        {
            get => (double)GetValue(DistortionCoefficientProperty);
            set => SetValue(DistortionCoefficientProperty, value);
        }

        public double CubicDistortion
        {
            get => (double)GetValue(CubicDistortionProperty);
            set => SetValue(CubicDistortionProperty, value);
        }

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public CubicLensDistortionEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(DistortionCoefficientProperty);
            UpdateShaderValue(CubicDistortionProperty);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class CubicChromaticAbberationEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(CubicChromaticAbberationEffect), 0);
        /// <summary>
        /// Identifies the <see cref="Amount"/> property
        /// </summary>
        public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(nameof(Amount), typeof(double), typeof(CubicChromaticAbberationEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(0)));


        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        public double Amount
        {
            get => (double)GetValue(AmountProperty);
            set => SetValue(AmountProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public CubicChromaticAbberationEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(AmountProperty);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class LinearChromaticAbberationEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(LinearChromaticAbberationEffect), 0);
        /// <summary>
        /// Identifies the <see cref="Amount"/> property
        /// </summary>
        public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(nameof(Amount), typeof(double), typeof(LinearChromaticAbberationEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(0)));
        /// <summary>
        /// Identifies the <see cref="Angle"/> property
        /// </summary>
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(nameof(Angle), typeof(double), typeof(LinearChromaticAbberationEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(1)));


        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        public double Amount
        {
            get => (double)GetValue(AmountProperty);
            set => SetValue(AmountProperty, value);
        }

        public double Angle
        {
            get => (double)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public LinearChromaticAbberationEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(AmountProperty);
            UpdateShaderValue(AngleProperty);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class RoundedGlassTilesEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(RoundedGlassTilesEffect), 0);
        /// <summary>
        /// Identifies the <see cref="Density"/> property
        /// </summary>
        public static readonly DependencyProperty DensityProperty = DependencyProperty.Register(nameof(Density), typeof(double), typeof(RoundedGlassTilesEffect), new UIPropertyMetadata(0.04d, PixelShaderConstantCallback(0)));


        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        public double Density
        {
            get => (double)GetValue(DensityProperty);
            set => SetValue(DensityProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public RoundedGlassTilesEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(DensityProperty);
        }
    }

    /// <summary>
    /// Represents an hexagonal pixelation effect
    /// </summary>
    public sealed class HexagonalPixelationEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(HexagonalPixelationEffect), 0);
        /// <summary>
        /// Identifies the <see cref="Amount"/> property
        /// </summary>
        public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(nameof(Amount), typeof(double), typeof(HexagonalPixelationEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(0)));


        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        /// <summary>
        /// The pixelation amount
        /// <para/>
        /// The value should be between (0..1]
        /// </summary>
        public double Amount
        {
            get => (double)GetValue(AmountProperty);
            set => SetValue(AmountProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public HexagonalPixelationEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(AmountProperty);
        }
    }

    /// <summary>
    /// Represents a rippling effect
    /// </summary>
    public class RippleEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(RippleEffect), 0);
        /// <summary>
        /// Identifies the <see cref="Center"/> property
        /// </summary>
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(nameof(Center), typeof(Point), typeof(RippleEffect), new UIPropertyMetadata(new Point(.5, .5), PixelShaderConstantCallback(0)));
        /// <summary>
        /// Identifies the <see cref="Amplitude"/> property
        /// </summary>
        public static readonly DependencyProperty AmplitudeProperty = DependencyProperty.Register(nameof(Amplitude), typeof(double), typeof(RippleEffect), new UIPropertyMetadata(.1, PixelShaderConstantCallback(1)));
        /// <summary>
        /// Identifies the <see cref="Frequency"/> property
        /// </summary>
        public static readonly DependencyProperty FrequencyProperty = DependencyProperty.Register(nameof(Frequency), typeof(double), typeof(RippleEffect), new UIPropertyMetadata(70d, PixelShaderConstantCallback(2)));
        /// <summary>
        /// Identifies the <see cref="Phase"/> property
        /// </summary>
        public static readonly DependencyProperty PhaseProperty = DependencyProperty.Register(nameof(Phase), typeof(double), typeof(RippleEffect), new UIPropertyMetadata(.0, PixelShaderConstantCallback(3)));
        /// <summary>
        /// Identifies the <see cref="AspectRatio"/> property
        /// </summary>
        public static readonly DependencyProperty AspectRatioProperty = DependencyProperty.Register(nameof(AspectRatio), typeof(double), typeof(RippleEffect), new UIPropertyMetadata(1.5, PixelShaderConstantCallback(4)));


        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        /// <summary>
        /// The ripple effect's center point.
        /// <para/>
        /// Should be set to a value between [(0,0)..(1,1)]
        /// </summary>
        public Point Center
        {
            get => (Point)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        /// <summary>
        /// The rippling effect's wave amplitude.
        /// <para/>
        /// Should be set to a value between [0..1]
        /// </summary>
        public double Amplitude
        {
            get => (double)GetValue(AmplitudeProperty);
            set => SetValue(AmplitudeProperty, value);
        }

        /// <summary>
        /// The rippling effect's wave frequency.
        /// <para/>
        /// Should be set to a value between [0..300]
        /// </summary>
        public double Frequency
        {
            get => (double)GetValue(FrequencyProperty);
            set => SetValue(FrequencyProperty, value);
        }

        /// <summary>
        /// The rippling effect's phase.
        /// <para/>
        /// Should be set to a value between [-π..+π]
        /// </summary>
        public double Phase
        {
            get => (double)GetValue(PhaseProperty);
            set => SetValue(PhaseProperty, value);
        }

        /// <summary>
        /// The rippling effect's aspect ratio.
        /// <para/>
        /// Should be set to `Width / Height` 
        /// </summary>
        public double AspectRatio
        {
            get => (double)GetValue(AspectRatioProperty);
            set => SetValue(AspectRatioProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public RippleEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(CenterProperty);
            UpdateShaderValue(AmplitudeProperty);
            UpdateShaderValue(FrequencyProperty);
            UpdateShaderValue(PhaseProperty);
            UpdateShaderValue(AspectRatioProperty);
        }
    }

    /// <summary>
    /// Represents a pixelation effect
    /// </summary>
    public class PixelationEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(PixelationEffect), 0);
        /// <summary>
        /// Identifies the <see cref="Count"/> property
        /// </summary>
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(nameof(Count), typeof(Point), typeof(PixelationEffect), new UIPropertyMetadata(new Point(0, 0), PixelShaderConstantCallback(0)));


        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        /// <summary>
        /// The horizontal and vertical pixel count
        /// </summary>
        public Point Count
        {
            get => (Point)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public PixelationEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(CountProperty);
        }
    }

    /// <summary>
    /// Represents an offset/diagonalized pixelation effect
    /// </summary>
    public class DiagonalPixelationEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(DiagonalPixelationEffect), 0);
        /// <summary>
        /// Identifies the <see cref="Count"/> property
        /// </summary>
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(nameof(Count), typeof(Point), typeof(DiagonalPixelationEffect), new UIPropertyMetadata(new Point(0, 0), PixelShaderConstantCallback(0)));


        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        /// <summary>
        /// The horizontal and vertical pixel count
        /// </summary>
        public Point Count
        {
            get => (Point)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public DiagonalPixelationEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(CountProperty);
        }
    }

    /// <summary>
    /// Represents an effect creating a gravitational lensing around a black hole
    /// </summary>
    public sealed class BlackHoleEffect
            : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Position"/> property
        /// </summary>
        public static readonly DependencyProperty PositionProperty = Register(nameof(Position), typeof(Point), typeof(BlackHoleEffect), default(Point), 0);
        /// <summary>
        /// Identifies the <see cref="Aspectratio"/> property
        /// </summary>
        public static readonly DependencyProperty AspectratioProperty = Register(nameof(Aspectratio), typeof(double), typeof(BlackHoleEffect), default(double), 1);
        /// <summary>
        /// Identifies the <see cref="Radius"/> property
        /// </summary>
        public static readonly DependencyProperty RadiusProperty = Register(nameof(Radius), typeof(double), typeof(BlackHoleEffect), default(double), 2);
        /// <summary>
        /// Identifies the <see cref="Dist"/> property
        /// </summary>
        public static readonly DependencyProperty DistProperty = Register(nameof(Dist), typeof(double), typeof(BlackHoleEffect), default(double), 3);
        /// <summary>
        /// Identifies the <see cref="Size"/> property
        /// </summary>
        public static readonly DependencyProperty SizeProperty = Register(nameof(Size), typeof(double), typeof(BlackHoleEffect), default(double), 4);
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(BlackHoleEffect), 0);


        /// <summary>
        /// The black hole's center position
        /// <para/>
        /// Should be set to a value between [(0,0)..(1,1)]
        /// </summary>
        public Point Position
        {
            get => (Point)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public Double Aspectratio
        {
            get => (double)GetValue(AspectratioProperty);
            set => SetValue(AspectratioProperty, value);
        }

        public Double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        public Double Dist
        {
            get => (double)GetValue(DistProperty);
            set => SetValue(DistProperty, value);
        }

        public Double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public BlackHoleEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(PositionProperty);
            UpdateShaderValue(AspectratioProperty);
            UpdateShaderValue(RadiusProperty);
            UpdateShaderValue(DistProperty);
            UpdateShaderValue(SizeProperty);
            UpdateShaderValue(InputProperty);
        }
    }

    /// <summary>
    /// Represents a color inversion effect
    /// </summary>
    public sealed class InvertEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Amount"/> property
        /// </summary>
        public static readonly DependencyProperty AmountProperty = Register(nameof(Amount), typeof(double), typeof(InvertEffect), 1d, 0);
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(InvertEffect), 0);


        /// <summary>
        /// The inversion amount.
        /// <para/>
        /// Should be set to a value between [0..1]
        /// </summary>
        public Double Amount
        {
            get => (double)GetValue(AmountProperty);
            set => SetValue(AmountProperty, value);
        }

        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public InvertEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(AmountProperty);
            UpdateShaderValue(InputProperty);
        }
    }
    public sealed class GrayscaleEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(GrayscaleEffect), 0);
        /// <summary>
        /// Identifies the <see cref="Factor"/> property
        /// </summary>
        public static readonly DependencyProperty FactorProperty = DependencyProperty.Register(nameof(Factor), typeof(double), typeof(GrayscaleEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(0)));


        /// <summary>
        /// The imagery input data
        /// </summary>
        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        public double Factor
        {
            get => (double)GetValue(FactorProperty);
            set => SetValue(FactorProperty, value);
        }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public GrayscaleEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(FactorProperty);
        }
    }
    
    /// <summary>
    /// Updates all pixel shader properties
    /// </summary>
    public sealed class ChannelMaskEffect
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(ChannelMaskEffect), 0);
        /// <summary>
        /// Identifies the <see cref="R"/> property
        /// </summary>
        public static readonly DependencyProperty RProperty = DependencyProperty.Register(nameof(R), typeof(double), typeof(ChannelMaskEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(0)));
        /// <summary>
        /// Identifies the <see cref="G"/> property
        /// </summary>
        public static readonly DependencyProperty GProperty = DependencyProperty.Register(nameof(G), typeof(double), typeof(ChannelMaskEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(1)));
        /// <summary>
        /// Identifies the <see cref="B"/> property
        /// </summary>
        public static readonly DependencyProperty BProperty = DependencyProperty.Register(nameof(B), typeof(double), typeof(ChannelMaskEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(2)));
        /// <summary>
        /// Identifies the <see cref="RIndex"/> property
        /// </summary>
        public static readonly DependencyProperty RIndexProperty = DependencyProperty.Register(nameof(RIndex), typeof(double), typeof(ChannelMaskEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(3)));
        /// <summary>
        /// Identifies the <see cref="GIndex"/> property
        /// </summary>
        public static readonly DependencyProperty GIndexProperty = DependencyProperty.Register(nameof(GIndex), typeof(double), typeof(ChannelMaskEffect), new UIPropertyMetadata(1d, PixelShaderConstantCallback(4)));
        /// <summary>
        /// Identifies the <see cref="BIndex"/> property
        /// </summary>
        public static readonly DependencyProperty BIndexProperty = DependencyProperty.Register(nameof(BIndex), typeof(double), typeof(ChannelMaskEffect), new UIPropertyMetadata(2d, PixelShaderConstantCallback(5)));




        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        public double R
        {
            get => (double)GetValue(RProperty);
            set => SetValue(RProperty, value);
        }

        public double G
        {
            get => (double)GetValue(GProperty);
            set => SetValue(GProperty, value);
        }

        public double B
        {
            get => (double)GetValue(BProperty);
            set => SetValue(BProperty, value);
        }

        public double RIndex
        {
            get => (double)GetValue(RIndexProperty);
            set => SetValue(RIndexProperty, value);
        }

        public double GIndex
        {
            get => (double)GetValue(GIndexProperty);
            set => SetValue(GIndexProperty, value);
        }

        public double BIndex
        {
            get => (double)GetValue(BIndexProperty);
            set => SetValue(BIndexProperty, value);
        }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public ChannelMaskEffect()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(RProperty);
            UpdateShaderValue(GProperty);
            UpdateShaderValue(BProperty);
            UpdateShaderValue(RIndexProperty);
            UpdateShaderValue(GIndexProperty);
            UpdateShaderValue(BIndexProperty);
        }
    }
    public sealed class IHC
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(IHC), 0);
        /// <summary>
        /// Identifies the <see cref="HematoxylinIndex"/> property
        /// </summary>
        public static readonly DependencyProperty HematoxylinIndexProperty = DependencyProperty.Register(nameof(HematoxylinIndex), typeof(double), typeof(IHC), new UIPropertyMetadata(0d, PixelShaderConstantCallback(0)));
        /// <summary>
        /// Identifies the <see cref="DabIndex"/> property
        /// </summary>
        public static readonly DependencyProperty DabIndexProperty = DependencyProperty.Register(nameof(DabIndex), typeof(double), typeof(IHC), new UIPropertyMetadata(1d, PixelShaderConstantCallback(1)));
        /// <summary>
        /// Identifies the <see cref="BackgroundIndex"/> property
        /// </summary>
        public static readonly DependencyProperty BackgroundIndexProperty = DependencyProperty.Register(nameof(BackgroundIndex), typeof(double), typeof(IHC), new UIPropertyMetadata(2d, PixelShaderConstantCallback(2)));


        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        public double HematoxylinIndex
        {
            get => (double)GetValue(HematoxylinIndexProperty);
            set => SetValue(HematoxylinIndexProperty, value);
        }

        public double DabIndex
        {
            get => (double)GetValue(DabIndexProperty);
            set => SetValue(DabIndexProperty, value);
        }

        public double BackgroundIndex
        {
            get => (double)GetValue(BackgroundIndexProperty);
            set => SetValue(BackgroundIndexProperty, value);
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public IHC()
            : base()
        {
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(HematoxylinIndexProperty);
            UpdateShaderValue(DabIndexProperty);
            UpdateShaderValue(BackgroundIndexProperty);
        }
    }

    public sealed class HE
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(HE), 0);
        /// <summary>
        /// Identifies the <see cref="HematoxylinIndex"/> property
        /// </summary>
        public static readonly DependencyProperty HematoxylinIndexProperty = Register(nameof(HematoxylinIndex), typeof(double), typeof(HE), 0d, 0);
        /// <summary>
        /// Identifies the <see cref="EosinIndex"/> property
        /// </summary>
        public static readonly DependencyProperty EosinIndexProperty = Register(nameof(EosinIndex), typeof(double), typeof(HE), 1d, 1);
        /// <summary>
        /// Identifies the <see cref="BackgroundIndex"/> property
        /// </summary>
        public static readonly DependencyProperty BackgroundIndexProperty = Register(nameof(BackgroundIndex), typeof(double), typeof(HE), 2d, 2);
        
        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        public double HematoxylinIndex
        {
            get => (double)GetValue(HematoxylinIndexProperty);
            set => SetValue(HematoxylinIndexProperty, value);
        }

        public double EosinIndex
        {
            get => (double)GetValue(EosinIndexProperty);
            set => SetValue(EosinIndexProperty, value);
        }

        public double BackgroundIndex
        {
            get => (double)GetValue(BackgroundIndexProperty);
            set => SetValue(BackgroundIndexProperty, value);
        }

        
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public HE()
            : base()
        {
            
        }
        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(HematoxylinIndexProperty);
            UpdateShaderValue(EosinIndexProperty);
            UpdateShaderValue(BackgroundIndexProperty);
        }
    }
    public sealed class PseudoStaining
        : PixelShaderEffectBase
    {
        /// <summary>
        /// Identifies the <see cref="Input"/> property
        /// </summary>
        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(PseudoStaining), 0);
        /// <summary>
        /// Identifies the <see cref="HematoxylinIndex"/> property
        /// </summary>
        public static readonly DependencyProperty HematoxylinIndexProperty = Register(nameof(HematoxylinIndex), typeof(double), typeof(PseudoStaining), 0d, 0);
        /// <summary>
        /// Identifies the <see cref="EosinIndex"/> property
        /// </summary>
        public static readonly DependencyProperty EosinIndexProperty = Register(nameof(EosinIndex), typeof(double), typeof(PseudoStaining), 1d, 1);
        /// <summary>
        /// Identifies the <see cref="BackgroundIndex"/> property
        /// </summary>
        public static readonly DependencyProperty DabIndexProperty = Register(nameof(DabIndex), typeof(double), typeof(PseudoStaining), 2d, 2);
        /// <summary>
        /// Identifies the <see cref="HematoxylinIntensity"/> property
        /// </summary>
        public static readonly DependencyProperty HematoxylinIntensityProperty = Register(nameof(HematoxylinIntensity), typeof(double), typeof(PseudoStaining), 1d, 3);
        /// <summary>
        /// Identifies the <see cref="EosinIntensity"/> property
        /// </summary>
        public static readonly DependencyProperty EosinIntensityProperty = Register(nameof(EosinIntensity), typeof(double), typeof(PseudoStaining), 1d, 4);
        /// <summary>
        /// Identifies the <see cref="DabIntensity"/> property
        /// </summary>
        public static readonly DependencyProperty DabIntensityProperty = Register(nameof(DabIntensity), typeof(double), typeof(PseudoStaining), 1d, 5);
        /// <summary>
        /// Identifies the <see cref="HematoxylinColor"/> property
        /// </summary>
        public static readonly DependencyProperty HematoxylinColorProperty = Register(nameof(HematoxylinColor), typeof(Color), typeof(PseudoStaining), Color.FromArgb(255, 0, 0, 79), 6);
        /// <summary>
        /// Identifies the <see cref="EosinColor"/> property
        /// </summary>
        public static readonly DependencyProperty EosinColorProperty = Register(nameof(EosinColor), typeof(Color), typeof(PseudoStaining), Color.FromArgb(255, 187, 93, 119), 7);
        /// <summary>
        /// Identifies the <see cref="DabColor"/> property
        /// </summary>
        //public static readonly DependencyProperty EosinColorProperty = DependencyProperty.Register(nameof(EosinColor), typeof(double), typeof(HE), new UIPropertyMetadata(0d, PixelShaderConstantCallback(6)));
        public static readonly DependencyProperty DabColorProperty = Register(nameof(DabColor), typeof(Color), typeof(PseudoStaining), Color.FromArgb(255, 98, 40, 30), 8);

        public Brush Input
        {
            get => GetValue(InputProperty) as Brush;
            set => SetValue(InputProperty, value);
        }

        public double HematoxylinIndex
        {
            get => (double)GetValue(HematoxylinIndexProperty);
            set => SetValue(HematoxylinIndexProperty, value);
        }

        public double EosinIndex
        {
            get => (double)GetValue(EosinIndexProperty);
            set => SetValue(EosinIndexProperty, value);
        }

        public double DabIndex
        {
            get => (double)GetValue(DabIndexProperty);
            set => SetValue(DabIndexProperty, value);
        }

        public double HematoxylinIntensity
        {
            get => (double)GetValue(HematoxylinIntensityProperty);
            set => SetValue(HematoxylinIntensityProperty, value);
        }

        public double EosinIntensity
        {
            get => (double)GetValue(EosinIntensityProperty);
            set => SetValue(EosinIntensityProperty, value);
        }

        public double DabIntensity
        {
            get => (double)GetValue(DabIntensityProperty);
            set => SetValue(DabIntensityProperty, value);
        }

        public Color HematoxylinColor
        {
            get => (Color)GetValue(HematoxylinColorProperty);
            set => SetValue(HematoxylinColorProperty, value);
        }

        public Color EosinColor
        {
            get => (Color)GetValue(EosinColorProperty);
            set => SetValue(EosinColorProperty, value);
        }

        public Color DabColor
        {
            get => (Color)GetValue(DabColorProperty);
            set => SetValue(DabColorProperty, value);
        }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public PseudoStaining()
            : base()
        {
            
        }

        /// <summary>
        /// Updates all pixel shader properties
        /// </summary>
        protected internal override void UpdateShader()
        {
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(HematoxylinIndexProperty);
            UpdateShaderValue(EosinIndexProperty);
            UpdateShaderValue(DabIndexProperty);
            UpdateShaderValue(HematoxylinIntensityProperty);
            UpdateShaderValue(EosinIntensityProperty);
            UpdateShaderValue(DabIntensityProperty);
            UpdateShaderValue(HematoxylinColorProperty);
            UpdateShaderValue(EosinColorProperty);
            UpdateShaderValue(DabColorProperty);
        }
    }
}
